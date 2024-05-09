using UnityEngine;
using UnityEngine.UI;

public class ExitPanel : MonoBehaviour
{
    [SerializeField]
    private Button _exitButton;
    [SerializeField]
    private Button _returnButton;
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void ActivateExitPanel()
    {
        if (UIManager.Instance.IsExitPanelActivated == false)
        {
            gameObject.SetActive(true);
            UIManager.Instance.IsExitPanelActivated = true;
        }
    }

    public void DeactivateExitPanel()
    {
        if (UIManager.Instance.IsExitPanelActivated == true)
        {
            gameObject.SetActive(false);
            UIManager.Instance.IsExitPanelActivated = false;
        }
    }
}
