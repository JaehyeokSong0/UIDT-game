using UnityEngine;
using Button = UnityEngine.UI.Button;

public class CharacterSelection : MonoBehaviour, IListener
{
    private readonly Vector3 DefaultPos = new Vector3(23.8f, 13.03f, 6.94f);
    private readonly Quaternion DefaultRot = Quaternion.Euler(48.43f, -95.82f, -0.48f);
    private readonly Vector3 BerserkerPos = new Vector3(23.8f, 6.9f, 7.34f);
    private readonly Quaternion BerserkerRot = Quaternion.Euler(26.27f, -30.0f, 1.96f);
    private readonly Vector3 MagePos = new Vector3(11.9f, 6.5f, -2.24f);
    private readonly Quaternion MageRot = Quaternion.Euler(27.68f, -107.3f, -0.54f);
    private readonly Vector3 RoguePos = new Vector3(22.27f, 5.35f, 4.46f);
    private readonly Quaternion RogueRot = Quaternion.Euler(18.5f, -93.7f, 2.47f);
    private readonly Vector3 WarriorPos = new Vector3(16.0f, 6.6f, 10.0f);
    private readonly Quaternion WarriorRot = Quaternion.Euler(27.7f, -94.0f, 1.8f);

    [SerializeField]
    private Camera _mainCam;

    [SerializeField]
    private Button _deselectCharBtn, _selectCharBtn;

    [Header("Characters")]
    [SerializeField]
    private GameObject _berserker;
    [SerializeField] 
    private GameObject _mage, _rogue, _warrior;

    [Header("Character Selection UI")]
    [SerializeField]
    private GameObject _berserkerUI;
    [SerializeField]
    private GameObject _mageUI, _rogueUI, _warriorUI;
    [HideInInspector]
    public GameObject CurrUI;

    private bool _isReady;

    private void Awake()
    {
        _selectCharBtn.onClick.AddListener(SelectCharacter);
        _deselectCharBtn.onClick.AddListener(DeselectCharacter);

        EventManager.Instance.AddListener(EventType.EnterCardSelectionPhase, this);
    }

    private void Start()
    {
        _mainCam.transform.position = DefaultPos;
        _mainCam.transform.rotation = DefaultRot;

        _isReady = false;
    }

    public void OnEvent(EventType eventType, Component sender, object param = null)
    {
        Debug.LogFormat("[CharacterSelection] OnEvent() / EVENT : {0}, Sender : {1}, Param : {2} ", eventType, sender.gameObject.name.ToString(), param);

        switch (eventType)
        {
            case EventType.EnterCardSelectionPhase:
                Debug.Log("[CharacterSelection] (E)ENTER_CARD_SELECTION_PHASE");
                EnterCardSelectionPhase();
                break;
        }
    }
    public void SelectCharacter_UI(CharacterType character)
    {
        Debug.Log("[CharacterSelection] SelectCharacter_UI : " + character.ToString());

        switch (character)
        {
            case CharacterType.Berserker:
                MoveCamera(BerserkerPos, BerserkerRot);
                break;
            case CharacterType.Mage:
                MoveCamera(MagePos, MageRot);
                break;
            case CharacterType.Rogue:
                MoveCamera(RoguePos, RogueRot);
                break;
            case CharacterType.Warrior:
                MoveCamera(WarriorPos, WarriorRot);
                break;
        }

        ShowSelectUI(character);
    }

    private void SelectCharacter()
    {
        if (_isReady == false)
            _isReady = true;
        else
            return;

        if (CurrUI == null)
        {
            Debug.LogError("[CharacterSelection] SelectCharacter : Cannot find selected character");
            return;
        }

        CharacterType currCharacter;
        if (CurrUI == _berserkerUI)
            currCharacter = CharacterType.Berserker;
        else if (CurrUI == _mageUI)
            currCharacter = CharacterType.Mage;
        else if (CurrUI == _rogueUI)
            currCharacter = CharacterType.Rogue;
        else if (CurrUI = _warriorUI)
            currCharacter = CharacterType.Warrior;
        else
        {
            Debug.LogError("[CharacterSelection] SelectCharacter : Unexpected Error");
            return;
        }
        GameManager.Instance.SetCharacter(currCharacter);
        NetworkManager.Instance.CharacterSelected();
    }

    private void DeselectCharacter()
    {
        Debug.Log("[CharacterSelection] DeselectCharacter");

        MoveCamera(DefaultPos, DefaultRot);
        HideSelectUI();
    }

    private void EnterCardSelectionPhase() // Game environment configuration & Scene transition
    {
        Debug.Log("[CharacterSelection] EnterCardSelectionPhase");

        int[] p1_pos = { 1, 2 };
        int[] p2_pos = { 4, 2 };
        GameManager.Instance.SetPlayerStatus(1, 100, 100, p1_pos);
        GameManager.Instance.SetPlayerStatus(2, 100, 100, p2_pos);
        GameManager.Instance.SetReadyStatus(1, false);
        GameManager.Instance.SetReadyStatus(2, false);
        GameManager.Instance.currPlayer = (NetworkManager.Instance.IsMasterClient()) ? 1 : 2;
        GameManager.Instance.SetEnemyCharacter();

        GameManager.Instance.LoadSceneByIndex(3);
    }

    private void MoveCamera(Vector3 pos, Quaternion rot)
    {
        Debug.Log("[CharacterSelection] MoveCamera");

        StartCoroutine(_mainCam.GetComponent<CameraManager>().LerpCamera(pos, rot));
    }

    private void ShowSelectUI(CharacterType character)
    {
        _selectCharBtn.gameObject.SetActive(true);
        _deselectCharBtn.gameObject.SetActive(true);
        switch (character)
        {
            case CharacterType.Berserker:
                _berserkerUI.gameObject.SetActive(true);
                CurrUI = _berserkerUI;
                break;
            case CharacterType.Mage:
                _mageUI.gameObject.SetActive(true);
                CurrUI = _mageUI;
                break;
            case CharacterType.Rogue:
                _rogueUI.gameObject.SetActive(true);
                CurrUI = _rogueUI;
                break;
            case CharacterType.Warrior:
                _warriorUI.gameObject.SetActive(true);
                CurrUI = _warriorUI;
                break;
        }
    }

    private void HideSelectUI()
    {
        Debug.Log("[CharacterSelection] HideSelectUI");

        _selectCharBtn.gameObject.SetActive(false);
        _deselectCharBtn.gameObject.SetActive(false);
        CurrUI.gameObject.SetActive(false);

        CurrUI = null;
    }
}
