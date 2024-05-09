using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum CharacterType
{
    Common,
    Berserker,
    Mage,
    Rogue,
    Warrior,
}

public class GameManager : MonoBehaviour, IPunObservable
{
    public static GameManager Instance = null;
    [SerializeField]
    private PhotonView _photonView;

    public class Player
    {
    #region Variable
        CharacterType character;
        int hp;
        int en;
        int[] pos = new int[2];
        Queue<Card> cards;
        bool isReady;
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

        public bool IsReady
        {
            get { return isReady; }
            set { isReady = value; }
        }
     #endregion
        public Player()
        {
            this.isReady = false;
            this.cards = new Queue<Card>();
        }
        public Player(CharacterType character, int hp, int en, int[] pos, Queue<Card> cards)
        {
            this.character = character;
            this.hp = hp;
            this.en = en;
            this.pos = pos;
            this.cards = cards;
            this.isReady = false;
        }

        public void PushCard(Card card)
        {
            this.cards.Enqueue(card);
        }
        public void ClearCardQueue()
        {
            this.cards.Clear();
        }
    }

    public Player[] player = new Player[2];
    #region GameStatus
    [HideInInspector]
    public CharacterType playerCharacter, enemyCharacter;
    [HideInInspector]
    public int currPlayer;

    #endregion

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            if(Instance != this)
                Destroy(this.gameObject);
        }
        _photonView = GetComponent<PhotonView>();

        for (int i = 0; i <= 1; i++)
            player[i] = new Player();
    }

    public void InitGameStatus()
    {
        player[0].ClearCardQueue();
        player[1].ClearCardQueue();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {

        }
        else if(stream.IsReading)
        {

        }
    }

    public void ExitGame()
    {
        Debug.Log("[GameManager] ExitGame");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void LoadSceneByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    #region GameStatus Function
    public void SetCharacter(CharacterType character)
    {
        playerCharacter = character;
    }

    public CharacterType GetCharacter()
    {
        return playerCharacter;
    }

    public void SetEnemyCharacter()
    {
        enemyCharacter = NetworkManager.Instance.GetEnemyCharacter();
        if (currPlayer == 0)
        {
            player[0].Character = playerCharacter;
            player[1].Character = enemyCharacter;
        }
        else if (currPlayer == 1)
        {
            player[1].Character = playerCharacter;
            player[0].Character = enemyCharacter;
        }
        else
            Debug.LogError("[GameManager] SetEnemyCharacter : Invalid playerIdx");

        Debug.LogFormat("[GameManager] SetEnemyCharacter / MY : {0}, ENEMY : {1}", playerCharacter, enemyCharacter);
    }

    public void SetPlayerStatus(int playerIdx, int hp, int en, int[] pos)
    {
        if ((playerIdx == 0) || (playerIdx == 1))
        {
            player[playerIdx].Hp = hp;
            player[playerIdx].En = en;
            player[playerIdx].Pos = pos;
        }
        else
            Debug.LogError("[GameManager] SetPlayerStatus : Invalid playerIdx " + playerIdx);
    }

    public void SetReadyStatus(int playerIdx, bool readyStatus)
    {
        if ((playerIdx == 0) || (playerIdx == 1))
            player[playerIdx].IsReady = readyStatus;
        else
            Debug.LogError("[GameManager] SetReadyStatus : Invalid playerIdx");
    }

    public void SetCard(int playerIdx, Queue<Card> cardQueue)
    {
        if (!((playerIdx == 0) || (playerIdx == 1)))
            Debug.LogError("[GameManager] SetCard : Invalid playerIdx");

        // Convert object type for RPC
        string[] cardNames = new string[cardQueue.Count];
        string[] characterTypes = new string[cardQueue.Count];

        int idx = 0;
        foreach (Card card in cardQueue)
        {
            cardNames[idx] = card.cardName;
            characterTypes[idx] = card.characterType.ToString();
            idx++;
        }

        _photonView.RPC("SetCardRPC", RpcTarget.All, playerIdx, cardNames as object, characterTypes as object);
    }

    [PunRPC]
    public void SetCardRPC(int playerIdx, string[] s_cardNames, string[] s_characterTypes)
    {
        string _path = "Prefabs/Cards/";
        Debug.Log("[GameManager] SetCardRPC : " + playerIdx + " / " + _path + s_characterTypes[0].ToString() + "/"  + s_cardNames[0].ToString());

        if ((playerIdx == 0) || (playerIdx == 1))
        {
            for (int i = 0; i < s_cardNames.Length; i++)
                player[playerIdx].PushCard(Resources.Load<Card>(_path + s_characterTypes[i].ToString() + "/" + s_cardNames[i].ToString()));
            
            if(playerIdx == 0)
                EventManager.Instance.PostNotification(EventType.Player1CardSelected, this, null);
            else if (playerIdx == 1)
                EventManager.Instance.PostNotification(EventType.Player2CardSelected, this, null);
        }
        else
            Debug.LogError("[GameManager] SetCardRPC : Invalid playerIdx" + playerIdx);

        if ((player[0].Cards.Count + player[1].Cards.Count) == 6)
        {
            if (NetworkManager.Instance.IsMasterClient())
                NetworkManager.Instance.LoadScene(4);
        }
    }
    #endregion
}
