using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public enum GameResult
{
    Continue,
    Player1Win,
    Player2Win,
    Draw,
}

public class BattleManager : MonoBehaviour, IListener
{
    #region Predefined Variables
    const int maxHP = 100;
    const int maxEN = 100;
    const int minWidth = 1;
    const int maxWidth = 4;
    const int minHeight = 1;
    const int maxHeight = 3;

    Vector3[,] positions = new Vector3[2, 12]
    {
        {
            new Vector3(-25, 1, 102.5f), new Vector3(-25, 1, 52.1f) ,new Vector3(-25, 1, 2.5f ) ,new Vector3(-25, 1,-47.5f ),
            new Vector3(26, 1,102.5f ),new Vector3(26, 1, 52.1f),new Vector3(26, 1, 2.5f),new Vector3(26, 1, -47.5f),
            new Vector3(75, 1, 102.5f),new Vector3(75, 1, 52.1f),new Vector3(75, 1, 2.5f),new Vector3(75, 1, -47.5f)
        } ,
        {
            new Vector3(-25, 1, 80.0f), new Vector3(-25, 1, 30.0f) ,new Vector3(-25, 1, -20.0f ) ,new Vector3(-25, 1, -70.0f ),
            new Vector3(26, 1, 80.0f ),new Vector3(26, 1, 30.0f),new Vector3(26, 1, -20.0f),new Vector3(26, 1, -70.0f),
            new Vector3(75, 1, 80.0f),new Vector3(75, 1, 30.0f),new Vector3(75, 1, -20.0f),new Vector3(75, 1, -70.0f)
        }
    };

    Vector3[] rotations = new Vector3[4] // UP, LEFT, RIGHT, DOWN
    {
        new Vector3(0, 90, 0), new Vector3(0, 0, 0), new Vector3(0, 180, 0), new Vector3(0, -90, 0)
    };
    #endregion
    public class Player
    {
    #region Variable
        CharacterType character;
        int hp;
        int en;
        int[] pos = new int[2];
        Queue<Card> cards;
        GameObject characterGO; // Character GameObject
    #endregion
    #region Property
        public CharacterType Character
        {
            get { return character; }
            set { character = value; }
        }

        public int Hp
        {
            get { return hp; }
            set { hp = value; }
        }

        public int En
        {
            get { return en; }
            set { en = value; }
        }

        public int[] Pos
        {
            get { return pos; }
            set { pos = value; }
        }

        public Queue<Card> Cards
        {
            get { return cards; }
            set { cards = value; }
        }

        public GameObject CharacterGO
        {
            get { return characterGO; }
            set { characterGO = value; }
        }
    #endregion
        public Player(CharacterType character, int hp, int en, int[] pos, Queue<Card> cards)
        {
            this.character = character;
            this.hp = hp;
            this.en = en;
            this.pos = pos;
            this.cards = cards;
        }

    }

    Player[] player = new Player[2];

    #region UI GameObjects
    [Header("Player1 UI")]
    [SerializeField]
    private CardUI _p1_cardUI;
    [SerializeField]
    private TMP_Text _p1_username, _p1_character, _p1_HP_text, _p1_EN_text;
    [SerializeField]
    private Image _p1_HP_img, _p1_EN_img;

    [Header("Player2 UI")]
    [SerializeField]
    private CardUI _p2_cardUI;
    [SerializeField]
    private TMP_Text _p2_username, _p2_character, _p2_HP_text, _p2_EN_text;
    [SerializeField]
    private Image _p2_HP_img, _p2_EN_img;

    [Header("ETC")]
    [SerializeField]
    [FormerlySerializedAs("logBox")]
    private ScrollRect _logBox;
    [SerializeField]
    private Button _enterCardSelectionPhaseButton, _exitGameButton;
    [SerializeField]
    [FormerlySerializedAs("rangePainted")]
    private GameObject[] _rangePainted = new GameObject[12];
    #endregion

    private void Awake()
    {
        _enterCardSelectionPhaseButton.onClick.AddListener(EnterCardSelectionPhase);
        _exitGameButton.onClick.AddListener(TryExitGame);

        EventManager.Instance.AddListener(EventType.ExitGame, this);
        EventManager.Instance.AddListener(EventType.LeftRoomSuccess, this);
    }

