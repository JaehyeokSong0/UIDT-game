using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    public Card cardInfo;

    public GameObject[] characterColor = new GameObject[5];
    public Image[] rangeCell = new Image[9];
    public TMP_Text cardName;
    public TMP_Text description;
    public TMP_Text value;
    public TMP_Text cost;

    private readonly Color defaultCellColor = new Color32(210, 190, 160, 255);
    private readonly Color moveCellColor = new Color32(125, 205, 140, 255);
    private readonly Color attackCellColor = Color.red;
    private readonly Color guardCellColor = new Color32(160, 150, 150, 255);
    private readonly Color restoreCellColor = new Color32(90, 110, 200, 255);

    private void Start()
    {
        if (cardInfo != null)
            SetCardUI();
    }
    private void SetCardUI()
    {
        Debug.Log("[CardUI] SetCardUI : " + cardInfo.cardName);

        cardName.text = cardInfo.cardName;
        description.text = SetDescription();
        value.text = cardInfo.value.ToString();
        cost.text = cardInfo.energy.ToString();

        // CharacterColor
        for (int i = 0; i < System.Enum.GetValues(typeof(CHARACTER_TYPE)).Length; i++) // Init 
            characterColor[i].gameObject.SetActive(false);
        characterColor[(int)cardInfo.characterType].gameObject.SetActive(true);

        // Range
        for (int i = 0; i < 9; i++) // Init
            PaintCell(i, defaultCellColor);
        switch(cardInfo.cardType)
        {
            case CARD_TYPE.MOVE:
                PaintCell((int)cardInfo.moveDir, moveCellColor);
                break;
            case CARD_TYPE.ATTACK:
                for (int i = 0; i < 9; i++)
                    if (cardInfo.attackAttr[i]) PaintCell(i, attackCellColor);
                break;
            case CARD_TYPE.GUARD:
                PaintCell(4, guardCellColor);
                break;
            case CARD_TYPE.RESTORE:
                PaintCell(4, restoreCellColor);
                break;
        }
    }

    private void PaintCell(int index, Color newColor)
    {
        rangeCell[index].color = newColor;
    }

    private string SetDescription()
    {
        string description = cardInfo.characterType.ToString() + "\n" + cardInfo.cardType.ToString();

        return description;
    }

    public void SetCard(Card card)
    {
        cardInfo = card;

        SetCardUI();
    }
}
