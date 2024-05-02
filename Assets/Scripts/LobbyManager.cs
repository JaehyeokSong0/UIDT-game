using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using TMPro;
using Photon.Pun;

public class LobbyManager : MonoBehaviour, IListener
{
    const int roomsCount = 4;

    public GameObject[] UI_Room = new GameObject[4];
    public GameObject LobbyButtons; // LobbyScene\Canvas\Lobby\UI_Buttons
    public GameObject UI_InRoom;

    private void Awake()
    {
        Debug.Log("[LobbyManager] Awake");

        EventManager.instance.AddListener(EVENT_TYPE.UPDATE_LOBBY, this);
        EventManager.instance.AddListener(EVENT_TYPE.CREATE_ROOM_SUCCESS, this);
        EventManager.instance.AddListener(EVENT_TYPE.CREATE_ROOM_FAILURE, this);
        EventManager.instance.AddListener(EVENT_TYPE.JOIN_ROOM_SUCCESS, this);
        EventManager.instance.AddListener(EVENT_TYPE.JOIN_ROOM_FAILURE, this);

    }

    private void Start()
    {
        Debug.Log("[LobbyManager] Start");
    }

    public void OnEvent(EVENT_TYPE event_type, Component sender, object param = null)
    {
        Debug.LogFormat("[LobbyManager] OnEvent() / EVENT : {0}, Sender : {1}, Param : {2} ", event_type, sender.gameObject.name.ToString(), param);

        switch (event_type)
        {
            case EVENT_TYPE.UPDATE_LOBBY:
                Debug.Log("[LobbyManager] (E)UPDATE_LOBBY");

                if(param is Dictionary<string, RoomInfo>)
                    UpdateLobby(param as Dictionary<string, RoomInfo>);
                else
                    Debug.LogError("[LobbyManager] ((E)UPDATE_LOBBY) Casting invalid");
                break;
            case EVENT_TYPE.CREATE_ROOM_SUCCESS:
                Debug.Log("[LobbyManager] (E)CREATE_ROOM_SUCCESS");

                CreateRoom_Success();
                break;
            case EVENT_TYPE.CREATE_ROOM_FAILURE:
                Debug.Log("[LobbyManager] (E)CREATE_ROOM_FAILURE");

                CreateRoom_Fail();
                break;
            case EVENT_TYPE.JOIN_ROOM_SUCCESS:
                Debug.Log("[LobbyManager] (E)JOIN_ROOM_SUCCESS");

                JoinRoom_Success();
                break;
            case EVENT_TYPE.JOIN_ROOM_FAILURE:
                Debug.Log("[LobbyManager] (E)JOIN_ROOM_FAILURE");

                JoinRoom_Fail();
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
            GameObject activatedRoom = UI_Room[i].transform.GetChild(0).gameObject;
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

            GameObject activatedRoom = UI_Room[_roomCnt - 1].transform.GetChild(0).gameObject;
            activatedRoom.SetActive(true);

            ActivatedRoom roomScript = activatedRoom.GetComponent<ActivatedRoom>();
            roomScript.SetRoomInfo(info);
            roomScript.SetRoomName(info.Name);
            roomScript.SetPlayer2Status(info.PlayerCount);
            roomScript.SetGameStatus(info.IsOpen);
        }
    }

    public void Req_CreateRoom() // NetworkManager에 CreateRoom 요청 후 콜백을 통해 작업 수행
    {
        Debug.Log("[LobbyManager] Req_CreateRoom()");
        NetworkManager.instance.CreateRoom();
    }

    public void CreateRoom_Success()
    {
        Debug.Log("[LobbyManager] CreateRoom_Success()");

        if (NetworkManager.instance.GetRoomCount() < 4)
        {
            ShowInRoom();
            EventManager.instance.PostNotification(EVENT_TYPE.INIT_INROOM, this, null); // InRoom\SetInRoom() 수행
        }
        else
            Debug.LogError("[LobbyManager] CreateRoom() failed : Too many rooms.");
    }

    public void CreateRoom_Fail() 
    {
        Debug.Log("[LobbyManager] CreateRoom_Fail()");
    }
    public void Req_JoinRoom(string roomName)
    {
        Debug.Log("[LobbyManager] JoinRoom()");
        NetworkManager.instance.JoinRoom(roomName);
    }

    public void JoinRoom_Success()
    {
        Debug.Log("[LobbyManager] JoinRoom_Success()");
        ShowInRoom();
        EventManager.instance.PostNotification(EVENT_TYPE.INIT_INROOM, this, null); // InRoom\SetInRoom() 수행
    }
    public void JoinRoom_Fail()
    {
        Debug.Log("[LobbyManager] JoinRoom_Fail()");
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
            UI_Room[i].SetActive(false);

        UI_InRoom.SetActive(true);
    }

    public void ShowLobby()
    {
        LobbyButtons.SetActive(true);
        for (int i = 0; i < roomsCount; i++)
            UI_Room[i].SetActive(true);

        UI_InRoom.SetActive(false);
    }
}
