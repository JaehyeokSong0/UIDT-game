using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkLoadingPanel : MonoBehaviour
{
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void ActivateNetworkLoadingPanel()
    {
        if (UIManager.instance.isNetworkLoadingPanelActivated == false)
        {
            gameObject.SetActive(true);
            UIManager.instance.isNetworkLoadingPanelActivated = true;
        }
    }
    public void DeactivateNetworkLoadingPanel()
    {
        if (UIManager.instance.isNetworkLoadingPanelActivated == true)
        {
            gameObject.SetActive(false);
            UIManager.instance.isNetworkLoadingPanelActivated = false;
        }
    }
}
