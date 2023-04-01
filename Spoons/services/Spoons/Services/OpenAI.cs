using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Beamable.Common;
using Beamable.Server;
using Beamable.Server.Api.Notifications;
using Newtonsoft.Json;
using SpoonsCommon;

namespace Beamable.Spoons.Services;

public class OpenAI
{
    private const string COMPLETIONS_URL = "https://api.openai.com/v1/completions";
    private const string CHAT_URL = "https://api.openai.com/v1/chat/completions";
    
    private readonly RequestContext _ctx;
    private readonly HttpClient _httpClient;
    private readonly IMicroserviceNotificationsApi _notifications;
    private readonly Config _config;
    private readonly PromptService _promptService;

    public OpenAI(RequestContext ctx, HttpClient httpClient, IMicroserviceNotificationsApi notifications, Config config, PromptService promptService)
    {
        _ctx = ctx;
        _httpClient = httpClient;
        _notifications = notifications;
        _config = config;
        _promptService = promptService;
    }

    public static ChatMessage[] ConvertConvoToMessages(Convo convo)
    {
        var count = 1 + convo.aiMessages.Count + convo.userMessages.Count;
        var messages = new List<ChatMessage>();

        messages.Add(new ChatMessage
        {
            role = "system",
            content = convo.openingPrompt
        });
        foreach (var convoMessage in convo.ProcessConvo())
        {
            messages.Add(new ChatMessage
            {
                role = convoMessage.IsPlayer ? "user" : "assistant",
                content = convoMessage.Message
            });
        }

        return messages.ToArray();
    }
    
