using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGeneratorBehaviour : MonoBehaviour
{
	[Header("Scene references")]
	public PlayerBehaviour player;
	public Transform housesContainer;
	
	[Header("Asset references")]
	public HouseBehaviour housePrefab;

	[Header("config")]
	public float distanceBetweenHouses;
	public float distanceBetweenHouseRandomMod;
	public float distanceInFuture;
	public float killInPastX;

	[Header("runtime")]
	public float generatedToX;

	public List<HouseBehaviour> houses = new List<HouseBehaviour>();
	
	
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
	    var x = player.transform.position.x;
	    var futureX = x + distanceInFuture;

	    if (futureX <= generatedToX)
	    {
		    // we don't need to generate anything, because its already been generated!
		    return; // TODO: perhaps we need to clean up old houses.... 
	    }

	    var nextX = generatedToX + distanceBetweenHouses +
	                Random.Range(-distanceBetweenHouseRandomMod * .5f, distanceBetweenHouseRandomMod * .5f);
	    
	    generatedToX = nextX;

	    var newHouse = Instantiate(housePrefab, housesContainer);
	    houses.Add(newHouse);
	    newHouse.transform.localPosition = new Vector3(nextX, 0, 0);
	    
	    // kill any house further than pastX away...
	    var toKill = new List<HouseBehaviour>();
	    foreach (var house in houses)
	    {
		    var dist = house.transform.position.x - player.transform.position.x;
		    if (dist < killInPastX)
		    {
			    toKill.Add(house);
		    }
	    }

	    foreach (var kill in toKill)
	    {
		    houses.Remove(kill);
		    Destroy(kill.gameObject);
	    }
    }
}
