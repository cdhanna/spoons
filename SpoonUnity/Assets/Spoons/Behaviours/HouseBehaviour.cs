using Beamable;
using Beamable.Api;
using Beamable.Common;
using Beamable.Server.Clients;
using DefaultNamespace.Spoons.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class HouseBehaviour : MonoBehaviour
{
	[Header("Scene references")]
	public SpriteRenderer spriteRenderer;
	
	[Header("runtime state")]
	public bool visited;

	async void Start()
	{
		var ctx = await BeamContext.Default.Instance;
		ctx.HouseService().RegisterHouse(this);

		await LoadImage();
	}

	private async void OnDestroy()
    {
	    var ctx = await BeamContext.Default.Instance;
	    ctx.HouseService().RemoveHouse(this);

	    
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public async Promise LoadImage()
    {

	    var imageRef = await BeamContext.Default.Microservices().Spoons().GetHouse();
	    var sprite  = await LoadImageFromURL(imageRef.url);

	    // StartCoroutine(routine);
	    spriteRenderer.sprite = sprite;
    }
    
    
    public Promise<Sprite> LoadImageFromURL(string url)
    {
	    UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
	    var op = www.SendWebRequest();
	    var p = new Promise<Sprite>();
	    op.completed += (res) =>
	    {
		    if (www.IsNetworkError() || www.IsHttpError())
		    {
			    Debug.LogError(www.error);
		    }
		    else
		    {
			    Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
			    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f));
			    p.CompleteSuccess(sprite);
		    }
	    };
		return p;

    }
}
