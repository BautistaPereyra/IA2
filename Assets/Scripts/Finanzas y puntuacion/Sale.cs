using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sale
{
    public int ID { get; set; }
    public List<Product> SoldItems { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalCost { get; set; } // El costo de reponer esos items
    public DateTime Date { get; set; }
}
