using System.Collections.Generic;
using UnityEngine;

public enum GamePhase { Placement, Battle }
public class GridManager : MonoBehaviour
{
    public static GridManager Instance;
    
    public GamePhase currentPhase = GamePhase.Placement;
    private int _unitsPlaced = 0;
    private const int MaxUnits = 5;
    
    [Header("Prefabs")]
    public GameObject playerTilePrefab;
    public GameObject commonTilePrefab;
    public GameObject enemyTilePrefab;

    [Header("Grid Dimensions")]
    public int playerColumns = 2;   // Стовпці для гравця (зліва)
    public int commonColumns = 1;   // Центральні стовпці
    public int enemyColumns = 2;    // Стовпці для ворога (справа)
    public int rows = 5;            // Кількість рядів (висота поля)

    [Header("Spacing")]
    public float columnSpacing = 2.5f; 
    public Vector2 rowStep = new Vector2(0.5f, 1.5f);

    [Header("Selection Settings")]
    public Color moveRangeColor = new Color(0, 1, 0, 0.5f); // Зелений для ходу
    private Unit _selectedUnit;
    private List<Tile> _highlightedTiles = new List<Tile>();
    
    private Unit[,] _unitsOnGrid;
    private int _totalColumns;

    private Tile[,] _tilesOnGrid; // Масив для швидкого пошуку тайлів
    
    [Header("Combat Colors")]
    public Color attackRangeColor = new Color(1, 0, 0, 0.6f); // Червоний для атаки
    
    public Unit SelectedUnit => _selectedUnit;
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
        SpawnInitialUnits();
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
            
                // ПРАВИЛЬНИЙ ПОРЯДОК:
                Tile tileScript = tileGO.GetComponent<Tile>(); // 1. Спочатку отримуємо компонент
            
                if (tileScript != null)
                {
                    tileScript.x = x;
                    tileScript.y = y;
                    _tilesOnGrid[x, y] = tileScript; // 2. Потім записуємо в масив
                }
            
                tileGO.name = $"Tile_{x}_{y}";
            }
        }
    }

    // Метод для перевірки, чи можна ставити юніта в цю колонку
    public bool IsInPlacementZone(int x, bool isPlayer)
    {
        if (isPlayer) return x < playerColumns; // Колонки 0 та 1
        return x >= (playerColumns + commonColumns); // Колонки 3 та 4 для ворога
    }

    public void OnUnitPlaced()
    {
        _unitsPlaced++;
        if (_unitsPlaced >= MaxUnits)
        {
            // Показуємо кнопку "Start Battle"
            Debug.Log("Всі юніти виставлені! Готові до бою.");
        }
    }
    
    private GameObject GetPrefabByColumn(int x)
    {
        // Зона гравця (зліва)
        if (x < playerColumns) return playerTilePrefab;
        // Спільна зона (центр)
        if (x < playerColumns + commonColumns) return commonTilePrefab;
        // Зона ворога (справа)
        return enemyTilePrefab;
    }

    private Vector3 CalculatePosition(int x, int y, float midX, float midY)
    {
        float relativeX = x - midX;
        float relativeY = y - midY;

        float posX = relativeX * columnSpacing + relativeY * rowStep.x;
        float posY = relativeY * rowStep.y;

        return new Vector3(posX, posY, 0);
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        float midCol = playerColumns + (commonColumns - 1) / 2f;
        float midRow = (rows - 1) / 2f;
        return CalculatePosition(x, y, midCol, midRow);
    }

    private void SpawnInitialUnits()
    {
        // Герой: остання цифра true (це гравець)
        CreateUnit("Hero", Color.blue, 30, 0, rows / 2, true);
    
        // Гоблін: остання цифра false (це ворог)
        CreateUnit("Goblin", Color.red, 10, _totalColumns - 1, rows / 2, false);
    }

    public void SpawnEnemyUnitsRandomly()
    {
        int placed = 0;
        while (placed < MaxUnits)
        {
            // Ворожі колонки за GDD: 3 та 4 (якщо рахувати з 0)
            int randomX = Random.Range(playerColumns + commonColumns, _totalColumns);
            int randomY = Random.Range(0, rows);

            if (GetUnitAtPosition(randomX, randomY) == null)
            {
                CreateUnit("Enemy_" + placed, Color.red, 20, randomX, randomY, false);
                placed++;
            }
        }
    }
    
    // Метод для перевірки, чи натиснута кнопка "Start Battle"
    public void StartBattle()
    {
        if (_unitsPlaced >= MaxUnits)
        {
            currentPhase = GamePhase.Battle;
            Debug.Log("Фаза бою розпочата!");
            // Тут можна викликати TurnManager.Instance.StartFirstTurn();
        }
    }
    
    private void CreateUnit(string name, Color color, int hp, int x, int y, bool isPlayer)
    {
        GameObject go = new GameObject(name + "Unit");
    
        // 1. Спочатку додаємо візуал
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = commonTilePrefab.GetComponent<SpriteRenderer>().sprite;
        sr.color = color;
        sr.sortingOrder = 2;

        // 2. Потім додаємо логіку
        Unit unit = go.AddComponent<Unit>();
        unit.unitName = name;
        unit.Health = hp;
        unit.isPlayerUnit = isPlayer;
    
        // 3. Ініціалізуємо колір ПІСЛЯ того, як sr вже налаштований
        unit.InitBaseColor();

        // 4. Тільки в кінці розміщуємо на сітці
        PlaceUnit(unit, x, y);
    }

    public bool PlaceUnit(Unit unitToPlace, int x, int y)
    {
        if (IsValidPosition(x, y) && GetUnitAtPosition(x, y) == null)
        {
            _unitsOnGrid[x, y] = unitToPlace;
            unitToPlace.xPosition = x;
            unitToPlace.yPosition = y;
            
            Vector3 worldPos = GetWorldPosition(x, y);
            worldPos.z = -0.1f; 
            unitToPlace.transform.position = worldPos;
        
            return true;
        }
        return false;
    }

    public Unit GetUnitAtPosition(int x, int y)
    {
        if (IsValidPosition(x, y)) return _unitsOnGrid[x, y];
        return null;
    }

    private bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < _totalColumns && y >= 0 && y < rows;
    }

    public List<Unit> GetAllUnits()
    {
        List<Unit> units = new List<Unit>();
        for (int x = 0; x < _totalColumns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                if (_unitsOnGrid[x, y] != null) 
                    units.Add(_unitsOnGrid[x, y]);
            }
        }
        return units;  
    }

    public void SelectUnit(Unit unit)
    {
        ClearSelection(); // Очищуємо старе виділення
        _selectedUnit = unit;
        HighlightMoveRange(unit.xPosition, unit.yPosition);
    }
    
    public void ClearSelection()
    {
        _selectedUnit = null;
        foreach (var tile in _highlightedTiles)
        {
            tile.ResetColor(); // Повертаємо тайлам базовий колір
        }
        _highlightedTiles.Clear();
    }

    private void HighlightMoveRange(int startX, int startY)
    {
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue; // Пропускаємо клітину під самим воїном

                int targetX = startX + dx;
                int targetY = startY + dy;

                if (IsValidPosition(targetX, targetY))
                {
                    Tile tile = FindTileAt(targetX, targetY);
                    Unit targetUnit = GetUnitAtPosition(targetX, targetY);

                    if (targetUnit == null)
                    {
                        // Порожня клітина - хід (Зелений)
                        tile.SetHighlightColor(moveRangeColor);
                        _highlightedTiles.Add(tile);
                    }
                    else if (targetUnit.isPlayerUnit != _selectedUnit.isPlayerUnit)
                    {
                        // Ворожий юніт - атака (Червоний)
                        tile.SetHighlightColor(attackRangeColor);
                        _highlightedTiles.Add(tile);
                    }
                    // Якщо дружній юніт - клітина заблокована (нічого не робимо)
                }
            }
        }
    }
    
    public void AttackWithSelectedUnit(int targetX, int targetY)
    {
        if (_selectedUnit == null) return;

        // Використовуємо твій ExecuteAttack
        bool success = ExecuteAttack(_selectedUnit, targetX, targetY);
    
        if (success)
        {
            _selectedUnit.SetState(false); // Блокуємо після дії
            ClearSelection();
        }
    }

    // Перевірка, чи є ця клітинка серед підсвічених для ходу
    public bool IsTileHighlightedForMove(int x, int y)
    {
        foreach (var tile in _highlightedTiles)
        {
            if (tile.x == x && tile.y == y) return true;
        }
        return false;
    }

