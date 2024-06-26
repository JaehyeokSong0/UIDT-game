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
        if (UIManager.instance.b_exitPanelActivated == false)
        {
            gameObject.SetActive(true);
            UIManager.instance.b_exitPanelActivated = true;
        }
    }

    public void DeactivateExitPanel()
    {
        if (UIManager.instance.b_exitPanelActivated == true)
        {
            gameObject.SetActive(false);
            UIManager.instance.b_exitPanelActivated = false;
        }
    }
}
