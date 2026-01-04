using UnityEngine;

public class Tile : MonoBehaviour
{
    private SpriteRenderer _renderer;
    private Color _defaultColor;
    private bool _isHighlightedForMove;
    
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
        // Підсвічуємо лише якщо клітинка не чекає на хід
        if (!_isHighlightedForMove) _renderer.color = highlightColor;
    }
    
    private void OnMouseExit() 
    {
        if (!_isHighlightedForMove) _renderer.color = _defaultColor;
    }

    public void SetHighlightColor(Color color)
    {
        _isHighlightedForMove = true;
        _renderer.color = color;
    }

    public void ResetColor()
    {
        _isHighlightedForMove = false;
        _renderer.color = _defaultColor;
    }

    private void OnMouseDown()
    {
        // Якщо зараз НЕ хід гравця — ігноруємо будь-які кліки по полю
        if (!TurnManager.Instance.isPlayerTurn) return;

        Unit unitOnMe = GridManager.Instance.GetUnitAtPosition(x, y);

        if (GridManager.Instance.SelectedUnit != null)
        {
            if (GridManager.Instance.IsTileHighlightedForMove(x, y))
            {
                if (unitOnMe == null)
                    GridManager.Instance.MoveSelectedUnitTo(x, y);
                else if (unitOnMe.isPlayerUnit != GridManager.Instance.SelectedUnit.isPlayerUnit)
                    GridManager.Instance.AttackWithSelectedUnit(x, y);
            }
            else GridManager.Instance.ClearSelection();
        }
        // Гравець може обирати ТІЛЬКИ своїх юнітів
        else if (unitOnMe != null && unitOnMe.isPlayerUnit && unitOnMe.hasAction)
        {
            GridManager.Instance.SelectUnit(unitOnMe);
        }
    }
}