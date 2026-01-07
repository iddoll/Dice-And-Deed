using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewUnitData", menuName = "TacticalGame/Unit Data")]
public class UnitData : ScriptableObject
{
    public string unitName;
    public Unit.Element element;
    public Unit.UnitClass unitClass;
    
    [Header("Projectiles")]
    public GameObject projectilePrefab; // Сюди перетягніть префаб вогняної кулі/льоду
    public int attackRange = 3;         // Дальність стрільби
    public int attackDamage = 20;
    
    [Header("Base Stats")]
    public int baseHealth;
    public int baseDamage;
    public int classBonus; // Наприклад, +2 до урону воїнам свого класу
    public GameObject healthBarPrefab;
    
    [TextArea]
    public string description;

    public Sprite unitSprite;
    
    public List<UnitAbility> abilities;
}