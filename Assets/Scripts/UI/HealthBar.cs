using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider hpSlider; // Перетягни об'єкт Slider сюди в інспекторі
    public Image fillImage;  // Перетягни об'єкт Fill (всередині Slider) сюди
    private Unit _owner;

    public void Setup(Unit owner)
    {
        _owner = owner;
        // На старті слайдер має бути повним
        if (hpSlider != null) hpSlider.value = 1f;
        UpdateHealthBar();
    }

    public void UpdateHealthBar()
    {
        if (_owner == null || hpSlider == null) return;

        // Рахуємо відсоток здоров'я
        float healthPercent = (float)_owner.curentHealth / _owner.maxHealth;
        
        // Змінюємо значення слайдера (від 0 до 1)
        hpSlider.value = healthPercent;

        // Зміна кольору (опціонально, через fillImage)
        if (fillImage != null)
        {
            if (healthPercent < 0.3f) fillImage.color = Color.red;
            else if (healthPercent < 0.6f) fillImage.color = Color.yellow;
            else fillImage.color = Color.green;
        }
    }
}