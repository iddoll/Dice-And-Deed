using UnityEngine;

public class Unit : MonoBehaviour
{
    
    public enum Element
    {
        
    }

    public int xPosition;
    public int yPosition;
    public Element element;
    public bool isAlive = true;
    public int ClassBonus;
    public int Health;
    public string unitName;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    

    public bool IsDead()
    {
        return Health <= 0;
    }
}
