using Beamable;
using DefaultNamespace.Spoons.Services;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameWalkingUIController : MonoBehaviour
{
	[Header("scene references")]
	public GameObject walkingButtonStrip;
	public Button knockButton;
	public Button menuButton;

	public TextMeshProUGUI timeText;
	public TextMeshProUGUI scoreText;

	public AudioClip[] approachSounds;
	public int approachSoundIndex;
	
    // Start is called before the first frame update
    async void Start()
    {
	    HouseNearbyChanged(null);

	    var ctx = await BeamContext.Default.Instance;
	    var playerService = ctx.PlayerService();

	    var stateService = ctx.GameStateService();
	    stateService.OnStateChanged += (old, next) =>
	    {
		    var isWalking = next == GameState.WALKING;
		    walkingButtonStrip.SetActive(isWalking);
	    };

	    var isWalking = stateService.State == GameState.WALKING;
	    walkingButtonStrip.SetActive(isWalking);
	    
	    playerService.OnHouseNearbyChanged += HouseNearbyChanged;
	    
	    knockButton.onClick.AddListener(OnKnock);
	    menuButton.onClick.AddListener(OnMenu);
    }

    public string GenerateScoreText()
    {
	    if (!BeamContext.Default.OnReady.IsCompleted)
	    {
		    return "";
	    }

	    var data = BeamContext.Default.GameStateService().data;
	    return $"<color=green> <b>{data.sales}</b> sales</color>\n<color=red><b>{data.slams}</b> failures</color>";
    }

    void OnMenu()
    {
	    BeamContext.Default.GameStateService().GoToMenu();

    }
    void OnKnock()
    {
	    var house = BeamContext.Default.PlayerService().nearbyHouse;
	    if (house == null) return;

	    BeamContext.Default.GameStateService().TalkToHouse();
	    
    }
    
    private void HouseNearbyChanged(HouseBehaviour house)
    {
	    var hasHouse = house != null;

	    if (hasHouse)
	    {
		    knockButton.interactable = true;
		    knockButton.GetComponentInChildren<TextMeshProUGUI>().text = "Knock Knock!";

		    var clip = approachSounds[approachSoundIndex++ % approachSounds.Length];
		    SoundManager.Instance.sfxSource.PlayOneShot(clip);
	    }
	    else
	    {
		    knockButton.interactable = false;
		    knockButton.GetComponentInChildren<TextMeshProUGUI>().text = "...";
	    }
    }

    // Update is called once per frame
    void Update()
    {
	    scoreText.text = GenerateScoreText();
    }
}
