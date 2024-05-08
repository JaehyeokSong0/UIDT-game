using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public enum EventType
{
    // LobbyScene
    CreateRoomSuccess,
    CreateRoomFailure, 
    JoinRoomSuccess, 
    JoinRoomFailure, 
    LeftRoomSuccess,
    UpdateLobby,
    InitInRoom, // Create 혹은 Join 시 RoomStatus 초기화
    HostLeftRoom,
    ClientJoinRoom,
    ClientLeftRoom,
    ClientReady,
    ClientNotReady,
    EnterCharacterSelectionPhase,

    // CharacterSelectionScene
    EnterCardSelectionPhase,

    // CardSelectionScene
    Player1CardSelected,
    Player2CardSelected,
    EnterBattlePhase,

    // BattleScene
    ExitGame
};


public class EventManager : MonoBehaviour
{
    public static EventManager instance = null;
    private Dictionary<EventType, List<IListener>> Listeners = new Dictionary<EventType, List<IListener>>();

    private void Awake()
    {
        Debug.Log("[EventManager] Awake");
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
    }

    private void OnEnable() 
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RemoveRedundancies();
    }

    private void OnDisable() 
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    public void AddListener(EventType eventType, IListener listener)
    {
 
        List<IListener> ListenList = null;

        if(Listeners.TryGetValue(eventType, out ListenList))
            ListenList.Add(listener);
        else
        {
            ListenList = new List<IListener>();
            ListenList.Add(listener);
            Listeners.Add(eventType, ListenList);
        }
    }

    public void PostNotification(EventType eventType, Component sender, object param = null)
    {
        List<IListener> ListenList = null;

        if(Listeners.TryGetValue(eventType, out ListenList))
        {
            Debug.Log("[EventManager] PostNotification() : " + eventType + " / " + ListenList);
            for (int i = 0; i< ListenList.Count; i++)
            {
                if(!ListenList[i].Equals(null))
                    ListenList[i].OnEvent(eventType, sender, param);
            }
        }
    }

    public void RemoveEvent(EventType eventType)
    {
        Listeners.Remove(eventType);
    }

    public void RemoveRedundancies()
    // Listeners 내의 중복 제거
    {
        Dictionary<EventType, List<IListener>> _Listeners = new Dictionary<EventType, List<IListener>>();

        foreach(KeyValuePair<EventType, List<IListener>> item in Listeners)
        {
            for(int i = item.Value.Count - 1; i >= 0; i--) 
            // IListener
            {
                if (item.Value[i].Equals(null))
                    item.Value.RemoveAt(i);
            }

            if(item.Value.Count > 0)
                _Listeners.Add(item.Key, item.Value);
        }

        Listeners = _Listeners;
    }
}
