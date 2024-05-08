using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public CharacterType characterType;
    public CharacterSelection SelectSceneManager;
    private void OnMouseUp()
    {
        if(SelectSceneManager.currUI == null)
            SelectSceneManager.SelectCharacter_UI(characterType);
    }
}
