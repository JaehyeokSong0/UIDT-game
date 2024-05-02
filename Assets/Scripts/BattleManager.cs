using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using Unity.VisualScripting;
//using UnityEditor.PackageManager.UI;
//using UnityEditor.Search;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
//using static UnityEditor.PlayerSettings;

public enum GAME_RESULT
{
    CONTINUE,
    PLAYER1_WIN,
    PLAYER2_WIN,
    DRAW,
}

public class BattleManager : MonoBehaviour, IListener
{
    public class Player
    {
        #region Variable
        CHARACTER_TYPE character;
        int hp;
        int en;
        int[] pos = new int[2];
        Queue<Card> cards;
        GameObject characterGO; // Character GameObject
        #endregion
        #region Property
        public CHARACTER_TYPE Character
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
        public Player(CHARACTER_TYPE character, int hp, int en, int[] pos, Queue<Card> cards)
        {
            this.character = character;
            this.hp = hp;
            this.en = en;
            this.pos = pos;
            this.cards = cards;
        }

    }
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


    const int MAX_HP = 100;
    const int MAX_EN = 100;
    const int MIN_WIDTH = 1;
    const int MAX_WIDTH = 4;
    const int MIN_HEIGHT = 1;
    const int MAX_HEIGHT = 3;

    Player[] player = new Player[2];

    #region UI
    public TMP_Text p1_username;
    public TMP_Text p1_character;
    public Image p1_HP_img;
    public TMP_Text p1_HP_text;
    public Image p1_EN_img;
    public TMP_Text p1_EN_text;
    public CardUI p1_card;
    public Button scene_trasition_button;
    public Button exit_game_button;

    public TMP_Text p2_username;
    public TMP_Text p2_character;
    public Image p2_HP_img;
    public TMP_Text p2_HP_text;
    public Image p2_EN_img;
    public TMP_Text p2_EN_text;
    public CardUI p2_card;

    public ScrollRect logBox;
    public GameObject[] rangePainted = new GameObject[12];
    #endregion

    private void Awake()
    {
        scene_trasition_button.onClick.AddListener(EnterCardSelectionPhase);
        exit_game_button.onClick.AddListener(Req_ExitGame);

        EventManager.instance.AddListener(EVENT_TYPE.EXIT_GAME, this);
        EventManager.instance.AddListener(EVENT_TYPE.LEFT_ROOM_SUCCESS, this);
    }

    private void Start()
    {
        Debug.Log("[BattleManager] Start");
        scene_trasition_button.gameObject.SetActive(false);
        exit_game_button.gameObject.SetActive(false);

        // Init player status
        player[0] = new Player(GameManager.instance.p1_character, GameManager.instance.p1_HP, GameManager.instance.p1_EN, GameManager.instance.p1_pos, GameManager.instance.q_p1_card);
        player[1] = new Player(GameManager.instance.p2_character, GameManager.instance.p2_HP, GameManager.instance.p2_EN, GameManager.instance.p2_pos, GameManager.instance.q_p2_card);
        for (int i = 0; i < 2; i++)
            player[i].CharacterGO = InstantiateCharacter(i, player[i].Character);

        p1_username.text = NetworkManager.instance.p1_username;
        p1_character.text = GameManager.instance.p1_character.ToString();
        p2_username.text = NetworkManager.instance.p2_username;
        p2_character.text = GameManager.instance.p2_character.ToString();
        
        SetUserUI();

        StartCoroutine(EnterBattlePhase());
    }

    public void OnEvent(EVENT_TYPE event_type, Component sender, object param = null)
    {
        Debug.LogFormat("[BattleManager] OnEvent() / EVENT : {0}, Sender : {1}, Param : {2} ", event_type, sender.gameObject.name.ToString(), param);

        switch (event_type)
        {
            case EVENT_TYPE.EXIT_GAME:
                Debug.Log("[BattleManager] (E)EXIT_GAME");
                ExitGame_Success();
                break;
            case EVENT_TYPE.LEFT_ROOM_SUCCESS:
                Debug.Log("[BattleManager] (E)LEFT_ROOM_SUCCESS");
                LeaveRoom_Success();
                break;
        }
    }
    private void SetUserUI()
    { 
        p1_HP_img.fillAmount = (float)player[0].Hp / 100.0f;
        p1_HP_text.text = ((float)player[0].Hp).ToString();
        p1_EN_img.fillAmount = (float)player[0].En / 100.0f;
        p1_EN_text.text = ((float)player[0].En).ToString();

        p2_HP_img.fillAmount = (float)player[1].Hp / 100.0f;
        p2_HP_text.text = ((float)player[1].Hp).ToString();
        p2_EN_img.fillAmount = (float)player[1].En / 100.0f;
        p2_EN_text.text = ((float)player[1].En).ToString();

        SetGameStatus();
    }

