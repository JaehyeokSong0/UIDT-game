using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CardSelection : MonoBehaviour, IListener
{
    //public static CardSelection instance = null;
    [HideInInspector]
    public int currPlayer; // 클라이언트가 player1이라면 1, 그렇지 않다면 2

    private const int deckSize = 11; // 1 ~ 9 : Common - 정적 할당 / 10 ~ 11 : Unique - 동적 할당
    private const int selectedCardSize = 3;
    private readonly Color notReadyColor = Color.white;
    private readonly Color readyColor = Color.green;

    [Header("Page")]
    public GameObject cardPage1;
    public GameObject cardPage2;
    private GameObject currCardPage;

    [Header("Buttons")]
    public Button leftPage;
    public Button rightPage;
    public Button readyButton;

    [Header("Cards")]
    public CardUI[] cards = new CardUI[deckSize];
    public CardUI[] selectedCardUI = new CardUI[selectedCardSize];
    private Queue<Card> q_selectedCard = new Queue<Card>();

    [Header("Player1 UI")]
    public Image p1_HP_img;
    public TMP_Text p1_HP_text;
    private int p1_HP;
    public Image p1_EN_img;
    public TMP_Text p1_EN_text;
    public TMP_Text p1_username;
    public Image p1_ready_img;
    private int p1_EN;
    private GameObject p1_icon;

    [Header("Player2 UI")]
    public Image p2_HP_img;
    public TMP_Text p2_HP_text;
    private int p2_HP;
    public Image p2_EN_img;
    public TMP_Text p2_EN_text;
    public TMP_Text p2_username;
    public Image p2_ready_img;
    private int p2_EN;
    private GameObject p2_icon;

    [Header("ETC")]
    public TMP_Text requiredEN_text;
    private int requiredEN;
    private bool isReady;
    public Transform[] miniMap = new Transform[12];

    private void Awake()
    {
        /*
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            if (instance != this)
                Destroy(this.gameObject);
        }
        */
        leftPage.onClick.AddListener(SwipeLeft);
        rightPage.onClick.AddListener(SwipeRight);
        readyButton.onClick.AddListener(ReadyForBattle);

        EventManager.instance.AddListener(EVENT_TYPE.PLAYER_1_CARD_SELECTED, this);
        EventManager.instance.AddListener(EVENT_TYPE.PLAYER_2_CARD_SELECTED, this);
    }

    private void Start()
    {
        Debug.Log("[CardSelection] Start");
        currCardPage = cardPage1;

        GameManager.instance.GetPlayerStatus(1, out p1_HP, out p1_EN);
        GameManager.instance.GetPlayerStatus(2, out p2_HP, out p2_EN);
        currPlayer = GameManager.instance.currPlayer;
        p1_username.text = NetworkManager.instance.p1_username;
        p2_username.text = NetworkManager.instance.p2_username;

        switch (GameManager.instance.GetCharacter())
        {
            case CHARACTER_TYPE.BERSERKER:
                cards[deckSize - 2].SetCard(Resources.Load<Card>("Prefabs/Cards/BERSERKER/Explosion"));
                cards[deckSize - 1].SetCard(Resources.Load<Card>("Prefabs/Cards/BERSERKER/Restore_20"));
                break;
            case CHARACTER_TYPE.MAGE:
                cards[deckSize - 2].SetCard(Resources.Load<Card>("Prefabs/Cards/MAGE/Scatter"));
                cards[deckSize - 1].SetCard(Resources.Load<Card>("Prefabs/Cards/MAGE/Shot"));
                break;
            case CHARACTER_TYPE.ROGUE:
                cards[deckSize - 2].SetCard(Resources.Load<Card>("Prefabs/Cards/ROGUE/Move_Left_2"));
                cards[deckSize - 1].SetCard(Resources.Load<Card>("Prefabs/Cards/ROGUE/Move_Right_2"));
                break;
            case CHARACTER_TYPE.WARRIOR:
                cards[deckSize - 2].SetCard(Resources.Load<Card>("Prefabs/Cards/WARRIOR/Guard_30"));
                cards[deckSize - 1].SetCard(Resources.Load<Card>("Prefabs/Cards/WARRIOR/Tackle"));
                break;
            default:
                Debug.LogError("[CardSelection] Start : GetCharacter() invalid");
                break;
        }
        Init();
    }

    public void OnEvent(EVENT_TYPE event_type, Component sender, object param = null)
    {
        switch (event_type)
        {
            case EVENT_TYPE.PLAYER_1_CARD_SELECTED:
                Debug.Log("[CardSelection] (E)PLAYER_1_CARD_SELECTED");
                GameManager.instance.SetReadyStatus(1, true);
                SetReadyUI(1, true);
                break;
            case EVENT_TYPE.PLAYER_2_CARD_SELECTED:
                Debug.Log("[CardSelection] (E)PLAYER_2_CARD_SELECTED");
                GameManager.instance.SetReadyStatus(2, true);
                SetReadyUI(2, true);
                break;
        }
    }

    void ShowPlayerIcon(int[] p1_pos, int[] p2_pos)
    {
        GameObject _p1_icon = Resources.Load("Prefabs/P1_icon") as GameObject;
        GameObject _p2_icon = Resources.Load("Prefabs/P2_icon") as GameObject;
     
        int p1_idx = (p1_pos[0] - 1) + (p1_pos[1] - 1) * 4;
        int p2_idx = (p2_pos[0] - 1) + (p2_pos[1] - 1) * 4;

        p1_icon = Instantiate(_p1_icon, miniMap[p1_idx]);
        p2_icon = Instantiate(_p2_icon, miniMap[p2_idx]);
    }

    public void Init() // CardSelectionPhase 전환마다 수행
    {
        isReady = false;
        requiredEN = 0;
        q_selectedCard.Clear();
        RestoreEnergy();
        GameManager.instance.SetReadyStatus(1, false);
        GameManager.instance.SetReadyStatus(2, false);

        SetUserUI(p1_HP, p1_EN, p2_HP, p2_EN);
        SetReadyUI(1, false);
        SetReadyUI(2, false);
        ShowPlayerIcon(GameManager.instance.p1_pos, GameManager.instance.p2_pos);
    }

    void RestoreEnergy()
    {
        Debug.Log("[CardSelection] RestoreEnergy");
        int restoreAmount = 20;

        p1_EN = (p1_EN + restoreAmount > 100) ? 100 : p1_EN + restoreAmount;
        p2_EN = (p2_EN + restoreAmount > 100) ? 100 : p2_EN + restoreAmount;

        GameManager.instance.p1_EN = p1_EN;
        GameManager.instance.p2_EN = p2_EN;
    }

    private void SwipeLeft()
    {
        if (currCardPage != cardPage2)
            return;

        cardPage2.gameObject.SetActive(false);
        cardPage1.gameObject.SetActive(true);

        currCardPage = cardPage1;
    }

    private void SwipeRight()
    {
        if (currCardPage != cardPage1)
            return;

        cardPage1.gameObject.SetActive(false);
        cardPage2.gameObject.SetActive(true);

        currCardPage = cardPage2;
    }

    public bool CheckSelectValidation(Card card)
    {
        Debug.Log("[CardSelection] / q_selectedCard.Count" + q_selectedCard.Count);
        // Check queue size validation
        if ((q_selectedCard.Count < 0) || (q_selectedCard.Count > selectedCardSize))
        {
            Debug.LogError("[CardSelection] CheckSelectValidation : Wrong value in q_selectedCard");
            return false;
        }
        else if (q_selectedCard.Count == selectedCardSize) // Full queue
            return false;

        // Check Energy Requirement
        int _currEnergy = (currPlayer == 1) ? p1_EN : p2_EN;
        if (card.energy + requiredEN > _currEnergy)
            return false;

        return true;
    }

    public void SelectCard(CardUI card)
    {
        Debug.Log("[CardSelection] SelectCard : " + card.cardName);

        if (q_selectedCard.Count < selectedCardSize)
        {  
            q_selectedCard.Enqueue(card.cardInfo);
            if (card.cardInfo.cardType == CARD_TYPE.RESTORE)
                requiredEN -= card.cardInfo.value;
            else
                requiredEN += card.cardInfo.energy;
            SetRequiredEnergyUI();
        }

        SetSelectedCardUI();
    }

    public void DeselectCard(CardUI card)
    {
        Debug.Log("[CardSelection] DeselectCard : " + card.cardInfo.cardName);

        // Calculate required energy
        if (card.cardInfo.cardType == CARD_TYPE.RESTORE)
            requiredEN += card.cardInfo.value;
        else
            requiredEN -= card.cardInfo.energy;
        SetRequiredEnergyUI();

        // Return card to deck
        foreach (CardUI c in cards)
        {
            if (card.cardInfo == c.cardInfo)
            {
                c.gameObject.SetActive(true);
                break;
            }
        }

        // Delete card in queue
        int currQSize = q_selectedCard.Count;
        for(int i = 0; i < currQSize; i++)
        {
            Card _card = q_selectedCard.Dequeue();
            if (_card.cardName != card.cardInfo.cardName)
                q_selectedCard.Enqueue(_card);
        }

        SetSelectedCardUI();
    }
    public void ReadyForBattle()
    {
        if (!isReady && (q_selectedCard.Count == 3))
            isReady = true;
        else
            return;

        Debug.Log("[CardSelection] ReadyForBattle");
        GameManager.instance.SetCard(currPlayer, q_selectedCard);
    }

    private void SetSelectedCardUI()
    {
        int currQSize = q_selectedCard.Count;
        for (int i = 0; i < selectedCardSize; i++)
        {
            if (i < currQSize)
            {
                Card _card = q_selectedCard.Dequeue();
                q_selectedCard.Enqueue(_card);
                Debug.Log(i + " : "+ _card.cardName);
                selectedCardUI[i].SetCard(_card);
                selectedCardUI[i].gameObject.SetActive(true);
            }
            else
                selectedCardUI[i].gameObject.SetActive(false);
        }
    }

    private void SetUserUI(int p1HP, int p1EN, int p2HP, int p2EN)
    {
        p1_HP_img.fillAmount = (float)p1HP / 100.0f;
        p1_HP_text.text = p1HP.ToString();
        p1_EN_img.fillAmount = (float)p1EN / 100.0f;
        p1_EN_text.text = p1EN.ToString();

        p2_HP_img.fillAmount = (float)p2HP / 100.0f;
        p2_HP_text.text = p2HP.ToString();
        p2_EN_img.fillAmount = (float)p2EN / 100.0f;
        p2_EN_text.text = p2EN.ToString();
    }

    private void SetReadyUI(int playerIndex, bool ready)
    {
        if (playerIndex == 1)
            p1_ready_img.color = ready ? readyColor : notReadyColor;
        else if (playerIndex == 2)
            p2_ready_img.color = ready ? readyColor : notReadyColor;
        else
            Debug.LogError("[CardSelection] SetReadyUI : Wrong player index");
    }

    private void SetRequiredEnergyUI()
    {
        requiredEN_text.text = requiredEN.ToString();
    }
}
