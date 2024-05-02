using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Timebar : MonoBehaviour
{
    const float maxTime = 30.0f;

    public Image leftTimeImage;
    public TMP_Text leftTimeText;

    private float leftTime;

    void Start()
    {
        Init();
    }

    void Update()
    {
        if (leftTime > 0)
        {
            leftTime -= Time.deltaTime;
            leftTimeText.text = $"{leftTime:N1}";
            leftTimeImage.fillAmount = leftTime / maxTime;
        }

    }

    public void Init()
    {
        leftTime = maxTime;
    }
}
