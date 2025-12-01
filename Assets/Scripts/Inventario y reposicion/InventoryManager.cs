using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;


//Celeste - 2 linq, generator, tupla y tipo anonimo / 1 linq, Aggregate, Time slicing
public class InventoryManager
{
    public class DangerReport
    {
        public string DangerZone { get; set; } // "Crítico", "Bajo", "Normal"
        public int TotalProducts { get; set; }
        public float StockRate { get; set; } // Aggregate más complejo
    }

    public List<DangerReport> GenerarAggregatePeligro(List<Product> inventory)
    {
        return inventory
            .GroupBy(p =>
            {
                // Definición de la Zona de Peligro
                float percentage = (float)p.CurrentStock / p.MaxStock;
                if (percentage <= 0.10f) return "Crítico"; // Stock < 10%
                if (percentage <= 0.50f) return "Bajo"; // Stock entre 10% y 50%
                return "Normal";
            })
            .Select(g => new DangerReport
            {
                DangerZone = g.Key,
                TotalProducts = g.Count(),
                // Usamos el Aggregate 'Average' para el porcentaje promedio.
                StockRate = g.Average(p => (float)p.CurrentStock / p.MaxStock) * 100
            })
            .OrderBy(r => r.DangerZone) // Opcional: ordenar por prioridad
            .ToList();
    }

    public IEnumerable<Product> ObtenerProductosBajoStock(List<Product> inventory)
    {
        // En el juego real, esta iteración sería monitoreada por un temporizador (Time-Slice).
        // Aquí usamos SkipWhile para saltar todo lo que está BIEN (stock > 75%)

        // Devolvemos los productos que tienen stock bajo (los que quedan después del skip)
        return inventory
            .OrderByDescending(p => p.CurrentStock)
            .SkipWhile(p => ((float)p.CurrentStock / p.MaxStock) > 0.75f);
    }

    public IEnumerable<Product> GetAllTheCatalogue(List<Providers> providers)
    {
        // Aplanamos la lista de listas (ProductosSuministrados de cada Proveedor)
        return providers.SelectMany(p => p.SuppliedProducts);
    }

    public Dictionary<int, int> GenerateRestokedMap(List<Product> restokedProducts)
    {
        // Creamos un generador (yield) para producir los pares Key-Value
        // antes de que ToDictionary los consuma.
        IEnumerable<KeyValuePair<int, int>> PairGenerator()
        {
            foreach (var p in restokedProducts)
            {
                // Solo devolvemos productos que realmente se reabastecieron
                if (p.CurrentStock > p.MaxStock * 0.5m)
                {
                    yield return new KeyValuePair<int, int>(p.ID, p.CurrentStock);
                }
            }
        }

        // Convertimos la secuencia generada a un Dictionary
        return PairGenerator().ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public (int ProductID, int Amount, decimal TotalCost) RequestRestoke(Product p, int amount)
    {
        decimal totalCost = amount * p.RestokeCost;
        // ... lógica para actualizar el stock ...
        return (p.ID, amount, totalCost);
    }

    public object RegisterSuccessfullyRestoke(Product p, int amount)
    {
        var successfullyRestoke = new
        {
            Producto = p.Name,
            Unidad = p.Category,
            Cantidad = amount,
            Tiempo = DateTime.Now
        };
        return successfullyRestoke;
    }
    
    public string GetInventoryDangerReport(List<Product> inventory)
    {
        var report = GenerarAggregatePeligro(inventory);
        StringBuilder sb = new StringBuilder();
        
        sb.AppendLine("--- INVENTORY STATUS ---");
        
        foreach (var state in report)
        {
            // Ejemplo visual: "Crítico (2 items): 5.0% Avg"
            string color = state.DangerZone == "Crítico" ? "red" : "white";
            sb.AppendLine($"<color={color}>{state.DangerZone}</color> ({state.TotalProducts}): {state.StockRate:F1}%");
        }
        
        // Alerta extra si hay críticos
        var critical = report.FirstOrDefault(r => r.DangerZone == "Crítico");
        if (critical != null)
        {
            sb.AppendLine($"\n!! RESTOCK NEEDED: {critical.TotalProducts} items !!");
        }

        return sb.ToString();
    }
}