// Виконання ходу обраного юніта
    public void MoveSelectedUnitTo(int targetX, int targetY)
    {
        if (_selectedUnit == null) return;

        // Спроба змінити позицію в логічній сітці та перемістити об'єкт
        bool success = MoveUnit(_selectedUnit, _selectedUnit.xPosition, _selectedUnit.yPosition, targetX, targetY);

        if (success)
        {
            Debug.Log($"{_selectedUnit.unitName} походив і завершив дію.");
        
            // ВАЖЛИВО: Одразу блокуємо юніта після ходу
            _selectedUnit.SetState(false); 
        
            ClearSelection();
        }
    }
    
// Допоміжний метод для пошуку скрипта Tile за координатами
    private Tile FindTileAt(int x, int y)
    {
        if (IsValidPosition(x, y)) return _tilesOnGrid[x, y];
        return null;
    }
    
    private bool RemoveUnit(Unit unitToRemove, int x, int y)
    {
        if (IsValidPosition(x, y) && _unitsOnGrid[x, y] == unitToRemove)
        {
            _unitsOnGrid[x, y] = null;
            return true;
        }
        return false;
    }

    public bool MoveUnit(Unit unitToMove, int oldX, int oldY, int newX, int newY)
    {
        if (RemoveUnit(unitToMove, oldX, oldY))
        {   
            return PlaceUnit(unitToMove, newX, newY);
        }
        return false;
    }

    public bool ExecuteAttack(Unit attacker, int targetX, int targetY)
    {
        Unit target = GetUnitAtPosition(targetX, targetY);
        if (target == null) return false;

        int damage = CombatCalculator.CalculateDamage(attacker, target);
        CombatCalculator.ApplyDamage(attacker, target, damage);
        CleanupDeadUnit(target);
        return true;
    }
    
    private void CleanupDeadUnit(Unit unit)
    {
        if (unit.IsDead())
        {
            RemoveUnit(unit, unit.xPosition, unit.yPosition);
            Destroy(unit.gameObject);
        }
    }
}