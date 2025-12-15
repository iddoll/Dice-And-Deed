using UnityEngine;

public enum CombatPhase
{
        StartTurnPhase,
        MainPhase,
        EndTurnPhase
}
public class GameManager : MonoBehaviour
{
    [SerializeField] CombatPhase currentPhase;
    [SerializeField] bool isPlayerTurn;
        
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void StartTurn()
    {
        currentPhase = CombatPhase.StartTurnPhase;
        Debug.Log($"--- НОВИЙ ХІД ---. Фаза: {currentPhase}");
        isPlayerTurn = true;
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
        if (currentPhase == CombatPhase.MainPhase)
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
        isPlayerTurn = false;
        Debug.Log($"Хід завершено. Тепер хід: {(isPlayerTurn ? "Гравця" : "Опонента")}");
        StartTurn();
    }
}
