using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExitPanel : MonoBehaviour
{
    public Button exitButton;
    public Button returnButton;
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void ActivateExitPanel()
    {
        if (UIManager.instance.isExitPanelActivated == false)
        {
            gameObject.SetActive(true);
            UIManager.instance.isExitPanelActivated = true;
        }
    }

    public void DeactivateExitPanel()
    {
        if (UIManager.instance.isExitPanelActivated == true)
        {
            gameObject.SetActive(false);
            UIManager.instance.isExitPanelActivated = false;
        }
    }
}
