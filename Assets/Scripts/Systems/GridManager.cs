using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    private const int GRID_SIZE = 5;

    private Unit[,] _unitsOnGrid;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _unitsOnGrid = new Unit[GRID_SIZE, GRID_SIZE];
        GameObject heroGO = new GameObject("HeroUnit"); 
        Unit playerUnit = heroGO.AddComponent<Unit>();
        playerUnit.unitName = "Hero";
        playerUnit.Health = 30;
        
        GameObject goblinGO = new GameObject("GoblinUnit"); 
        Unit enemyUnit = goblinGO.AddComponent<Unit>();
        enemyUnit.unitName = "Goblin";
        enemyUnit.Health = 10;
        
        PlaceUnit(playerUnit, 0, 0); // Позиція (0, 0)
        PlaceUnit(enemyUnit, 4, 4);  // Позиція (4, 4)
    }
    
    public Unit GetUnitAtPosition(int x, int y)
    {
        
        if (IsValidPosition(x,y))
        {
            return _unitsOnGrid[x, y];
        }
        return null;
    }

    private bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < GRID_SIZE && y >= 0 && y < GRID_SIZE;
    }

    private bool PlaceUnit(Unit unitToPlace, int x, int y)
    {
        if (IsValidPosition(x, y) && GetUnitAtPosition(x,y) == null)
        {
            _unitsOnGrid[x, y] = unitToPlace;
            unitToPlace.xPosition = x;
            unitToPlace.yPosition = y;
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
