using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewUnitData", menuName = "TacticalGame/Unit Data")]
public class UnitData : ScriptableObject
{
    public string unitName;
    public Unit.Element element;
    public Unit.UnitClass unitClass;
    
    [Header("Base Stats")]
    public int baseHealth;
    public int baseDamage;
    public int classBonus; // Наприклад, +2 до урону воїнам свого класу

    [TextArea]
    public string description;

    public Sprite unitSprite;
    
    public List<UnitAbility> abilities;
}