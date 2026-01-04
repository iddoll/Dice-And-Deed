using UnityEngine;

[CreateAssetMenu(fileName = "Regeneration", menuName = "TacticalGame/Abilities/Regeneration")]
public class RegenerationAbility : UnitAbility
{
    public int healAmount = 20;

    public override void Execute(Unit owner, Unit target = null)
    {
        if (owner.curentHealth < owner.maxHealth)
        {
            owner.Heal(healAmount);
        }
    }
}