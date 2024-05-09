using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance = null;
    #region Variables
    private const byte MaxPlayersPerRoom = 2;

    private Dictionary<string, RoomInfo> _roomList = new Dictionary<string, RoomInfo>();
    private bool _isReconnectionToLobby = false; // leaveRoom 등의 메소드 이후 master 서버에 재접속 시 true

    [HideInInspector]
    public string P1_username, P2_username;
    #endregion

    private void Awake()
    {
        if (Instance == null)
        {
            Debug.Log("[NetworkManager] Awake");
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.Log("[NetworkManager] Already Exists");
            if (Instance != this)
                Destroy(this.gameObject);
        }

        // PhotonNetwork.LoadLevel() 사용 가능 / 클라이언트 간 자동 동기화
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        Debug.Log("[NetworkManager] Start");
        Connect();
    }


    public void Connect()
    {
        if (!PhotonNetwork.IsConnected)
        {
            // Photon Cloud 연결 시작 지점
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    #region MonoBehaviorPunCallbacks Callbacks

    public override void OnConnectedToMaster()
    {
        Debug.Log("[NetworkManager] OnConnectedToMaster() / reconnection : " + _isReconnectionToLobby);

        UIManager.Instance.DeactivateNetworkLoadingPanel();
        
        if (_isReconnectionToLobby)
            PhotonNetwork.JoinLobby();
        else
            _isReconnectionToLobby = true;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat("[NetworkManager] OnDisconnected() : {0}", cause);
        _roomList.Clear();

        UIManager.Instance.ActivateNetworkLoadingPanel();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("[NetworkManager] OnJoinedLobby()");
        _roomList.Clear();
    }

    public override void OnLeftLobby()
    {
        Debug.Log("[NetworkManager] OnLeftLobby()");
        _roomList.Clear();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("[NetworkManager] OnRoomListUpdate()" + roomList);
        UpdateRoomList(roomList);

        EventManager.Instance.PostNotification(EventType.UpdateLobby, this, _roomList);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("[NetworkManager] OnCreatedRoom()");

        EventManager.Instance.PostNotification(EventType.CreateRoomSuccess, this, null);
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "RoomStatus", "Waiting" }, { "ReadyPlayerCnt", 0 }, { "CharacterSelectedCnt", 0 } });
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogWarningFormat("[NetworkManager] OnCreatedRoomFailed() : {0} , {1}", returnCode, message);
        EventManager.Instance.PostNotification(EventType.CreateRoomFailure, this, null);
    }

    public override void OnLeftRoom()
    {
        Debug.Log("[NetworkManager] OnLeftRoom()");
        EventManager.Instance.PostNotification(EventType.LeftRoomSuccess, this, null);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("[NetworkManager] OnJoinedRoom()");
        EventManager.Instance.PostNotification(EventType.JoinRoomSuccess, this, null);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogWarningFormat("[NetworkManager] OnJoinRoomFailed() : {0} , {1}", returnCode, message);
        EventManager.Instance.PostNotification(EventType.JoinRoomFailure, this, null);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("[NetworkManager] OnPlayerEnteredRoom()");
        EventManager.Instance.PostNotification(EventType.ClientJoinRoom, this, newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("[NetworkManager] OnPlayerLeftRoom()");
        EventManager.Instance.PostNotification(EventType.ClientLeftRoom, this, null);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log("[NetworkManager] OnMasterClientSwitched()");
        if ((PhotonNetwork.LocalPlayer.NickName == newMasterClient.NickName) && (PhotonNetwork.CurrentRoom.IsOpen == true))
        {
            Hashtable hash = new Hashtable() { { "ReadyPlayerCnt", 0 } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
            EventManager.Instance.PostNotification(EventType.HostLeftRoom, this, PhotonNetwork.CurrentRoom.Name);
        }
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        Debug.Log("[NetworkManager] OnRoomPropertiesUpdate()" + propertiesThatChanged);
        if (propertiesThatChanged.ContainsKey("ReadyPlayerCnt"))
        {
            int readyPlayerCnt = (int)propertiesThatChanged["ReadyPlayerCnt"];
            if (readyPlayerCnt == 0)
                EventManager.Instance.PostNotification(EventType.ClientNotReady, this, null);
            else if (readyPlayerCnt == 1)
                EventManager.Instance.PostNotification(EventType.ClientReady, this, null);
            else
                Debug.LogError("[NetworkManager] OnRoomPropertiesUpdate() / RoomCustomProperty - Wrong ReadyPlayerCnt");
        }

        if (propertiesThatChanged.ContainsKey("RoomStatus"))
        {
            string roomStatus = (string)propertiesThatChanged["RoomStatus"];
            if (roomStatus == "Waiting")
                EventManager.Instance.PostNotification(EventType.ExitGame, this, null);
            else if (roomStatus == "Gaming")
            {
                // Set usernames
                Player player1 = PhotonNetwork.CurrentRoom.GetPlayer(PhotonNetwork.CurrentRoom.masterClientId);
                P1_username = player1.NickName;
                P2_username = player1.GetNext().NickName;

                EventManager.Instance.PostNotification(EventType.EnterCharacterSelectionPhase, this, null);
                PhotonNetwork.CurrentRoom.IsOpen = false;
            }
            else
                Debug.LogError("[NetworkManager] OnRoomPropertiesUpdate() / RoomCustomProperty - Wrong RoomStatus");
        }

        if (propertiesThatChanged.ContainsKey("CharacterSelectedCnt"))
        {
            int readyPlayerCnt = (int)propertiesThatChanged["CharacterSelectedCnt"];
            if (readyPlayerCnt == 2)
                EventManager.Instance.PostNotification(EventType.EnterCardSelectionPhase, this, null);
        }

    }
    #endregion

    #region UI Interactions
    public void SetPlayerNickname(string nickname)
    {
        PhotonNetwork.LocalPlayer.NickName = nickname;

        Debug.Log("[NetworkManager] Nickname set : " + PhotonNetwork.LocalPlayer.NickName);

        PhotonNetwork.JoinLobby();
    }

    public int GetRoomCount()
    {
        return _roomList.Count;
    }

    public bool IsMasterClient()
    {
        return PhotonNetwork.LocalPlayer.IsMasterClient;
    }

    public void LoadScene(int level)
    {
        PhotonNetwork.LoadLevel(level);
    }

    public CharacterType GetEnemyCharacter()
    {
        Debug.Log("[NetworkManager] GetEnemyCharacter : " + (CharacterType)(PhotonNetwork.LocalPlayer.GetNext().CustomProperties["Character"]));
        return (CharacterType)(PhotonNetwork.LocalPlayer.GetNext().CustomProperties["Character"]);
    }
    #endregion

    #region Room Functions
    public void UpdateRoomList(List<RoomInfo> roomList)
    {
        Debug.Log("[NetworkManager] UpdateRoomList()");

        for (int i = 0; i < roomList.Count; i++)
        {
            RoomInfo room = roomList[i];

            if (room.RemovedFromList) // room이 삭제된 경우
            {
                _roomList.Remove(room.Name);
            }
            else // roomList가 갱신된 경우
            {
                if (_roomList.ContainsKey(room.Name) == false) // 새로 생성된 room인 경우
                    _roomList.Add(room.Name, room);
                else // 이미 존재하는 room인 경우
                    _roomList[room.Name] = room;
            }
        }
    }

    public void CreateRoom()
    // 성공 여부 콜백 함수에서 이벤트 발생시켜 클라이언트 동작 수행
    {
        Debug.Log("[NetworkManager] CreateRoom");

        if (_roomList.Count < 4)
            PhotonNetwork.CreateRoom(PhotonNetwork.LocalPlayer.NickName, new RoomOptions { MaxPlayers = MaxPlayersPerRoom });
        else
        {
            Debug.LogError("[NetworkManager] CreateRoom() failed : Too many rooms.");

            EventManager.Instance.PostNotification(EventType.CreateRoomFailure, this, null);
        }
    }

    public void LeaveRoom()
    {
        Debug.Log("[NetworkManager] LeaveRoom");

        if (!PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            Hashtable hash = new Hashtable() { { "ReadyPlayerCnt", 0 } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
        }

        PhotonNetwork.LeaveRoom();
    }

    public void JoinRoom(string roomName)
    {
        Debug.Log("[NetworkManager] JoinRoom");
        if (_roomList[roomName].PlayerCount == 1)
            PhotonNetwork.JoinRoom(roomName);
        else
            Debug.LogError("[NetworkManager] JoinRoom() : Cannot join room. PlayerCount : " + _roomList[roomName].PlayerCount);
    }

    public void ReadyInRoom(bool isReady)
    // Client가 Ready하는 경우 방의 ReadyPlayerCnt를 1, Ready를 취소하는 경우 0으로 변경
    // [MEMO] 추후 예외처리가 필요할 가능성 존재
    {
        Debug.Log("[NetworkManager] ReadyInRoom");

        Hashtable hash;

        if (isReady)
            hash = new Hashtable() { { "ReadyPlayerCnt", 1 } };
        else
            hash = new Hashtable() { { "ReadyPlayerCnt", 0 } };

        PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
    }

    public void StartGame() // ready check
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("ReadyPlayerCnt"))
        {
            if ((int)PhotonNetwork.CurrentRoom.CustomProperties["ReadyPlayerCnt"] == 1)
            {
                Hashtable hash = new Hashtable() { { "RoomStatus", "Gaming" } };
                PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
            }
        }

    }

    public void CharacterSelected()
    {
        int currReadyCnt = (int)PhotonNetwork.CurrentRoom.CustomProperties["CharacterSelectedCnt"];
        Debug.Log("[NetworkManager] CharacterSelected / CharacterSelectedCnt : " + currReadyCnt);

        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "Character", GameManager.Instance.playerCharacter } });

        Hashtable hash = new Hashtable() { { "CharacterSelectedCnt", currReadyCnt + 1 } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
    }

    public void ExitGame()
    {
        Hashtable hash = new Hashtable() { { "RoomStatus", "Waiting" } }; // Invoke EventType.ExitGame
        PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
    }
    #endregion
}
