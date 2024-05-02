using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InRoom : MonoBehaviour, IListener
{
    public LobbyManager lobbyManager;

    private readonly Color ReadyTextColor = new Color32(100,100,0,255);
    private readonly Color NotReadyTextColor = new Color32(200, 200, 200, 255);

    public GameObject UI_InRoom;
    public TMP_Text player1Name;
    public TMP_Text player2Name;
    public TMP_Text player2Ready;
    public Button ReadyButton;
    public Button ReadyButton_Pressed;
    public Button StartButton;
    public Button ExitButton;

    private void Awake()
    {
        EventManager.instance.AddListener(EVENT_TYPE.INIT_INROOM, this);
        EventManager.instance.AddListener(EVENT_TYPE.HOST_LEFT_ROOM, this);
        EventManager.instance.AddListener(EVENT_TYPE.LEFT_ROOM_SUCCESS, this);
        EventManager.instance.AddListener(EVENT_TYPE.CLIENT_JOIN_ROOM, this);
        EventManager.instance.AddListener(EVENT_TYPE.CLIENT_LEFT_ROOM, this);
        EventManager.instance.AddListener(EVENT_TYPE.CLIENT_READY, this);
        EventManager.instance.AddListener(EVENT_TYPE.CLIENT_NOT_READY, this);
        EventManager.instance.AddListener(EVENT_TYPE.START_GAME, this);
    }

    // Start is called before the first frame update
    private void Start()
    {
        UI_InRoom = gameObject;

        ReadyButton.onClick.AddListener(Ready_InRoom);
        ReadyButton_Pressed.onClick.AddListener(NotReady_InRoom);
        StartButton.onClick.AddListener(Req_StartGame_InRoom);
        ExitButton.onClick.AddListener(Req_Exit_InRoom);
    }

    public void OnEvent(EVENT_TYPE event_type, Component sender, object param = null)
    {
        Debug.LogFormat("[InRoom] OnEvent() / EVENT : {0}, Sender : {1}, Param : {2} ", event_type, sender.gameObject.name.ToString(), param);

        switch (event_type)
        {
            case EVENT_TYPE.INIT_INROOM:
                Debug.Log("[InRoom] (E)INIT_INROOM");
                SetInRoom();
                break;

            case EVENT_TYPE.HOST_LEFT_ROOM:
                Debug.Log("[InRoom] (E)HOST_LEFT_ROOM");
                MigrateHost();
                break;

            case EVENT_TYPE.LEFT_ROOM_SUCCESS:
                Debug.Log("[InRoom] (E)LEFT_ROOM_SUCCESS");
                Exit_InRoom_Success();
                break;

            case EVENT_TYPE.CLIENT_JOIN_ROOM:
                Debug.Log("[InRoom] (E)CLIENT_JOIN_ROOM");
                if (param is Player)
                    ClientJoinedRoom(param as Player);
                else
                    Debug.LogError("[InRoom] ((E)CLIENT_JOIN_ROOM) Casting invalid");
                break;

            case EVENT_TYPE.CLIENT_LEFT_ROOM:
                Debug.Log("[InRoom] (E)CLIENT_LEFT_ROOM");
                ClientLeftRoom();
                break;

            case EVENT_TYPE.CLIENT_READY:
                Debug.Log("[InRoom] (E)CLIENT_READY");
                SetClientReadyText(ReadyTextColor);
                break;

            case EVENT_TYPE.CLIENT_NOT_READY:
                Debug.Log("[InRoom] (E)CLIENT_NOT_READY");
                SetClientReadyText(NotReadyTextColor);
                break;

            case EVENT_TYPE.START_GAME:
                Debug.Log("[InRoom] (E)START_GAME");
                StartGame_InRoom();
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
            ReadyButton_Pressed.gameObject.SetActive(false);
            StartButton.gameObject.SetActive(true);
        }
        else // JoinRoom의 경우
        {
            player2Name.text = player2.NickName;
            ReadyButton.gameObject.SetActive(true);
            ReadyButton_Pressed.gameObject.SetActive(false);
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

    public void Ready_InRoom()
    // Player2의 Status를 Ready로 변경
    {
        EventManager.instance.PostNotification(EVENT_TYPE.CLIENT_READY, this, null);
        NetworkManager.instance.ReadyInRoom(true);

        ReadyButton.gameObject.SetActive(false);
        ReadyButton_Pressed.gameObject.SetActive(true);
    }

    public void NotReady_InRoom()
    // Player2의 Status를 NotReady로 변경
    { 
        EventManager.instance.PostNotification(EVENT_TYPE.CLIENT_NOT_READY, this, null);
        NetworkManager.instance.ReadyInRoom(false);

        ReadyButton.gameObject.SetActive(true);
        ReadyButton_Pressed.gameObject.SetActive(false);
    }

    public void SetClientReadyText(Color newColor)
    {
        player2Ready.color = newColor;
    }

    public void Req_StartGame_InRoom()
    // Room의 Status를 Gaming으로 변경 + Character Select Scene으로 전환
    {
        NetworkManager.instance.p1_username = player1Name.text;
        NetworkManager.instance.p2_username = player2Name.text;
        NetworkManager.instance.StartGame();
    }

    public void StartGame_InRoom()
    {
        GameManager.instance.LoadSceneByIndex(2); // SelectScene
    }

    public void Req_Exit_InRoom()
    {
        NetworkManager.instance.LeaveRoom();
    }

    public void Exit_InRoom_Success()
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
        ReadyButton_Pressed.gameObject.SetActive(false);
        StartButton.gameObject.SetActive(true);
    }
}