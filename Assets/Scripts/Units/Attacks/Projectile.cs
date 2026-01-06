using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Unit _attacker; // Посилання на того, хто стріляв
    private Unit _target;
    private int _damage;
    public float speed = 12f;

    public void Setup(Unit attacker, Unit target, int damage)
    {
        _attacker = attacker; // ОБОВ'ЯЗКОВО зберігаємо атакувальника
        _target = target;
        _damage = damage;

        Vector3 direction = _target.transform.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Update()
    {
        if (_target == null) { Destroy(gameObject); return; }

        transform.position = Vector3.MoveTowards(transform.position, _target.transform.position, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, _target.transform.position) < 0.2f)
        {
            // Тепер передаємо _attacker, і помилки NullReference не буде
            CombatCalculator.ApplyDamage(_attacker, _target, _damage);
    
            if (_target.IsDead())
            {
                GridManager.Instance.ClearUnitFromGrid(_target.xPosition, _target.yPosition);
                Destroy(_target.gameObject);
            }
    
            Destroy(gameObject);
        }
    }
}