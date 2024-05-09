using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InRoom : MonoBehaviour, IListener
{
    private readonly Color ReadyTextColor = new Color32(100, 100, 0, 255);
    private readonly Color NotReadyTextColor = new Color32(200, 200, 200, 255);

    [SerializeField]
    private LobbyManager _lobbyManager;
    [SerializeField]
    private TMP_Text _player1Name, _player2Name, _player2Ready;
    [SerializeField]
    private Button _readyButton, _readyButtonPressed, _startButton, _exitButton;

    private void Awake()
    {
        EventManager.Instance.AddListener(EventType.InitInRoom, this);
        EventManager.Instance.AddListener(EventType.HostLeftRoom, this);
        EventManager.Instance.AddListener(EventType.LeftRoomSuccess, this);
        EventManager.Instance.AddListener(EventType.ClientJoinRoom, this);
        EventManager.Instance.AddListener(EventType.ClientLeftRoom, this);
        EventManager.Instance.AddListener(EventType.ClientReady, this);
        EventManager.Instance.AddListener(EventType.ClientNotReady, this);
        EventManager.Instance.AddListener(EventType.EnterCharacterSelectionPhase, this);
    }

    private void Start()
    {
        _readyButton.onClick.AddListener(ReadyInRoom);
        _readyButtonPressed.onClick.AddListener(NotReadyInRoom);
        _startButton.onClick.AddListener(TryStartGameInRoom);
        _exitButton.onClick.AddListener(TryExitInRoom);
    }

    public void OnEvent(EventType eventType, Component sender, object param = null)
    {
        Debug.LogFormat("[InRoom] OnEvent() / EVENT : {0}, Sender : {1}, Param : {2} ", eventType, sender.gameObject.name.ToString(), param);

        switch (eventType)
        {
            case EventType.InitInRoom:
                Debug.Log("[InRoom] (E)INIT_INROOM");
                SetInRoom();
                break;

            case EventType.HostLeftRoom:
                Debug.Log("[InRoom] (E)HOST_LEFT_ROOM");
                MigrateHost();
                break;

            case EventType.LeftRoomSuccess:
                Debug.Log("[InRoom] (E)LEFT_ROOM_SUCCESS");
                ExitInRoomSuccess();
                break;

            case EventType.ClientJoinRoom:
                Debug.Log("[InRoom] (E)CLIENT_JOIN_ROOM");
                if (param is Player)
                    ClientJoinedRoom(param as Player);
                else
                    Debug.LogError("[InRoom] ((E)CLIENT_JOIN_ROOM) Casting invalid");
                break;

            case EventType.ClientLeftRoom:
                Debug.Log("[InRoom] (E)CLIENT_LEFT_ROOM");
                ClientLeftRoom();
                break;

            case EventType.ClientReady:
                Debug.Log("[InRoom] (E)CLIENT_READY");
                SetClientReadyText(ReadyTextColor);
                break;

            case EventType.ClientNotReady:
                Debug.Log("[InRoom] (E)CLIENT_NOT_READY");
                SetClientReadyText(NotReadyTextColor);
                break;

            case EventType.EnterCharacterSelectionPhase:
                Debug.Log("[InRoom] (E)EnterCharacterSelectionPhase");
                StartGameInRoom();
                break;
        }
    }

    public void SetInRoom()
    {
        Player player1 = PhotonNetwork.MasterClient;
        Player player2 = null;

        if (PhotonNetwork.CurrentRoom.Players.Count == 2) // JoinRoom 
            player2 = PhotonNetwork.LocalPlayer;

        bool isHost = (player1.NickName == PhotonNetwork.LocalPlayer.NickName);

        _player1Name.text = player1.NickName;
        _player2Ready.color = NotReadyTextColor;

        if (isHost) // CreateRoom
        {
            _readyButton.gameObject.SetActive(false);
            _readyButtonPressed.gameObject.SetActive(false);
            _startButton.gameObject.SetActive(true);
        }
        else // JoinRoom
        {
            _player2Name.text = player2.NickName;
            _readyButton.gameObject.SetActive(true);
            _readyButtonPressed.gameObject.SetActive(false);
            _startButton.gameObject.SetActive(false);
        }
    }

    public void ClientJoinedRoom(Player newPlayer)
    // (For host) Set InRoom UI after client join room
    {
        Debug.Log("[InRoom] ClientJoinedRoom()");
        _player2Name.text = newPlayer.NickName;
    }

    public void ClientLeftRoom()
    {
        Debug.Log("[InRoom] ClientLeftRoom()");
        _player2Name.text = "";
    }

    public void ReadyInRoom()
    // Set Player2 Status to Ready
    {
        EventManager.Instance.PostNotification(EventType.ClientReady, this, null);
        NetworkManager.Instance.ReadyInRoom(true);

        _readyButton.gameObject.SetActive(false);
        _readyButtonPressed.gameObject.SetActive(true);
    }

    public void NotReadyInRoom()
    // Set Player2 Status to NotReady
    {
        EventManager.Instance.PostNotification(EventType.ClientNotReady, this, null);
        NetworkManager.Instance.ReadyInRoom(false);

        _readyButton.gameObject.SetActive(true);
        _readyButtonPressed.gameObject.SetActive(false);
    }

    public void SetClientReadyText(Color newColor)
    {
        _player2Ready.color = newColor;
    }

    public void TryStartGameInRoom()
    // Set RoomStatus to gaming + Load CharacterSelectionScene
    {
        NetworkManager.Instance.P1_username = _player1Name.text;
        NetworkManager.Instance.P2_username = _player2Name.text;
        NetworkManager.Instance.StartGame();
    }

    public void StartGameInRoom()
    {
        GameManager.Instance.LoadSceneByIndex(2); // SelectScene
    }

    public void TryExitInRoom()
    {
        NetworkManager.Instance.LeaveRoom();
    }

    public void ExitInRoomSuccess()
    // Unload InRoomUI
    {
        _lobbyManager.ExitRoom();
    }

    public void MigrateHost()
    // [UI] If Host(Player1) leave room, Set Player2 be the host
    {
        _player1Name.text = PhotonNetwork.LocalPlayer.NickName;
        _player2Name.text = "";

        SetClientReadyText(NotReadyTextColor);

        _readyButton.gameObject.SetActive(false);
        _readyButtonPressed.gameObject.SetActive(false);
        _startButton.gameObject.SetActive(true);
    }
}