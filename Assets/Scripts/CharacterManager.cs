using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public CHARACTER_TYPE character;
    public CharacterSelection SelectSceneManager;
    private void OnMouseUp()
    {
        if(SelectSceneManager.currUI == null)
            SelectSceneManager.SelectCharacter_UI(character);
    }
}
