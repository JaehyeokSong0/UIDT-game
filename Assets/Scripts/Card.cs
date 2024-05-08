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

public enum MoveDirection // 3*3 grid에서의 index 기준
{
    Up = 1, 
    Left = 3,
    Right = 5,
    Down = 7, 
}

// [MEMO] 변수명 갱신 시 [FormerlySerializedAs()] 사용할 것
public class Card : MonoBehaviour
{
    [HideInInspector]
    public string cardName;

    public CardType cardType;
    public CharacterType characterType;

    public int value; // ATTACK, GUARD, RESTORE의 값
    public int energy;

    public MoveDirection moveDir;
    public bool[] attackAttr = new bool[9]; // CardType.Attack : 좌상단을 0으로 설정하여 0 ~ 8 까지의 공격 범위 설정
}


