using Beamable;
using Beamable.Common;
using DefaultNamespace.Spoons.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using Spoons.Behaviours;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUIController : StandardBehaviour
{
	public Button playButton;
	public Button quitButton;
	public Button instructionOkayButton;
	public Button hatButton;
	public Button tieButton;

	public TextMeshProUGUI playText;

	public GameObject menuGob;
	public GameObject instructionsMenuGob;
	public GameObject giftMenuGob;
	// public TextMeshProUGUI playText;
	
    // Start is called before the first frame update
    

    async Promise RefreshText()
    {
	    playText.text = "CONNECTING...";

	    var ctx = await BeamContext.Default.Instance;
	    var data = ctx.GameStateService().data;
	    playText.text = data.hasGame ? "CONTINUE CAREER" : "NEW CAREER";
    }

    protected override async Promise OnStart()
    {
	    await RefreshText();
	    ctx.GameStateService().OnStateChanged += (old, next) =>
	    {
		    var _ = RefreshText();
	    };
		    
	    playButton.onClick.AddListener(HandlePlay);
	    quitButton.onClick.AddListener(HandleQuit);
	    instructionOkayButton.onClick.AddListener(HandleInstructionOkay);
	    hatButton.onClick.AddListener(HandleHat);
	    tieButton.onClick.AddListener(HandleTie);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public async void HandlePlay()
    {
	    var ctx = await BeamContext.Default.Instance;
	    var data = ctx.GameStateService().data;

	    menuGob.SetActive(false);
	    
	    if (data.hasGame)
	    {
		    ctx.GameStateService().GotoGame();
		    return;
	    }
	    
	    instructionsMenuGob.SetActive(true);
	    
	    
	    // data.hasGame
    }

    public void HandleInstructionOkay()
    {
	    instructionsMenuGob.SetActive(false);
	    giftMenuGob.SetActive(true);
    }

    public async void HandleHat()
    {
	    var ctx = await BeamContext.Default.Instance;
	    var data = ctx.GameStateService().data;

	    data.acceptedGift = true;
	    data.hasGame = true;
	    data.hasHat = true;
	    
	    giftMenuGob.SetActive(false);
	    ctx.GameStateService().GotoGame();
    }

    public async void HandleTie()
    {
	    var ctx = await BeamContext.Default.Instance;
	    var data = ctx.GameStateService().data;

	    data.acceptedGift = true;
	    data.hasGame = true;
	    data.hasTie = true;
	    
	    giftMenuGob.SetActive(false);
	    ctx.GameStateService().GotoGame();
    }

    public void HandleQuit()
    {
	    ctx.GameStateService().GoToDesktop();
    }
}
