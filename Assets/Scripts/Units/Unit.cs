using UnityEngine;
using System.Collections.Generic;

public class Unit : MonoBehaviour
{
    public UnitData unitData;
    public enum Element { None, Fire, Ice, Lightning, Wood, Stone }
    public enum UnitClass { Tank, Archer, Mage, Assassin }

    [Header("Unit Profile")]
    public string unitName;
    public Element element;
    public UnitClass unitClass;
    [TextArea] public string description;
    public int unitID;
    
    [Header("Stats")]
    public int maxHealth;
    public int curentHealth;
    public int attackDamage;
    public int classBonus;
    public bool isAlive = true;

    [Header("Abilities")]
    public List<UnitAbility> activeAbilities = new List<UnitAbility>();
    
    [Header("Turn System")]
    public bool isPlayerUnit;
    public bool hasAction = true;
    public int xPosition;
    public int yPosition;

    private SpriteRenderer _sr;
    private Color _baseColor;

    public string LogName => $"[{unitID}]{unitName}";
    public void Setup(UnitData data, bool isPlayer)
    {
        unitData = data;
        unitName = data.unitName;
        element = data.element;
        unitClass = data.unitClass;
        description = data.description;
        
        maxHealth = data.baseHealth;
        curentHealth = data.baseHealth;
        attackDamage = data.baseDamage;
        classBonus = data.classBonus;
        
        isPlayerUnit = isPlayer;

        if (_sr == null) _sr = GetComponent<SpriteRenderer>();
        if (data.unitSprite != null) _sr.sprite = data.unitSprite;

        InitBaseColor();
        
        ExecuteAbilities(AbilityTrigger.OnSpawn);
    }

    public void ExecuteAbilities(AbilityTrigger triggerType, Unit target = null)
    {
        foreach (var ability in activeAbilities)
        {
            if (ability.trigger == triggerType)
            {
                ability.Execute(this, target);
            }
        }
    }
    
    public void InitBaseColor()
    {
        if (_sr == null) _sr = GetComponent<SpriteRenderer>();
        if (_sr != null) _baseColor = _sr.color;
    }

    public void SetState(bool active)
    {
        hasAction = active;
        if (_sr == null) _sr = GetComponent<SpriteRenderer>();
        if (_sr != null)
        {
            _sr.color = active ? _baseColor : new Color(_baseColor.r * 0.5f, _baseColor.g * 0.5f, _baseColor.b * 0.5f, 1f);
        }
    }

    // Метод для лікування (з перевіркою ліміту)
    public void Heal(int amount)
    {
        curentHealth += amount;
        if (curentHealth > maxHealth) curentHealth = maxHealth;
        Debug.Log($"{unitName} відновив здоров'я. Поточне HP: {curentHealth}");
    }
    
    public bool IsDead() => curentHealth <= 0;
}