    private void SetGameStatus()
    {
        Debug.Log("[BattleManager] SetGameStatus");

        GameManager.instance.p1_HP = player[0].Hp;
        GameManager.instance.p1_EN = player[0].En;
        GameManager.instance.p1_pos = player[0].Pos;

        GameManager.instance.p2_HP = player[1].Hp;
        GameManager.instance.p2_EN = player[1].En;
        GameManager.instance.p2_pos = player[1].Pos;
    }

    private GameObject InstantiateCharacter(int playerIdx, CHARACTER_TYPE character)
    {
        Debug.Log("[BattleManager] InstantiateCharacter");
        if (character == CHARACTER_TYPE.COMMON)
        {
            Debug.LogError("[BattleManager] InstatiateCharacter : Cannot instantiate \"common\"");
            return null;
        }

        string commonPath = "Prefabs/Characters/BattleScene/";
        GameObject _characterGO = Resources.Load(commonPath + character.ToString()) as GameObject;
        
        int _startPos = (playerIdx == 0) ? (GameManager.instance.p1_pos[0] - 1) + (GameManager.instance.p1_pos[1] - 1) * 4 : (GameManager.instance.p2_pos[0] - 1) + (GameManager.instance.p2_pos[1] - 1) * 4;

        int _startRot;
        if (playerIdx == 0)
            _startRot = (int)(GetDirection(GameManager.instance.p1_pos, GameManager.instance.p2_pos, playerIdx)) / 2; // index : 0,1,2,3 -> UP, LEFT, RIGHT, DOWN
        else if (playerIdx == 1)
            _startRot = (int)(GetDirection(GameManager.instance.p2_pos, GameManager.instance.p1_pos, playerIdx)) / 2;
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

        scene_trasition_button.gameObject.SetActive(true);
    }

    void EnterCardSelectionPhase()
    {
        Debug.Log("[BattleManager] EnterCardSelectionPhase");
        SetGameStatus();

        if (NetworkManager.instance.IsMasterClient())
            NetworkManager.instance.LoadScene(3);
    }

    void Req_ExitGame()
    {
        NetworkManager.instance.ExitGame();
    }

    void ExitGame_Success()
    {
        PhotonNetwork.LeaveRoom();
    }

    void LeaveRoom_Success()
    {
        NetworkManager.instance.LoadScene(1);
        GameManager.instance.InitGameStatus();
    }


