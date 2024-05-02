using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExitPanel : MonoBehaviour
{
    public Button exitButton;
    public Button returnButton;

    public void ActivateExitPanel()
    {
        if (UIManager.instance.b_exitPanelActivated == false)
        {
            Instantiate(gameObject);
            UIManager.instance.b_exitPanelActivated = true;
        }
    }

    public void DeactivateExitPanel()
    {
        if (UIManager.instance.b_exitPanelActivated == true)
        {
            Destroy(gameObject);
            UIManager.instance.b_exitPanelActivated = false;
        }
    }

}
