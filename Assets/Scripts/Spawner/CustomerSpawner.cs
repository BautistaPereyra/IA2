using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [HideInInspector] public ObjectPool<Customer> CustomerPool; 
    
    [Header("Configuración de Spawn")]
    [SerializeField] private Transform spawnPoint; // Punto donde aparece el cliente
    [SerializeField] private float minSpawnInterval = 3f;
    [SerializeField] private float maxSpawnInterval = 7f;
    
    // CONTROL DEL TIEMPO
    private float currentSpawnTimer;
    private float nextSpawnTime;
    private int _nextCustomerID = 1000;

    // EVENTO PARA NOTIFICAR AL GAMEMANAGER
    public delegate void OnCustomerSpawned(Customer newCustomer);
    public event OnCustomerSpawned CustomerSpawned;
    
    public List<string> AvailableProductNames = new List<string>();

    void Start()
    {
        SetNextSpawnTime();
    }

    void Update()
    {
        // Lógica de spawn con tiempo aleatorio
        currentSpawnTimer += Time.deltaTime;
        if (currentSpawnTimer >= nextSpawnTime)
        {
            SpawnCustomer();
            SetNextSpawnTime();
        }
    }

    private void SetNextSpawnTime()
    {
        nextSpawnTime = Random.Range(minSpawnInterval, maxSpawnInterval);
        currentSpawnTimer = 0f;
    }

    private void SpawnCustomer()
    {
        if (CustomerPool == null) return;

        // 1. Obtener el cliente de la Pool (reutilizado o creado por Factory)
        Customer newCustomer = CustomerPool.Get(); 
        
        if (newCustomer == null) return;

        // 2. Posicionar y Asignar datos dinámicos (Setup)
        
        // El CustomerPool.Get() ya se encarga de activar el GameObject
        newCustomer.transform.position = spawnPoint.position;
        newCustomer.transform.rotation = Quaternion.identity;

        // Asignar datos de juego
        newCustomer.ID = _nextCustomerID++;
        newCustomer.customerName = "Client " + newCustomer.ID;
        newCustomer.actualTimeInQueue = 0f;
        newCustomer.order = GenerateRandomOrder(); // Generar el pedido

        // 3. Notificar al GameManager
        CustomerSpawned?.Invoke(newCustomer);
    }

    // MÉTODO PARA DEVOLVER CLIENTES AL POOL (Llamado desde GameManager)
    public void ReturnCustomerToPool(Customer customer)
    {
        if (CustomerPool != null)
        {
            // StockAdd() llama internamente a la Action<T, bool> para desactivar el objeto.
            CustomerPool.StockAdd(customer); 
        }
    }

    // LÓGICA DE PEDIDO
    private List<string> GenerateRandomOrder()
    {
        // Lista de ejemplo: idealmente, usaría el InventoryManager para obtener nombres válidos
        List<string> order = new List<string>();
        
        if (AvailableProductNames == null || AvailableProductNames.Count == 0)
        {
            Debug.LogWarning("No hay productos disponibles para generar orden. Usando default.");
            return new List<string> { "Nail" }; // Fallback por si acaso
        }
        int numItems = Random.Range(1, 4); 
        
        for (int i = 0; i < numItems; i++)
        {
            string randomItem = AvailableProductNames[Random.Range(0, AvailableProductNames.Count)];
            order.Add(randomItem);
        }
        return order;
    }
}
