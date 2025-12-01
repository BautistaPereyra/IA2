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
        var rawStats = inventory.Aggregate(
             new Dictionary<string, (int count, float sumPct)>(), // Semilla
             (acc, p) =>
             {
                 // Lógica de clasificación dentro del Aggregate
                 float percentage = (float)p.CurrentStock / p.MaxStock;
                 string zone = "Normal";

                 if (percentage <= 0.10f) zone = "Crítico";
                 else if (percentage <= 0.50f) zone = "Bajo";

                 if (!acc.ContainsKey(zone))
                     acc[zone] = (0, 0f);

                 // Acumulamos
                 acc[zone] = (acc[zone].count + 1, acc[zone].sumPct + percentage);

                 return acc;
             });

        // Proyección final
        return rawStats.Select(kvp => new DangerReport
        {
            DangerZone = kvp.Key,
            TotalProducts = kvp.Value.count,
            StockRate = (kvp.Value.sumPct / kvp.Value.count) * 100 // Promedio
        })
        .OrderBy(r => r.DangerZone)
        .ToList();
    }

    public List<Product> ObtenerProductosBajoStock(List<Product> inventory)
    {
        return inventory
            .Where(p => ((float)p.CurrentStock / p.MaxStock) <= 0.75f) // G1: Where (Filtro)
            .OrderByDescending(p => p.CurrentStock)                    // G2: OrderByDescending (Orden)
            .ToList();                                                 // G3: ToList (Ejecución inmediata)
    }

    public IEnumerator CheckInventoryHealthRoutine(List<Product> inventory, float maxTimeSlice, Action<Product> alertCallback)
    {
        float timer = Time.realtimeSinceStartup;

        foreach (var p in inventory)
        {
            // Time-Slicing check
            if (Time.realtimeSinceStartup - timer > maxTimeSlice)
            {
                yield return null; // Esperar al siguiente frame
                timer = Time.realtimeSinceStartup;
            }

            // Chequeo de condición crítica (ej: stock 0)
            if (p.CurrentStock <= 0)
            {
                alertCallback?.Invoke(p); // Notificamos
                // No hacemos yield break aquí para seguir chequeando el resto, 
                // a menos que solo quieras encontrar el primero.
            }
        }
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