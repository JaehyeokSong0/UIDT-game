using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    public Card CardInfo;

    [SerializeField]
    private GameObject[] _characterColor = new GameObject[5];
    [SerializeField]
    private Image[] _rangeCell = new Image[9];
    public TMP_Text CardName, Description, Value, Cost;

    private readonly Color DefaultCellColor = new Color32(210, 190, 160, 255);
    private readonly Color MoveCellColor = new Color32(125, 205, 140, 255);
    private readonly Color AttackCellColor = Color.red;
    private readonly Color GuardCellColor = new Color32(160, 150, 150, 255);
    private readonly Color RestoreCellColor = new Color32(90, 110, 200, 255);

    private void Start()
    {
        if (CardInfo != null)
            SetCardUI();
    }
    private void SetCardUI()
    {
        CardName.text = CardInfo.cardName;
        Description.text = SetDescription();
        Value.text = CardInfo.value.ToString();
        Cost.text = CardInfo.energy.ToString();

        // CharacterColor
        for (int i = 0; i < System.Enum.GetValues(typeof(CharacterType)).Length; i++) // Init 
            _characterColor[i].gameObject.SetActive(false);
        _characterColor[(int)CardInfo.characterType].gameObject.SetActive(true);

        // Range
        for (int i = 0; i < 9; i++) // Init
            PaintCell(i, DefaultCellColor);
        switch(CardInfo.cardType)
        {
            case CardType.Move:
                PaintCell((int)CardInfo.moveDir, MoveCellColor);
                break;
            case CardType.Attack:
                for (int i = 0; i < 9; i++)
                    if (CardInfo.attackAttr[i]) PaintCell(i, AttackCellColor);
                break;
            case CardType.Guard:
                PaintCell(4, GuardCellColor);
                break;
            case CardType.Restore:
                PaintCell(4, RestoreCellColor);
                break;
        }
    }

    private void PaintCell(int index, Color newColor)
    {
        _rangeCell[index].color = newColor;
    }

    private string SetDescription()
    {
        string description = CardInfo.characterType.ToString() + "\n" + CardInfo.cardType.ToString();

        return description;
    }

    public void SetCard(Card card)
    {
        CardInfo = card;

        SetCardUI();
    }
}
