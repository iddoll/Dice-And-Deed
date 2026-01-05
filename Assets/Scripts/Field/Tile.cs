using UnityEngine;

public class Tile : MonoBehaviour
{
    private SpriteRenderer _renderer;
    private Color _originalColor; 
    private bool _isHighlightedForMove;
    private static Tile _draggingSource; 
    private Vector3 _startDragPosition; // Початкова позиція юніта

    public Color hoverColor = Color.cyan; 
    public int x;
    public int y;

    void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        if (_renderer != null) _originalColor = _renderer.color;
    }

    // Метод для зміни базового кольору (викликається з GridManager)
    public void SetBaseColor(Color color)
    {
        _originalColor = color;
        if (!_isHighlightedForMove) _renderer.color = color;
    }
    
    public void SetDim(bool dim)
    {
        if (_renderer == null) _renderer = GetComponent<SpriteRenderer>();
        _renderer.color = dim ? _originalColor * 0.4f : _originalColor;
    }

    public void SetHighlightColor(Color color)
    {
        _isHighlightedForMove = true;
        if (_renderer != null) _renderer.color = color;
    }

    public void ResetColor()
    {
        _isHighlightedForMove = false;
        if (GridManager.Instance.currentPhase == GamePhase.Placement)
            SetDim(!GridManager.Instance.IsInPlacementZone(x, true));
        else
            SetDim(false);
    }

    private void OnMouseDown()
    {
        if (GridManager.Instance.currentPhase == GamePhase.Placement)
        {
            Unit unitOnMe = GridManager.Instance.GetUnitAtPosition(x, y);
            if (unitOnMe != null && unitOnMe.isPlayerUnit)
            {
                _draggingSource = this;
                _startDragPosition = unitOnMe.transform.position;
                _renderer.color = Color.yellow;
            }
            else GridManager.Instance.HandlePlacementClick(x, y);
        }
        else HandleLeftClick();
    }

    private void OnMouseDrag()
    {
        if (GridManager.Instance.currentPhase == GamePhase.Placement && _draggingSource == this)
        {
            Unit unitToDrag = GridManager.Instance.GetUnitAtPosition(x, y);
            if (unitToDrag != null)
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                unitToDrag.transform.position = new Vector3(mousePos.x, mousePos.y, -0.5f);
            }
        }
    }

    private void OnMouseUp()
    {
        if (GridManager.Instance.currentPhase == GamePhase.Placement && _draggingSource == this)
        {
            Unit draggedUnit = GridManager.Instance.GetUnitAtPosition(x, y);
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            
            bool success = false;
            if (hit.collider != null)
            {
                Tile targetTile = hit.collider.GetComponent<Tile>();
                if (targetTile != null && GridManager.Instance.IsInPlacementZone(targetTile.x, true))
                {
                    GridManager.Instance.SwapOrMoveUnits(_draggingSource, targetTile);
                    success = true;
                }
            }

            // Якщо не влучили в тайл або зона ворожа — повертаємо юніта назад
            if (!success && draggedUnit != null)
                draggedUnit.transform.position = _startDragPosition;

            _draggingSource.ResetColor();
            _draggingSource = null;
        }
    }

    // Решта методів (OnMouseEnter, HandleLeftClick) залишаються без змін
    private void OnMouseEnter() { if (!_isHighlightedForMove && _renderer != null) _renderer.color = hoverColor; }
    private void OnMouseExit() { if (!_isHighlightedForMove) ResetColor(); }
    private void HandleLeftClick()
    {
        // Якщо фаза БОЮ
        if (GridManager.Instance.currentPhase == GamePhase.Battle)
        {
            // Дозволяємо виділяти юніта ТІЛЬКИ якщо зараз хід гравця
            if (!TurnManager.Instance.isPlayerTurn) return;
    
            Unit unitOnTile = GridManager.Instance.GetUnitAtPosition(x, y);
            Unit selected = GridManager.Instance.SelectedUnit;
    
            if (unitOnTile != null && unitOnTile.isPlayerUnit && unitOnTile.hasAction)
            {
                GridManager.Instance.SelectUnit(unitOnTile);
            }
            else if (selected != null && GridManager.Instance.IsTileHighlightedForMove(x, y))
            {
                if (unitOnTile != null && !unitOnTile.isPlayerUnit)
                    GridManager.Instance.AttackWithSelectedUnit(x, y);
                else
                    GridManager.Instance.MoveSelectedUnitTo(x, y);
            }
        }
    }
}