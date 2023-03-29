using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class WorldGeneratorBehaviour : MonoBehaviour
{
	[Header("Scene references")]
	public PlayerBehaviour player;
	public Transform housesContainer;
	public Transform sidewalkContainer;
	public Transform[] stickyObjects;
	public Camera renderCamera;
	
	[Header("Asset references")]
	public HouseBehaviour housePrefab;
	public GameObject sidewalkSlab;
	
	[Header("config")]
	public float distanceBetweenHouses;
	public float distanceBetweenHouseRandomMod;
	public float distanceInFuture;
	public float killInPastX;
	public float distanceBetweenSidewalkSlabs;

	[Header("runtime")]
	public float generatedToX;

	public float generatedSidewalksToX;
	private ObjectPool<GameObject> _sideWalkPool;
	private List<GameObject> _sidewalkSlabs = new List<GameObject>();

	public List<HouseBehaviour> houses = new List<HouseBehaviour>();
	
	
    // Start is called before the first frame update
    void Start()
    {
	    _sideWalkPool = new ObjectPool<GameObject>(() => Instantiate(sidewalkSlab, sidewalkContainer));
    }

    // Update is called once per frame
    void Update()
    {
	    // foreach (var sticky in stickyObjects)
	    // {
		   //  sticky.po
	    // }
	    
	    
	    UpdateSidewalks();
	    
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

    void UpdateSidewalks()
    {
	    var x = player.transform.position.x;
	    var futureX = x + distanceInFuture;

	    if (futureX <= generatedSidewalksToX)
	    {
		    // we don't need to generate anything, because its already been generated!
		    return; // TODO: perhaps we need to clean up old houses.... 
	    }

	    var nextX = generatedSidewalksToX + distanceBetweenSidewalkSlabs;
	    generatedSidewalksToX = nextX;

	    var slab = _sideWalkPool.Get();
	    slab.transform.localPosition = new Vector3(nextX, 0, 0);

	    var yRot = 0;
	    if (Random.value > .6f)
	    {
		    yRot = Random.Range(-1, 2);
	    }
	    var zRot = 0;
	    if (Random.value > .9f)
	    {
		    zRot = Random.Range(-1, 2);
	    }
	    slab.transform.localRotation = Quaternion.Euler(0, yRot, zRot);
	    _sidewalkSlabs.Add(slab);
	    
	    var toKill = new List<GameObject>();
	    foreach (var oldSlab in _sidewalkSlabs)
	    {
		    var dist = oldSlab.transform.position.x - player.transform.position.x;
		    if (dist < -30)
		    {
			    toKill.Add(oldSlab);
		    }
	    }

	    foreach (var kill in toKill)
	    {
		    _sidewalkSlabs.Remove(kill);
		    _sideWalkPool.Release(kill);
	    }
    }
}
