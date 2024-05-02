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
    private GameObject exitPanel;

    [HideInInspector]
    public bool b_exitPanelActivated;
    [HideInInspector]
    public bool b_networkLoadingPanelActivated;

    public Button btn_resolution_1920;
    public Button btn_resolution_960;

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

        btn_resolution_1920.onClick.AddListener(() => SetResolution(1920, 1080));
        btn_resolution_960.onClick.AddListener(() => SetResolution(960, 540));
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
        if (networkLoadingPanel == null)
            networkLoadingPanel = Instantiate(Resources.Load("Prefabs/Panel_Loading")) as GameObject;

        networkLoadingPanel.GetComponent<NetworkLoadingPanel>().ActivateNetworkLoadingPanel();
    }
    public void DeactivateNetworkLoadingPanel()
    {
        if (networkLoadingPanel == null)
            networkLoadingPanel = Instantiate(Resources.Load("Prefabs/Panel_Loading")) as GameObject;

        networkLoadingPanel.GetComponent<NetworkLoadingPanel>().DeactivateNetworkLoadingPanel();
    }

    #endregion

    #region UI Functions
    public void SetResolution(int width, int height)
    {
        Screen.SetResolution(width, height, false);
    }
    #endregion
}