    int GetPriority(Card card)
    // 우선순위가 높을수록 더 낮은 숫자 반환
    // 공격은 2순위, 그 외는 1순위
    {
        Debug.Log("[BattleManager] GetPriority");
        return (card.cardType == CARD_TYPE.ATTACK) ? 2 : 1;
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

        if (CheckTurnResult() != GAME_RESULT.CONTINUE) // 게임이 끝났다면
        {
            Debug.Log("GAME END");
            InsertLog(CheckTurnResult().ToString()); // 게임결과 띄우고 나가기 버튼 활성화 -> 클릭 : 로비로 이동
            StopAllCoroutines();
            exit_game_button.gameObject.SetActive(true);
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
            p1_card.gameObject.SetActive(true);
            p1_card.SetCard(playerCard);
        }
        else if(playerIdx == 1)
        {
            p2_card.gameObject.SetActive(true);
            p2_card.SetCard(playerCard);
        }

        player[playerIdx].En -= playerCard.energy;
        string username = (playerIdx == 0) ? NetworkManager.instance.p1_username : NetworkManager.instance.p2_username;

        switch (playerCard.cardType)
        {
            case CARD_TYPE.MOVE:
                InsertLog("[MOVE] " + username + " : " + playerCard.cardName);
                MovePos(playerIdx, playerCard);
                break;
            case CARD_TYPE.ATTACK:
                InsertLog("[ATTACK] " + username + " : " + playerCard.cardName);
                Queue<int[]> attackRange = new Queue<int[]>();
                GetAttackRange(player[playerIdx].Pos, playerCard.attackAttr, ref attackRange);
                while(attackRange.Count > 0)
                {
                    var cell = attackRange.Dequeue();
                    int _attackedIdx = (cell[0] - 1) + (cell[1] - 1) * 4;
                    Debug.Log(cell[0] + "," +cell[1] + " + " +  _attackedIdx);

                    StartCoroutine(MarkRange(_attackedIdx)) ;
                    if ((player[_enemyIdx].Pos[0] == cell[0]) && (player[_enemyIdx].Pos[1] == cell[1])) // 피격
                        Attack(_enemyIdx, playerCard, enemyCard);
                }

                StartCoroutine(player[playerIdx].CharacterGO.GetComponent<CharacterAnimator>().Attack()); // 공격에 실패하더라도 모션은 나와야 함
                break;
            case CARD_TYPE.GUARD:
                InsertLog("[GUARD] " + username + " : " + playerCard.cardName);
                StartCoroutine(player[playerIdx].CharacterGO.GetComponent<CharacterAnimator>().Guard());
                break;
            case CARD_TYPE.RESTORE:
                int _en = player[playerIdx].En;
                player[playerIdx].En = (player[playerIdx].En + playerCard.value <= MAX_EN) ? (player[playerIdx].En + playerCard.value) : MAX_EN;
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
        if (card.cardType != CARD_TYPE.MOVE)
        {
            Debug.LogError("[BattleManager] MovePos : Wrong card type.");
            return;
        }

        int posX = player[playerIdx].Pos[0];
        int posY = player[playerIdx].Pos[1];
        int moveVal = card.value;
        string username = (playerIdx == 0) ? NetworkManager.instance.p1_username : NetworkManager.instance.p2_username;
        Debug.Log("[BattleManager] MovePos : " + posX + " , " + posY);

        switch (card.moveDir)
        {
            case MOVE_DIR.LEFT:
                player[playerIdx].Pos[0] = (posX - moveVal >= 1) ? posX - moveVal : MIN_WIDTH;
                break;
            case MOVE_DIR.RIGHT:
                player[playerIdx].Pos[0] = (posX + moveVal <= MAX_WIDTH) ? posX + moveVal : MAX_WIDTH;
                break;
            case MOVE_DIR.UP:
                player[playerIdx].Pos[1] = (posY + moveVal <= MAX_HEIGHT) ? posY + moveVal : MAX_HEIGHT;
                break;
            case MOVE_DIR.DOWN:
                player[playerIdx].Pos[1] = (posY - moveVal >= 1) ? posY - moveVal : MIN_HEIGHT;
                break;
        }
        InsertLog("[MOVE] Player" + username + " : From (" + posX.ToString() + "," + posY.ToString() + ") To (" + player[playerIdx].Pos[0].ToString() + "," + player[playerIdx].Pos[1].ToString() + ")");

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
                        retX = (posX - 1 >= MIN_WIDTH) ? (posX - 1) : -1;
                        retY = (posY + 1 <= MAX_HEIGHT) ? (posY + 1) : -1;
                        break;
                    case 2: // UP
                        retY = (posY + 1 <= MAX_HEIGHT) ? (posY + 1) : -1;
                        break;
                    case 3: // RIGHT, UP
                        retX = (posX + 1 <= MAX_WIDTH) ? (posX + 1) : -1;
                        retY = (posY + 1 <= MAX_HEIGHT) ? (posY + 1) : -1;
                        break;
                    case 4: // LEFT
                        retX = (posX - 1 >= MIN_WIDTH) ? (posX - 1) : -1;
                        break;
                    case 5: // CENTER
                        break;
                    case 6: // RIGHT
                        retX = (posX + 1 <= MAX_WIDTH) ? (posX + 1) : -1;
                        break;
                    case 7: // LEFT, DOWN
                        retX = (posX - 1 >= MIN_WIDTH) ? (posX - 1) : -1;
                        retY = (posY - 1 <= MIN_HEIGHT) ? (posY - 1) : -1;
                        break;
                    case 8: // DOWN
                        retY = (posY - 1 <= MIN_HEIGHT) ? (posY - 1) : -1;
                        break;
                    case 9: // RIGHT, DOWN
                        retX = (posX + 1 <= MAX_WIDTH) ? (posX + 1) : -1;
                        retY = (posY - 1 <= MIN_HEIGHT) ? (posY - 1) : -1;
                        break;
                }

                if ((retX < 0) || (retY < 0)) // Invalid range
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
        if (defenderCard.cardType == CARD_TYPE.GUARD)
            damage = (attackerCard.value - defenderCard.value >= 0) ? (attackerCard.value - defenderCard.value) : 0;
        else
            damage = attackerCard.value;

        StartCoroutine(player[attackedPlayerIdx].CharacterGO.GetComponent<CharacterAnimator>().GetHit());
        player[attackedPlayerIdx].Hp = (player[attackedPlayerIdx].Hp - damage >= 0) ? (player[attackedPlayerIdx].Hp - damage) : 0;

        string username = (attackedPlayerIdx == 0) ? NetworkManager.instance.p1_username : NetworkManager.instance.p2_username;
        string enemyname = (attackedPlayerIdx == 0) ? NetworkManager.instance.p2_username : NetworkManager.instance.p1_username;
        InsertLog("[ATTACK] " + enemyname + " attacked " + username + " / (HP) " + _hp.ToString() + " -> " + player[attackedPlayerIdx].Hp.ToString());
    }

