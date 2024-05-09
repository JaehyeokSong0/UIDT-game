using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardSelection : MonoBehaviour, IListener
{
    private const int DeckSize = 11; // (1 ~ 9) Common Cards / (10 ~ 11) Unique Cards
    private const int SelectedCardSize = 3;
    private readonly Color NotReadyColor = Color.white;
    private readonly Color ReadyColor = Color.green;

    #region UI GameObjects
    [Header("Page")]
    [SerializeField]
    private GameObject _cardPage1;
    [SerializeField]
    private GameObject _cardPage2;
    private GameObject _currCardPage;

    [Header("Buttons")]
    [SerializeField]
    private Button _readyButton;
    [SerializeField]
    private Button _leftPage, _rightPage;
    
    [Header("Cards")]
    [SerializeField]
    private CardUI[] _deselectedCardUI = new CardUI[DeckSize];
    [SerializeField]
    private CardUI[] _selectedCardUI = new CardUI[SelectedCardSize];
    private Queue<Card> _selectedCardQueue = new Queue<Card>();

    [Header("Player1 UI")]
    [SerializeField]
    private TMP_Text _p1_username;
    [SerializeField]
    private TMP_Text _p1_HP_text, _p1_EN_text;
    [SerializeField]
    private Image _p1_HP_img, _p1_EN_img, _p1_ready_img;
    private GameObject _p1_icon;

    [Header("Player2 UI")]
    [SerializeField]
    private TMP_Text _p2_username;
    [SerializeField]
    private TMP_Text _p2_HP_text, _p2_EN_text;
    [SerializeField]
    private Image _p2_HP_img, _p2_EN_img, _p2_ready_img;
    private GameObject _p2_icon;

    [Header("ETC")]
    [SerializeField]
    private TMP_Text _requiredEN_text;
    private int _requiredEN;
    private bool _isReady;
    [SerializeField]
    private Transform[] _miniMap = new Transform[12];
    #endregion

    private void Awake()
    {
        _leftPage.onClick.AddListener(SwipeLeft);
        _rightPage.onClick.AddListener(SwipeRight);
        _readyButton.onClick.AddListener(ReadyForBattle);

        EventManager.Instance.AddListener(EventType.Player1CardSelected, this);
        EventManager.Instance.AddListener(EventType.Player2CardSelected, this);
    }

    private void Start()
    {
        Debug.Log("[CardSelection] Start");
        _currCardPage = _cardPage1;

        _p1_username.text = NetworkManager.Instance.P1_username;
        _p2_username.text = NetworkManager.Instance.P2_username;

        switch (GameManager.Instance.GetCharacter())
        {
            case CharacterType.Berserker:
                _deselectedCardUI[DeckSize - 2].SetCard(Resources.Load<Card>("Prefabs/Cards/BERSERKER/Explosion"));
                _deselectedCardUI[DeckSize - 1].SetCard(Resources.Load<Card>("Prefabs/Cards/BERSERKER/Restore_20"));
                break;
            case CharacterType.Mage:
                _deselectedCardUI[DeckSize - 2].SetCard(Resources.Load<Card>("Prefabs/Cards/MAGE/Scatter"));
                _deselectedCardUI[DeckSize - 1].SetCard(Resources.Load<Card>("Prefabs/Cards/MAGE/Shot"));
                break;
            case CharacterType.Rogue:
                _deselectedCardUI[DeckSize - 2].SetCard(Resources.Load<Card>("Prefabs/Cards/ROGUE/Move_Left_2"));
                _deselectedCardUI[DeckSize - 1].SetCard(Resources.Load<Card>("Prefabs/Cards/ROGUE/Move_Right_2"));
                break;
            case CharacterType.Warrior:
                _deselectedCardUI[DeckSize - 2].SetCard(Resources.Load<Card>("Prefabs/Cards/WARRIOR/Guard_30"));
                _deselectedCardUI[DeckSize - 1].SetCard(Resources.Load<Card>("Prefabs/Cards/WARRIOR/Tackle"));
                break;
            default:
                Debug.LogError("[CardSelection] Start : GetCharacter() invalid");
                break;
        }
        Init();
    }

    public void OnEvent(EventType eventType, Component sender, object param = null)
    {
        switch (eventType)
        {
            case EventType.Player1CardSelected:
                Debug.Log("[CardSelection] (E)PLAYER_1_CARD_SELECTED");
                GameManager.Instance.SetReadyStatus(1, true);
                SetReadyUI(1, true);
                break;
            case EventType.Player2CardSelected:
                Debug.Log("[CardSelection] (E)PLAYER_2_CARD_SELECTED");
                GameManager.Instance.SetReadyStatus(2, true);
                SetReadyUI(2, true);
                break;
        }
    }

    void ShowPlayerIcon(int[] p1_pos, int[] p2_pos)
    {
        GameObject p1_icon = Resources.Load("Prefabs/P1_icon") as GameObject;
        GameObject p2_icon = Resources.Load("Prefabs/P2_icon") as GameObject;
     
        int p1_idx = (p1_pos[0] - 1) + (p1_pos[1] - 1) * 4;
        int p2_idx = (p2_pos[0] - 1) + (p2_pos[1] - 1) * 4;

        _p1_icon = Instantiate(p1_icon, _miniMap[p1_idx]);
        _p2_icon = Instantiate(p2_icon, _miniMap[p2_idx]);
    }

    public void Init()
    {
        _isReady = false;
        _requiredEN = 0;
        _selectedCardQueue.Clear();
        RestoreEnergy();
        GameManager.Instance.SetReadyStatus(1, false);
        GameManager.Instance.SetReadyStatus(2, false);

        SetUserUI(GameManager.Instance.p1_HP, GameManager.Instance.p1_EN, GameManager.Instance.p2_HP, GameManager.Instance.p2_EN);
        SetReadyUI(1, false);
        SetReadyUI(2, false);
        ShowPlayerIcon(GameManager.Instance.p1_pos, GameManager.Instance.p2_pos);
    }

    void RestoreEnergy()
    {
        int restoreAmount = 20;

        GameManager.Instance.p1_EN = (GameManager.Instance.p1_EN + restoreAmount > 100) ? 100 : GameManager.Instance.p1_EN + restoreAmount;
        GameManager.Instance.p2_EN = (GameManager.Instance.p2_EN + restoreAmount > 100) ? 100 : GameManager.Instance.p2_EN + restoreAmount;
    }

    private void SwipeLeft()
    {
        if (_currCardPage != _cardPage2)
            return;

        _cardPage2.gameObject.SetActive(false);
        _cardPage1.gameObject.SetActive(true);

        _currCardPage = _cardPage1;
    }

    private void SwipeRight()
    {
        if (_currCardPage != _cardPage1)
            return;

        _cardPage1.gameObject.SetActive(false);
        _cardPage2.gameObject.SetActive(true);

        _currCardPage = _cardPage2;
    }

    public bool CheckSelectValidation(Card card)
    {
        Debug.Log("[CardSelection] / selectedCardQueue.Count" + _selectedCardQueue.Count);
        // Check queue size validation
        if ((_selectedCardQueue.Count < 0) || (_selectedCardQueue.Count > SelectedCardSize))
        {
            Debug.LogError("[CardSelection] CheckSelectValidation : Wrong value in selectedCardQueue");
            return false;
        }
        else if (_selectedCardQueue.Count == SelectedCardSize) // Full queue
            return false;

        // Check Energy Requirement
        int _currEnergy = (GameManager.Instance.currPlayer == 1) ? GameManager.Instance.p1_EN : GameManager.Instance.p2_EN;
        if (card.energy + _requiredEN > _currEnergy)
            return false;

        return true;
    }

    public void SelectCard(CardUI card)
    {
        Debug.Log("[CardSelection] SelectCard : " + card.CardName);

        if (_selectedCardQueue.Count < SelectedCardSize)
        {
            _selectedCardQueue.Enqueue(card.CardInfo);
            if (card.CardInfo.cardType == CardType.Restore)
                _requiredEN -= card.CardInfo.value;
            else
                _requiredEN += card.CardInfo.energy;
            SetRequiredEnergyUI();
        }

        SetSelectedCardUI();
    }

    public void DeselectCard(CardUI card)
    {
        Debug.Log("[CardSelection] DeselectCard : " + card.CardInfo.cardName);

        // Calculate required energy
        if (card.CardInfo.cardType == CardType.Restore)
            _requiredEN += card.CardInfo.value;
        else
            _requiredEN -= card.CardInfo.energy;
        SetRequiredEnergyUI();

        // Return card to deck
        foreach (CardUI c in _deselectedCardUI)
        {
            if (card.CardInfo == c.CardInfo)
            {
                c.gameObject.SetActive(true);
                break;
            }
        }

        // Delete card in queue
        int currQSize = _selectedCardQueue.Count;
        for(int i = 0; i < currQSize; i++)
        {
            Card _card = _selectedCardQueue.Dequeue();
            if (_card.cardName != card.CardInfo.cardName)
                _selectedCardQueue.Enqueue(_card);
        }

        SetSelectedCardUI();
    }
    public void ReadyForBattle()
    {
        if (!_isReady && (_selectedCardQueue.Count == 3))
            _isReady = true;
        else
            return;

        Debug.Log("[CardSelection] ReadyForBattle");
        GameManager.Instance.SetCard(GameManager.Instance.currPlayer, _selectedCardQueue);
    }

    private void SetSelectedCardUI()
    {
        int currQSize = _selectedCardQueue.Count;
        for (int i = 0; i < SelectedCardSize; i++)
        {
            if (i < currQSize)
            {
                Card _card = _selectedCardQueue.Dequeue();
                _selectedCardQueue.Enqueue(_card);
                Debug.Log(i + " : "+ _card.cardName);
                _selectedCardUI[i].SetCard(_card);
                _selectedCardUI[i].gameObject.SetActive(true);
            }
            else
                _selectedCardUI[i].gameObject.SetActive(false);
        }
    }

    private void SetUserUI(int p1HP, int p1EN, int p2HP, int p2EN)
    {
        _p1_HP_img.fillAmount = (float)p1HP / 100.0f;
        _p1_HP_text.text = p1HP.ToString();
        _p1_EN_img.fillAmount = (float)p1EN / 100.0f;
        _p1_EN_text.text = p1EN.ToString();

        _p2_HP_img.fillAmount = (float)p2HP / 100.0f;
        _p2_HP_text.text = p2HP.ToString();
        _p2_EN_img.fillAmount = (float)p2EN / 100.0f;
        _p2_EN_text.text = p2EN.ToString();
    }

    private void SetReadyUI(int playerIndex, bool ready)
    {
        if (playerIndex == 1)
            _p1_ready_img.color = ready ? ReadyColor : NotReadyColor;
        else if (playerIndex == 2)
            _p2_ready_img.color = ready ? ReadyColor : NotReadyColor;
        else
            Debug.LogError("[CardSelection] SetReadyUI : Wrong player index");
    }

    private void SetRequiredEnergyUI()
    {
        _requiredEN_text.text = _requiredEN.ToString();
    }
}
