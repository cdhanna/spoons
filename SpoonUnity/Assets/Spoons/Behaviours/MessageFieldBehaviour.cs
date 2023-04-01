

using DG.Tweening;
using SpoonsCommon;
using System;
using System.Linq;
using TMPro;
using UnityEngine;

public class MessageFieldBehaviour : MonoBehaviour
{
	[Header("config data")]
	public float typeWriterSpeed = .08f;
	public bool useTypeWriter;
	
	[Header("scene references")]
	public TextMeshProUGUI messageText;

	public AudioClip[] typeWriterSounds;
	
	public IConvoMessage Message { get; private set; }

	[Header("runtime data")]
	public float nextTypeWriterTime;

	public int typeWriterSoundIndex;
	
	public bool IsFinishedDisplaying { get; private set; }
	
	public void SetText(IConvoMessage message)
	{
		Message = message;
		IsFinishedDisplaying = false;
	}

	private void Update()
	{
		HandleTypeWriter();
	}

	void HandleTypeWriter()
	{
		if (!useTypeWriter)
		{
			messageText.text = Message.Message;
			IsFinishedDisplaying = true;
			return;
		}
		var currText = messageText.text;
		var desiredText = Message.Message;

		if (Time.realtimeSinceStartup > nextTypeWriterTime)
		{
			nextTypeWriterTime = Time.realtimeSinceStartup + typeWriterSpeed;

			var index = desiredText.IndexOf(currText, StringComparison.Ordinal);
			if (index < 0)
			{
				messageText.text = "";
				return;
			}

			

			var next  = desiredText.Substring(0, Mathf.Min(currText.Length + 1, desiredText.Length) );
			// Debug.Log("SETTING TEXT (type) " + next);

			if (next != messageText.text)
			{
				// messageText.text = next;
				if (typeWriterSounds.Length > 0 && next.EndsWith(" "))
				{
					var clip = typeWriterSounds[
						(UnityEngine.Random.Range(0, 3) + typeWriterSoundIndex++) % typeWriterSounds.Length];
					// SoundManager.Instance.sfxSource.clip = clip;
					// SoundManager.Instance.sfxSource.volume = .3f;
					// SoundManager.Instance.sfxSource.Play();
					SoundManager.Instance.sfxSource.PlayOneShot(clip, .3f);
				}
				messageText.SetText(next);

				messageText.ForceMeshUpdate();
			}
			IsFinishedDisplaying = messageText.text == desiredText;

		}
	}
}

