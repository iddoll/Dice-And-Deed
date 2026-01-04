using UnityEngine;

[CreateAssetMenu(fileName = "ElementBoost", menuName = "TacticalGame/Abilities/ElementBoost")]
public class ElementBoostAbility : UnitAbility
{
    public int attackBonus = 20;

    public override void Execute(Unit owner, Unit target = null)
    {
        // Шукаємо всіх юнітів на полі через GridManager
        var allUnits = GridManager.Instance.GetAllUnits();
        
        foreach (var unit in allUnits)
        {
            // Якщо це союзник і у нього та ж стихія (але не сам власник бонусу)
            if (unit.isPlayerUnit == owner.isPlayerUnit && 
                unit.element == owner.element && 
                unit != owner)
            {
                unit.attackDamage += attackBonus;
                Debug.Log($"Бонус {abilityName}: {unit.unitName} отримав +{attackBonus} до атаки!");
            }
        }
    }
}