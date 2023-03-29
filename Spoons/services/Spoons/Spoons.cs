using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Beamable.Common;
using Beamable.Server;
using Beamable.Server.Api.RealmConfig;
using Beamable.Spoons.Services;
using SpoonsCommon;

namespace Beamable.Spoons
{
	[Microservice("Spoons")]
	public class Spoons : Microservice
	{
		[ConfigureServices()]
		public static void Configure(IServiceBuilder builder)
		{
			builder.Builder.AddScoped<OpenAI>();
			builder.Builder.AddSingleton<Config>();
			builder.Builder.AddSingleton<ScenarioGG>();
			builder.Builder.AddSingleton<HttpClient>(p => new HttpClient());
		}

		[InitializeServices()]
		public static async Task Init(IServiceInitializer init)
		{
			var config = init.GetService<Config>();
			await config.Init();
		}

		[ClientCallable]
		public async Promise<ImageReference> GetHouse()
		{
			var gg = Provider.GetService<ScenarioGG>();
			var image = await gg.GetRandomURL();
			return new ImageReference
			{
				url = image.url
			};
		}
		
		[ClientCallable]
		public async Promise<Convo> Start(string message)
		{
			var oai = Provider.GetService<OpenAI>();

			var visits = await Services.Inventory.GetCurrency("currency.visits");

			var prompts = new string[]
			{
				"The AI doesn't want the spoons, but wants to waste the salesman time by asking deep life questions. If the Salesman doesn't answer the questions, the AI will slam the door in his face and say, \"Slam\".  The AI is very insecure.",
				"The AI will only buy the spoons if the Salesman pretends to be a fish. If the AI doesn't find the saleman interesting, it will slam the door in his face and say, \"Slam\".  The AI only uses words with more than 3 syllables",
				"The AI won't buy the spoons . No matter what, the AI won't buy spoons. The AI is very rude, skeptical, and often insults the player.",
				"The AI is a travelling salesman also, but they sell toasters. The AI will only buy the spoons if the other person agrees to buy a toaster.",
				"The AI only speaks in the third person, and it thinks its name is Tim. Every time the AI speaks, it speaks in third person.",
				"The AI is terrified of spoons. The AI thinks that their parents died in a spoon factory accident.",
				"The AI will only buy the spoons if they are from Boston. The AI hates New York, but loves Boston. The AI will want to make sure that the spoons are from Boston by asking the salesman stories about Boston. ",
				"The AI will only buy the spoons if they are part of a religious ritual. The AI will want to know specific details about the ritual. if the AI thinks the salesman is making it up, the AI will slam the door. ",
				"The AI will try to convince the Salesman that the Salesman is a chicken. ",
				"The AI is doing its math homework, and needs help on it. The AI will only buy the spoons if the Salesman can help with the Math homework. ",
				"The AI only speaks pig latin. ",
				"The AI will only buy the spoons if the Salesman flatters him.",
			};

			var mods = new string[]
			{
				"The AI will slam the door in the Salesman's face after 2 or 3 sentences. ",
				"The AI will slam the door in the Salesman's face immediately. ",
				"The AI is skeptical and doesn't believe anything. ",
				"The AI grew up in a spoon town. ",
				"The AI is morbid and brings up death a lot. ",
				"The AI is an old man who thinks that technology is evil. ",
				"The AI thinks the Salesman is a CIA agent.  ",
				"The AI is curt. ",
			};

			
			var promptText = prompts[visits % prompts.Length];
			promptText = "The following is a conversation between a traveling spoon salesman an AI. " +
			             promptText;

			var r = new Random();
			var m = r.Next();
			var mod = mods[m % mods.Length];
			promptText += " " + mod;
			
			promptText += "The AI doesn't say the word \"interesting\". If the AI catches the salesman in a lie, it will slam the door in his face and say, \"Slam\". If the AI says \"Slam\", the conversation is over. If the AI offers to buy the spoons, it will say the exact words, \"I'll buy the spoons\".";
			promptText +=
				"Everytime the AI responds, it always includes an annoyance rating from 1 to 10. This rating is a number only and is in square brackets and always comes first.";
			var convoId = Guid.NewGuid().ToString();
			var convo = new Convo
			{
				id = convoId,
				openingPrompt = promptText,
				// openingPrompt = "The following is a conversation between a traveling spoon salesman an AI who needs spoons, but needs a tiny bit of convincing. The AI will eventually offer to buy 10 spoons. The AI will only be convinced if the salesman can explain how to make chicken noodle soup. If the AI doesn't find the saleman interesting, it will slam the door in his face and say, \"Slam\". If the AI catches the salesman in a lie, it will slam the door in his face and say, \"Slam\". If the AI says \"Slam\", the conversation is over. If the AI offers to buy the spoons, it says, \"I'll buy 10\". The AI is mean and hurtful and often insults the salesman or implies the salesman is a bad human.",
				userMessages = new List<ConvoUserMessage>
				{
					new ConvoUserMessage
					{
						order = 0,
						message = message
					}
				},
				aiMessages = new List<ConvoAIMessage>
				{
					new ConvoAIMessage
					{
						order = 1,
						message = ""
					}
				}
			};
			
			var request = new ConvoRequest
			{
				convoId = convoId,
				prompt = BuildRequestMessage(convo)
			};
			
			/*
			 * container
			 * connection (websocket connection)
			 * request
			 */
			
			var _ = oai.Send(request); // TODO: this leaves a dangling process, and it would be good to prevent service shutdown on this completing... Expose the monitor api.
			
			await Services.Inventory.Update(b => b.CurrencyChange("currency.visits", 1));
			return convo;
		}

		[ClientCallable]
		public async Promise<Convo> Continue(Convo convo, string message)
		{
			var oai = Provider.GetService<OpenAI>();
			
			// re-assemble the whole convo... 
			convo.userMessages.Add(new ConvoUserMessage
			{
				order = convo.NextOrder,
				message = message
			});
			convo.aiMessages.Add(new ConvoAIMessage
			{
				order = convo.NextOrder,
				message = ""
			});
			
			var request = new ConvoRequest
			{
				convoId = convo.id,
				prompt = BuildRequestMessage(convo)
			};

			var _ = oai.Send(request); // TODO: this leaves a dangling process, and it would be good to prevent service shutdown on this completing... Expose the monitor api.
			return convo;
		}

		string BuildRequestMessage(Convo convo)
		{
			var sb = new StringBuilder();
			sb.Append(convo.openingPrompt);
			sb.Append("\n");
			sb.Append("\n");
			
			// foreach (var message in convo.ProcessConvo())
			var messages = convo.ProcessConvo();
			for (var i = 0 ; i < messages.Count; i ++)
			{
				var message = messages[i];
				if (message.IsPlayer)
				{
					sb.Append("Human:");
					sb.Append(message.Message);
				}
				else
				{
					sb.Append("AI:");
					sb.Append(message.Message);
				}

				if (i < messages.Count - 1)
				{
					sb.Append("\n");
				}
			}

			return sb.ToString();
		}
	}
}
