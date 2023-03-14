using Beamable;
using Beamable.Common;
using DefaultNamespace.Spoons.Services;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SampleUIController : MonoBehaviour
{
	public TextMeshProUGUI outputText;
	public Button submitButton;
	public TMP_InputField inputField;

	public OptionalConvo convo;
    // Start is called before the first frame update
    async void Start()
    {
	    await BeamContext.Default.OnReady;
	    submitButton.onClick.AddListener(HandleSubmit);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    async void HandleSubmit()
    {
	    var value = inputField.text;
	    inputField.text = "";

	    if (!convo.HasValue)
	    {
		    await CreateConvo(value);
	    }
	    else
	    {
		    await ContinueConvo(value);
	    }

    }

    async Promise CreateConvo(string openingPrompt)
    {
	    var ai = BeamContext.Default.AIService();
	    // TODO: handle mock convo to speed up user speed... 
	    var newConvo = await ai.StartConvo(openingPrompt);
	    newConvo.OnUpdated += OnConvoUpdate;
	    convo.Set(newConvo);
    }

    async Promise ContinueConvo(string nextPrompt)
    {
	    var ai = BeamContext.Default.AIService();
	    await ai.ContinueConvo(convo.Value.id, nextPrompt);
    }

    void OnConvoUpdate()
    {
	    var sb = new StringBuilder();
	    foreach (var message in convo.Value.ProcessConvo())
	    {
		    if (message.IsPlayer)
		    {
			    sb.Append("[YOU] ");
			    sb.Append(message.Message);
		    }
		    else
		    {
			    sb.Append("[AI] ");
			    sb.Append(message.Message);
		    }

		    sb.Append("\n");
	    }

	    var str = sb.ToString();
	    outputText.text = str;
    }
}
