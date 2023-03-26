using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Beamable.Common;
using Newtonsoft.Json;
using SpoonsCommon;

namespace Beamable.Spoons.Services;

public class ScenarioGG
{
    private readonly HttpClient _client;
    private readonly Config _config;

    public ScenarioGG(HttpClient client, Config config)
    {
        _client = client;
        _config = config;
    }

    public async Promise<ScenarioGGImage> GetRandomURL()
    {
        // TODO: use a service user cache...
        var all = await MakeRequest();
        var r = new Random();

        return all[r.Next(all.Count)];
    }

    public async Promise<List<ScenarioGGImage>> MakeRequest()
    {
        var modelId = "EXTncT75SCGbEyO5X1Macg";
        var url = $"https://api.cloud.scenario.gg/v1/models/{modelId}/inferences";
        
        var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Add("Authorization", $"Basic {_config.ScenarioGGKey}");

        var res = await _client.SendAsync(req);
        var jsonRes = await res.Content.ReadAsStringAsync();
        var response = JsonConvert.DeserializeObject<ScenarioGGInferencesResponse>(jsonRes);

        return response.inferences.SelectMany(i => i.images).ToList();
    }

    
    public class ScenarioGGInferencesResponse
    {
        public List<ScenarioGGInference> inferences = new List<ScenarioGGInference>();
    }

    public class ScenarioGGInference
    {
        public List<ScenarioGGImage> images = new List<ScenarioGGImage>();
    }

    public class ScenarioGGImage
    {
        public string id;
        public string url;
    }
}