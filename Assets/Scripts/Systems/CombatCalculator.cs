using UnityEngine;

public class CombatCalculator : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static int CalculateDamage(Unit attacker, Unit defender)
    {
        return 10;
    }

    public static  void ApplyDamage(Unit attacker, Unit defender, int damage)
    {
        defender.Health -= damage;
    }
}
