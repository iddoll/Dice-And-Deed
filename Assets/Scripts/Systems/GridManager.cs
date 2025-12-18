using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    private const int GRID_SIZE = 5;

    private Unit[,] _unitsOnGrid;
    
    [Header("Visual Settings")]
    public GameObject tilePrefab; 
    public float spacing = 1.1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static GridManager Instance;

    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        // ПРАВИЛО: Спочатку створюємо "пам'ять" (масив), а потім "тіло" (об'єкти)
        _unitsOnGrid = new Unit[GRID_SIZE, GRID_SIZE];
    
        GenerateVisualGrid();

        // Створюємо Hero
        GameObject heroGO = new GameObject("HeroUnit"); 
        Unit playerUnit = heroGO.AddComponent<Unit>();
        // ТУТ ВАЖЛИВО: додамо йому колір, щоб бачити його (як ми робили раніше)
        SpriteRenderer srH = heroGO.AddComponent<SpriteRenderer>();
        srH.sprite = tilePrefab.GetComponent<SpriteRenderer>().sprite; 
        srH.color = Color.blue;
        srH.sortingOrder = 2; 

        playerUnit.unitName = "Hero";
        playerUnit.Health = 30;
    
        // Створюємо Goblin
        GameObject goblinGO = new GameObject("GoblinUnit"); 
        Unit enemyUnit = goblinGO.AddComponent<Unit>();
        SpriteRenderer srG = goblinGO.AddComponent<SpriteRenderer>();
        srG.sprite = tilePrefab.GetComponent<SpriteRenderer>().sprite;
        srG.color = Color.red;
        srG.sortingOrder = 2;

        enemyUnit.unitName = "Goblin";
        enemyUnit.Health = 10;
    
        PlaceUnit(playerUnit, 0, 0); 
        PlaceUnit(enemyUnit, 4, 4);  
    }
    public Unit GetUnitAtPosition(int x, int y)
    {
        
        if (IsValidPosition(x,y))
        {
            return _unitsOnGrid[x, y];
        }
        return null;
    }
    
    private void GenerateVisualGrid()
    {
        for (int x = 0; x < GRID_SIZE; x++)
        {
            for (int y = 0; y < GRID_SIZE; y++)
            {
                Vector3 position = new Vector3(x * spacing, y * spacing, 0);
                GameObject tileGO = Instantiate(tilePrefab, position, Quaternion.identity);
            
                // ЗВ'ЯЗОК: Передаємо координати в скрипт Tile
                Tile tileScript = tileGO.GetComponent<Tile>();
                tileScript.x = x;
                tileScript.y = y;
            
                tileGO.name = $"Tile_{x}_{y}";
                tileGO.transform.parent = transform;
            }
        }
    }

    private bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < GRID_SIZE && y >= 0 && y < GRID_SIZE;
    }

    private bool PlaceUnit(Unit unitToPlace, int x, int y)
    {
        if (IsValidPosition(x, y) && GetUnitAtPosition(x, y) == null)
        {
            _unitsOnGrid[x, y] = unitToPlace;
            unitToPlace.xPosition = x;
            unitToPlace.yPosition = y;
            
            unitToPlace.transform.position = new Vector3(x * spacing, y * spacing, -0.1f);
        
            return true;
        }
        return false;
    }

    private bool RemoveUnit(Unit unitToRemove, int x, int y)
    {
        if (_unitsOnGrid[x, y] != null && _unitsOnGrid[x, y] == unitToRemove)
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
    // Update is called once per frame
    void Update()
    {
        
    }

    public bool ExecuteAttack(Unit attacker, int targetX, int targetY)
    {
        Unit target = GetUnitAtPosition(targetX, targetY);
        if (target == null)
        {
            return false;
        }
        int damage = CombatCalculator.CalculateDamage(attacker, target);
        CombatCalculator.ApplyDamage(attacker, target, damage);
        CleanupDeadUnit(target);
        return true;
    }

    public List<Unit> GetAllUnits()
    {
        List<Unit> units = new List<Unit>();
        for (int x = 0; x < GRID_SIZE; x++)
        {
            for (int y = 0; y < GRID_SIZE; y++)
            {
                Unit currentUnit = _unitsOnGrid[x, y];
                if (currentUnit != null)
                {
                    units.Add(currentUnit);
                }
            }
        }
        return units;  
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
