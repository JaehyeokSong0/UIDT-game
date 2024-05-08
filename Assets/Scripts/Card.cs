using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Serialization;

public enum CardType
{
    Move,
    Attack,
    Guard,
    Restore
}

public enum MoveDirection // 3*3 grid������ index ����
{
    Up = 1, 
    Left = 3,
    Right = 5,
    Down = 7, 
}

// [MEMO] ������ ���� �� [FormerlySerializedAs()] ����� ��
public class Card : MonoBehaviour
{
    [HideInInspector]
    public string cardName;

    public CardType cardType;
    public CharacterType characterType;

    public int value; // ATTACK, GUARD, RESTORE�� ��
    public int energy;

    public MoveDirection moveDir;
    public bool[] attackAttr = new bool[9]; // CardType.Attack : �»���� 0���� �����Ͽ� 0 ~ 8 ������ ���� ���� ����
}


