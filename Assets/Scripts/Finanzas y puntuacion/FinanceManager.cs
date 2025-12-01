using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

//Bauti - 1 linq, tupla, tipo anonimo, generator / 2 linq, Aggregate, Time slicing
public class FinanceManager
{
// Estructura de resultado para el Aggregate
    public class ProfitabilityReport
    {
        public string Category { get; set; } // Ej: "Snacks", "Perecederos"
        public decimal TotalIncome { get; set; }
        public decimal TotalCost { get; set; }
        public decimal NetProfit => TotalIncome - TotalCost; // Propiedad calculada
    }

    public List<ProfitabilityReport> GenerateAggregateProfit(List<Sale> closedWindows)
    {
        // Aplanamos primero las ventas a items individuales
        var allSoldItems = closedWindows.SelectMany(s => s.SoldItems);

        // Acumulador: Dictionary<Categoria, (Ingreso, Costo)>
        var rawStats = allSoldItems.Aggregate(
            new Dictionary<string, (decimal income, decimal cost)>(), // Semilla
            (acc, item) =>
            {
                // Lógica de agrupación
                string cat = item.Category;

                if (!acc.ContainsKey(cat))
                    acc[cat] = (0m, 0m);

                // Acumulación compleja
                acc[cat] = (
                    acc[cat].income + item.SellPrice,
                    acc[cat].cost + item.RestokeCost
                );

                return acc;
            });

        // Proyección final
        return rawStats.Select(kvp => new ProfitabilityReport
        {
            Category = kvp.Key,
            TotalIncome = kvp.Value.income,
            TotalCost = kvp.Value.cost
        })
        .OrderByDescending(r => r.NetProfit)
        .ToList();
    }
    public IEnumerable<Sale> GetTopNTransactions(List<Sale> saleLogs, int N)
    {
        // 1. Generator: Simula un flujo de datos continuo que solo se lee cuando se itera.
        IEnumerable<Sale> SaleFLow()
        {
            // En un juego real, esto podría ser un flujo de datos que no termina.
            foreach (var sale in saleLogs.OrderByDescending(v => v.Date))
            {
                yield return sale;
            }
        }

        // 2. Take: Solo consume los primeros N elementos del flujo generado.
        return SaleFLow().Take(N);
    }
    public List<Sale> GetHighValueSales(List<Sale> sales)
    {
        return sales
            .Where(s => s.TotalIncome > 50m)          // G1: Where
            .OrderByDescending(s => s.TotalIncome)    // G2: OrderByDescending
            .ToList();                                // G3: ToList
    }
    public IEnumerable<decimal> CalculateGrossMarginPerItem(List<decimal> salePrice, List<decimal> restokeCost)
    {
        // Combina elemento a elemento las dos listas (precioVenta - costoReposicion)
        return salePrice.Zip(restokeCost, (sale, cost) => sale - cost);
    }
    public IEnumerator ProcessTransactionsRoutine(Queue<Sale> transactionsPendings, float maxTimeSlice, Action<List<Sale>> onBatchComplete)
    {
        var processedBatch = new List<Sale>();
        float timer = Time.realtimeSinceStartup;

        // Mientras queden transacciones...
        while (transactionsPendings.Count > 0)
        {
            // Chequeo de tiempo (Time-Slicing)
            if (Time.realtimeSinceStartup - timer > maxTimeSlice)
            {
                yield return null; // Esperar al siguiente frame
                timer = Time.realtimeSinceStartup;
            }

            var sale = transactionsPendings.Dequeue();

            // Simulación de proceso pesado (impuestos, analíticas, etc.)
            // ...

            processedBatch.Add(sale);
        }

        // Devolvemos el lote procesado al finalizar
        onBatchComplete?.Invoke(processedBatch);
    }
    public (decimal DaylyGain, decimal DaylySpending) GetDaylyBalance(List<Sale> sales, List<decimal> costs)
    {
        decimal gain = sales.Sum(s => s.TotalIncome);
        decimal spend = costs.Sum(); // Suma de todos los costos de reposición
    
        // Devolvemos el balance como una Tupla
        return (gain, spend);
    }
    public object RegisterSale(Sale t)
    {
        var registry = new 
        { 
            IdTransaccion = t.ID, 
            Total = t.TotalIncome, 
            NetProfit = t.TotalIncome - t.TotalCost, 
            Time = DateTime.Now 
        };
        return registry;
    }
    public string GetProfitabilityReportString(List<Sale> closedSales)
    {
        var report = GenerateAggregateProfit(closedSales);
        StringBuilder sb = new StringBuilder();
        
        sb.AppendLine("--- PROFITABILITY REPORT ---");
        
        if (report.Count == 0)
        {
            sb.AppendLine("No data available yet.");
            return sb.ToString();
        }

        foreach (var item in report)
        {
            // Ejemplo: "Snacks: +$150.00 (Net: $50.00)"
            sb.AppendLine($"{item.Category}: +{item.TotalIncome:C} (Net: {item.NetProfit:C})");
        }

        // Destacar la mejor categoría
        var bestCategory = report.First();
        sb.AppendLine($"\n$$ Best Performer: {bestCategory.Category}");

        return sb.ToString();
    }
    
    
}
