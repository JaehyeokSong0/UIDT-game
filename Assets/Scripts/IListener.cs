using UnityEngine;

public interface IListener
{
    void OnEvent(EventType eventType, Component Sender, object Param = null);
}
