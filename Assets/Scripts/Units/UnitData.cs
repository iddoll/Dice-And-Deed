using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitData", menuName = "TacticalGame/Unit Data")]
public class UnitData : ScriptableObject
{
    public string unitName;
    public int baseHealth;
    public int baseDamage;
    public Unit.Element element;
    public int classBonus;
    
    [TextArea]
    public string description;
    
    // Можна додати спрайт спеціально для цього класу
    public Sprite unitSprite;
}