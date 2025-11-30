using System.Collections.Generic;
using UnityEngine;

public class CustomerFactory : Factory<Customer>
{
    [field: SerializeField]
    public List<Sprite> CustomerSprites
    {
        get;
        set;
    }
    
    public CustomerFactory(Customer p, List<Sprite> sprites)
    {
        prefab = p;
        CustomerSprites = sprites;
    }
    public override Customer GetObj()
    {
        // Llama al método base para instanciar el prefab
        Customer newCustomer = base.GetObj(); 

        // Lógica ESPECÍFICA: Asignar sprite aleatorio
        if (CustomerSprites != null && CustomerSprites.Count > 0)
        {
            Sprite randomSprite = CustomerSprites[Random.Range(0, CustomerSprites.Count)];
            
            // Suponiendo que el SpriteRenderer está en el objeto raíz del Customer
            SpriteRenderer customerRenderer = newCustomer.GetComponent<SpriteRenderer>();
            
            if (customerRenderer != null)
            {
                customerRenderer.sprite = randomSprite;
            }
        }
        
        // El resto de los datos (ID, order, etc.) se asignan en el Spawner al usar Get()
        return newCustomer;
    }
}