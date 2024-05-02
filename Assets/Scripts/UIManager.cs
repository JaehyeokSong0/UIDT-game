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

    private GameObject networkLoadingPanel;
    public GameObject exitPanel;

    [HideInInspector]
    public bool b_exitPanelActivated;
    [HideInInspector]
    public bool b_networkLoadingPanelActivated;

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
        b_networkLoadingPanelActivated = false;

        if(networkLoadingPanel == null)
            networkLoadingPanel = Instantiate(Resources.Load("Prefabs/Panel_Loading")) as GameObject;

        ActivateNetworkLoadingPanel();
    }

    private void Update() 
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            exitPanel.GetComponent<ExitPanel>().ActivateExitPanel();
    }

    #region ActivatePrefabs
    public void ActivateNetworkLoadingPanel()
    {
        networkLoadingPanel.GetComponent<NetworkLoadingPanel>().ActivateNetworkLoadingPanel();
    }
    public void DeactivateNetworkLoadingPanel()
    {
        networkLoadingPanel.GetComponent<NetworkLoadingPanel>().DeactivateNetworkLoadingPanel();
    }

    #endregion

    #region UI Functions
    #endregion
}
