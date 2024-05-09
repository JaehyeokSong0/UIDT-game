using UnityEngine;
using UnityEngine.UI;
public class UIManager : MonoBehaviour
{
    public static UIManager Instance = null;

    #region UI objects   
    private GameObject _exitPanel, _networkLoadingPanel;

    [HideInInspector]
    public bool IsExitPanelActivated, IsNetworkLoadingPanelActivated;

    [SerializeField]
    private Button _resolution1920Button, _resolution960Button;
    #endregion

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[UIManager] Awake");
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            if (Instance != this)
                Destroy(this.gameObject);
        }

        _resolution1920Button.onClick.AddListener(() => SetResolution(1920, 1080));
        _resolution960Button.onClick.AddListener(() => SetResolution(960, 540));
    }

    private void Start()
    {
        Debug.Log("[UIManager] Start");
        IsExitPanelActivated = false;
        IsNetworkLoadingPanelActivated = false;


        if (_exitPanel == null)
        {
            _exitPanel = Instantiate(Resources.Load("Prefabs/Panel_Exit")) as GameObject;
            _exitPanel.SetActive(false);
        }

        if (_networkLoadingPanel == null)
            _networkLoadingPanel = Instantiate(Resources.Load("Prefabs/Panel_Loading")) as GameObject;

        ActivateNetworkLoadingPanel();
    }

    private void Update() 
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            ActivateExitPanel();
    }

    #region ActivatePrefabs
    public void ActivateExitPanel()
    {
        if (_exitPanel == null)
            _exitPanel = Instantiate(Resources.Load("Prefabs/Panel_Exit")) as GameObject;

        _exitPanel.GetComponent<ExitPanel>().ActivateExitPanel();
    }

    public void ActivateNetworkLoadingPanel()
    {
        if (_networkLoadingPanel == null)
            _networkLoadingPanel = Instantiate(Resources.Load("Prefabs/Panel_Loading")) as GameObject;

        _networkLoadingPanel.GetComponent<NetworkLoadingPanel>().ActivateNetworkLoadingPanel();
    }
    public void DeactivateNetworkLoadingPanel()
    {
        if (_networkLoadingPanel == null)
            _networkLoadingPanel = Instantiate(Resources.Load("Prefabs/Panel_Loading")) as GameObject;

        _networkLoadingPanel.GetComponent<NetworkLoadingPanel>().DeactivateNetworkLoadingPanel();
    }

    #endregion

    #region UI Functions
    private void SetResolution(int width, int height)
    {
        Screen.SetResolution(width, height, false);
    }

    public void OpenURL(string URL)
    {
        Application.OpenURL(URL);
    }
    #endregion
}
