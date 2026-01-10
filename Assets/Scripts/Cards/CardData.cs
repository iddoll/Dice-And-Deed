using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "TacticalGame/Card Data")]
public class CardData : ScriptableObject
{
    public string cardName;
    public Unit.Element element; // Стихія карти
    public int manaCost;
    public Sprite cardArt;
    [TextArea] public string description;

    [Header("Effect Settings")]
    public int baseValue; // Базовий урон або лікування
    public float diceMultiplier; // Наскільки сильно кубик впливає на результат
    
    public CardEffectType effectType;
}

public enum CardEffectType
{
    DamageSingle,   // Урон по одному ворогу
    DamageArea,     // Урон по площі
    HealSingle,     // Лікування свого
    Freeze,         // Заморозка
    Shield,         // Додавання броні
    BuffDamage      // Посилення атаки
}