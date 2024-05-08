using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ActivatedRoom : MonoBehaviour, IPointerClickHandler
{
    public LobbyManager lobbyManager;

    private RoomInfo _roomInfo;
    public GameObject roomID;
    public GameObject roomName;
    public GameObject player1;
    public GameObject player2Empty;
    public GameObject player2Full;
    public GameObject Waiting;
    public GameObject Gaming;

    // Start is called before the first frame update
    void Start()
    {
        if (lobbyManager== null)
        {
            lobbyManager = GameObject.FindObjectOfType<LobbyManager>();
            if (lobbyManager == null)
                Debug.LogError("[ActivatedRoom] Cannot find LobbyManager");
        }
    }

    public void SetRoomInfo(RoomInfo roomInfo)
    {
        _roomInfo = roomInfo;
    }

    public void SetRoomName(string str)
    {
        roomName.GetComponent<TMP_Text>().text = str;
    }

    public void SetPlayer2Status(int playerCount)
    {
        if(playerCount == 1)
        {
            player2Empty.SetActive(true);
            player2Full.SetActive(false);
        }
        else if(playerCount == 2)
        {
            player2Full.SetActive(true);
            player2Empty.SetActive(false);
        }
        else
        {
            Debug.LogError("[ActivatedRoom] Invalid player count");
        }
    }

    public void SetGameStatus(bool isWaiting)
    {
        if(isWaiting == true) // Room is open
        {
            Waiting.SetActive(true);
            Gaming.SetActive(false);
        }
        else
        {
            Gaming.SetActive(true);
            Waiting.SetActive(false);
        }
    }

    public void TryJoinRoom()
    // JoinRoom 요청을 보내 수신한 콜백 이벤트에 따라 LobbyManager에서 작업 수행
    {
        lobbyManager.TryJoinRoom(_roomInfo.Name);
    }

    #region IPointClickHandler Implementation
    public void OnPointerClick(PointerEventData eventData)
    {
        if (_roomInfo.PlayerCount == 1)
            TryJoinRoom();
    }
    #endregion
}