    public async Promise SendChat(Convo convo)
    {
        var messages = ConvertConvoToMessages(convo);
        var model = new ChatGpt3_5TurboRequest()
        {
            stream = true,
            messages = messages,
            temperature = 1.3,
            presence_penalty = 1.5,
        };
        var json = JsonConvert.SerializeObject(model);

        var previousAnnoyedRating = 1;
        if (convo.aiMessages.Count > 0)
        {
            previousAnnoyedRating = convo.aiMessages.Last().parts[0].annoyedRating;
        }
        
        convo.aiMessages.Add(new ConvoAIMessage
        {
            message = "",
            order = convo.NextOrder
        });
        
        var req = new HttpRequestMessage(HttpMethod.Post, CHAT_URL);
        req.Headers.Add("Authorization", $"Bearer {_config.OpenApiKey}");
        req.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var order = 0;
        // var sw = new Stopwatch();
        // sw.Start();
        using HttpResponseMessage response = await _httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
        using Stream streamToReadFrom = await response.Content.ReadAsStreamAsync();
        using StreamReader reader = new StreamReader(streamToReadFrom);
        
        
        var wholeText = "";
        var hasRating = false;
        var annoyedRating = 3;
        var ratingStartIndex = -1;
        var ratingEndIndex = -1;
        try
        {
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(line)) continue;
        
                if (!line.StartsWith("data: ")) continue;
                if (line.Equals("data: [DONE]"))
                {
                    break;
                }
        
                var lineJson = line.Substring("data: ".Length);
        
                var completionResponse = JsonConvert.DeserializeObject<ChatResponse>(lineJson);

                if (completionResponse?.choices[0].delta?.content == null)
                {
                    continue;
                }
                var text = completionResponse.choices[0].delta.content.ReplaceLineEndings(" ");
                if (string.IsNullOrEmpty(text))
                {
                    continue;
                }
                //
                if (!hasRating)
                {
                    var openBracket = wholeText.IndexOf("[");
                    if (openBracket >= 0)
                    {
                        var endBrack = wholeText.IndexOf("]", openBracket);
                        if (endBrack >= 0)
                        {
                            hasRating = true;
                            openBracket += 1;
                            var annoyedStr = wholeText.Substring(openBracket , endBrack - openBracket);
                            
                            annoyedRating = int.Parse(annoyedStr);
                            ratingEndIndex = endBrack;
                            ratingStartIndex = openBracket ;
                        }
                    }
                }

                if (wholeText.Length > "[10]".Length && !hasRating)
                {
                    hasRating = true;
                    annoyedRating = previousAnnoyedRating;
                    ratingStartIndex = 0;
                    ratingEndIndex = -1;
                }
                //
                wholeText += text;
                var update = new ConvoUpdate
                {
                    order = order++,
                    convoId = convo.id,
                    nextPart = text,
                    annoyedRating = annoyedRating,
                    ratingEndIndex = ratingEndIndex,
                    ratingStartIndex = ratingStartIndex
                };
                _notifications.NotifyPlayer(_ctx.UserId, SpoonsCommon.Constants.AI_EVENT_CHANNEL, update);
            }
        }
        finally
        {
            var final = wholeText.ToLower().Trim();
            BeamableLogger.Log("DONE READING");
            BeamableLogger.Log(final);
            var result = new ConvoAIMessageOutcome
            {
        
            };
            if (final.Contains("i'll buy") || final.Contains("I will buy") || final.Contains("ill buy") || final.Contains("i will take"))
            {
                result.isSale = true;
                result.saleCount = 1;
                result.salePrice = 1;
            }
            if (final.Contains("slam"))
            {
                result.isSale = false;
                result.isSlam = true;
            }
            // // TODO: Hack, wait a bit...
            await Task.Delay(100);
            _notifications.NotifyPlayer(_ctx.UserId, SpoonsCommon.Constants.AI_EVENT_CHANNEL_COMPLETE, new ConvoUpdateAIDone
            {
                convoId = convo.id,
                result = result
            });
        }
        
    }
    
    
    public class CompletionResponseUsage
    {
        public int total_tokens;
        private int prompt_tokens;
        public int completion_tokens;
    }

    
    
    public class ChatResponse
    {
        public string id;
        [JsonProperty("object")]
        public string objectType;

        public string model;
        public long created;

        public ChatMessageResponseChoice[] choices;
        public CompletionResponseUsage usage;
    }

    public class ChatMessageResponseChoice
    {
        public int index;
        public ChatMessage delta;
    }
    
    
    public class ChatMessage
    {
        /// <summary>
        /// must be "system", "assistant", or "user"
        /// </summary>
        public string role;
        
        public string content;
    }
    public class ChatGpt3_5TurboRequest
    {
        public string model = "gpt-3.5-turbo";

        public ChatMessage[] messages;
        
        /// <summary>
        /// What sampling temperature to use, between 0 and 2.
        /// Higher values like 0.8 will make the output more random,
        /// while lower values like 0.2 will make it more focused and deterministic.
        /// We generally recommend altering this or top_p but not both.
        /// </summary>
        public double temperature = .9;
        
        /// <summary>
        /// An alternative to sampling with temperature, called nucleus sampling,
        /// where the model considers the results of the tokens with top_p probability mass.
        /// So 0.1 means only the tokens comprising the top 10% probability mass are considered.
        /// </summary>
        public int top_p = 1;

        public bool stream = false;
        
        /// <summary>
        /// How many chat completion choices to generate for each input message.
        /// </summary>
        public int n = 1;
        
        /// <summary>
        /// Up to 4 sequences where the API will stop generating further tokens.
        /// </summary>
        public string[] stop = new[] { "AI:", "Human:" };

        /// <summary>
        /// The maximum number of tokens to generate in the chat completion.
        /// The total length of input tokens and generated tokens is limited by the model's context length.
        /// </summary>
        public int max_tokens = 100;
        
        /// <summary>
        /// Number between -2.0 and 2.0.
        /// Positive values penalize new tokens based on whether they appear in the text so far,
        /// increasing the model's likelihood to talk about new topics.
        /// </summary>
        public double frequency_penalty = 1.3;
        
        /// <summary>
        /// Number between -2.0 and 2.0.
        /// Positive values penalize new tokens based on their existing frequency in the text so far,
        /// decreasing the model's likelihood to repeat the same line verbatim.
        /// </summary>
        public double presence_penalty = .7;
    }
}