    IEnumerator MarkRange(int idx)
    {
        if((idx < 0) || (idx > 11))
            Debug.LogError("[BattleManager] MarkRange / Wrong index " + idx);

        rangePainted[idx].SetActive(true);
        yield return new WaitForSeconds(1.5f);
        rangePainted[idx].SetActive(false);
    }

    GAME_RESULT CheckTurnResult()
    {
        Debug.Log("[BattleManager] CheckTurnResult");
        int p1_hp = player[0].Hp;
        int p2_hp = player[1].Hp; ;

        p1_card.gameObject.SetActive(false);
        p2_card.gameObject.SetActive(false);

        if(p1_hp <= 0)
            StartCoroutine(player[0].CharacterGO.GetComponent<CharacterAnimator>().Die());
        if (p2_hp <= 0)
            StartCoroutine(player[1].CharacterGO.GetComponent<CharacterAnimator>().Die());

        if ((p1_hp > 0) && (p2_hp > 0))
            return GAME_RESULT.CONTINUE;
        else if ((p1_hp == 0) && (p2_hp == 0))
            return GAME_RESULT.DRAW;
        else if (p1_hp == 0)
            return GAME_RESULT.PLAYER2_WIN;
        else
            return GAME_RESULT.PLAYER1_WIN;
    }

    // fromPos의 Character가 바라봐야할 방향 반환
    MOVE_DIR GetDirection(int[] fromPos, int[] toPos, int playerIdx)
    {
        Debug.Log("[BattleManager] GetDirection");
        if (fromPos[0] < toPos[0])
            return MOVE_DIR.RIGHT;
        else if (fromPos[0] > toPos[0])
            return MOVE_DIR.LEFT;
        else if (fromPos[1] > toPos[1])
            return MOVE_DIR.DOWN;
        else if (fromPos[1] < toPos[1])
            return MOVE_DIR.UP;
        else
            return (playerIdx == 0) ? MOVE_DIR.RIGHT : MOVE_DIR.LEFT;
    }

    void SetCharacterDirection(GameObject character, MOVE_DIR dir)
    {
        Debug.Log("[BattleManager] SetCharacterDirection");
        character.transform.rotation = Quaternion.Euler(rotations[(int)dir / 2]);
    }

    void InsertLog(string text)
    {
        GameObject log = Instantiate(Resources.Load("Prefabs/Text_Log") as GameObject);
        TMP_Text logText = log.GetComponent<TMP_Text>();
        
        logText.transform.SetParent(logBox.content.transform);
        logText.text = text;

        const float textHeight = 15.0f;
        MoveScroll(textHeight + 1.0f);
    }

    void MoveScroll(float height)
    {
        logBox.content.anchoredPosition += new Vector2(0, height);
    }
}