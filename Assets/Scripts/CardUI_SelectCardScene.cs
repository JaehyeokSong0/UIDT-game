using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

// SelectCardScene에서 Unselected CardUI에 추가로 붙이는 컴포넌트
// Mouse event 등을 관리
public class CardUI_SelectCardScene : MonoBehaviour
{
    CardUI cardUI;
    public CardSelection cardSelection;
    private void OnEnable()
    {
        cardUI = this.gameObject.GetComponent<CardUI>();
        if (cardUI == null)
            Debug.LogError("[CardUI_SelectCardScene] Start : cardUI cannot be null");
    }

    private void OnMouseUp()
    {
        Debug.Log("[CardUI_SelectCardScene] OnMouseUp");

        if ((cardSelection.CheckSelectValidation(cardUI.cardInfo)))
        {
            cardSelection.SelectCard(cardUI);

            this.gameObject.SetActive(false);
        }
    }
}
