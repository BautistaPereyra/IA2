using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Product
{
    public int ID { get; set; }
    public string Name { get; set; }
    public string Category { get; set; } // Ejemplo: "Perecedero", "Limpieza", "Snack"
    public decimal SellPrice { get; set; } // Usado por Integrante 3
    public decimal RestokeCost { get; set; } // Usado por Integrante 3
    public int MaxStock { get; set; } // La capacidad máxima
    public int CurrentStock { get; set; } // El nivel de stock actual
}
