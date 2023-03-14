
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
	public AudioSource sfxSource;
	public AudioSource musicSource;

	private static SoundManager _instance;
	public static SoundManager Instance => _instance ??= FindObjectOfType<SoundManager>();

	public Button toggleSoundButton;
	public Button toggleMusicButton;

	private void Start()
	{
		toggleMusicButton.onClick.AddListener(ToggleMusic);
		toggleSoundButton.onClick.AddListener(ToggleSound);
		UpdateTexts();
	}

	void ToggleMusic()
	{
		// musicGroup.audioMixer.
		musicSource.volume = musicSource.volume < .5f ? 1 : 0;
		UpdateTexts();
	}

	void ToggleSound()
	{
		sfxSource.volume = sfxSource.volume < .5f ? 1 : 0;
		UpdateTexts();
	}

	void UpdateTexts()
	{
		var soundText = toggleSoundButton.GetComponentInChildren<TextMeshProUGUI>();
		var musicText = toggleMusicButton.GetComponentInChildren<TextMeshProUGUI>();
		soundText.text = $"SOUND [{(sfxSource.volume > .5 ? 1 : 0)}]";
		musicText.text = $"MUSIC [{(musicSource.volume > .5 ? 1 : 0)}]";
	}
}
