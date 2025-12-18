using UnityEngine;

public class Tile : MonoBehaviour
{
    private SpriteRenderer _renderer;
    private Color _defaultColor;

    public Color highlightColor = Color.yellow; // Колір підсвічування (можна змінити в інспекторі)
    public int x;
    public int y;

    void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        // Запам'ятовуємо початковий колір (зазвичай це білий)
        _defaultColor = _renderer.color;
        Debug.Log($"Тайл ініціалізовано на координатах: {x}, {y}");
    }

    private void OnMouseEnter() 
    {
        // Змінюємо колір на колір підсвічування
        _renderer.color = highlightColor;
    }
    
    private void OnMouseExit() 
    {
        // Повертаємо початковий колір
        _renderer.color = _defaultColor;
    }

    private void OnMouseDown()
    {
        // Викликаємо метод менеджера, щоб дізнатися, хто тут стоїть
        Unit unitOnMe = GridManager.Instance.GetUnitAtPosition(x, y);

        if (unitOnMe != null)
        {
            Debug.Log($"[{x}, {y}] Тут стоїть: {unitOnMe.unitName} (HP: {unitOnMe.Health})");
        }
        else
        {
            Debug.Log($"[{x}, {y}] Клітинка порожня");
        }
    }
}