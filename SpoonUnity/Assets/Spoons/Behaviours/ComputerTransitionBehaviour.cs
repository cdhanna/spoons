using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputerTransitionBehaviour : MonoBehaviour
{
	[Header("scene references")]
	public Camera mainCamera;
	public Camera uiCamera;
	public BoxCollider BoxCollider;
	
	[Header("references")]
	public RenderTexture renderTexture;
	public Material renderMaterial;

	[Header("config")]
	public float downSampleRatio = .5f;

	public float distance = 1;
	
	[Header("runtime")]
	public bool isFocused;
	public MaterialPropertyBlock materialBlock;
	
    // Start is called before the first frame update
    void Start()
    {
	
    }

    private int _lastHeight, _lastWidth;
    
    // Update is called once per frame
    void Update()
    {
	    if (Input.GetKeyDown(KeyCode.Space))
	    {
		    var theBounds = BoxCollider.bounds;
		    float cameraDistance = this.distance; // Constant factor
		    Vector3 objectSizes = theBounds.max - theBounds.min;
		    float objectSize = Mathf.Max(objectSizes.x, objectSizes.y, objectSizes.z);
		    float cameraView = 2.0f * Mathf.Tan(0.5f * Mathf.Deg2Rad * mainCamera.fieldOfView); // Visible height 1 meter in front
		    float distance = cameraDistance * objectSize / cameraView; // Combined wanted distance from the object
		    // var distance = objectSize; // Estimated offset from the center to the outside of the object
		    mainCamera.transform.position = theBounds.center - distance * mainCamera.transform.forward;


	    }


	    // var height = (int)(downSampleRatio * mainCamera.pixelHeight);
	    // var width = (int)(downSampleRatio * mainCamera.pixelWidth);
	    // if (width != _lastWidth || height != _lastHeight)
	    // {
		   //  renderTexture.Release();
		   //  renderTexture.height = height;
		   //  renderTexture.width = width;
		   //  renderTexture.Create();
		   //  _lastHeight = height;
		   //  _lastWidth = width;
		   //  
		   //
		   //  renderMaterial.SetVector("_ScreenSize", new Vector4(width, height, 0, 0));
	    // }
	    // renderMaterial.SetV

    }


    public void Toggle()
    {
	    if (isFocused)
	    {
		    Unfocus();
	    }
	    else
	    {
		    Focus();
	    }
    }

    public void Unfocus()
    {
	    isFocused = false;
    }

    public void Focus()
    {
	    isFocused = true;
    }
}
