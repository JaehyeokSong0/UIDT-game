using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using TMPro;
using Photon.Pun;
using UnityEngine.Serialization;

public class LobbyManager : MonoBehaviour, IListener
{
    const int roomsCount = 4;

    [FormerlySerializedAs("UI_Room")]
    public GameObject[] RoomUI = new GameObject[4];
    public GameObject LobbyButtons; // LobbyScene/Canvas/Lobby/UI_Buttons
    [FormerlySerializedAs("UI_InRoom")]
    public GameObject InRoomUI;

    private void Awake()
    {
        Debug.Log("[LobbyManager] Awake");

        EventManager.instance.AddListener(EventType.UpdateLobby, this);
        EventManager.instance.AddListener(EventType.CreateRoomSuccess, this);
        EventManager.instance.AddListener(EventType.CreateRoomFailure, this);
        EventManager.instance.AddListener(EventType.JoinRoomSuccess, this);
        EventManager.instance.AddListener(EventType.JoinRoomFailure, this);

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

        foreach(var room in roomList)
            Debug.Log(room.Key + ": " + room.Value);

        if (roomList == null)
            return;

        // Reset all rooms UI
        for(int i = 0; i < 4; i++)
        {
            GameObject activatedRoom = RoomUI[i].transform.GetChild(0).gameObject;
            activatedRoom.SetActive(false);
        }

        int _roomCnt = 0;
        foreach (RoomInfo info in roomList.Values)
        {
            _roomCnt++;

            if (_roomCnt > 4)
            {
                Debug.LogError("[LobbyManager] UpdateLobby() / Invalid roomCnt");
                return;
            }
            Debug.LogFormat("[LobbyManager] {0}, {1}, {2}", info.Name, info.PlayerCount, info.IsOpen);
            // clone, 1, True

            GameObject activatedRoom = RoomUI[_roomCnt - 1].transform.GetChild(0).gameObject;
            activatedRoom.SetActive(true);

            ActivatedRoom roomScript = activatedRoom.GetComponent<ActivatedRoom>();
            roomScript.SetRoomInfo(info);
            roomScript.SetRoomName(info.Name);
            roomScript.SetPlayer2Status(info.PlayerCount);
            roomScript.SetGameStatus(info.IsOpen);
        }
    }

    public void TryCreateRoom() // NetworkManager에 CreateRoom 요청 후 콜백을 통해 작업 수행
    {
        Debug.Log("[LobbyManager] TryCreateRoom()");
        NetworkManager.instance.CreateRoom();
    }

    public void CreateRoomSuccess()
    {
        Debug.Log("[LobbyManager] CreateRoomSuccess()");

        if (NetworkManager.instance.GetRoomCount() < 4)
        {
            ShowInRoom();
            EventManager.instance.PostNotification(EventType.InitInRoom, this, null); // InRoom\SetInRoom() 수행
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
        NetworkManager.instance.JoinRoom(roomName);
    }

    public void JoinRoomSuccess()
    {
        Debug.Log("[LobbyManager] JoinRoomSuccess()");
        ShowInRoom();
        EventManager.instance.PostNotification(EventType.InitInRoom, this, null); // InRoom/SetInRoom() 수행
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
        LobbyButtons.SetActive(false);
        for (int i = 0; i < roomsCount; i++)
            RoomUI[i].SetActive(false);

        InRoomUI.SetActive(true);
    }

    public void ShowLobby()
    {
        LobbyButtons.SetActive(true);
        for (int i = 0; i < roomsCount; i++)
            RoomUI[i].SetActive(true);

        InRoomUI.SetActive(false);
    }
}
