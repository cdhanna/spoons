
using Beamable;
using Beamable.Common;
using DefaultNamespace.Spoons.Services;
using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	#if UNITY_EDITOR
	[UnityEditor.MenuItem("SPOONS/CLEARPREFS")]
	public static void ClearPrefs()
	{
		PlayerPrefs.DeleteAll();
	}
	#endif
	
	[Header("scene references")]
	public GameObject mainMenuGob;
	public GameObject instructionGob;
	public GameObject giftGob;

	private BeamContext _ctx;
	// public GameObject

	private async void Start()
	{
		_ctx = await BeamContext.Default.Instance;

		var stateService = _ctx.GameStateService();

		stateService.OnStateChanged += (old, next) =>
		{
			if (next == GameState.MENU)
			{
				SetForMenu();
			}
		};
		if (stateService.State == GameState.MENU)
		{
			SetForMenu();
		}
	}

	public void SetForMenu()
	{
		var stateService = _ctx.GameStateService();
		if (stateService.State == GameState.MENU)
		{
			mainMenuGob.SetActive(true);
		}
	}
}

