using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Serialization;

public enum CARD_TYPE
{
    MOVE,
    ATTACK,
    GUARD,
    RESTORE
}

public enum MOVE_DIR // 3*3 grid������ index ����
{
    UP = 1, 
    LEFT = 3,
    RIGHT = 5,
    DOWN = 7, 
}

// [MEMO] ������ ���� �� [FormerlySerializedAs()] ����� ��
public class Card : MonoBehaviour
{
    [HideInInspector]
    public string cardName;

    public CARD_TYPE cardType;
    public CHARACTER_TYPE characterType;

    public int value; // ATTACK, GUARD, RESTORE�� ��
    public int energy;

    public MOVE_DIR moveDir;
    public bool[] attackAttr = new bool[9]; // CARD_TYPE.ATTACK : �»���� 0���� �����Ͽ� 0 ~ 8 ������ ���� ���� ����
}