    private void Start()
    {
        Debug.Log("[BattleManager] Start");
        _enterCardSelectionPhaseButton.gameObject.SetActive(false);
        _exitGameButton.gameObject.SetActive(false);

        // Init player status
        player[0] = new Player(GameManager.Instance.p1_character, GameManager.Instance.p1_HP, GameManager.Instance.p1_EN, GameManager.Instance.p1_pos, GameManager.Instance.p1_cardQueue);
        player[1] = new Player(GameManager.Instance.p2_character, GameManager.Instance.p2_HP, GameManager.Instance.p2_EN, GameManager.Instance.p2_pos, GameManager.Instance.p2_cardQueue);
        for (int i = 0; i < 2; i++)
            player[i].CharacterGO = InstantiateCharacter(i, player[i].Character);

        _p1_username.text = NetworkManager.Instance.P1_username;
        _p1_character.text = GameManager.Instance.p1_character.ToString();
        _p2_username.text = NetworkManager.Instance.P2_username;
        _p2_character.text = GameManager.Instance.p2_character.ToString();
        
        SetUserUI();

        StartCoroutine(EnterBattlePhase());
    }

    public void OnEvent(EventType eventType, Component sender, object param = null)
    {
        Debug.LogFormat("[BattleManager] OnEvent() / EVENT : {0}, Sender : {1}, Param : {2} ", eventType, sender.gameObject.name.ToString(), param);

        switch (eventType)
        {
            case EventType.ExitGame:
                Debug.Log("[BattleManager] (E)EXIT_GAME");
                ExitGameSuccess();
                break;
            case EventType.LeftRoomSuccess:
                Debug.Log("[BattleManager] (E)LEFT_ROOM_SUCCESS");
                LeaveRoomSuccess();
                break;
        }
    }
    private void SetUserUI()
    {
        _p1_HP_img.fillAmount = (float)player[0].Hp / 100.0f;
        _p1_HP_text.text = ((float)player[0].Hp).ToString();
        _p1_EN_img.fillAmount = (float)player[0].En / 100.0f;
        _p1_EN_text.text = ((float)player[0].En).ToString();

        _p2_HP_img.fillAmount = (float)player[1].Hp / 100.0f;
        _p2_HP_text.text = ((float)player[1].Hp).ToString();
        _p2_EN_img.fillAmount = (float)player[1].En / 100.0f;
        _p2_EN_text.text = ((float)player[1].En).ToString();

        SetGameStatus();
    }

    private void SetGameStatus()
    {
        Debug.Log("[BattleManager] SetGameStatus");

        GameManager.Instance.p1_HP = player[0].Hp;
        GameManager.Instance.p1_EN = player[0].En;
        GameManager.Instance.p1_pos = player[0].Pos;

        GameManager.Instance.p2_HP = player[1].Hp;
        GameManager.Instance.p2_EN = player[1].En;
        GameManager.Instance.p2_pos = player[1].Pos;
    }

    private GameObject InstantiateCharacter(int playerIdx, CharacterType character)
    {
        Debug.Log("[BattleManager] InstantiateCharacter");
        if (character == CharacterType.Common)
        {
            Debug.LogError("[BattleManager] InstatiateCharacter : Cannot instantiate \"common\"");
            return null;
        }

        string commonPath = "Prefabs/Characters/BattleScene/";
        GameObject _characterGO = Resources.Load(commonPath + character.ToString()) as GameObject;
        
        int _startPos = (playerIdx == 0) ? (GameManager.Instance.p1_pos[0] - 1) + (GameManager.Instance.p1_pos[1] - 1) * 4 : (GameManager.Instance.p2_pos[0] - 1) + (GameManager.Instance.p2_pos[1] - 1) * 4;

        int _startRot;
        if (playerIdx == 0)
            _startRot = (int)(GetDirection(GameManager.Instance.p1_pos, GameManager.Instance.p2_pos, playerIdx)) / 2; // index : 0,1,2,3 -> UP, LEFT, RIGHT, DOWN
        else if (playerIdx == 1)
            _startRot = (int)(GetDirection(GameManager.Instance.p2_pos, GameManager.Instance.p1_pos, playerIdx)) / 2;
        else
        {
            _startRot = 0;
            Debug.LogError("[BattleManager] InstantiateCharacter : Wrong playerIdx");
        }
        
        return Instantiate(_characterGO, positions[playerIdx, _startPos], Quaternion.Euler(rotations[_startRot]));
    }

    IEnumerator EnterBattlePhase()
    {
        Debug.Log("[BattleManager] EnterBattlePhase");
        while(player[0].Cards.Count > 0)
            yield return StartCoroutine(TakeTurn(player[0].Cards.Dequeue(), player[1].Cards.Dequeue()));

        _enterCardSelectionPhaseButton.gameObject.SetActive(true);
    }

