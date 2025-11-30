using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

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
        return closedWindows
            .SelectMany(s => s.SoldItems.Select(item => new { Item = item, Sale = s }))
            .GroupBy(x => x.Item.Category) // Agrupa por la Categoría del Producto
            .Select(g => new ProfitabilityReport
            {
                Category = g.Key,
                // Aggregate: Suma de Ingresos para esta categoría
                TotalIncome = g.Sum(x => x.Item.SellPrice), 
                // Aggregate: Suma de Costos para esta categoría
                TotalCost = g.Sum(x => x.Item.RestokeCost) 
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
    public IEnumerable<decimal> CalculateGrossMarginPerItem(List<decimal> salePrice, List<decimal> restokeCost)
    {
        // Combina elemento a elemento las dos listas (precioVenta - costoReposicion)
        return salePrice.Zip(restokeCost, (sale, cost) => sale - cost);
    }
    public List<Sale> CloseBatchTransactions(Queue<Sale> transactionsPendings, int maxTransactionsPerSlice)
    {
        var processedTransactions = new List<Sale>();
        int count = 0;

        // Aquí se implementa el Time-Slicing: solo procesamos 'maxTransaccionesPorSlice' por llamada
        while (transactionsPendings.Count > 0 && count < maxTransactionsPerSlice)
        {
            var sale = transactionsPendings.Dequeue();
        
            // Simulación de un cálculo pesado
            // ... Lógica para verificar impuestos y comisiones ...

            processedTransactions.Add(sale);
            count++;
        }

        // El resultado final del "slice" se convierte a List, cumpliendo el requisito G3.
        // En el GameLoop, esta lista parcial se sumaría al balance general.
        return processedTransactions.ToList(); 
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
