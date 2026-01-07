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

    [Header("UI")]
    public GameObject healthBarPrefab;
    private HealthBar _hpBar;
    
    [Header("VFX")]
    public GameObject damageTextPrefab; // Призначте префаб DamageTextCanvas в інспекторі
    
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

        // Автоматично розвертаємо ворогів обличчям до гравця
        if (_sr == null) _sr = GetComponent<SpriteRenderer>();
        _sr.flipX = !isPlayer; 
    
        if (data.unitSprite != null) _sr.sprite = data.unitSprite;

        // Створюємо HP Bar
        if (data.healthBarPrefab != null)
        {
            GameObject hbGO = Instantiate(data.healthBarPrefab, transform);
            hbGO.transform.localPosition = new Vector3(0, 1.1f, 0); // Трохи вище
            _hpBar = hbGO.GetComponent<HealthBar>();
            if (_hpBar != null) _hpBar.Setup(this);
        }

        InitBaseColor();
        // ExecuteAbilities(AbilityTrigger.OnSpawn); // Розкоментуй, якщо готово
    }

    private void LateUpdate()
    {
        if (_sr != null)
        {
            // Множимо Y на -100. 
            // Чим нижче маг (менше Y), тим більше число Order in Layer він отримає.
            // Наприклад: Y = -1 => Order = 100. Y = -2 => Order = 200.
            _sr.sortingOrder = Mathf.RoundToInt(transform.position.y * -100f);
        }
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
    
    public void TakeDamage(int damage)
    {
        curentHealth -= damage;
        if (curentHealth < 0) curentHealth = 0;

        if (_hpBar != null) _hpBar.UpdateHealthBar();

        // СПАВН ЦИФР ШКОДИ З UNIT DATA
        if (unitData != null && unitData.damageTextPrefab != null)
        {
            // Додаємо випадковий зсув по горизонталі (від -0.3 до 0.3)
            float randomX = Random.Range(-0.3f, 0.3f);
            Vector3 spawnPos = transform.position + new Vector3(randomX, 1.6f, 0);
        
            GameObject dtGO = Instantiate(unitData.damageTextPrefab, spawnPos, Quaternion.identity);
        
            DamageText dtScript = dtGO.GetComponent<DamageText>();
            if (dtScript != null) 
            {
                dtScript.Setup(damage);
            }
        }

        if (IsDead())
        {
            isAlive = false;
            // Тут можна додати ефект смерті
        }
    }
    
    public bool IsDead() => curentHealth <= 0;
}