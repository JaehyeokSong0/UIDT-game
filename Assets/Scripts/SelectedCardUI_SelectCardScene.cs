using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// SelectCardScene에서 Selected CardUI에 추가로 붙이는 컴포넌트
// Mouse event 등을 관리
public class SelectedCardUI_SelectCardScene : MonoBehaviour
{
    CardUI cardUI;
    public CardSelection cardSelection;

    private void OnEnable()
    {
        cardUI = this.gameObject.GetComponent<CardUI>();
        if (cardUI == null)
            Debug.LogError("[SelectedCardUI_SelectCardScene] Start : cardUI cannot be null");
    }


    private void OnMouseUp()
    {
        Debug.Log("[SelectedCardUI_SelectCardScene] OnMouseUp");

        cardSelection.DeselectCard(cardUI);
    }
}
