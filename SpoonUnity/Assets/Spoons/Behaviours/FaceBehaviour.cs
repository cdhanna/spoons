using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FaceBehaviour : MonoBehaviour
{
	[Header("scene references")]
	public SpriteRenderer hatImage;
	public SpriteRenderer stashImage;
	public SpriteRenderer glassesImage;

	public SpriteRenderer faceImage;
	
	[Header("runtime")]
	public FaceData _data;

	public void Set(FaceData data)
	{
		_data = data;

		faceImage.color = data.skinColor;
		
		if (data.hatSprite == null)
		{
			hatImage.gameObject.SetActive(false);
		}
		else
		{
			hatImage.gameObject.SetActive(true);
			hatImage.sprite = data.hatSprite;
			hatImage.color = data.hairColor;
		}
		if (data.stashSprite == null)
		{
			stashImage.gameObject.SetActive(false);
		}
		else
		{
			stashImage.gameObject.SetActive(true);
			stashImage.sprite = data.stashSprite;
			stashImage.color = data.hairColor;
		}
		if (data.glassesSprite == null)
		{
			glassesImage.gameObject.SetActive(false);
		}
		else
		{
			glassesImage.gameObject.SetActive(true);

			glassesImage.sprite = data.glassesSprite;
		}
	}
}
