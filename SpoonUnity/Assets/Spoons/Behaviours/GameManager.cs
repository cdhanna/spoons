
using Beamable;
using Beamable.Common;
using DefaultNamespace.Spoons.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Spoons.Behaviours;
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
	public List<EnabledOnStateChangeBehaviour> stateObjects;
	private BeamContext _ctx;
	// public GameObject

	private async void Start()
	{
		_ctx = await BeamContext.Default.Instance;

		var stateService = _ctx.GameStateService();

		stateService.OnStateChanged += (old, next) =>
		{
			SetForState(next);
		};
		if (stateService.State == GameState.MENU)
		{
			SetForState(stateService.State);

		}
	}

	public void SetForState(GameState state)
	{
		foreach (var obj in stateObjects)
		{
			var shouldBeEnabled = obj.enableOnState == state;
			obj.gameObject.SetActive(shouldBeEnabled);
		}
	}
	
	
}

