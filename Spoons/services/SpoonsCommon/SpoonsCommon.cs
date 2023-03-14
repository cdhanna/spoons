using System;
using System.Collections.Generic;

namespace SpoonsCommon
{
	/*
	 * Code you intend to share between the Microservice and a Unity project
	 */

	public class Constants
	{
		public const string AI_EVENT_CHANNEL = "AI.UPDATE";
		public const string AI_EVENT_CHANNEL_COMPLETE = "AI.UPDATE.COMPLETE";
	}
	
	public class ConvoResponse
	{
		
		public string message;
	}

	[Serializable]
	public class ConvoRequest
	{
		public string convoId;
		public string prompt;
	}

	[Serializable]
	public class ConvoUpdate
	{
		public string convoId;
		public string nextPart;
		public int order;
		public int annoyedRating;
		public int ratingStartIndex, ratingEndIndex;
	}

	[Serializable]
	public class ConvoUpdateAIDone
	{
		public string convoId;
		public ConvoAIMessageOutcome result;
	}
	
	
	[Serializable]
	public class Convo
	{
		public string id;

		public string openingPrompt;
		public List<ConvoUserMessage> userMessages = new List<ConvoUserMessage>();
		public List<ConvoAIMessage> aiMessages = new List<ConvoAIMessage>();

		public Action OnUpdated;
		public Action OnSlam;
		public Action<ConvoAIMessageOutcome> OnSale;
	
		public int NextOrder => userMessages.Count + aiMessages.Count;

		public bool IsAiTalking => !GetOrCreateLatestAIMessage().isComplete && aiMessages.Count == userMessages.Count;
	
		public List<IConvoMessage> ProcessConvo()
		{
			var messages = new List<IConvoMessage>();
			messages.AddRange(userMessages);
			messages.AddRange(aiMessages);
			messages.Sort((a, b) => a.Order.CompareTo(b.Order));

			return messages;
		}

		public ConvoAIMessage GetOrCreateLatestAIMessage()
		{
			ConvoAIMessage aiMessage = null;
			if (aiMessages.Count == 0 || aiMessages[aiMessages.Count - 1].isComplete)
			{
				aiMessage = new ConvoAIMessage {order = NextOrder, isComplete = false, parts = new List<ConvoAIMessagePart>()};
				aiMessages.Add(aiMessage);
			}
			else
			{
				aiMessage = aiMessages[aiMessages.Count - 1];
			}

			return aiMessage;
		}
	}

	public interface IConvoMessage
	{
		int Order { get; }
		string Message { get; }
		bool IsPlayer { get; }
	}

	public static class IConvoMessageExtensions
	{
		public static string Id(this IConvoMessage msg)
		{
			return $"{(msg.IsPlayer ? "plr" : "ai")}.{msg.Order}";
		}
	}

	[Serializable]
	public class ConvoAIMessageOutcome
	{
		public bool isSlam;
		public bool isSale;
		public int saleCount;
		public int salePrice;
	}

	[Serializable]
	public class ConvoAIMessage : IConvoMessage
	{
		public int order;
		public List<ConvoAIMessagePart> parts = new List<ConvoAIMessagePart>();
		public bool isComplete;
		public string message;
		public ConvoAIMessageOutcome result;
		public int Order => order;
		public string Message => message;
		public bool IsPlayer => false;
	}

	[Serializable]
	public class ConvoAIMessagePart
	{
		public int order;
		public string part;
		public int annoyedRating;
	}

	[Serializable]
	public class ConvoUserMessage : IConvoMessage
	{
		public int order;
		public string message;
		public int Order => order;
		public string Message => message;
		public bool IsPlayer => true;
	}

}
