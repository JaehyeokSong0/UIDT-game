using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    public static UIManager instance = null;

    #region UI objects   

    public GameObject networkLoadingPanel;
    public GameObject exitPanel;

    [HideInInspector]
    public bool b_exitPanelActivated;

    #endregion

    #region Events
    public enum EventType
    {

    }
    #endregion

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            Debug.Log("[UIManager] Awake");
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            if (instance != this)
                Destroy(this.gameObject);
        }

    }

    private void Start()
    {
        Debug.Log("[UIManager] Start");
        b_exitPanelActivated = false;
    }

    private void Update() 
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            ActivateExitPanel();
    }

    #region ActivatePrefabs
    public void ActivateNetworkLoadingPanel()
    {
        Debug.Log("[UIManager] ActivateNetworkLoadingPanel");
        if (networkLoadingPanel == null)
        {
            Debug.LogWarning("[UIManager] ActivateNetworkLoadingPanel() Failed");
            networkLoadingPanel = Resources.Load<GameObject>("Prefabs/Panel_Loading");
        }
        networkLoadingPanel.gameObject.SetActive(true);
    }
    public void DeactivateNetworkLoadingPanel()
    {
        Debug.Log("[UIManager] DeactivateNetworkLoadingPanel");
        if (networkLoadingPanel == null)
        {
            Debug.LogWarning("[UIManager] DeactivateNetworkLoadingPanel() Failed");
            networkLoadingPanel = Resources.Load<GameObject>("Prefabs/Panel_Loading");
        }
        networkLoadingPanel.gameObject.SetActive(false);
    }

    public void ActivateExitPanel()
    {
        exitPanel.GetComponent<ExitPanel>().ActivateExitPanel();
    }
    #endregion

    #region UI Functions
    #endregion
}
