using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTrackerBehaviour : MonoBehaviour
{
	[Header("Scene references")]
	public PlayerBehaviour player;
	public Camera cam;

	[Header("Config")]
	public float padding = 1;

	public float speed = 1;


	public float requiredMovement;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // get the distance from the player to the center of the screen
        var diff = cam.transform.position.x - player.transform.position.x;
        var dist = Mathf.Abs(diff);

        requiredMovement = -Mathf.Sign(diff) * Mathf.Max(0, dist - padding);

        cam.transform.position += new Vector3(speed * Time.deltaTime * requiredMovement ,0,0);
    }
}
