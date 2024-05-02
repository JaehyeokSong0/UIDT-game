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

public enum MOVE_DIR // 3*3 grid에서의 index 기준
{
    UP = 1, 
    LEFT = 3,
    RIGHT = 5,
    DOWN = 7, 
}

// [MEMO] 변수명 갱신 시 [FormerlySerializedAs()] 사용할 것
public class Card : MonoBehaviour
{
    [HideInInspector]
    public string cardName;

    public CARD_TYPE cardType;
    public CHARACTER_TYPE characterType;

    public int value; // ATTACK, GUARD, RESTORE의 값
    public int energy;

    public MOVE_DIR moveDir;
    public bool[] attackAttr = new bool[9]; // CARD_TYPE.ATTACK : 좌상단을 0으로 설정하여 0 ~ 8 까지의 공격 범위 설정
}


