using UnityEngine;

public static class DiceSystem
{
    public static int Roll()
    {
        int result = Random.Range(1, 7); // Від 1 до 6
        Debug.Log($"🎲 Випало на кубику: {result}");
        return result;
    }

    // Розрахунок значення карти з урахуванням кубика
    public static int CalculateEffectValue(CardData card, int diceResult)
    {
        // Формула: Базове значення + (Результат кубика * Множник)
        return Mathf.RoundToInt(card.baseValue + (diceResult * card.diceMultiplier));
    }
}