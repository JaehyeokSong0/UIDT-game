using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ActivatedRoom : MonoBehaviour, IPointerClickHandler
{
    private LobbyManager _lobbyManager;
    [SerializeField]
    private GameObject _roomID, _roomName, _player1, _player2Empty, _player2Full, _waiting, _gaming;
    private RoomInfo _roomInfo;

    void Start()
    {
        if (_lobbyManager== null)
        {
            _lobbyManager = GameObject.FindObjectOfType<LobbyManager>();
            if (_lobbyManager == null)
                Debug.LogError("[ActivatedRoom] Cannot find LobbyManager");
        }
    }

    public void SetRoomInfo(RoomInfo roomInfo)
    {
        _roomInfo = roomInfo;
    }

    public void SetRoomName(string str)
    {
        _roomName.GetComponent<TMP_Text>().text = str;
    }

    public void SetPlayer2Status(int playerCount)
    {
        if(playerCount == 1)
        {
            _player2Empty.SetActive(true);
            _player2Full.SetActive(false);
        }
        else if(playerCount == 2)
        {
            _player2Full.SetActive(true);
            _player2Empty.SetActive(false);
        }
        else
        {
            Debug.LogError("[ActivatedRoom] Invalid player count");
        }
    }

    public void SetGameStatus(bool isWaiting)
    {
        if(isWaiting == true)
        {
            _waiting.SetActive(true);
            _gaming.SetActive(false);
        }
        else
        {
            _gaming.SetActive(true);
            _waiting.SetActive(false);
        }
    }

    public void TryJoinRoom()
    {
        _lobbyManager.TryJoinRoom(_roomInfo.Name);
    }

    #region IPointClickHandler Implementation
    public void OnPointerClick(PointerEventData eventData)
    {
        if (_roomInfo.PlayerCount == 1)
            TryJoinRoom();
    }
    #endregion
}
