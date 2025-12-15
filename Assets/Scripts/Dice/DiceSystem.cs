using UnityEngine;

public static class DiceSystem
{
    public static int RollDice(int maxRoll)
    {
        return Random.Range(1, maxRoll + 1);
    }
}