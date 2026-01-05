using UnityEngine;

public class Tile : MonoBehaviour
{
    private SpriteRenderer _renderer;
    private Color _defaultColor;
    private bool _isHighlightedForMove;
    
    public Color hoverColor = Color.cyan; 
    public int x;
    public int y;

    void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _defaultColor = _renderer.color;
    }
    
    private void OnMouseEnter() 
    {
        if (!_isHighlightedForMove) _renderer.color = hoverColor;
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
        // ЛІВИЙ КЛІК
        HandleLeftClick();
    }

    private void OnMouseOver()
    {
        // ПРАВИЙ КЛІК (Видалення під час розстановки)
        if (Input.GetMouseButtonDown(1)) 
        {
            if (GridManager.Instance.currentPhase == GamePhase.Placement)
            {
                Unit unitOnMe = GridManager.Instance.GetUnitAtPosition(x, y);
                if (unitOnMe != null && unitOnMe.isPlayerUnit)
                {
                    GridManager.Instance.RemoveUnitAndRefund(unitOnMe, x, y);
                }
            }
        }
    }

    private void HandleLeftClick()
    {
        // 1. ФАЗА РОЗСТАНОВКИ
        if (GridManager.Instance.currentPhase == GamePhase.Placement)
        {
            // Викликаємо універсальний метод менеджера (він сам розбереться: ставити чи рухати)
            GridManager.Instance.HandlePlacementClick(x, y);
            return;
        }

        // 2. ФАЗА БОЮ
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
        else if (unitOnMe != null && unitOnMe.isPlayerUnit && unitOnMe.hasAction)
        {
            GridManager.Instance.SelectUnit(unitOnMe);
        }
    }
}