using UnityEngine;

public class NetworkLoadingPanel : MonoBehaviour
{
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void ActivateNetworkLoadingPanel()
    {
        if (UIManager.Instance.IsNetworkLoadingPanelActivated == false)
        {
            gameObject.SetActive(true);
            UIManager.Instance.IsNetworkLoadingPanelActivated = true;
        }
    }
    public void DeactivateNetworkLoadingPanel()
    {
        if (UIManager.Instance.IsNetworkLoadingPanelActivated == true)
        {
            gameObject.SetActive(false);
            UIManager.Instance.IsNetworkLoadingPanelActivated = false;
        }
    }
}
