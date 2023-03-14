
using Beamable.Common.Dependencies;
using Beamable.Coroutines;
using System;
using System.Collections;
using UnityEngine;

public class PlayerService
{
	private readonly IDependencyProviderScope _provider;
	private readonly HouseService _houseService;
	private readonly CoroutineService _coroutineService;
	public PlayerBehaviour player;


	public HouseBehaviour nearbyHouse;
	public Action<HouseBehaviour> OnHouseNearbyChanged;

	public PlayerService(IDependencyProviderScope provider, HouseService houseService, CoroutineService coroutineService)
	{
		_provider = provider;
		_houseService = houseService;
		_coroutineService = coroutineService;

		_coroutineService.StartNew("player", Loop());
	}
	
	public void RegisterPlayer(PlayerBehaviour player)
	{
		this.player = player;
	}

	IEnumerator Loop()
	{
		while (_provider.IsActive)
		{
			yield return null;

			if (player == null) continue;
			
			// check if the player is in range of any of the houses....
			var found = false;
			foreach (var house in _houseService.houses)
			{
				if (house.visited) continue;
				var diff = player.transform.position - house.transform.position;
				var dist = Mathf.Abs(diff.x);
				if (dist < 1f)
				{
					if (nearbyHouse != house)
					{
						OnHouseNearbyChanged?.Invoke(house);
					}
					nearbyHouse = house;
					found = true;
					break;
				}
			}

			if (!found)
			{
				if (nearbyHouse != null)
				{
					OnHouseNearbyChanged?.Invoke(null);
				}

				nearbyHouse = null;
			}

		}
	}
	
}

