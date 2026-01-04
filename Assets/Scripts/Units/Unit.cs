using UnityEngine;

public class Unit : MonoBehaviour
{
    // Твої існуючі поля
    public enum Element { None, Fire, Water, Earth, Air }
    public Element element;
    public string unitName;
    public int Health;
    public int ClassBonus;
    public bool isAlive = true;
    
    // Нові поля для системи
    public bool isPlayerUnit;
    public bool hasAction = true;
    public int xPosition;
    public int yPosition;

    private SpriteRenderer _sr;
    private Color _baseColor;

    // Метод ініціалізації через дані
    public void Setup(UnitData data, bool isPlayer)
    {
        unitName = data.unitName;
        Health = data.baseHealth;
        element = data.element;
        ClassBonus = data.classBonus;
        isPlayerUnit = isPlayer;
        
        if (data.unitSprite != null)
            GetComponent<SpriteRenderer>().sprite = data.unitSprite;
            
        InitBaseColor();
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
        // Затемнення замість відбілювання
        _sr.color = active ? _baseColor : new Color(_baseColor.r * 0.5f, _baseColor.g * 0.5f, _baseColor.b * 0.5f, 1f);
    }

    public bool IsDead() => Health <= 0;
}