    void EnterCardSelectionPhase()
    {
        Debug.Log("[BattleManager] EnterCardSelectionPhase");
        SetGameStatus();

        if (NetworkManager.Instance.IsMasterClient())
            NetworkManager.Instance.LoadScene(3);
    }

    void TryExitGame()
    {
        NetworkManager.Instance.ExitGame();
    }

    void ExitGameSuccess()
    {
        PhotonNetwork.LeaveRoom();
    }

    void LeaveRoomSuccess()
    {
        //NetworkManager.instance.LoadScene(1);
        GameManager.Instance.LoadSceneByIndex(1);
        GameManager.Instance.InitGameStatus();
    }


    int GetPriority(Card card)
    // 우선순위가 높을수록 더 낮은 숫자 반환
    // 공격은 2순위, 그 외는 1순위
    {
        Debug.Log("[BattleManager] GetPriority");
        return (card.cardType == CardType.Attack) ? 2 : 1;
    }

    // 이긴 Player 반환
    IEnumerator TakeTurn(Card p1Card, Card p2Card)
    {
        Debug.Log("[BattleManager] TakeTurn");
        InsertLog("---- Turn " + (3 - player[0].Cards.Count) + " ----");
        int p1Priority = GetPriority(p1Card);
        int p2Priority = GetPriority(p2Card);

        if (p1Priority <= p2Priority) // Player1 first OR Both players attack
        {
            Action(0, p1Card, p2Card);
            yield return new WaitForSeconds(5.0f);
            Action(1, p1Card, p2Card);
            yield return new WaitForSeconds(5.0f);
        }
        else if (p1Priority > p2Priority) // Player2 first
        {
            Action(1, p1Card, p2Card);
            yield return new WaitForSeconds(5.0f);
            Action(0, p1Card, p2Card);
            yield return new WaitForSeconds(5.0f);
        }

        if (CheckTurnResult() != GameResult.Continue) // 게임이 끝났다면
        {
            Debug.Log("GAME END");
            InsertLog(CheckTurnResult().ToString()); // 게임결과 띄우고 나가기 버튼 활성화 -> 클릭 : 로비로 이동
            StopAllCoroutines();
            _exitGameButton.gameObject.SetActive(true);
        }
    }

    void Action(int playerIdx, Card P1Card, Card P2Card)
    {
        Debug.Log("[BattleManager] Action : " + playerIdx);

        Card playerCard = (playerIdx == 0) ? P1Card : P2Card;
        Card enemyCard = (playerIdx == 0) ? P2Card : P1Card;
        int _enemyIdx = (playerIdx == 0) ? 1 : 0; // [MEMO] myIdx/enemyIdx와 구분 : 리팩토링 필요

        if (playerIdx == 0)
        {
            _p1_cardUI.gameObject.SetActive(true);
            _p1_cardUI.SetCard(playerCard);
        }
        else if(playerIdx == 1)
        {
            _p2_cardUI.gameObject.SetActive(true);
            _p2_cardUI.SetCard(playerCard);
        }

        player[playerIdx].En -= playerCard.energy;
        string username = (playerIdx == 0) ? NetworkManager.Instance.P1_username : NetworkManager.Instance.P2_username;

        switch (playerCard.cardType)
        {
            case CardType.Move:
                InsertLog("[MOVE] " + username + " : " + playerCard.cardName);
                MovePos(playerIdx, playerCard);
                break;
            case CardType.Attack:
                InsertLog("[ATTACK] " + username + " : " + playerCard.cardName);
                Queue<int[]> attackRange = new Queue<int[]>();
                GetAttackRange(player[playerIdx].Pos, playerCard.attackAttr, ref attackRange);
                while(attackRange.Count > 0)
                {
                    var cell = attackRange.Dequeue();
                    int _attackedIdx = (cell[0] - 1) + (cell[1] - 1) * 4;

                    StartCoroutine(MarkRange(_attackedIdx)) ;
                    if ((player[_enemyIdx].Pos[0] == cell[0]) && (player[_enemyIdx].Pos[1] == cell[1])) // 피격
                        Attack(_enemyIdx, playerCard, enemyCard);
                }

                StartCoroutine(player[playerIdx].CharacterGO.GetComponent<CharacterAnimator>().Attack()); // 공격에 실패하더라도 모션은 나와야 함
                break;
            case CardType.Guard:
                InsertLog("[GUARD] " + username + " : " + playerCard.cardName);
                StartCoroutine(player[playerIdx].CharacterGO.GetComponent<CharacterAnimator>().Guard());
                break;
            case CardType.Restore:
                int _en = player[playerIdx].En;
                player[playerIdx].En = (player[playerIdx].En + playerCard.value <= maxEN) ? (player[playerIdx].En + playerCard.value) : maxEN;
                InsertLog("[RESTORE] " + username + " / (EN) " + _en.ToString() + " -> " + player[playerIdx].En.ToString());
                StartCoroutine(player[playerIdx].CharacterGO.GetComponent<CharacterAnimator>().Restore());
                break;
        }
        SetUserUI();
    }

