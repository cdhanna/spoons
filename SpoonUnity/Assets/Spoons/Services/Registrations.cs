using Beamable;
using Beamable.Common.Dependencies;
using Beamable.Player;

namespace DefaultNamespace.Spoons.Services
{
	[BeamContextSystem]
	public static class Registrations
	{
		// register custom services
		[RegisterBeamableDependencies()]
		public static void Register(IDependencyBuilder builder)
		{
			builder.AddSingleton<OpenAIService>();
			builder.AddSingleton<HouseService>();
			builder.AddSingleton<PlayerService>();
			builder.AddScopedStorage<GameStateService, OfflineCacheStorageLayer>();
			builder.AddSingleton<GameStateService>();
		}

		// extension methods for ease of access
		public static OpenAIService AIService(this BeamContext ctx)
		{
			return ctx.ServiceProvider.GetService<OpenAIService>();
		}
		public static HouseService HouseService(this BeamContext ctx)
		{
			return ctx.ServiceProvider.GetService<HouseService>();
		}
		public static PlayerService PlayerService(this BeamContext ctx)
		{
			return ctx.ServiceProvider.GetService<PlayerService>();
		}
		public static GameStateService GameStateService(this BeamContext ctx)
		{
			return ctx.ServiceProvider.GetService<GameStateService>();
		}
	}
}
