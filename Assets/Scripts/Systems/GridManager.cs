using UnityEngine;

public class GridManager : MonoBehaviour
{
    private const int GRID_SIZE = 5;

    private Unit[,] Unit;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Unit = new Unit[GRID_SIZE, GRID_SIZE];
    }
    
    public Unit GetUnitAtPosition(int x, int y)
    {
        
        if (IsValidPosition(x,y))
        {
            return Unit[x, y];
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
            Unit[x, y] = unitToPlace;
            unitToPlace.xPosition = x;
            unitToPlace.yPosition = y;
            return true;
        }
        return false;
    }

    private bool RemoveUnit(Unit unitToRemove, int x, int y)
    {
        if (Unit[x, y] != null && Unit[x, y] == unitToRemove)
        {
            Unit[x, y] = null;
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
}
