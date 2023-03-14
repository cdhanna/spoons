using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu]
public class FaceObject : ScriptableObject
{
	public List<FaceObjectSpriteEntry> dataEntries;
	public List<WeightedFacePart> hairs;
	public List<WeightedFacePart> glasses;
	public List<WeightedFacePart> stashes;

	public Color[] skinTones;
	public Color[] hairTones;
	
	public FaceConfig RandomConfig()
	{
		return new FaceConfig
		{
			skinColor = skinTones[Random.Range(0, skinTones.Length)],
			hairColor = hairTones[Random.Range(0, hairTones.Length)],
			glassesSprite = GetWeightedRandom(glasses).sprite,
			hatSprite = GetWeightedRandom(hairs).sprite,
			stashSprite = GetWeightedRandom(stashes).sprite,
		};
	}
	
	public WeightedFacePart GetWeightedRandom(List<WeightedFacePart> values)
	{
		var total = 0f;
		foreach (var v in values)
		{
			total += v.weight;
		}

		var n = Random.Range(0, total);
		total = 0;
		foreach (var v in values)
		{
			if (n > total && n < total + v.weight)
			{
				return v;
			}

			total += v.weight;
		}

		return null;
	}
}

[Serializable]
public class FaceObjectSpriteEntry
{
	public FaceBehaviour sprite;
	public int annoyedRating;
}

[Serializable]
public class WeightedFacePart
{
	public Sprite sprite;
	public float weight = 1;
}

[Serializable]
public class FaceConfig
{
	public Sprite hatSprite;
	public Sprite glassesSprite;
	public Sprite stashSprite;
	public Color skinColor;
	public Color hairColor;
}

[Serializable]
public class FaceData
{
	public Sprite hatSprite;
	public Sprite glassesSprite;
	public Sprite stashSprite;
	public Color skinColor;
	public Color hairColor;
}
