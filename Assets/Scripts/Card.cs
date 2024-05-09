using UnityEngine;

public enum CardType
{
    Move,
    Attack,
    Guard,
    Restore
}

public enum MoveDirection 
// 0 1 2
// 3 4 5
// 6 7 8
{
    Up = 1, 
    Left = 3,
    Right = 5,
    Down = 7, 
}

public class Card : MonoBehaviour 
// Exceptionally non-compliant with coding standards
{
    [HideInInspector]
    public string cardName;

    public CardType cardType;
    public CharacterType characterType;

    public int value; // Attack, Guard, Restore
    public int energy;

    public MoveDirection moveDir;
    public bool[] attackAttr = new bool[9]; // Range : 0 ~ 8 (Left - Up : 0)
}