    // 좌하단 좌표 : (1,1)
    // MAXWIDTH = 4, MAXHEIGHT = 3
    void MovePos(int playerIdx, Card card)
    {
        if (card.cardType != CardType.Move)
        {
            Debug.LogError("[BattleManager] MovePos : Wrong card type.");
            return;
        }

        int posX = player[playerIdx].Pos[0];
        int posY = player[playerIdx].Pos[1];
        int moveVal = card.value;
        string username = (playerIdx == 0) ? NetworkManager.Instance.P1_username : NetworkManager.Instance.P2_username;
        Debug.Log("[BattleManager] MovePos : " + posX + " , " + posY);

        switch (card.moveDir)
        {
            case MoveDirection.Left:
                player[playerIdx].Pos[0] = (posX - moveVal >= 1) ? posX - moveVal : minWidth;
                break;
            case MoveDirection.Right:
                player[playerIdx].Pos[0] = (posX + moveVal <= maxWidth) ? posX + moveVal : maxWidth;
                break;
            case MoveDirection.Up:
                player[playerIdx].Pos[1] = (posY + moveVal <= maxHeight) ? posY + moveVal : maxHeight;
                break;
            case MoveDirection.Down:
                player[playerIdx].Pos[1] = (posY - moveVal >= 1) ? posY - moveVal : minHeight;
                break;
        }
        InsertLog("[MOVE] " + username + " : From (" + posX.ToString() + "," + posY.ToString() + ") To (" + player[playerIdx].Pos[0].ToString() + "," + player[playerIdx].Pos[1].ToString() + ")");

        // Animation 
        var toPos = positions[playerIdx, (player[playerIdx].Pos[1] - 1) * 4 + (player[playerIdx].Pos[0] - 1)];
        StartCoroutine(C_SetCharacterDirection(playerIdx, toPos));
    }

    IEnumerator C_SetCharacterDirection(int playerIdx, Vector3 toPos)
    {
        int _enemyIdx = (playerIdx == 1) ? 0 : 1; // 상대 클라이언트의 index가 아닌 movepos를 수행하는 클라이언트의 상대 index

        yield return StartCoroutine(player[playerIdx].CharacterGO.GetComponent<CharacterAnimator>().Move(toPos));
        SetCharacterDirection(player[playerIdx].CharacterGO, GetDirection(player[playerIdx].Pos, player[_enemyIdx].Pos, playerIdx));
    }
    // cardRange의 가운데 index는 5인 플레이어 중심 좌표계
    // attackRange는 player들이 위치한 map 좌표계

    void GetAttackRange(int[] playerPos, bool[] cardRange, ref Queue<int[]> attackRange)
    { 
        int posX = playerPos[0];
        int posY = playerPos[1];
        Debug.Log("[BattleManager] GetAttackRange : " + posX + "," + posY);

        for (int i = 0; i < 9; i++)
        {
            if (cardRange[i])
            {
                int retX = posX, retY = posY;
                switch (i + 1)
                {
                    case 1: // LEFT, UP 
                        retX = (posX - 1 >= minWidth) ? (posX - 1) : -1;
                        retY = (posY + 1 <= maxHeight) ? (posY + 1) : -1;
                        break;
                    case 2: // UP
                        retY = (posY + 1 <= maxHeight) ? (posY + 1) : -1;
                        break;
                    case 3: // RIGHT, UP
                        retX = (posX + 1 <= maxWidth) ? (posX + 1) : -1;
                        retY = (posY + 1 <= maxHeight) ? (posY + 1) : -1;
                        break;
                    case 4: // LEFT
                        retX = (posX - 1 >= minWidth) ? (posX - 1) : -1;
                        break;
                    case 5: // CENTER
                        break;
                    case 6: // RIGHT
                        retX = (posX + 1 <= maxWidth) ? (posX + 1) : -1;
                        break;
                    case 7: // LEFT, DOWN
                        retX = (posX - 1 >= minWidth) ? (posX - 1) : -1;
                        retY = (posY - 1 >= minHeight) ? (posY - 1) : -1;
                        break;
                    case 8: // DOWN
                        retY = (posY - 1 >= minHeight) ? (posY - 1) : -1;
                        break;
                    case 9: // RIGHT, DOWN
                        retX = (posX + 1 <= maxWidth) ? (posX + 1) : -1;
                        retY = (posY - 1 >= minHeight) ? (posY - 1) : -1;
                        break;
                }

                if ((retX < 1) || (retY < 1)) // Invalid range
                    continue;

                int[] retPos = { retX, retY };
                Debug.Log("[BattleManager] GetAttackRange / Enqueued : " + retX + "," + retY);
                attackRange.Enqueue(retPos);
            }
        }
    }

