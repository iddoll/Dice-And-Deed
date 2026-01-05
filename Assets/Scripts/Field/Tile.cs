using UnityEngine;

public class Tile : MonoBehaviour
{
    private SpriteRenderer _renderer;
    private Color _originalColor; 
    private bool _isHighlightedForMove;
    private static Tile _draggingSource; 

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

    // Метод для затемнення/освітлення
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
        
        // Повертаємо колір з урахуванням фази
        if (GridManager.Instance.currentPhase == GamePhase.Placement)
        {
            bool shouldBeDim = !GridManager.Instance.IsInPlacementZone(x, true);
            SetDim(shouldBeDim);
        }
        else
        {
            SetDim(false);
        }
    }

    private void OnMouseEnter() 
    {
        if (!_isHighlightedForMove && _renderer != null) _renderer.color = hoverColor;
    }
    
    private void OnMouseExit() 
    {
        if (!_isHighlightedForMove) ResetColor();
    }

    private void OnMouseDown()
    {
        if (GridManager.Instance.currentPhase == GamePhase.Placement)
        {
            Unit unitOnMe = GridManager.Instance.GetUnitAtPosition(x, y);
            if (unitOnMe != null && unitOnMe.isPlayerUnit)
            {
                _draggingSource = this;
                _renderer.color = Color.yellow; // Візуальний відгук
            }
            else 
            {
                GridManager.Instance.HandlePlacementClick(x, y);
            }
        }
        else 
        {
            HandleLeftClick();
        }
    }

    private void OnMouseUp()
    {
        if (GridManager.Instance.currentPhase == GamePhase.Placement && _draggingSource != null)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            
            if (hit.collider != null)
            {
                Tile targetTile = hit.collider.GetComponent<Tile>();
                if (targetTile != null) 
                {
                    GridManager.Instance.SwapOrMoveUnits(_draggingSource, targetTile);
                }
            }
            _draggingSource.ResetColor();
            _draggingSource = null;
        }
    }

    private void HandleLeftClick() 
    {
        if (!TurnManager.Instance.isPlayerTurn) return;
        Unit unitOnMe = GridManager.Instance.GetUnitAtPosition(x, y);

        if (GridManager.Instance.SelectedUnit != null)
        {
            if (GridManager.Instance.IsTileHighlightedForMove(x, y))
            {
                if (unitOnMe == null) GridManager.Instance.MoveSelectedUnitTo(x, y);
                else if (unitOnMe.isPlayerUnit != GridManager.Instance.SelectedUnit.isPlayerUnit)
                    GridManager.Instance.AttackWithSelectedUnit(x, y);
            }
            else GridManager.Instance.ClearSelection();
        }
        else if (unitOnMe != null && unitOnMe.isPlayerUnit && unitOnMe.hasAction)
            GridManager.Instance.SelectUnit(unitOnMe);
    }
}