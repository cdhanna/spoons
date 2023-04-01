using Beamable;
using Beamable.Common;
using DefaultNamespace.Spoons.Services;
using DG.Tweening;
using SpoonsCommon;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameConvoUIController : MonoBehaviour
{
	[Header("scene references")]
	public TMP_InputField userInputField;
	public RectTransform scrollArea;
	public Button giveUpButton;
	public Button submitButton;
	public GameObject dialogPanel;
	public GameObject dialogButtonStrip;
	public Image backdropImage;
	public GameObject faceRoot;
	public GameObject winIcon;
	public GameObject lossIcon;
	public TextMeshProUGUI winText;
	public TextMeshProUGUI lossText;
	
	[Header("prefabs references")]
	public MessageFieldBehaviour messagePrefab;
	public MessageFieldBehaviour aiMessagePrefab;
	public FaceObject faceObject;

	public AudioClip[] typingSounds;
	public AudioClip submitSound;

	public AudioClip[] winSounds;
	public AudioClip[] lossSounds;

	public AudioClip[] suddenSounds;
	
	[Header("Config")]
	public Color backgroundImageColor;

	public string[] winTextOptions;
	public string[] lossTextOptions;

	[Header("runtime data")]
	public int typingSoundIndex;

	public int suddenSoundIndex;
	public Convo convo;
	public List<MessageFieldBehaviour> messageInstances = new List<MessageFieldBehaviour>();
	public bool isPlayerTurn;
	public bool isShowingUserTurn;
	public bool isGameOver;
	public FaceConfig faceConfig;
	private Dictionary<int, FaceBehaviour> _ratingToFace = new Dictionary<int, FaceBehaviour>();
	public List<FaceBehaviour> _allFaces = new List<FaceBehaviour>(); 

    // Start is called before the first frame update
    async void Start()
    {

	    _allFaces = faceRoot.GetComponentsInChildren<FaceBehaviour>().ToList();
	    foreach (var face in _allFaces)
	    {
		    var rating = int.Parse(face.gameObject.name.Replace("Face_", ""));
		    face.gameObject.SetActive(false);
		    _ratingToFace[rating] = face;
	    }
	    
	    userInputField.onSubmit.AddListener(HandleUserSubmission);
	    userInputField.onValueChanged.AddListener((x) =>
	    {
		    var clip = typingSounds[(Random.Range(0, 3) + typingSoundIndex++) % typingSounds.Length];
		    SoundManager.Instance.sfxSource.PlayOneShot(clip, .3f);
	    });
	    submitButton.onClick.AddListener(HandleUserSubmitClick);
	    giveUpButton.onClick.AddListener(HandleGiveUpClick);
	    Clear();

	    var ctx = await BeamContext.Default.Instance;
	    var stateService = ctx.GameStateService();
	    stateService.OnStateChanged += (old, next) =>
	    {
		    var wasTalking = old == GameState.TALKING;
		    var isTalking = next == GameState.TALKING;
		    dialogButtonStrip.SetActive(isTalking);
		    dialogPanel.SetActive(isTalking);

		    if (!wasTalking && isTalking)
		    {
			    Clear();
			    var _ = StartConvo();
		    }
	    };
    }

    void Clear()
    {
	    var submitText = submitButton.GetComponentInChildren<TextMeshProUGUI>();
	    submitText.DOFade(1, .3f);
	    submitText.text = "SUBMIT";
	    oldRating = 0;
	    
	    winText.gameObject.SetActive(false);
	    lossIcon.gameObject.SetActive(false);
	    lossText.gameObject.SetActive(false);
	    winIcon.gameObject.SetActive(false);
	    
	    faceConfig = faceObject.RandomConfig();

	    isPlayerTurn = false;
	    isGameOver = false;
	    isShowingUserTurn = false;
	    for (var i = 0; i < scrollArea.childCount; i++)
	    {
		    Destroy(scrollArea.GetChild(i).gameObject);
	    }

	    convo = null;
	    messageInstances.Clear();
	    SetForAiTurn();
    }
    public async Promise StartConvo()
    {
	    SetAiFace(4);
	    
	    backdropImage.color = Color.clear;
	    backdropImage.DOColor(backgroundImageColor, .4f);
	    var openingText = "Hello, would you like to buy some spoons?";
	    var fakeMessage = new ConvoUserMessage {order = 0, message = openingText};
	    CreateMessage(fakeMessage);
	    SetForAiTurn();

	    var ctx = await BeamContext.Default.Instance;
	    var aiService = ctx.AIService();
	    convo = await aiService.StartConvo(openingText);
	    convo.OnUpdated += OnConvoUpdated;
	    convo.OnSlam += OnSlam;
	    convo.OnSale += OnSale;
    }

    private void OnSale(ConvoAIMessageOutcome obj)
    {
	    SoundManager.Instance.sfxSource.PlayOneShot(winSounds[Random.Range(0,winSounds.Length)]);
	    isGameOver = true;

	    var submitText = submitButton.GetComponentInChildren<TextMeshProUGUI>();
	    submitText.DOFade(.3f, .3f);
	    submitText.text = "...";
	    
	    winText.gameObject.SetActive(true);
	    lossIcon.gameObject.SetActive(false);
	    lossText.gameObject.SetActive(false);
	    winIcon.gameObject.SetActive(true);
	    winText.text = winTextOptions[Random.Range(0, winTextOptions.Length)];
	    winIcon.transform.DOPunchScale(Vector3.one * .1f, .3f);
	    winText.transform.DOPunchScale(Vector3.one * .1f, .2f);
	    giveUpButton.GetComponentInChildren<TextMeshProUGUI>().text = "Back";
	    BeamContext.Default.GameStateService().data.sales++;

	    SetForAiTurn();
    }

    private void OnSlam()
    {
	    SoundManager.Instance.sfxSource.PlayOneShot(lossSounds[Random.Range(0,lossSounds.Length)]);

	    var submitText = submitButton.GetComponentInChildren<TextMeshProUGUI>();
	    submitText.DOFade(.3f, .3f);
	    submitText.text = "...";

	    winText.gameObject.SetActive(false);
	    lossIcon.gameObject.SetActive(true);
	    lossText.gameObject.SetActive(true);
	    lossIcon.transform.DOPunchScale(Vector3.one * .1f, .3f);
	    lossText.transform.DOPunchScale(Vector3.one * .1f, .2f);

	    winIcon.gameObject.SetActive(false);
	    lossText.text = lossTextOptions[Random.Range(0, winTextOptions.Length)];

	    
	    isGameOver = true;
	    if (messageInstances[messageInstances.Count - 1].Message is ConvoAIMessage aiMessage)
	    {
		    messageInstances[messageInstances.Count - 1].useTypeWriter = false;
			aiMessage.message = aiMessage.message.Replace("slam", "<b>**Slams the Door**</b>");
	    }
	    giveUpButton.GetComponentInChildren<TextMeshProUGUI>().text = "Leave";

	    BeamContext.Default.GameStateService().data.slams++;
	    SetForAiTurn();
    }
    

    private void OnConvoUpdated()
    {
	    var messages = convo.ProcessConvo();
	    var idToExisting = messageInstances.ToDictionary(instance => instance.Message.Id());
	    
	    for (var i = 0; i < messages.Count; i++)
	    {
		    var message = messages[i];
		    var messageId = message.Id();

		    if (!idToExisting.TryGetValue(messageId, out var existing))
		    {
			    existing = CreateMessage(message);
		    }
		    
		    existing.SetText(message);
	    }

	    if (messages[messages.Count - 1] is ConvoAIMessage ai && ai.parts.Count > 0)
	    {
		    var rating = ai.parts[ai.parts.Count - 1].annoyedRating;
		    SetAiFace(rating);
	    }

	    isPlayerTurn = messages[messages.Count - 1] is ConvoAIMessage aiMessage && aiMessage.isComplete;
	    if (!isPlayerTurn)
	    {
		    SetForAiTurn();
	    }
    }

    // Update is called once per frame
    void Update()
    {
	    if (!isShowingUserTurn && isPlayerTurn && !isGameOver)
	    {
		    var previousAiInstance = messageInstances[messageInstances.Count - 1];
		    if (previousAiInstance.Message is ConvoAIMessage && previousAiInstance.IsFinishedDisplaying)
		    {
			   SetForUserTurn(); 
		    }
	    }
    }

    public float punch = 1;
    public float duration = 1;

    private int oldRating = 2;
    public void SetAiFace(int rating)
    {
	    foreach (var old in _allFaces)
	    {
		    old.gameObject.SetActive(false);
	    }
	    
	    if (!_ratingToFace.TryGetValue(rating, out var face))
	    {
		    rating = 3;
		    face = _ratingToFace[3];
	    }

	    if (rating > oldRating + 3)
	    {
		    var clip = suddenSounds[(Random.Range(0, 4) + suddenSoundIndex++) % suddenSounds.Length];
		    SoundManager.Instance.sfxSource.PlayOneShot(clip);
	    }
	    
	    face.Set(new FaceData
	    {
		    hairColor = faceConfig.hairColor,
		    skinColor = faceConfig.skinColor,
		    glassesSprite = faceConfig.glassesSprite,
		    hatSprite = faceConfig.hatSprite,
		    stashSprite = faceConfig.stashSprite
	    });
	    face.gameObject.SetActive(true);

	    oldRating = rating;

    }
    
    public void SetForAiTurn()
    {
	    userInputField.readOnly = true;
	    submitButton.interactable = false;
	    userInputField.interactable = false;
	    isShowingUserTurn = false;
    }
    
    
    public void SetForUserTurn()
    {
	    isShowingUserTurn = true;
	    userInputField.readOnly = false;
	    userInputField.interactable = true;
	    submitButton.interactable = true;

	    userInputField.transform.DOPunchPosition(new Vector3(0, punch, 0), duration);
	    userInputField.onFocusSelectAll = true;
	    userInputField.Select();
	    userInputField.ActivateInputField();
	    userInputField.text = "";

    }

    public MessageFieldBehaviour CreateMessage(IConvoMessage message)
    {
	    var prefab = message.IsPlayer ? messagePrefab : aiMessagePrefab;
	    var box = Instantiate(prefab, scrollArea);
	    box.SetText(message);
	    box.transform.DOPunchScale(new Vector3(0, .2f, 0), .5f);

	    messageInstances.Add(box);
	    return box;
    }

    public void HandleGiveUpClick()
    {
	    BeamContext.Default.GameStateService().LeaveHouse();
    }
    public void HandleUserSubmitClick()
    {
	    HandleUserSubmission(userInputField.text);
    }
    public async void HandleUserSubmission(string message)
    {
	    if (string.IsNullOrEmpty(message) || string.IsNullOrWhiteSpace(message)) return;
	    
	    userInputField.text = "";

	    SoundManager.Instance.sfxSource.PlayOneShot(submitSound, .5f);
	    var service = BeamContext.Default.AIService();
	    await service.ContinueConvo(convo.id, message);
	   
    }
}
