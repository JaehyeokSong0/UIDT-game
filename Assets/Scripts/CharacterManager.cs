using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    [SerializeField]
    private CharacterType _characterType;
    private CharacterSelection _characterSelection;

    private void Start()
    {
        if (_characterSelection == null)
        {
            _characterSelection = GameObject.FindObjectOfType<CharacterSelection>();
            if (_characterSelection == null)
                Debug.LogError("[CharacterManager] Cannot find CharacterSelection");
        }
    }

    private void OnMouseUp()
    {
        if(_characterSelection.CurrUI == null)
            _characterSelection.SelectCharacter_UI(_characterType);
    }
}
