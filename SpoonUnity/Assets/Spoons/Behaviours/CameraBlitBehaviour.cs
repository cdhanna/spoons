using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class CameraBlitBehaviour : MonoBehaviour
{

	public Camera selfCamera;

	public Material crtMaterial;

	public Material backgroundMaterial;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnPreRender()
    {
	    
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
	    if (backgroundMaterial != null)
	    {
		    var desc = src.descriptor;
		    desc.width /= 5;
		    desc.height /= 5;
		    
		    var temp = RenderTexture.GetTemporary(desc);
		    temp.filterMode = FilterMode.Point;
		    src.filterMode = FilterMode.Point;
		    backgroundMaterial.SetVector("_ScreenSize", new Vector4(desc.width, desc.height, 0, 0));
		    // dest.filterMode = FilterMode.Point;
		    Graphics.Blit(src, temp, backgroundMaterial);
		    Graphics.Blit(temp, dest, crtMaterial);
		    RenderTexture.ReleaseTemporary(temp);
	    }
	    else
	    {
		    Graphics.Blit(src, dest, crtMaterial);

	    }
    }
}
