using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Timebar : MonoBehaviour
{
    private const float MaxTime = 30.0f;

    [SerializeField]
    private Image _leftTimeImage;
    [SerializeField]
    private TMP_Text _leftTimeText;
    private float _leftTime;

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        if (_leftTime > 0)
        {
            _leftTime -= Time.deltaTime;
            _leftTimeText.text = $"{_leftTime:N1}";
            _leftTimeImage.fillAmount = _leftTime / MaxTime;
        }

    }

    private void Init()
    {
        _leftTime = MaxTime;
    }
}
