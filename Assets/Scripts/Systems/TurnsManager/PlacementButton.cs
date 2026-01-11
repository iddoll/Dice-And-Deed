using UnityEngine;

public class PlacementButton : MonoBehaviour
{
    public UnitData unitData; // Сюди в інспекторі перетягни ScriptableObject твого мага

    public void OnButtonClick()
    {
        // Передаємо дані юніта в GridManager
        GridManager.Instance.SetUnitToSpawn(unitData);
    }
}