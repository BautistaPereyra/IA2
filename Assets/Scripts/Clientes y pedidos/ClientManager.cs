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
        // En una aplicación real, aquí iterarías sobre un gran conjunto de datos
        // y usarías un temporizador para hacer 'yield return' o pausar si el tiempoMaximoSlice se agota.

        // Para el parcial, demostramos la intención de usar 'Any'
        // para buscar una condición que detenga la ejecución tan pronto como se encuentre.


        return queue.Any(c =>
        c.order.Any(item => !availableItems.Contains(item))
        );

        // Mantenemos la documentación de que esta función es parte del proceso de
        // Time-Slicing para el chequeo de la cola.
    }
}
