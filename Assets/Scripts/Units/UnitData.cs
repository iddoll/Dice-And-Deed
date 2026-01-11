using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewUnitData", menuName = "TacticalGame/Unit Data")]
public class UnitData : ScriptableObject
{
    public string unitName;
    public Unit.Element element;
    public Unit.UnitClass unitClass;
    
    [Header("Deck Settings")]
    // Цей список виправить помилку в GridManager
    public List<CardData> availableCards;
    
    [Header("Visuals & UI")]
    public GameObject projectilePrefab; // Сюди перетягніть префаб вогняної кулі/льоду
    public int attackRange = 3;         // Дальність стрільби
    public int attackDamage = 20;
    public GameObject damageTextPrefab;
    public GameObject healthBarPrefab;
    public Sprite unitSprite;
    
    [Header("Base Stats")]
    public int baseHealth;
    public int baseDamage;
    public int classBonus; // Наприклад, +2 до урону воїнам свого класу
    public int armor = 5; // Зменшує вхідний урон на фіксоване число
    
    [Header("Elemental Balance")]
    [Range(0, 1)] 
    public float elementalResist = 0f; // 1.0 = повний ігнор стихійного бонусу (для Танків)
    
    [TextArea]
    public string description;
    
    public List<UnitAbility> abilities;
}