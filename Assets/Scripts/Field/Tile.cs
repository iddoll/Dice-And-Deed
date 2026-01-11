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
        if (_renderer != null) 
        {
            // Додаємо прозорість (0.5f), щоб бачити плитку під кольором
            color.a = 0.5f; 
            _renderer.color = color;
        }
    }

    public void ResetColor()
    {
        _isHighlightedForMove = false;
        if (GridManager.Instance.currentPhase == GamePhase.Placement)
            SetDim(!GridManager.Instance.IsInPlacementZone(x, true));
        else
            SetDim(false);
    }

    // Додаємо цей метод, щоб ловити ПКМ
    private void OnMouseOver()
    {
        // Перевіряємо ПКМ (індекс 1)
        if (Input.GetMouseButtonDown(1))
        {
            if (GridManager.Instance.currentPhase == GamePhase.Placement)
            {
                // Викликаємо HandlePlacementClick з параметром mouseButton = 1
                GridManager.Instance.HandlePlacementClick(x, y, 1);
            }
        }
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
            else 
            {
                // Для ЛКМ передаємо mouseButton = 0
                GridManager.Instance.HandlePlacementClick(x, y, 0);
            }
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
        if (GridManager.Instance.currentPhase == GamePhase.Battle)
        {
            if (!TurnManager.Instance.isPlayerTurn) return;

            Unit unitOnTile = GridManager.Instance.GetUnitAtPosition(x, y);
            Unit selected = GridManager.Instance.SelectedUnit;

            // 1. Вибір свого юніта
            if (unitOnTile != null && unitOnTile.isPlayerUnit && unitOnTile.hasAction)
            {
                GridManager.Instance.SelectUnit(unitOnTile);
            }
            // 2. Дія підсвіченою клітинкою
            else if (selected != null && _isHighlightedForMove)
            {
                if (unitOnTile != null && !unitOnTile.isPlayerUnit)
                {
                    // Якщо на клітинці ворог — ЗАВЖДИ атакуємо (і в радіусі 1, і в радіусі 2)
                    GridManager.Instance.AttackWithSelectedUnit(x, y);
                }
                else if (unitOnTile == null)
                {
                    // Якщо клітинка порожня
                    int dist = Mathf.Max(Mathf.Abs(x - selected.xPosition), Mathf.Abs(y - selected.yPosition));
                
                    if (dist == 1) 
                        GridManager.Instance.MoveSelectedUnitTo(x, y); // Йдемо (тільки на 1 клітинку)
                    else 
                        Debug.Log("Сюди не можна йти, тільки стріляти, але ворога немає.");
                }
            }
        }
    }
}