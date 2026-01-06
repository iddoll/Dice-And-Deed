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
    
    [Header("Timers")]
    public float placementTime = 30f; // Час на розстановку
    public float turnTime = 20f;      // Час на хід
    private float _currentTime;
    private bool _isTimerActive = true;
    
    public TMPro.TextMeshProUGUI timerText; // Признач об'єкт "118" сюди
    
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
        
        // На старті розстановки:
        if (startBattleButton != null) startBattleButton.SetActive(true);
        if (endTurnButton != null) endTurnButton.SetActive(false); // Ховаємо кнопку бою
    }
    // В Update GridManager залиш ТІЛЬКИ логіку для Placement
    void Update()
    {
        if (currentPhase == GamePhase.Placement)
        {
            placementTime -= Time.deltaTime;
            if (timerText != null) timerText.text = Mathf.CeilToInt(placementTime).ToString();
    
            if (placementTime <= 0) StartBattlePhase();
        }
    }
    
    // Додай цей метод, щоб TurnManager міг скидати таймер
    public void ResetTurnTimer(float time)
    {
        // Ми будемо використовувати TurnManager для керування часом у бою
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
        if (currentPhase == GamePhase.Battle) return; // Запобігаємо повторному запуску
    
        int realUnitCount = GetAllUnits().Count;
        if (realUnitCount == 0) return;
    
        currentPhase = GamePhase.Battle;
    
        // Оновлення UI
        if (startBattleButton != null) startBattleButton.SetActive(false);
        if (endTurnButton != null) 
        {
            endTurnButton.SetActive(true);
            // Важливо: Оновлюємо посилання в TurnManager, якщо воно збилося
            TurnManager.Instance.endTurnButton = endTurnButton.GetComponent<UnityEngine.UI.Button>();
        }
    
        foreach (var tile in _tilesOnGrid) if (tile != null) tile.SetDim(false);
    
        SpawnEnemyUnitsRandomly();
        TurnManager.Instance.OnBattleStarted();
    }

    public void ResetAllTilesToDefault()
    {
        foreach (Tile t in _tilesOnGrid)
        {
            if (t == null) continue;
            t.SetBaseColor(Color.white); // Тепер цей метод існує
            t.ResetColor(); // Поверне яскравість або затемнення згідно фази
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
        sr.sprite = data.unitSprite;
        sr.color = isPlayer ? Color.white : new Color(1, 0.8f, 0.8f);
        sr.sortingOrder = 5; 

        // Встановлюємо масштаб (залиште свій поточний, наприклад 1.5 або 2)
        go.transform.localScale = new Vector3(2f, 2f, 1f);

        if (!isPlayer) sr.flipX = true; 

        Unit unit = go.AddComponent<Unit>();
        unit.unitID = _globalUnitCount++;
        unit.Setup(data, isPlayer);

        // ОТРИМАННЯ ЦЕНТРАЛЬНОЇ ПОЗИЦІЇ ТАЙЛА
        Vector3 tileCenterPos = GetWorldPosition(x, y);

        // Якщо ви налаштували Pivot на Bottom, то y + 0 (центр тайла) 
        // поставить "ноги" мага рівно в центр.
        // Якщо ж Pivot залишився Center, додайте невеликий offset (наприклад, +0.3f), 
        // щоб маг не "тонув" у тайлі.
        float yOffset = 0f; // Налаштуйте це значення, щоб маг візуально стояв на площині
    
        unit.xPosition = x;
        unit.yPosition = y;
        _unitsOnGrid[x, y] = unit;
    
        // Встановлюємо фінальну позицію
        unit.transform.position = new Vector3(tileCenterPos.x, tileCenterPos.y + yOffset, -0.1f);
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
    if (_selectedUnit == null) return;

    int range = _selectedUnit.unitData.attackRange; // Має бути 2

    for (int dx = -range; dx <= range; dx++)
    {
        for (int dy = -range; dy <= range; dy++)
        {
            if (dx == 0 && dy == 0) continue;

            int targetX = startX + dx;
            int targetY = startY + dy;

            if (IsValidPosition(targetX, targetY))
            {
                Tile tile = _tilesOnGrid[targetX, targetY];
                Unit targetUnit = GetUnitAtPosition(targetX, targetY);
                int dist = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));

                if (dist == 1) // Радіус 1 (сусідні)
                {
                    if (targetUnit == null)
                        tile.SetHighlightColor(Color.green); // Можна йти
                    else if (!targetUnit.isPlayerUnit)
                        tile.SetHighlightColor(Color.red);   // Можна бити впритул
                }
                else // Радіус 2 (через одну)
                {
                    if (targetUnit == null)
                        tile.SetHighlightColor(Color.blue);  // Тільки стріляти (порожньо)
                    else if (!targetUnit.isPlayerUnit)
                        tile.SetHighlightColor(Color.red);   // Тільки стріляти (ворог)
                }
                
                _highlightedTiles.Add(tile);
            }
        }
    }
}

public bool ExecuteAttack(Unit attacker, int targetX, int targetY)
{
    Unit target = GetUnitAtPosition(targetX, targetY);
    if (target == null) return false;

    // Перевірка наявності префаба снаряда
    if (attacker.unitData != null && attacker.unitData.projectilePrefab != null)
    {
        GameObject projGO = Instantiate(attacker.unitData.projectilePrefab, attacker.transform.position, Quaternion.identity);
        Projectile proj = projGO.GetComponent<Projectile>();
        
        int damage = CombatCalculator.CalculateDamage(attacker, target);
        proj.Setup(attacker, target, damage); // Передаємо атакувальника!
        
        Debug.Log($"{attacker.unitName} атакує дистанційно ціль {target.unitName}");
    }
    else
    {
        // Ближній бій (якщо немає префаба)
        int damage = CombatCalculator.CalculateDamage(attacker, target);
        CombatCalculator.ApplyDamage(attacker, target, damage);
        
        if (target.IsDead())
        {
            ClearUnitFromGrid(target.xPosition, target.yPosition);
            Destroy(target.gameObject);
        }
    }
    return true;
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

    public void ClearUnitFromGrid(int x, int y)
    {
        if (x >= 0 && x < _totalColumns && y >= 0 && y < rows)
        {
            _unitsOnGrid[x, y] = null;
        }
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