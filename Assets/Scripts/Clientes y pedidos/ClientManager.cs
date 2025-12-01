using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

//Nacho - 1 linq, tupla, tipo anonimo, generator / 2 linq, Aggregate, Time Slicing
public class ClientManager
{
    public class QueueState
    {
        public string largestOrderItem = default;
        public float averageWaitTime = default;
        public int clientsCount = default;
    }

    public List<QueueState> CalcularAggregate(List<Customer> queueClients)
    {
        var rawStats = queueClients.Aggregate(
            new Dictionary<string, (float totalTime, int count)>(), // Semilla (Seed)
            (acc, c) =>
            {
                // Lógica de agrupación dentro del Aggregate
                string key = c.order.FirstOrDefault() ?? "Unknown";

                if (!acc.ContainsKey(key))
                    acc[key] = (0f, 0);

                // Acumulamos tiempo y cantidad
                acc[key] = (acc[key].totalTime + c.actualTimeInQueue, acc[key].count + 1);

                return acc;
            }
        );

        // Proyección final para mantener tu tipo de retorno original (List<QueueState>)
        return rawStats.Select(kvp => new QueueState
        {
            largestOrderItem = kvp.Key,
            clientsCount = kvp.Value.count,
            averageWaitTime = kvp.Value.totalTime / kvp.Value.count
        })
        .OrderByDescending(e => e.averageWaitTime)
        .ToList();
    }

    public (int ID, float finalTime, List<string> items) ToServeCustomer(Customer c)
    {
        //logica d eatencion y calculo de tiempo...


        return (c.ID, c.actualTimeInQueue, c.order);
    }

    public object ClientLeaves(Customer c, float estimatedLoss)
    {
        var frustrationLog = new
        {
            Client = c.customerName,
            Reason = "Tiempo de espera excesivo",
            EstimatedLoss = estimatedLoss
        };

        return frustrationLog;
    }

    public IEnumerable<Customer> AngryCustomer(List<Customer> queue, float maxWaitTime)
    {
        return queue.Where(c => c.actualTimeInQueue > maxWaitTime);
    }

    public static IEnumerable<Customer> GetPriorityOrders(List<Customer> queue)
    {
        var orderedOrders = queue
            .OrderByDescending(c => c.order.Count);

        foreach (var customer in orderedOrders)
        {
            yield return customer;
        }
    }

    public IEnumerator CheckLackOfStock(List<Customer> queue, HashSet<string> availableItems, float maxTimeSlice, System.Action<bool> resultCallback)
    {
        float timer = Time.realtimeSinceStartup;

        // G3: Any (Usado dentro de la lógica, pero la estructura principal es la coroutina)
        foreach (var c in queue)
        {
            // Time-Slicing check
            if (Time.realtimeSinceStartup - timer > maxTimeSlice)
            {
                yield return null; // Espera al siguiente frame
                timer = Time.realtimeSinceStartup; // Resetea timer
            }

            // Lógica de chequeo
            if (c.order.Any(item => !availableItems.Contains(item)))
            {
                resultCallback?.Invoke(true); // Devolvemos True a través del callback
                yield break; // Cortamos la ejecución
            }
        }

        resultCallback?.Invoke(false); // Si termina el loop, devolvemos False
}

    public string GetQueueHealthReport(List<Customer> queueClients)
    {
        var report = CalcularAggregate(queueClients);
        var sb = new StringBuilder();

        if (report.Any())
        {
            sb.AppendLine("--- Queue Health Report ---");
            foreach (var state in report)
            {
                sb.AppendLine($"Category: {state.largestOrderItem ?? "Mixed"}");
                sb.AppendLine($"  Clients: {state.clientsCount}");
                sb.AppendLine($"  Avg Wait: {state.averageWaitTime:F2}s");
            }

            // Muestra el grupo más problemático (el que tiene el mayor tiempo promedio de espera)
            var worstGroup = report.First();
            sb.AppendLine(
                $"\n!! Critical Alert: {worstGroup.largestOrderItem} (Avg: {worstGroup.averageWaitTime:F2}s)");
        }
        else
        {
            sb.AppendLine("Queue is empty. All clear!");
        }

        return sb.ToString();
    }

    public int GetNextPriorityClientID(List<Customer> queue)
    {
        var priorityCustomer = queue
            .Where(c => c.order.Count > 0)          // GRUPO 1: Where
            .OrderByDescending(c => c.order.Count)  // GRUPO 2: OrderByDescending
            .FirstOrDefault();                      // GRUPO 3: FirstOrDefault (o podrías usar ToList().First())

        return priorityCustomer?.ID ?? -1;
    }
}