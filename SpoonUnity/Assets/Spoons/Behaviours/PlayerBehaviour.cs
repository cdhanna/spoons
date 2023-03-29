using Beamable;
using DefaultNamespace.Spoons.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerBehaviour : MonoBehaviour
{
	[Header("scene references")]
	public SpriteRenderer playerRenderer;
	public CameraTrackerBehaviour tracker;
	public GameObject standingSuitcase;
	public GameObject movingSuitcase;
	public GameObject hat;
	public GameObject tie;
	public SpriteRenderer background;

	[Header("asset references")]
	public Sprite standingSprite;
	public Sprite movingSprite;

	public AudioClip[] footsteps;
	
	[Header("config")]
	public float speed;
	public float friction;
	public float distanceToRotate;
	public int rotateIndexCount = 2;
	public float rotateRange = 20;

	[Header("runtime state")]
	public int footstepIndex;
	public float acceleration;
	public float velocity;
	public float distanceTravelledSinceLastRotation;
	public int rotateIndex;
	public float distanceTravelled;
	private GameStateService _stateService;

	// Start is called before the first frame update
    async void Start()
    {
	    var ctx = await BeamContext.Default.Instance;
	    ctx.PlayerService().RegisterPlayer(this);
	    
	    _stateService = ctx.GameStateService();

	    
	    hat.SetActive(ctx.GameStateService().data.hasHat);
	    tie.SetActive(ctx.GameStateService().data.hasTie);
	    _stateService.OnStateChanged += (state, gameState) =>
	    {
		    hat.SetActive(ctx.GameStateService().data.hasHat);
		    tie.SetActive(ctx.GameStateService().data.hasTie);
	    };
    }

    // Update is called once per frame
    void Update()
    {
	    if (_stateService != null && _stateService.State != GameState.WALKING) return; 
	    
	    
	    acceleration = 0;
	    if (Input.GetKey(KeyCode.LeftArrow) & tracker.requiredMovement >= 0)
	    {
		    acceleration -= speed;
		    
	    }

	    if (Input.GetKey(KeyCode.RightArrow))
	    {
		    acceleration += speed;
	    }

	    acceleration -= velocity * friction;
	    velocity += acceleration;
	    velocity *= Time.deltaTime;
	    transform.localPosition += new Vector3(velocity, 0, 0);
	    distanceTravelledSinceLastRotation += Mathf.Abs(velocity);
	    distanceTravelled += Mathf.Abs(velocity);

	    if (distanceTravelledSinceLastRotation > distanceToRotate)
	    {
		    rotateIndex++;
		    rotateIndex %= rotateIndexCount;

		    var min = -rotateRange;
		    var max = rotateRange;

		    var angle = rotateIndex == 0 ? min : max;
		    angle += Random.Range(-5, 5);
		    distanceTravelledSinceLastRotation = 0; // TODO: random factor?
		    playerRenderer.transform.localRotation = Quaternion.Euler(0, 0, angle);
		    
		    SoundManager.Instance.sfxSource.PlayOneShot(footsteps[(Random.Range(1, 3) + footstepIndex++)%footsteps.Length], .3f);
	    }

	    var isMoving = Mathf.Abs(velocity) > speed * Time.deltaTime * .5f;
	    if (!isMoving)
	    {
		    playerRenderer.transform.localRotation = Quaternion.Euler(0, 0, 0);
	    }
	    playerRenderer.sprite = isMoving ? movingSprite : standingSprite;
	    
	    standingSuitcase.SetActive(!isMoving);
	    movingSuitcase.SetActive(isMoving);

	    // background.transform.position = new Vector3(transform.position.x, background.transform.position.y, 0);
    }
}
