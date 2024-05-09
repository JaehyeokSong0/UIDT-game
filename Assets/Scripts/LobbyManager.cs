using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : MonoBehaviour, IListener
{
    private const int RoomsCount = 4;

    [SerializeField] 
    private GameObject[] _roomUI = new GameObject[4];
    [SerializeField] 
    private GameObject _lobbyButtons, _inRoomUI;

    private void Awake()
    {
        Debug.Log("[LobbyManager] Awake");

        EventManager.Instance.AddListener(EventType.UpdateLobby, this);
        EventManager.Instance.AddListener(EventType.CreateRoomSuccess, this);
        EventManager.Instance.AddListener(EventType.CreateRoomFailure, this);
        EventManager.Instance.AddListener(EventType.JoinRoomSuccess, this);
        EventManager.Instance.AddListener(EventType.JoinRoomFailure, this);

    }

    private void Start()
    {
        Debug.Log("[LobbyManager] Start");
    }

    public void OnEvent(EventType eventType, Component sender, object param = null)
    {
        Debug.LogFormat("[LobbyManager] OnEvent() / EVENT : {0}, Sender : {1}, Param : {2} ", eventType, sender.gameObject.name.ToString(), param);

        switch (eventType)
        {
            case EventType.UpdateLobby:
                Debug.Log("[LobbyManager] (E)UPDATE_LOBBY");

                if(param is Dictionary<string, RoomInfo>)
                    UpdateLobby(param as Dictionary<string, RoomInfo>);
                else
                    Debug.LogError("[LobbyManager] ((E)UPDATE_LOBBY) Casting invalid");
                break;
            case EventType.CreateRoomSuccess:
                Debug.Log("[LobbyManager] (E)CREATE_ROOM_SUCCESS");

                CreateRoomSuccess();
                break;
            case EventType.CreateRoomFailure:
                Debug.Log("[LobbyManager] (E)CREATE_ROOM_FAILURE");

                CreateRoomFailure();
                break;
            case EventType.JoinRoomSuccess:
                Debug.Log("[LobbyManager] (E)JOIN_ROOM_SUCCESS");

                JoinRoomSuccess();
                break;
            case EventType.JoinRoomFailure:
                Debug.Log("[LobbyManager] (E)JOIN_ROOM_FAILURE");

                JoinRoomFailure();
                break;
        }
    }

    public void UpdateLobby(Dictionary<string, RoomInfo> roomList)
    {
        Debug.Log("[LobbyManager] UpdateLobby() / Current RoomCnt : " + roomList.Count);

        if (roomList == null)
            return;

        // Reset all rooms UI
        for(int i = 0; i < 4; i++)
        {
            GameObject activatedRoom = _roomUI[i].transform.GetChild(0).gameObject;
            activatedRoom.SetActive(false);
        }

        int roomCnt = 0;
        foreach (RoomInfo info in roomList.Values)
        {
            roomCnt++;

            if (roomCnt > 4)
            {
                Debug.LogError("[LobbyManager] UpdateLobby() / Invalid roomCnt");
                return;
            }
            Debug.LogFormat("[LobbyManager] {0}, {1}, {2}", info.Name, info.PlayerCount, info.IsOpen);
            // clone, 1, True

            GameObject activatedRoom = _roomUI[roomCnt - 1].transform.GetChild(0).gameObject;
            activatedRoom.SetActive(true);

            ActivatedRoom roomScript = activatedRoom.GetComponent<ActivatedRoom>();
            roomScript.SetRoomInfo(info);
            roomScript.SetRoomName(info.Name);
            roomScript.SetPlayer2Status(info.PlayerCount);
            roomScript.SetGameStatus(info.IsOpen);
        }
    }

    public void TryCreateRoom() // Request CreateRoom to NetworkManager
    {
        Debug.Log("[LobbyManager] TryCreateRoom()");
        NetworkManager.Instance.CreateRoom();
    }

    public void CreateRoomSuccess()
    {
        Debug.Log("[LobbyManager] CreateRoomSuccess()");

        if (NetworkManager.Instance.GetRoomCount() < 4)
        {
            ShowInRoom();
            EventManager.Instance.PostNotification(EventType.InitInRoom, this, null); // Invoke InRoom/SetInRoom
        }
        else
            Debug.LogError("[LobbyManager] CreateRoom() failed : Too many rooms.");
    }

    public void CreateRoomFailure() 
    {
        Debug.Log("[LobbyManager] CreateRoomFailure()");
    }
    public void TryJoinRoom(string roomName)
    {
        Debug.Log("[LobbyManager] JoinRoom()");
        NetworkManager.Instance.JoinRoom(roomName);
    }

    public void JoinRoomSuccess()
    {
        Debug.Log("[LobbyManager] JoinRoomSuccess()");
        ShowInRoom();
        EventManager.Instance.PostNotification(EventType.InitInRoom, this, null); // Invoke InRoom/SetInRoom
    }
    public void JoinRoomFailure()
    {
        Debug.Log("[LobbyManager] JoinRoomFailure()");
    }

    public void ExitRoom()
    {
        Debug.Log("[LobbyManager] ExitRoom()");
        ShowLobby();
    }

    public void ShowInRoom()
    {
        _lobbyButtons.SetActive(false);
        for (int i = 0; i < RoomsCount; i++)
            _roomUI[i].SetActive(false);

        _inRoomUI.SetActive(true);
    }

    public void ShowLobby()
    {
        _lobbyButtons.SetActive(true);
        for (int i = 0; i < RoomsCount; i++)
            _roomUI[i].SetActive(true);

        _inRoomUI.SetActive(false);
    }
}