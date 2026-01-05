using System.Collections.Generic;
using UnityEngine;

public enum GamePhase { Placement, Battle }

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;
    
    [Header("Game State")]
    public GamePhase currentPhase = GamePhase.Placement;
    private int _unitsPlaced = 0;
    private const int MaxUnits = 5;

    [Header("Prefabs & Tiles")]
    public GameObject playerTilePrefab;
    public GameObject commonTilePrefab;
    public GameObject enemyTilePrefab;

    [Header("Grid Dimensions")]
    public int playerColumns = 2;   
    public int commonColumns = 1;   
    public int enemyColumns = 2;    
    public int rows = 5;            

    [Header("Spacing")]
    public float columnSpacing = 2.5f; 
    public Vector2 rowStep = new Vector2(0.5f, 1.5f);

    [Header("Placement Phase")]
    public List<UnitData> availablePlayerUnits; // Заповнити в інспекторі (5 штук)
    public List<UnitData> availableEnemyUnits;  // Заповнити в інспекторі
    private int _currentUnitIndex = 0;

    [Header("Selection & Combat")]
    public Color moveRangeColor = new Color(0, 1, 0, 0.5f); 
    public Color attackRangeColor = new Color(1, 0, 0, 0.6f);
    private Unit _selectedUnit;
    private List<Tile> _highlightedTiles = new List<Tile>();
    
    private Unit[,] _unitsOnGrid;
    private Tile[,] _tilesOnGrid; 
    private int _totalColumns;

    private int _globalUnitCount = 1;
    public Unit SelectedUnit => _selectedUnit;

    [Header("UI Elements")]
    public GameObject startBattleButton;
    public GameObject endTurnButton;
    
    void Awake()
    {
        Instance = this;
        _totalColumns = playerColumns + commonColumns + enemyColumns;
        _unitsOnGrid = new Unit[_totalColumns, rows];
        _tilesOnGrid = new Tile[_totalColumns, rows];
    }

    void Start()
    {
        GenerateVisualGrid();
    }

    private void GenerateVisualGrid()
    {
        float midCol = playerColumns + (commonColumns - 1) / 2f;
        float midRow = (rows - 1) / 2f;
    
        for (int x = 0; x < _totalColumns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                GameObject prefab = GetPrefabByColumn(x);
                Vector3 pos = CalculatePosition(x, y, midCol, midRow);
                GameObject tileGO = Instantiate(prefab, pos, prefab.transform.rotation, transform);
            
                Tile tileScript = tileGO.GetComponent<Tile>();
                if (tileScript != null)
                {
                    tileScript.x = x;
                    tileScript.y = y;
                    _tilesOnGrid[x, y] = tileScript;
                    
                    // --- НОВЕ: Затемнюємо все, що не є зоною гравця ---
                    bool isNotPlayerZone = !IsInPlacementZone(x, true);
                    tileScript.SetDim(isNotPlayerZone);
                }
            }
        }
    }
    
    public void StartBattlePhase()
    {
        // Отримуємо реальну кількість юнітів на полі
        int realUnitCount = GetAllUnits().Count;
        
        // Якщо на полі немає жодного юніта — не пускаємо в бій
        if (realUnitCount == 0) 
        {
            Debug.LogWarning("Неможливо почати бій: виставте хоча б одного юніта!");
            return;
        }
        
        if (GameManager.Instance == null) 
        {
            Debug.LogError("GameManager не знайдено на сцені!");
            return; 
        }
    
        currentPhase = GamePhase.Battle;
        
        // Повертаємо яскравість усім тайлам (як на image_c6e040.jpg)
        foreach (var tile in _tilesOnGrid)
        {
            if (tile != null) tile.SetDim(false);
        }
    
        SpawnEnemyUnitsRandomly();
        
        // Ховаємо кнопку старту і показуємо кнопку завершення ходу
        if(startBattleButton != null) startBattleButton.SetActive(false);
        if(endTurnButton != null) endTurnButton.SetActive(true);
    
        GameManager.Instance.OnBattleStarted();
        Debug.Log("Бій розпочато!");
    }

    public void ResetAllTilesToDefault()
    {
        foreach (Tile t in _tilesOnGrid)
        {
            t.SetBaseColor(Color.white);
            t.ResetColor();
        }
    }
    // --- Placement Logic ---

    public bool IsInPlacementZone(int x, bool isPlayer)
    {
        if (isPlayer) return x < playerColumns; 
        return x >= (playerColumns + commonColumns); 
    }

    public UnitData GetNextAvailableUnit()
    {
        if (_currentUnitIndex < availablePlayerUnits.Count)
            return availablePlayerUnits[_currentUnitIndex];
        return null;
    }
    
    public void RemoveUnitAndRefund(Unit unit, int x, int y)
    {
        _unitsOnGrid[x, y] = null;
        Destroy(unit.gameObject);
        _unitsPlaced--;
        _currentUnitIndex--; // Повертаємо можливість поставити цього ж юніта
        Debug.Log("Юніта видалено. Поставте його знову.");
    }

    public void SpawnEnemyUnitsRandomly()
    {
        int enemiesPlaced = 0;
        while (enemiesPlaced < availableEnemyUnits.Count && enemiesPlaced < MaxUnits)
        {
            int randomX = Random.Range(playerColumns + commonColumns, _totalColumns);
            int randomY = Random.Range(0, rows);

            if (GetUnitAtPosition(randomX, randomY) == null)
            {
                SpawnUnit(availableEnemyUnits[enemiesPlaced], randomX, randomY, false);
                enemiesPlaced++;
            }
        }
    }

    public void SpawnUnit(UnitData data, int x, int y, bool isPlayer)
    {
        GameObject go = new GameObject(data.unitName + (isPlayer ? "_Player" : "_Enemy"));
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = data.unitSprite != null ? data.unitSprite : playerTilePrefab.GetComponent<SpriteRenderer>().sprite;
        sr.color = isPlayer ? Color.white : new Color(1, 0.7f, 0.7f); // Вороги трохи червоніші
        sr.sortingOrder = 2;

        Unit unit = go.AddComponent<Unit>();
        unit.unitID = _globalUnitCount++; // Призначаємо номер і збільшуємо лічильник
        unit.Setup(data, isPlayer);
        PlaceUnit(unit, x, y);
    }

    // --- Grid Logic ---

    public bool PlaceUnit(Unit unitToPlace, int x, int y)
    {
        if (IsValidPosition(x, y) && GetUnitAtPosition(x, y) == null)
        {
            _unitsOnGrid[x, y] = unitToPlace;
            unitToPlace.xPosition = x;
            unitToPlace.yPosition = y;
            unitToPlace.transform.position = GetWorldPosition(x, y) + new Vector3(0, 0, -0.1f);
            return true;
        }
        return false;
    }

    public bool MoveUnit(Unit unitToMove, int oldX, int oldY, int newX, int newY)
    {
        if (IsValidPosition(newX, newY) && _unitsOnGrid[newX, newY] == null)
        {
            _unitsOnGrid[oldX, oldY] = null;
            bool success = PlaceUnit(unitToMove, newX, newY);
            if (success)
            {
                // ЛОГ ПЕРЕМІЩЕННЯ
                Debug.Log($"{unitToMove.LogName} перейшов на клітину [{newX} {newY}]");
            }
            return success;
        }
        return false;
    }

    public Unit GetUnitAtPosition(int x, int y) => IsValidPosition(x, y) ? _unitsOnGrid[x, y] : null;
    private bool IsValidPosition(int x, int y) => x >= 0 && x < _totalColumns && y >= 0 && y < rows;

    public Vector3 GetWorldPosition(int x, int y)
    {
        float midCol = playerColumns + (commonColumns - 1) / 2f;
        float midRow = (rows - 1) / 2f;
        return CalculatePosition(x, y, midCol, midRow);
    }

    private Vector3 CalculatePosition(int x, int y, float midX, float midY)
    {
        float posX = (x - midX) * columnSpacing + (y - midY) * rowStep.x;
        float posY = (y - midY) * rowStep.y;
        return new Vector3(posX, posY, 0);
    }

    private GameObject GetPrefabByColumn(int x)
    {
        if (x < playerColumns) return playerTilePrefab;
        if (x < playerColumns + commonColumns) return commonTilePrefab;
        return enemyTilePrefab;
    }

    public List<Unit> GetAllUnits()
    {
        List<Unit> units = new List<Unit>();
        for (int x = 0; x < _totalColumns; x++)
            for (int y = 0; y < rows; y++)
                if (_unitsOnGrid[x, y] != null) units.Add(_unitsOnGrid[x, y]);
        return units;  
    }

    // --- Selection & Combat ---

    public void SelectUnit(Unit unit)
    {
        ClearSelection();
        _selectedUnit = unit;
        HighlightMoveRange(unit.xPosition, unit.yPosition);
    }
    
    public void ClearSelection()
    {
        _selectedUnit = null;
        foreach (var tile in _highlightedTiles) tile.ResetColor();
        _highlightedTiles.Clear();
    }

    private void HighlightMoveRange(int startX, int startY)
    {
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                int targetX = startX + dx;
                int targetY = startY + dy;

                if (IsValidPosition(targetX, targetY))
                {
                    Tile tile = _tilesOnGrid[targetX, targetY];
                    Unit targetUnit = GetUnitAtPosition(targetX, targetY);

                    if (targetUnit == null)
                    {
                        tile.SetHighlightColor(moveRangeColor);
                        _highlightedTiles.Add(tile);
                    }
                    else if (targetUnit.isPlayerUnit != _selectedUnit.isPlayerUnit)
                    {
                        tile.SetHighlightColor(attackRangeColor);
                        _highlightedTiles.Add(tile);
                    }
                }
            }
        }
    }
    
   // Додай цей метод у GridManager
   public void SwapOrMoveUnits(Tile source, Tile target)
   {
       // Перевірка, чи ціль у зоні розстановки
       if (!IsInPlacementZone(target.x, true)) return;
   
       Unit unitA = GetUnitAtPosition(source.x, source.y);
       Unit unitB = GetUnitAtPosition(target.x, target.y);
   
       if (unitB == null)
       {
           // Переміщення на порожню клітинку
           MoveUnit(unitA, source.x, source.y, target.x, target.y);
       }
       else if (unitB.isPlayerUnit)
       {
           // Обмін місцями (Swap)
           _unitsOnGrid[source.x, source.y] = unitB;
           _unitsOnGrid[target.x, target.y] = unitA;
   
           unitA.xPosition = target.x; unitA.yPosition = target.y;
           unitB.xPosition = source.x; unitB.yPosition = source.y;
   
           unitA.transform.position = GetWorldPosition(target.x, target.y) + new Vector3(0, 0, -0.1f);
           unitB.transform.position = GetWorldPosition(source.x, source.y) + new Vector3(0, 0, -0.1f);
           
           Debug.Log($"Обмін: {unitA.LogName} <-> {unitB.LogName}");
       }
   }
   
    public bool IsTileHighlightedForMove(int x, int y) => _highlightedTiles.Exists(t => t.x == x && t.y == y);

    public void AttackWithSelectedUnit(int targetX, int targetY)
    {
        if (_selectedUnit == null) return;
        if (ExecuteAttack(_selectedUnit, targetX, targetY))
        {
            _selectedUnit.SetState(false);
            ClearSelection();
        }
    }

    public void MoveSelectedUnitTo(int targetX, int targetY)
    {
        if (_selectedUnit == null) return;
    
        // Передаємо старі координати та нові
        if (MoveUnit(_selectedUnit, _selectedUnit.xPosition, _selectedUnit.yPosition, targetX, targetY))
        {
            // Тільки в фазі бою юніт втрачає можливість діяти після ходу
            if (currentPhase == GamePhase.Battle)
            {
                _selectedUnit.SetState(false); 
            }
            ClearSelection();
        }
    }

    public bool ExecuteAttack(Unit attacker, int targetX, int targetY)
    {
        Unit target = GetUnitAtPosition(targetX, targetY);
        if (target == null) return false;

        int damage = CombatCalculator.CalculateDamage(attacker, target);
        CombatCalculator.ApplyDamage(attacker, target, damage);
        
        if (target.IsDead())
        {
            _unitsOnGrid[target.xPosition, target.yPosition] = null;
            Destroy(target.gameObject);
        }   
        return true;
    }
    
    // --- Додай ці методи та зміни в GridManager ---

