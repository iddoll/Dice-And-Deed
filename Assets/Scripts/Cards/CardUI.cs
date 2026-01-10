using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUI : MonoBehaviour
{
    private CardData _data;

    [Header("UI References")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI manaText;
    public Image cardArt;
    public Image background; // Щоб змінювати колір за стихією

    public void Setup(CardData data)
    {
        _data = data;
        nameText.text = data.cardName;
        descriptionText.text = data.description;
        manaText.text = data.manaCost.ToString();
        
        if (cardArt != null) cardArt.sprite = data.cardArt;

        // Змінюємо колір рамки залежно від елемента
        SetCardColor(data.element);
    }

    private void SetCardColor(Unit.Element element)
    {
        if (background == null) return; // Захист від помилки

        switch (element)
        {
            case Unit.Element.Fire: background.color = new Color(0.8f, 0.2f, 0.2f, 1f); break;
            case Unit.Element.Ice: background.color = new Color(0.2f, 0.6f, 0.8f, 1f); break;
            case Unit.Element.Lightning: background.color = new Color(0.8f, 0.8f, 0.2f, 1f); break;
            case Unit.Element.Wood: background.color = new Color(0.2f, 0.8f, 0.2f, 1f); break;
            case Unit.Element.Stone: background.color = new Color(0.5f, 0.5f, 0.5f, 1f); break;
            default: background.color = new Color(0.3f, 0.3f, 0.3f, 1f); break;
        }
    }

    public void OnClick()
    {
        // Перевіряємо ману перед використанням
        if (GridManager.Instance.SpendMana(_data.manaCost))
        {
            Debug.Log($"Використано карту: {_data.cardName}");
            // Тут ми пізніше додамо логіку "Виберіть ціль"
            Destroy(gameObject); // Карта зникає з руки
        }
        else
        {
            Debug.Log("Недостатньо мани!");
            // Можна додати візуальний ефект "хитання" карти
        }
    }
}