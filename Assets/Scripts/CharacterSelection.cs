using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using UnityEngine.UIElements;

// For ambiguous reference
using Button = UnityEngine.UI.Button;

public class CharacterSelection : MonoBehaviour, IListener
{
    public Camera mainCam;
    // public Camera uiCam;
    public Button deselectCharBtn;
    public Button selectCharBtn;

    [Header("Characters")]
    private readonly Vector3 defaultPos = new Vector3(23.8f, 13.03f, 6.94f);
    private readonly Quaternion defaultRot = Quaternion.Euler(48.43f, -95.82f, -0.48f);
    private readonly Vector3 berserkerPos = new Vector3(23.8f, 6.9f, 7.34f);
    private readonly Quaternion berserkerRot = Quaternion.Euler(26.27f, -30.0f, 1.96f);
    private readonly Vector3 magePos = new Vector3(11.9f, 6.5f, -2.24f);
    private readonly Quaternion mageRot = Quaternion.Euler(27.68f, -107.3f, -0.54f);
    private readonly Vector3 roguePos = new Vector3(22.27f, 5.35f, 4.46f);
    private readonly Quaternion rogueRot = Quaternion.Euler(18.5f, -93.7f, 2.47f);
    private readonly Vector3 warriorPos = new Vector3(16.0f, 6.6f, 10.0f);
    private readonly Quaternion warriorRot = Quaternion.Euler(27.7f, -94.0f, 1.8f);

    [Header("Characters")]
    public GameObject berserker;
    public GameObject mage;
    public GameObject rogue;
    public GameObject warrior;

    [Header("Character Selection UI")]
    public GameObject berserkerUI;
    public GameObject mageUI;
    public GameObject rogueUI;
    public GameObject warriorUI;

    [HideInInspector]
    public GameObject currUI;
    private bool isReady;

    private void Awake()
    {
        selectCharBtn.onClick.AddListener(SelectCharacter);
        deselectCharBtn.onClick.AddListener(DeselectCharacter);

        EventManager.instance.AddListener(EVENT_TYPE.ENTER_CARD_SELECTION_PHASE, this);
    }

    private void Start()
    {
        mainCam.transform.position = defaultPos;
        mainCam.transform.rotation = defaultRot;

        isReady = false;
    }

    public void OnEvent(EVENT_TYPE event_type, Component sender, object param = null)
    {
        Debug.LogFormat("[CharacterSelection] OnEvent() / EVENT : {0}, Sender : {1}, Param : {2} ", event_type, sender.gameObject.name.ToString(), param);

        switch (event_type)
        {
            case EVENT_TYPE.ENTER_CARD_SELECTION_PHASE:
                Debug.Log("[CharacterSelection] (E)ENTER_CARD_SELECTION_PHASE");
                EnterCardSelectionPhase();
                break;
        }
    }
    public void SelectCharacter_UI(CHARACTER_TYPE character)
    {
        Debug.Log("[CharacterSelection] SelectCharacter_UI : " + character.ToString());

        switch (character)
        {
            case CHARACTER_TYPE.BERSERKER:
                MoveCamera(berserkerPos, berserkerRot);
                break;
            case CHARACTER_TYPE.MAGE:
                MoveCamera(magePos, mageRot);
                break;
            case CHARACTER_TYPE.ROGUE:
                MoveCamera(roguePos, rogueRot);
                break;
            case CHARACTER_TYPE.WARRIOR:
                MoveCamera(warriorPos, warriorRot);
                break;
        }

        ShowSelectUI(character);
    }

    void SelectCharacter() // GameScene 전환
    {
        if (isReady == false)
            isReady = true;
        else
            return;

        if (currUI == null)
        {
            Debug.LogError("[CharacterSelection] SelectCharacter : Cannot find selected character");
            return;
        }

        CHARACTER_TYPE currCharacter;
        if (currUI == berserkerUI)
            currCharacter = CHARACTER_TYPE.BERSERKER;
        else if (currUI == mageUI)
            currCharacter = CHARACTER_TYPE.MAGE;
        else if (currUI == rogueUI)
            currCharacter = CHARACTER_TYPE.ROGUE;
        else if (currUI = warriorUI)
            currCharacter = CHARACTER_TYPE.WARRIOR;
        else
        {
            Debug.LogError("[CharacterSelection] SelectCharacter : Unexpected Error");
            return;
        }
        GameManager.instance.SetCharacter(currCharacter);
        NetworkManager.instance.CharacterSelected();
    }

    void DeselectCharacter()
    {
        Debug.Log("[CharacterSelection] DeselectCharacter");

        MoveCamera(defaultPos, defaultRot);
        HideSelectUI();
    }

    void EnterCardSelectionPhase() // 게임 환경설정 및 씬 전환
    {
        Debug.Log("[CharacterSelection] EnterCardSelectionPhase");

        int[] p1_pos = { 1, 2 };
        int[] p2_pos = { 4, 2 };
        GameManager.instance.SetPlayerStatus(1, 100, 100, p1_pos);
        GameManager.instance.SetPlayerStatus(2, 100, 100, p2_pos);
        GameManager.instance.SetReadyStatus(1, false);
        GameManager.instance.SetReadyStatus(2, false);
        GameManager.instance.currPlayer = (NetworkManager.instance.IsMasterClient()) ? 1 : 2;
        GameManager.instance.SetEnemyCharacter();

        GameManager.instance.LoadSceneByIndex(3);
    }

    void MoveCamera(Vector3 pos, Quaternion rot)
    {
        Debug.Log("[CharacterSelection] MoveCamera");

        StartCoroutine(mainCam.GetComponent<CameraManager>().LerpCamera(pos, rot));
    }

    void ShowSelectUI(CHARACTER_TYPE character)
    {
        selectCharBtn.gameObject.SetActive(true);
        deselectCharBtn.gameObject.SetActive(true);
        switch (character)
        {
            case CHARACTER_TYPE.BERSERKER:
                berserkerUI.gameObject.SetActive(true);
                currUI = berserkerUI;
                break;
            case CHARACTER_TYPE.MAGE:
                mageUI.gameObject.SetActive(true);
                currUI = mageUI;
                break;
            case CHARACTER_TYPE.ROGUE:
                rogueUI.gameObject.SetActive(true);
                currUI = rogueUI;
                break;
            case CHARACTER_TYPE.WARRIOR:
                warriorUI.gameObject.SetActive(true);
                currUI = warriorUI;
                break;
        }
    }

    void HideSelectUI()
    {
        Debug.Log("[CharacterSelection] HideSelectUI");

        selectCharBtn.gameObject.SetActive(false);
        deselectCharBtn.gameObject.SetActive(false);
        currUI.gameObject.SetActive(false);

        currUI = null;
    }
}
