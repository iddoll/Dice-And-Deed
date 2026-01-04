using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public float turnDuration = 30f;
    private float _timer;
    public bool isPlayerTurn = true;

    public TextMeshProUGUI timerText; // Посилання на текст у UI

    void Awake() => Instance = this;

    void Start()
    {
        StartTurn();
    }

    void Update()
    {
        _timer -= Time.deltaTime;
        if (timerText != null) timerText.text = Mathf.CeilToInt(_timer).ToString();

        if (_timer <= 0)
        {
            EndTurn();
        }
    }

    public void EndTurn()
    {
        isPlayerTurn = !isPlayerTurn;
        StartTurn();
    }

    private void StartTurn()
    {
        _timer = turnDuration;
        var allUnits = GridManager.Instance.GetAllUnits();

        foreach (var unit in allUnits)
        {
            unit.SetState(unit.isPlayerUnit == isPlayerTurn);
        }

        GridManager.Instance.ClearSelection();

        // ЯКЩО ЗАРАЗ ХІД КОМП'ЮТЕРА:
        if (!isPlayerTurn)
        {
            StartCoroutine(EnemySimpleAI());
        }
    }

    private System.Collections.IEnumerator EnemySimpleAI()
    {
        Debug.Log("ШІ починає думати...");
        yield return new WaitForSeconds(1.5f); // Пауза для ефекту "думки"

        // Отримуємо всіх юнітів ворога, які можуть діяти
        var allUnits = GridManager.Instance.GetAllUnits();
        foreach (var unit in allUnits)
        {
            if (!unit.isPlayerUnit && unit.hasAction)
            {
                ExecuteEnemyLogic(unit);
                yield return new WaitForSeconds(1.0f); // Пауза між ходами різних юнітів ворога
            }
        }

        Debug.Log("ШІ завершив хід.");
        EndTurn(); // Автоматично повертаємо хід гравцю
    }

    private void ExecuteEnemyLogic(Unit enemy)
    {
        Unit player = FindNearestPlayer(enemy);
        if (player == null) { enemy.SetState(false); return; }

        // 1. Перевіряємо, чи можемо ми вдарити (радіус 1 клітинка)
        int distX = Mathf.Abs(enemy.xPosition - player.xPosition);
        int distY = Mathf.Abs(enemy.yPosition - player.yPosition);

        if (distX <= 1 && distY <= 1)
        {
            GridManager.Instance.ExecuteAttack(enemy, player.xPosition, player.yPosition);
            enemy.SetState(false);
            return;
        }

        // 2. Якщо гравець далеко — робимо крок у його бік
        int moveX = enemy.xPosition + (int)Mathf.Sign(player.xPosition - enemy.xPosition);
        int moveY = enemy.yPosition + (int)Mathf.Sign(player.yPosition - enemy.yPosition);

        // Перевіряємо, чи вільна клітинка перед тим як йти
        if (GridManager.Instance.GetUnitAtPosition(moveX, moveY) == null)
        {
            GridManager.Instance.MoveUnit(enemy, enemy.xPosition, enemy.yPosition, moveX, moveY);
        }

        enemy.SetState(false); // Завершуємо дію після ходу або невдалої спроби ходу
    }

    private Unit FindNearestPlayer(Unit enemy)
    {
        // Поки у нас один герой, просто шукаємо його в списку
        foreach (var u in GridManager.Instance.GetAllUnits())
        {
            if (u.isPlayerUnit) return u;
        }
        return null;
    }
}