    void Attack(int attackedPlayerIdx, Card attackerCard, Card defenderCard)
    {
        Debug.Log("[BattleManager] Attack");
        int damage;
        int _hp = player[attackedPlayerIdx].Hp;
        if (defenderCard.cardType == CardType.Guard)
            damage = (attackerCard.value - defenderCard.value >= 0) ? (attackerCard.value - defenderCard.value) : 0;
        else
            damage = attackerCard.value;

        StartCoroutine(player[attackedPlayerIdx].CharacterGO.GetComponent<CharacterAnimator>().GetHit());
        player[attackedPlayerIdx].Hp = (player[attackedPlayerIdx].Hp - damage >= 0) ? (player[attackedPlayerIdx].Hp - damage) : 0;

        string username = (attackedPlayerIdx == 0) ? NetworkManager.Instance.P1_username : NetworkManager.Instance.P2_username;
        string enemyname = (attackedPlayerIdx == 0) ? NetworkManager.Instance.P2_username : NetworkManager.Instance.P1_username;
        InsertLog("[ATTACK] " + enemyname + " attacked " + username + " / (HP) " + _hp.ToString() + " -> " + player[attackedPlayerIdx].Hp.ToString());
    }

    IEnumerator MarkRange(int idx)
    {
        if((idx < 0) || (idx > 11))
            Debug.LogError("[BattleManager] MarkRange / Wrong index " + idx);

        _rangePainted[idx].SetActive(true);
        yield return new WaitForSeconds(1.5f);
        _rangePainted[idx].SetActive(false);
    }

    GameResult CheckTurnResult()
    {
        Debug.Log("[BattleManager] CheckTurnResult");
        int p1_hp = player[0].Hp;
        int p2_hp = player[1].Hp; ;

        _p1_cardUI.gameObject.SetActive(false);
        _p2_cardUI.gameObject.SetActive(false);

        if(p1_hp <= 0)
            StartCoroutine(player[0].CharacterGO.GetComponent<CharacterAnimator>().Die());
        if (p2_hp <= 0)
            StartCoroutine(player[1].CharacterGO.GetComponent<CharacterAnimator>().Die());

        if ((p1_hp > 0) && (p2_hp > 0))
            return GameResult.Continue;
        else if ((p1_hp == 0) && (p2_hp == 0))
            return GameResult.Draw;
        else if (p1_hp == 0)
            return GameResult.Player2Win;
        else
            return GameResult.Player1Win;
    }

    // fromPos의 Character가 바라봐야할 방향 반환
    MoveDirection GetDirection(int[] fromPos, int[] toPos, int playerIdx)
    {
        Debug.Log("[BattleManager] GetDirection");
        if (fromPos[0] < toPos[0])
            return MoveDirection.Right;
        else if (fromPos[0] > toPos[0])
            return MoveDirection.Left;
        else if (fromPos[1] > toPos[1])
            return MoveDirection.Down;
        else if (fromPos[1] < toPos[1])
            return MoveDirection.Up;
        else
            return (playerIdx == 0) ? MoveDirection.Right : MoveDirection.Left;
    }

    void SetCharacterDirection(GameObject character, MoveDirection direction)
    {
        Debug.Log("[BattleManager] SetCharacterDirection");
        character.transform.rotation = Quaternion.Euler(rotations[(int)direction / 2]);
    }

    void InsertLog(string text)
    {
        GameObject log = Instantiate(Resources.Load("Prefabs/Text_Log") as GameObject);
        TMP_Text logText = log.GetComponent<TMP_Text>();
        
        logText.transform.SetParent(_logBox.content.transform);
        logText.text = text;

        const float textHeight = 15.0f;
        MoveScroll(textHeight + 1.0f);
    }

    void MoveScroll(float height)
    {
        _logBox.content.anchoredPosition += new Vector2(0, height);
    }
}