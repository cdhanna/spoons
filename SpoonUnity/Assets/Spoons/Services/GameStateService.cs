
using Beamable.Common.Content;
using Beamable.Common.Dependencies;
using System;

[Serializable]
public class GameStateService : IServiceStorable
{
	private readonly PlayerService _playerService;

	public GameData data = new GameData();
	
	public GameState State { get; private set; }

	public Action<GameState, GameState> OnStateChanged;

	public GameStateService(PlayerService playerService)
	{
		_playerService = playerService;
		State = GameState.MENU; // TODO: config?
		// GotoState(GameState.WALKING);
	}
	
	

	public void TalkToHouse()
	{
		if (State != GameState.WALKING) return;
		
		var house = _playerService.nearbyHouse;
		house.visited = true;
		
		GotoState(GameState.TALKING);
	}
	
	
	public void LeaveHouse()
	{
		if (State != GameState.TALKING) return;
		GotoState(GameState.WALKING);
	}

	public void GotoGame()
	{
		if (State != GameState.MENU) return;
		GotoState(GameState.WALKING);
	}
	
	
	public void GoToMenu()
	{
		if (State != GameState.WALKING) return;
		GotoState(GameState.MENU);
	}

	private void GotoState(GameState state)
	{
		var old = State;
		State = state;
		OnStateChanged?.Invoke(old, state);
	}

	public void OnBeforeSaveState()
	{
		
	}

	public void OnAfterLoadState()
	{
		data = new GameData();
	}

}

public enum GameState
{
	MENU,
	WALKING,
	TALKING,
}

[Serializable]
public class GameData
{
	public bool hasGame;
	public bool acceptedGift;
	public bool hasHat;
	public bool hasTie;
	public int sales;
	public int slams;
	public long simulatedUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
}
