using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public float turnDuration = 20f;
    private float _timer;
    public bool isPlayerTurn = true;
    private bool _isBattleActive = false;

    public TextMeshProUGUI timerText; 
    public Button endTurnButton; // Пряме посилання на компонент Button

    void Awake() => Instance = this;

    public void OnBattleStarted()
    {
        _isBattleActive = true;
        StartTurn();
    }

    void Update()
    {
        if (!_isBattleActive) return;

        _timer -= Time.deltaTime;
        if (timerText != null) timerText.text = Mathf.CeilToInt(_timer).ToString();

        if (_timer <= 0) EndTurn();
    }

    private void StartTurn()
    {
        _timer = turnDuration;
             
             // Якщо кнопки немає в інспекторі, шукаємо її за тегом або через GridManager
             if (endTurnButton == null && GridManager.Instance.endTurnButton != null)
             {
                 endTurnButton = GridManager.Instance.endTurnButton.GetComponent<UnityEngine.UI.Button>();
             }
         
             if (endTurnButton != null)
             {
                 endTurnButton.gameObject.SetActive(true); // Гарантуємо, що вона видима
                 endTurnButton.interactable = isPlayerTurn;
             }

        var allUnits = GridManager.Instance.GetAllUnits();
        foreach (var unit in allUnits)
        {
            if (unit.isPlayerUnit == isPlayerTurn) 
                {
                    unit.SetState(true);
                    unit.hasAction = true;
                }
        }

        if (!isPlayerTurn) StartCoroutine(EnemySimpleAI());
    }

    public void EndTurn()
    {
        if (!_isBattleActive) return;
        isPlayerTurn = !isPlayerTurn;
        StartTurn();
    }
    
    public void SetPlayerTurn(bool isPlayer)
    {
        isPlayerTurn = isPlayer;
        if (endTurnButton != null) endTurnButton.interactable = isPlayerTurn;
        
        // Скидаємо таймер прямо тут
        _timer = turnDuration; 
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

    // 1. ПЕРЕВІРКА АТАКИ: чи дістаємо ми ціль з поточної позиції?
    // Використовуємо Mathf.Max для "квадратної" дистанції (діагоналі рахуються як 1 крок)
    int distX = Mathf.Abs(enemy.xPosition - player.xPosition);
    int distY = Mathf.Abs(enemy.yPosition - player.yPosition);
    int currentDist = Mathf.Max(distX, distY);

    if (currentDist <= enemy.unitData.attackRange)
    {
        GridManager.Instance.ExecuteAttack(enemy, player.xPosition, player.yPosition);
        enemy.SetState(false); // Зробив дію — хід закінчено
        return;
    }

    // 2. ЯКЩО НЕ ДІСТАЄМО — РОБИМО КРОК (вибираємо найкращу сусідню вільну клітину)
    Vector2Int bestTile = new Vector2Int(enemy.xPosition, enemy.yPosition);
    float minDistance = float.MaxValue;

    // Перевіряємо всі 8 клітин навколо ворога
    for (int dx = -1; dx <= 1; dx++)
    {
        for (int dy = -1; dy <= 1; dy++)
        {
            if (dx == 0 && dy == 0) continue;

            int checkX = enemy.xPosition + dx;
            int checkY = enemy.yPosition + dy;

            // Чи клітина на полі та чи вона вільна?
            if (GridManager.Instance.GetUnitAtPosition(checkX, checkY) == null)
            {
                // Рахуємо відстань від цієї клітини до гравця
                float d = Vector2.Distance(new Vector2(checkX, checkY), new Vector2(player.xPosition, player.yPosition));
                if (d < minDistance)
                {
                    minDistance = d;
                    bestTile = new Vector2Int(checkX, checkY);
                }
            }
        }
    }

    // Якщо знайшли кращу вільну клітину — йдемо туди
    if (bestTile.x != enemy.xPosition || bestTile.y != enemy.yPosition)
    {
        GridManager.Instance.MoveUnit(enemy, enemy.xPosition, enemy.yPosition, bestTile.x, bestTile.y);
    }

    enemy.SetState(false); // Після кроку хід теж закінчується (правило "або-або")
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