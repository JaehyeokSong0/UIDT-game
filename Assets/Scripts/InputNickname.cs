using System.Collections;
using TMPro;
using UnityEngine;

public class InputNickname : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _warningMessage;
    [SerializeField]
    private TMP_InputField _nicknameInputField;

    private void Awake()
    {
        _nicknameInputField.onSubmit.AddListener(delegate { SetPlayerNickname(); });
    }

    private void SetPlayerNickname()
    {
        string _nickname = _nicknameInputField.text;
        string _upperNickname = _nickname.ToUpper();

        if (string.IsNullOrEmpty(_nickname))
            StartCoroutine(BlinkText(_warningMessage));
        else if (_nickname.Contains(" "))
            StartCoroutine(BlinkText(_warningMessage));
        else if ((_upperNickname == "ADMIN") || (_upperNickname == "SYSTEM"))
            StartCoroutine(BlinkText(_warningMessage));
        else
        {
            NetworkManager.Instance.SetPlayerNickname(_nickname);
            GameManager.Instance.LoadSceneByIndex(1);
        }
    }

    private IEnumerator BlinkText(TMP_Text text)
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
