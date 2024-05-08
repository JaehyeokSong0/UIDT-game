using Photon.Pun;
using System.Collections;
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
    public static GameManager instance = null;
    private NetworkManager _networkManager;
    public PhotonView pView;

    #region GameStatus
    [HideInInspector]
    public CharacterType playerCharacter, enemyCharacter;
    [HideInInspector]
    public int currPlayer;

    [HideInInspector]
    public int p1_HP, p1_EN, p2_HP, p2_EN;
    [HideInInspector]
    public int[] p1_pos = new int[2];
    [HideInInspector]
    public int[] p2_pos = new int[2];
    [HideInInspector]
    public CharacterType p1_character, p2_character;
    [HideInInspector]
    public bool p1_ready, p2_ready;
    [HideInInspector]
    public Queue<Card> p1_cardQueue = new Queue<Card>();
    [HideInInspector]
    public Queue<Card> p2_cardQueue = new Queue<Card>();
    #endregion

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            if(instance != this)
                Destroy(this.gameObject);
        }

        pView = GetComponent<PhotonView>();
    }

    // Start is called before the first frame update
    private void Start()
    {
        Debug.Log("[GameManager] Start");
    }

    public void InitGameStatus()
    {
        p1_cardQueue.Clear();
        p2_cardQueue.Clear();
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
        enemyCharacter = NetworkManager.instance.GetEnemyCharacter();
        if (currPlayer == 1)
        {
            p1_character = playerCharacter;
            p2_character = enemyCharacter;
        }
        else if (currPlayer == 2)
        {
            p2_character = playerCharacter;
            p1_character = enemyCharacter;
        }
        else
            Debug.LogError("[GameManager] SetEnemyCharacter : Invalid playerNum");

        Debug.LogFormat("[GameManager] SetEnemyCharacter / MY : {0}, ENEMY : {1}", playerCharacter, enemyCharacter);
    }

    public void SetPlayerStatus(int playerNum, int hp, int en, int[] pos)
    {
        if (playerNum == 1)
        {
            p1_HP = hp;
            p1_EN = en;
            p1_pos = pos;
        }
        else if(playerNum == 2)
        {
            p2_HP = hp;
            p2_EN = en;
            p2_pos = pos;
        }
        else
            Debug.LogError("[GameManager] SetPlayerStatus : Invalid playerNum");
    }

    public void GetPlayerStatus(int playerNum, out int hp, out int en)
    {
        if (playerNum == 1)
        {
            hp = p1_HP;
            en = p1_EN;
        }
        else if (playerNum == 2)
        {
            hp = p2_HP;
            en = p2_EN;
        }
        else
        {
            Debug.LogError("[GameManager] GetPlayerStatus : Invalid playerNum");
            hp = en = -1;
        }
    }

    public void SetReadyStatus(int playerNum, bool ready)
    {
        if (playerNum == 1)
            p1_ready = ready;
        else if (playerNum == 2)
            p2_ready = ready;
        else
            Debug.LogError("[GameManager] SetReadyStatus : Invalid playerNum");
    }

    public bool GetReadyStatus(int playerNum)
    {
        if (playerNum == 1)
            return p1_ready;
        else if (playerNum == 2)
            return p2_ready;
        else
        {
            Debug.LogError("[GameManager] SetReadyStatus : Invalid playerNum");
            return false;
        }
    }

    public void SetCard(int playerNum, Queue<Card> cardQueue)
    {
        Debug.Log("[GameManager] SetCard");

        // RPC 통신이 가능한 형태로 변환
        string[] _cardNames = new string[cardQueue.Count];
        string[] _characterTypes = new string[cardQueue.Count];

        int idx = 0;
        foreach (Card card in cardQueue)
        {
            _cardNames[idx] = card.cardName;
            _characterTypes[idx] = card.characterType.ToString();
            idx++;
        }

        pView.RPC("SetCardRPC", RpcTarget.All, playerNum, _cardNames as object, _characterTypes as object);
    }

    [PunRPC]
    public void SetCardRPC(int playerNum, string[] s_cardNames, string[] s_characterTypes)
    {
        string _path = "Prefabs/Cards/";
        Debug.Log("[GameManager] SetCardRPC : " + playerNum + " / " + _path + s_characterTypes[0].ToString() + "/"  + s_cardNames[0].ToString());

        if (playerNum == 1)
        {
            for (int i = 0; i < s_cardNames.Length; i++)
                p1_cardQueue.Enqueue(Resources.Load<Card>(_path + s_characterTypes[i].ToString() + "/" + s_cardNames[i].ToString()));
            EventManager.instance.PostNotification(EventType.Player1CardSelected, this, null);
        }
        else if (playerNum == 2)
        {
            for (int i = 0; i < s_cardNames.Length; i++)
                p2_cardQueue.Enqueue(Resources.Load<Card>(_path + s_characterTypes[i].ToString() + "/" + s_cardNames[i].ToString()));
            EventManager.instance.PostNotification(EventType.Player2CardSelected, this, null);
        }
        else
            Debug.LogError("[GameManager] SetCardRPC : Invalid playerNum" + playerNum);

        Debug.Log("[GameManager] SetCardRPC / sum of q count (정상 작동 시 6): " + (p1_cardQueue.Count+ p2_cardQueue.Count));

        if ((p1_cardQueue.Count + p2_cardQueue.Count) == 6)
        {
            if (NetworkManager.instance.IsMasterClient())
                NetworkManager.instance.LoadScene(4);
        }
    }
    #endregion
}
