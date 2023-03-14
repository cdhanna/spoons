using System.IO;
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
    private const string URL = "https://api.openai.com/v1/completions";
    
    // TODO: Move this into a config!!!
    private const string KEY = "REDACTED";

    private readonly RequestContext _ctx;
    private readonly HttpClient _httpClient;
    private readonly IMicroserviceNotificationsApi _notifications;

    public OpenAI(RequestContext ctx, HttpClient httpClient, IMicroserviceNotificationsApi notifications)
    {
        _ctx = ctx;
        _httpClient = httpClient;
        _notifications = notifications;
    }

    public async Promise Send(ConvoRequest request)
    {
        var model = new CompletionRequest
        {
            prompt = request.prompt,
            stream = true
        };
        var json = JsonConvert.SerializeObject(model);
        
        var req = new HttpRequestMessage(HttpMethod.Post, URL);
        req.Headers.Add("Authorization", $"Bearer {KEY}");
        req.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var order = 0;
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

                var completionResponse = JsonConvert.DeserializeObject<CompletionResponse>(lineJson);

                var text = completionResponse.choices[0].text.ReplaceLineEndings(" ");
                if (string.IsNullOrEmpty(text))
                {
                    continue;
                }

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

                wholeText += text;
                var update = new ConvoUpdate
                {
                    order = order++,
                    convoId = request.convoId,
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
            // TODO: Hack, wait a bit...
            await Task.Delay(100);
            _notifications.NotifyPlayer(_ctx.UserId, SpoonsCommon.Constants.AI_EVENT_CHANNEL_COMPLETE, new ConvoUpdateAIDone
            {
                convoId = request.convoId,
                result = result
            });
        }

    }

    public class CompletionResponse
    {
        public string id;
        [JsonProperty("object")]
        public string objectType;

        public string model;
        public long created;

        public CompletionResponseChoice[] choices;
        public CompletionResponseUsage usage;
    }

    public class CompletionResponseChoice
    {
        public string text;
        private int index;
        public string finish_reason;
    }
    
    
    public class CompletionResponseUsage
    {
        public int total_tokens;
        private int prompt_tokens;
        public int completion_tokens;
    }
    class CompletionRequest
    {
        public string model = "text-davinci-003";
        public string prompt = "";
        public int max_tokens = 100;
        public double frequency_penalty = 1.3;
        public double presence_penalty = .7;
        public double temperature = .9;
        public int top_p = 1;
        public bool stream = false;
        public int n = 1;
        public string logprobs = null;
        public string[] stop = new[] { "AI:", "Human:" };
    }
}