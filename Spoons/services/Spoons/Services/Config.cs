using Beamable.Common;
using Beamable.Server.Api.RealmConfig;

namespace Beamable.Spoons.Services;

public class Config
{
    private readonly IMicroserviceRealmConfigService _realmConfigService;
    private RealmConfig _settings;

    public string OpenApiKey => _settings.GetSetting("game", "openapi");
    public string ScenarioGGKey => _settings.GetSetting("game", "scenariogg");

    public Config(IMicroserviceRealmConfigService realmConfigService)
    {
        _realmConfigService = realmConfigService;
    }

    public async Promise Init()
    {
        _settings = await _realmConfigService.GetRealmConfigSettings();
    }
    
    
}