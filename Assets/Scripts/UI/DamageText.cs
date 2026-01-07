using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    public TextMeshProUGUI textMesh;
    public float lifetime = 1f;
    public float moveSpeed = 1f;

    public void Setup(int damageValue)
    {
        textMesh.text = $"-{damageValue}";
        // Видаляємо через секунду
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Текст летить вгору
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
        
        // Поступово робимо текст прозорим (опціонально)
        float alpha = textMesh.color.a - (Time.deltaTime / lifetime);
        textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, alpha);
    }
}