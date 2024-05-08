using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InRoom : MonoBehaviour, IListener
{
    public LobbyManager lobbyManager;

    private readonly Color ReadyTextColor = new Color32(100, 100, 0, 255);
    private readonly Color NotReadyTextColor = new Color32(200, 200, 200, 255);

    [FormerlySerializedAs("UI_InRoom")]
    public GameObject InRoomUI;
    public TMP_Text player1Name;
    public TMP_Text player2Name;
    public TMP_Text player2Ready;
    public Button ReadyButton;
    [FormerlySerializedAs("ReadyButton_Pressed")]
    public Button ReadyButtonPressed;
    public Button StartButton;
    public Button ExitButton;

    private void Awake()
    {
        EventManager.instance.AddListener(EventType.InitInRoom, this);
        EventManager.instance.AddListener(EventType.HostLeftRoom, this);
        EventManager.instance.AddListener(EventType.LeftRoomSuccess, this);
        EventManager.instance.AddListener(EventType.ClientJoinRoom, this);
        EventManager.instance.AddListener(EventType.ClientLeftRoom, this);
        EventManager.instance.AddListener(EventType.ClientReady, this);
        EventManager.instance.AddListener(EventType.ClientNotReady, this);
        EventManager.instance.AddListener(EventType.EnterCharacterSelectionPhase, this);
    }

    // Start is called before the first frame update
    private void Start()
    {
        InRoomUI = gameObject;

        ReadyButton.onClick.AddListener(ReadyInRoom);
        ReadyButtonPressed.onClick.AddListener(NotReadyInRoom);
        StartButton.onClick.AddListener(TryStartGameInRoom);
        ExitButton.onClick.AddListener(TryExitInRoom);
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
        // Debug.Log("[InRoom] SetInRoom()");

        Player player1 = PhotonNetwork.MasterClient;
        Player player2 = null;

        Debug.Log("[InRoom] SetInRoom() / Current Room Player : " + PhotonNetwork.CurrentRoom.Players.Count);

        if (PhotonNetwork.CurrentRoom.Players.Count == 2) // JoinRoom 
             player2 = PhotonNetwork.LocalPlayer;

        bool isHost = (player1.NickName == PhotonNetwork.LocalPlayer.NickName);

        player1Name.text = player1.NickName;
        player2Ready.color = NotReadyTextColor;

        if (isHost) // CreateRoom의 경우
        {
            ReadyButton.gameObject.SetActive(false);
            ReadyButtonPressed.gameObject.SetActive(false);
            StartButton.gameObject.SetActive(true);
        }
        else // JoinRoom의 경우
        {
            player2Name.text = player2.NickName;
            ReadyButton.gameObject.SetActive(true);
            ReadyButtonPressed.gameObject.SetActive(false);
            StartButton.gameObject.SetActive(false);
        }
    }

    public void ClientJoinedRoom(Player newPlayer)
    // Host 기준 Client 입장 후 InRoom 갱신 - NetworkManager\OnPlayerEnteredRoom과 연동
    {
        Debug.Log("[InRoom] ClientJoinedRoom()");
        player2Name.text = newPlayer.NickName;
    }

    public void ClientLeftRoom()
    {
        Debug.Log("[InRoom] ClientLeftRoom()");
        player2Name.text = "";
    }

    public void ReadyInRoom()
    // Player2의 Status를 Ready로 변경
    {
        EventManager.instance.PostNotification(EventType.ClientReady, this, null);
        NetworkManager.instance.ReadyInRoom(true);

        ReadyButton.gameObject.SetActive(false);
        ReadyButtonPressed.gameObject.SetActive(true);
    }

    public void NotReadyInRoom()
    // Player2의 Status를 NotReady로 변경
    { 
        EventManager.instance.PostNotification(EventType.ClientNotReady, this, null);
        NetworkManager.instance.ReadyInRoom(false);

        ReadyButton.gameObject.SetActive(true);
        ReadyButtonPressed.gameObject.SetActive(false);
    }

    public void SetClientReadyText(Color newColor)
    {
        player2Ready.color = newColor;
    }

    public void TryStartGameInRoom()
    // Room의 Status를 Gaming으로 변경 + Character Select Scene으로 전환
    {
        NetworkManager.instance.p1_username = player1Name.text;
        NetworkManager.instance.p2_username = player2Name.text;
        NetworkManager.instance.StartGame();
    }

    public void StartGameInRoom()
    {
        GameManager.instance.LoadSceneByIndex(2); // SelectScene
    }

    public void TryExitInRoom()
    {
        NetworkManager.instance.LeaveRoom();
    }

    public void ExitInRoomSuccess()
    // InGame UI 해제
    {
        lobbyManager.ExitRoom();
    }

    public void MigrateHost()
    // Host(Player1)이 room에서 나간 경우 UI상에서 Player2의 위치(오른쪽)를 Player1의 위치(왼쪽)으로 이동
    // 서버단에서의 작업은 NetworkManager에서 구현
    {
        player1Name.text = PhotonNetwork.LocalPlayer.NickName;
        player2Name.text = "";

        SetClientReadyText(NotReadyTextColor);

        ReadyButton.gameObject.SetActive(false);
        ReadyButtonPressed.gameObject.SetActive(false);
        StartButton.gameObject.SetActive(true);
    }
}