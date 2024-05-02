using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class InputNickname : MonoBehaviour
{
    public TMP_Text warningMessage;
    public TMP_InputField nicknameInputField;

    private void Awake()
    {
        nicknameInputField.onSubmit.AddListener(delegate { SetPlayerNickname(); });
    }

    public void SetPlayerNickname()
    {
        string _nickname = nicknameInputField.text;
        string _upperNickname = _nickname.ToUpper();

        if (string.IsNullOrEmpty(_nickname))
            StartCoroutine(BlinkText(warningMessage));
        else if (_nickname.Contains(" "))
            StartCoroutine(BlinkText(warningMessage));
        else if ((_upperNickname == "ADMIN") || (_upperNickname == "SYSTEM"))
            StartCoroutine(BlinkText(warningMessage));
        else
        {
            NetworkManager.instance.SetPlayerNickname(_nickname);
            GameManager.instance.LoadSceneByIndex(1);
        }
    }

    public IEnumerator BlinkText(TMP_Text text)
    {
        float duration = 0.1f;
        int count = 2;

        for (int i = 0; i < count; i++)
        {
            text.gameObject.SetActive(false);
            yield return new WaitForSeconds(duration);
            text.gameObject.SetActive(true);
            yield return new WaitForSeconds(duration);
        }
    }
}
