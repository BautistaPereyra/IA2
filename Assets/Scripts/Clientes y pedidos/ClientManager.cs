using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

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
        return queueClients
            .GroupBy(c => c.order.FirstOrDefault())
            .Select(g => new QueueState
            {
                largestOrderItem = g.Key,
                clientsCount = g.Count(),
                averageWaitTime = g.Average(c => c.actualTimeInQueue)
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

    public static bool CheckLackOfStock(List<Customer> queue, HashSet<string> availableItems, float maxTimeSlice)
    {
        // En una aplicaci�n real, aqu� iterar�as sobre un gran conjunto de datos
        // y usar�as un temporizador para hacer 'yield return' o pausar si el tiempoMaximoSlice se agota.

        // Para el parcial, demostramos la intenci�n de usar 'Any'
        // para buscar una condici�n que detenga la ejecuci�n tan pronto como se encuentre.


        return queue.Any(c =>
            c.order.Any(item => !availableItems.Contains(item))
        );

        // Mantenemos la documentaci�n de que esta funci�n es parte del proceso de
        // Time-Slicing para el chequeo de la cola.
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
        // Llama al generator para obtener el primer cliente en la secuencia ordenada
        var priorityCustomer = GetPriorityOrders(queue).FirstOrDefault();

        return priorityCustomer?.ID ?? -1; // Devuelve el ID o -1 si la cola está vacía
    }
}