// 1. Видали автоматичний перехід у OnUnitPlaced
    public void OnUnitPlaced()
    {
        _currentUnitIndex++;
        _unitsPlaced++;
        // Видалили SpawnEnemyUnitsRandomly та currentPhase = GamePhase.Battle
    }
    
// 3. Логіка вільної перестановки під час Placement
    private Unit _unitToSwap;

    public void HandlePlacementClick(int x, int y)
    {
        // ПЕРЕВІРКА: чи клікаємо ми взагалі у свою зону?
        // Якщо ні - ігноруємо клік повністю
        if (!IsInPlacementZone(x, true)) 
        {
            Debug.LogWarning("Ви можете розставляти війська лише у своїй зоні (сині клітинки)!");
            return; 
        }
    
        Unit unitOnTile = GetUnitAtPosition(x, y);
    
        // 1. Перестановка існуючого юніта
        if (_unitToSwap != null && unitOnTile == null)
        {
            // Оскільки ми вже перевірили IsInPlacementZone вище, 
            // юніт переміститься лише всередині дозволеної зони
            MoveUnit(_unitToSwap, _unitToSwap.xPosition, _unitToSwap.yPosition, x, y);
            _unitToSwap = null;
        }
        // 2. Спавн нового юніта
        else if (unitOnTile == null)
        {
            // Додаємо перевірку кількості виставлених юнітів
                if (GetAllUnits().Count < 5) 
                {
                    UnitData data = GetNextAvailableUnit();
                    if (data != null)
                    {
                        SpawnUnit(data, x, y, true);
                        OnUnitPlaced();
                    }
                }
                else 
                {
                    Debug.Log("Максимальна кількість військ (5) вже на полі!");
                }
        }
        // 3. Вибір для перестановки
        else if (unitOnTile != null && unitOnTile.isPlayerUnit)
        {
            _unitToSwap = unitOnTile;
            Debug.Log($"Вибрано {unitOnTile.LogName} для перестановки. Оберіть вільну клітинку у своїй зоні.");
        }
    }
}