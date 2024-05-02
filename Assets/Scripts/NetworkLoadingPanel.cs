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
        if (UIManager.instance.b_networkLoadingPanelActivated == false)
        {
            gameObject.SetActive(true);
            UIManager.instance.b_networkLoadingPanelActivated = true;
        }
    }
    public void DeactivateNetworkLoadingPanel()
    {
        if (UIManager.instance.b_networkLoadingPanelActivated == true)
        {
            gameObject.SetActive(false);
            UIManager.instance.b_networkLoadingPanelActivated = false;
        }
    }
    
}
