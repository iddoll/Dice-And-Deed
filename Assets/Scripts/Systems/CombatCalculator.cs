using UnityEngine;

public static class CombatCalculator
{
    public static int CalculateDamage(Unit attacker, Unit defender)
    {
        float multiplier = 1.0f;

        // Логіка стихій з твого GDD
        switch (attacker.element)
        {
            case Unit.Element.Fire:
                if (defender.element == Unit.Element.Ice || defender.element == Unit.Element.Wood) multiplier = 1.1f;
                break;
            case Unit.Element.Ice:
                if (defender.element == Unit.Element.Lightning || defender.element == Unit.Element.Stone) multiplier = 1.1f;
                break;
            case Unit.Element.Lightning:
                if (defender.element == Unit.Element.Fire || defender.element == Unit.Element.Wood) multiplier = 1.1f;
                break;
            case Unit.Element.Wood:
                if (defender.element == Unit.Element.Stone || defender.element == Unit.Element.Ice) multiplier = 1.1f;
                break;
            case Unit.Element.Stone:
                if (defender.element == Unit.Element.Fire || defender.element == Unit.Element.Lightning) multiplier = 1.1f;
                break;
        }

        // Базовий розрахунок (Шкода + Бонус класу) * множник стихії
        int finalDamage = Mathf.RoundToInt((attacker.attackDamage + attacker.classBonus) * multiplier);
        return finalDamage;
    }

    public static void ApplyDamage(Unit attacker, Unit target, int damage)
    {
        if (target == null) return;

        // Передаємо відповідальність за отримання шкоди самому юніту
        target.TakeDamage(damage);

        string attackerName = (attacker != null) ? attacker.LogName : "Снаряд";
        Debug.Log($"{attackerName} наніс {target.LogName} [{damage}] шкоди. Залишилось: {target.curentHealth}");
    }
}