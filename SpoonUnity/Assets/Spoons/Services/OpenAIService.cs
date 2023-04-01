using Beamable.Common;
using Beamable.Common.Api.Notifications;
using Beamable.Common.Content;
using Beamable.Server.Clients;
using SpoonsCommon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OpenAIService
{
	private readonly SpoonsClient _client;
	private readonly INotificationService _notificationService;

	private Dictionary<string, Convo> _convos = new Dictionary<string, Convo>();

	public OpenAIService(SpoonsClient client, INotificationService notificationService)
	{
		_client = client;
		_notificationService = notificationService;
		
		_notificationService.Subscribe<ConvoUpdate>(SpoonsCommon.Constants.AI_EVENT_CHANNEL, OnAIUpdate);
		_notificationService.Subscribe<ConvoUpdateAIDone>(SpoonsCommon.Constants.AI_EVENT_CHANNEL_COMPLETE, OnAIComplete);
	}

	void OnAIComplete(ConvoUpdateAIDone data)
	{
		if (!_convos.TryGetValue(data.convoId, out var convo))
		{
			Debug.LogWarning("Received a convo update without any local tracking :(");
			return;
		}

		var aiMessage = convo.aiMessages[convo.aiMessages.Count -1];
		aiMessage.isComplete = true;
		aiMessage.result = data.result;
		
		convo.OnUpdated?.Invoke();
		if (aiMessage.result.isSlam)
		{
			convo.OnSlam?.Invoke();
			_convos.Remove(convo.id);
		} else if (aiMessage.result.isSale)
		{
			convo.OnSale?.Invoke(data.result);
			_convos.Remove(convo.id);
		}
		

	}
	
	void OnAIUpdate(ConvoUpdate data)
	{
		if (!_convos.TryGetValue(data.convoId, out var convo))
		{
			Debug.LogWarning("Received a convo update without any local tracking :(");
			return;
		}

		var aiMessage = convo.GetOrCreateLatestAIMessage();
		aiMessage.parts.Add(new ConvoAIMessagePart
		{
			order = data.order,
			part = data.nextPart,
			annoyedRating = data.annoyedRating
		});
		aiMessage.parts.Sort((a, b) => a.order.CompareTo(b.order));
		

		if (data.ratingStartIndex > -1)
		{
			aiMessage.message = string.Join("", aiMessage.parts.Select(p => p.part));
			aiMessage.message =
				aiMessage.message.Substring(data.ratingEndIndex + 1);
		}
		else
		{
			aiMessage.message = "....";
		}

		convo.OnUpdated?.Invoke();
	}

	public async Promise ContinueConvo(string convoId, string message)
	{
		if (!_convos.TryGetValue(convoId, out var convo))
		{
			Debug.LogWarning("No convo exists to continue by that id :(");
			return;
		}

		// be optimistic, and emit new event...
	
		var req = _client.Continue(convo, message);
		convo.userMessages.Add(new ConvoUserMessage
		{
			order = convo.NextOrder,
			message = message
		});
		convo.aiMessages.Add(new ConvoAIMessage
		{
			order = convo.NextOrder,
			message = "..."
		});
		convo.OnUpdated?.Invoke();

		var next = await req;
		convo.userMessages = next.userMessages;
		convo.aiMessages = next.aiMessages;
	}
	
    public async Promise<Convo> StartConvo(string message)
    {
	    var convo = await _client.Start(message);
	    _convos.Add(convo.id, convo);
	    return convo;
    }
}

[Serializable]
public class OptionalConvo : Optional<Convo>
{
	
}
