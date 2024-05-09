using UnityEngine;

// SelectCardScene에서 Selected CardUI에 추가로 붙이는 컴포넌트
// Mouse event 등을 관리
public class SelectedCardUI_SelectCardScene : MonoBehaviour
{
    private CardUI _cardUI;
    [SerializeField]
    private CardSelection _cardSelection;

    private void OnEnable()
    {
        _cardUI = this.gameObject.GetComponent<CardUI>();
        if (_cardUI == null)
            Debug.LogError("[SelectedCardUI_SelectCardScene] Start : cardUI cannot be null");
    }

    private void Start()
    {
        if (_cardSelection == null)
        {
            _cardSelection = GameObject.FindObjectOfType<CardSelection>();
            if (_cardSelection == null)
                Debug.LogError("[SelectedCardUI_SelectCardScene] Cannot find CardSelection");
        }
    }
    private void OnMouseUp()
    {
        Debug.Log("[SelectedCardUI_SelectCardScene] OnMouseUp");

        _cardSelection.DeselectCard(_cardUI);
    }
}
