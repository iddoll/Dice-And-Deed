using System;
using UnityEngine;
using System.Collections.Generic;

public enum CombatPhase
{
        StartTurnPhase,
        MainPhase,
        EndTurnPhase
}
public class GameManager : MonoBehaviour
{
    public static GameManager Instance {get; private set;}
    public GridManager gridManager;
    
    [SerializeField] CombatPhase currentPhase;
    public int currentUnitIndex;
    
    private List<Unit> unitList;
    
    public void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void OnBattleStarted()
    {
        InitializeGame();
        Debug.Log("<color=yellow>GameManager: Логіка черговості ходів запущена!</color>");
    }
    public void InitializeGame()
    {
        // unitList = new List<Unit>(); // Видали цей рядок, GetAllUnits вже повертає список
        unitList = gridManager.GetAllUnits();
    
        if (unitList.Count > 0)
        {
            currentUnitIndex = 0;
            StartTurn();
        }
    }

    public void StartTurn()
    {
        currentPhase = CombatPhase.StartTurnPhase;
        Debug.Log($"--- НОВИЙ ХІД ---. Фаза: {currentPhase}");
        ExecutePreparationPhase();
    }

    // Update is called once per frame
    private void ExecutePreparationPhase()
    {
        // TODO: Логіка: відновлення Енергії, добір карт, спрацювання DoT ефектів.
        Debug.Log("Підготовка завершена. Перехід до фази дій.");
        currentPhase = CombatPhase.MainPhase;
    }

    private void EndMainPhase()
    {
        if (currentPhase != CombatPhase.MainPhase)
        {
            Debug.LogError("Спроба завершити фазу дій не в той час!");
            return;
        }
        Debug.Log("Фаза дій завершена. Перехід до кінця ходу.");
        ExecuteEndTurnPhase();
    }
    private void ExecuteEndTurnPhase()
    {
        currentPhase = CombatPhase.EndTurnPhase;
        // TODO: Логіка: скидання лімітів руху, перевірка умов перемоги
        UpdateUnitList();
        Invoke("NextTurn", 1f);
    }

    public void NextTurn()
    {
        // Оновлюємо список, бо хтось міг померти під час ходу
        unitList = gridManager.GetAllUnits();

        if (unitList.Count <= 1)
        {
            CheckWinCondition();
            return; 
        }

        currentUnitIndex++;
        if (currentUnitIndex >= unitList.Count)
        {
            currentUnitIndex = 0;
            // Можна додати логіку початку нового раунду тут
        }

        Unit currentUnit = unitList[currentUnitIndex];
        currentPhase = CombatPhase.MainPhase;
    
        // Активуємо юніта (скидаємо його колір/стан)
        currentUnit.SetState(true); 
    
        Debug.Log($"Хід юніта: {currentUnit.LogName} (Гравець: {currentUnit.isPlayerUnit})");
    }

    public void UpdateUnitList()
    {
        unitList = gridManager.GetAllUnits();
        if (unitList.Count > 0)
        {
            currentUnitIndex = 0; 
        }
        CheckWinCondition();
    }

    public void CheckWinCondition()
    {
        if (unitList == null || unitList.Count == 1)
        {
            Debug.Log("ПЕРЕМОГА");
        }
    }

    public void TestAttack()
    {
        Unit attacker = unitList[0];
        gridManager.ExecuteAttack(attacker, 4, 4);
        EndMainPhase();
    }
}
