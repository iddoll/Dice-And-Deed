using UnityEngine;

public enum AbilityTrigger { OnSpawn, OnAttack, OnTurnStart, OnTakeDamage }

public abstract class UnitAbility : ScriptableObject
{
    public string abilityName;
    [TextArea] public string description;
    public AbilityTrigger trigger;

    // Метод, який буде виконувати логіку бонусу
    public abstract void Execute(Unit owner, Unit target = null);
}