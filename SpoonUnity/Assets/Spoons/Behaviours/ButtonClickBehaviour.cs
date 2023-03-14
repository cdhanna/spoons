using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonClickBehaviour : MonoBehaviour
{
	[Header("Scene references")]
	public Button simButton;
	public TextMeshProUGUI text;

	public AudioClip clip;
	
	[Header("Config")]
	public KeyCode key;
	
    // Start is called before the first frame update
    void Start()
    {
	    simButton.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
	    if (clip != null)
	    SoundManager.Instance.sfxSource.PlayOneShot(clip);
    }

    // Update is called once per frame
    void Update()
    {
	    if (!simButton.interactable || !simButton || !simButton.isActiveAndEnabled)
	    {
		    text.text = "";
	    }
	    else
	    {
		    text.text = $"[{GetDisplay(key)}]";
		    if (Input.GetKeyDown(key))
		    {
			    simButton.onClick.Invoke();
		    }
	    }
	    
    }

    public string GetDisplay(KeyCode code)
    {
	    switch (code)
	    {
		    case KeyCode.Alpha1: return "1";
		    case KeyCode.Alpha2: return "2";
		    default:
			    return code.ToString();
	    }
    }
}
