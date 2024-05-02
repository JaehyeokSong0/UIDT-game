using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public enum EVENT_TYPE
{
    CREATE_ROOM_SUCCESS, 
    CREATE_ROOM_FAILURE, 
    JOIN_ROOM_SUCCESS, 
    JOIN_ROOM_FAILURE, 
    LEFT_ROOM_SUCCESS,
    UPDATE_LOBBY,
    INIT_INROOM, // Create 혹은 Join 시 RoomStatus 초기화
    HOST_LEFT_ROOM,
    CLIENT_JOIN_ROOM,
    CLIENT_LEFT_ROOM,
    CLIENT_READY,
    CLIENT_NOT_READY,
    START_GAME, // ENTER_CHARACTER_SELECTION_PHASE
    ENTER_CARD_SELECTION_PHASE,
    PLAYER_1_CARD_SELECTED,
    PLAYER_2_CARD_SELECTED,
    ENTER_BATTLE_PHASE,
    EXIT_GAME
};


public class EventManager : MonoBehaviour
{
    public static EventManager instance = null;
    private Dictionary<EVENT_TYPE, List<IListener>> Listeners = new Dictionary<EVENT_TYPE, List<IListener>>();

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
    
    public void AddListener(EVENT_TYPE event_type, IListener listener)
    {
        // Debug.Log("[EventManager] AddListener() : " + event_type + " added to " + listener);
        List<IListener> ListenList = null;

        if(Listeners.TryGetValue(event_type, out ListenList))
            ListenList.Add(listener);
        else
        {
            ListenList = new List<IListener>();
            ListenList.Add(listener);
            Listeners.Add(event_type, ListenList);
        }
    }

    public void PostNotification(EVENT_TYPE event_type, Component sender, object param = null)
    {
        // Debug.Log("[EventManager] PostNotification() : " + event_type);
        List<IListener> ListenList = null;

        if(Listeners.TryGetValue(event_type, out ListenList))
        {
            Debug.Log("[EventManager] PostNotification() : " + event_type + " / " + ListenList);
            for (int i = 0; i< ListenList.Count; i++)
            {
                if(!ListenList[i].Equals(null))
                    ListenList[i].OnEvent(event_type, sender, param);
            }
        }
    }

    public void RemoveEvent(EVENT_TYPE event_type)
    {
        Listeners.Remove(event_type);
    }

    public void RemoveRedundancies()
    // Listeners 내의 중복 제거
    {
        Dictionary<EVENT_TYPE, List<IListener>> _Listeners = new Dictionary<EVENT_TYPE, List<IListener>>();

        foreach(KeyValuePair<EVENT_TYPE, List<IListener>> item in Listeners)
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
