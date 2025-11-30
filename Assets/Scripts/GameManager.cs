using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    private ClientManager _clientManager;
    private InventoryManager _inventoryManager;
    private FinanceManager _financeManager;

    // ... Datos del juego (puntuación, tiempo, etc.) ...
    public List<Customer> ClientsInQueue = new List<Customer>();
    public List<Product> MasterInventory = new List<Product>();
    public List<Providers> AvailableProviders = new List<Providers>(); // Para LINQ G2 de I2
    public Queue<Sale> PendingSalesQueue = new Queue<Sale>(); // Para Time-Slicing de I3
    public List<Sale> CompletedSalesLog = new List<Sale>();

    // Variables de control del juego
    private float _gameTimer = 0f;
    private float _clientSpawnRate = 5f; // Clientes nuevos cada 5 segundos

    [SerializeField] private Customer _customerPrefab; // El componente Customer del Prefab
    [SerializeField] private List<Sprite> _customerSprites;

    [SerializeField] private UIManager _uiManager;
    [SerializeField] private CustomerSpawner _customerSpawner;

    public List<decimal> DailyExpenses = new List<decimal>();

    private float _maxPatienceTime = 30f;

    void Start()
    {
        // Instancia los módulos al inicio del juego
        _clientManager = new ClientManager();
        _inventoryManager = new InventoryManager();
        _financeManager = new FinanceManager();

        CustomerFactory customerFactory = new CustomerFactory(_customerPrefab, _customerSprites);

        // 2. Definir la Action de encendido/apagado (para el Pool)
        Action<Customer, bool> turnOnOffAction = (customer, active) => { customer.gameObject.SetActive(active); };

        // 3. Crear el Pool, pasándole el Factory y la Action
        // Usamos customerFactory.GetObj como el método de fábrica
        _customerSpawner.CustomerPool = new ObjectPool<Customer>(
            customerFactory.GetObj, // FactoryMethod (delegate)
            turnOnOffAction, // Action<T, bool>
            initialCount: 5,
            dynamic: true);


        _customerSpawner.CustomerSpawned += OnNewCustomerSpawned;
        // Cargar datos iniciales (Inventario y Clientes iniciales)
        LoadStartDate();
        StartGameLoop();
    }

    private void LoadStartDate()
    {
        // Crear productos de ejemplo
        MasterInventory.Add(new Product
        {
            ID = 1, Name = "Nail", Category = "Metal", SellPrice = 1.05m, RestokeCost = 1.00m, MaxStock = 50,
            CurrentStock = 25
        });
        MasterInventory.Add(new Product
        {
            ID = 2, Name = "Wire", Category = "Electronic", SellPrice = 1.5m, RestokeCost = 1.00m, MaxStock = 50,
            CurrentStock = 40
        });
        MasterInventory.Add(new Product
        {
            ID = 3, Name = "Screwdriver", Category = "Tool", SellPrice = 6m, RestokeCost = 5m, MaxStock = 80,
            CurrentStock = 35
        });
        MasterInventory.Add(new Product
        {
            ID = 4, Name = "Saw", Category = "Tool", SellPrice = 8m, RestokeCost = 5m, MaxStock = 30,
            CurrentStock = 20
        });
        MasterInventory.Add(new Product
        {
            ID = 5, Name = "Tube", Category = "Metal", SellPrice = 10m, RestokeCost = 12.5m, MaxStock = 120,
            CurrentStock = 120
        });
        MasterInventory.Add(new Product
        {
            ID = 6, Name = "Light bulb", Category = "Electronic", SellPrice = 3m, RestokeCost = 1.00m, MaxStock = 10,
            CurrentStock = 6
        });
        MasterInventory.Add(new Product
        {
            ID = 7, Name = "Screw", Category = "Metal", SellPrice = 0.50m, RestokeCost = 0.30m, MaxStock = 100,
            CurrentStock = 5
        }); // ¡Bajo stock a propósito!

        // Simular un proveedor (para el SelectMany del Integrante 2)
        AvailableProviders.Add(new Providers
            { Name = "Guillote", SuppliedProducts = MasterInventory.Where(p => p.Category == "Metal").ToList() });
        AvailableProviders.Add(new Providers
            { Name = "Samid", SuppliedProducts = MasterInventory.Where(p => p.Category == "Tool").ToList() });
        AvailableProviders.Add(new Providers
            { Name = "Scrocchi", SuppliedProducts = MasterInventory.Where(p => p.Category == "Electronic").ToList() });


        if (_customerSpawner != null)
        {
            _customerSpawner.AvailableProductNames = MasterInventory.Select(p => p.Name).ToList();
        }

        Debug.Log("Datos cargados correctamente.");
    }

    private void StartGameLoop()
    {
        // Normalmente iniciarías Corrutinas aquí, pero usaremos el Update() simple
    }

    void Update()
    {
        _gameTimer += Time.deltaTime;

        foreach (var c in ClientsInQueue) c.actualTimeInQueue += Time.deltaTime;

        // 2. Proceso de Cliente y Frustración (I1)
        ProcessClientQueue();

        // 3. Chequeos Periódicos y Time-Slicing (I2, I3)
        RunMaintenanceSlices();

        UpdateUIPanels();
    }

    private void UpdateStationUI()
    {
        // TRUCO: FirstOrDefault() es lo mismo que decir "Dame la posición 0"
        // Si la lista está vacía, devuelve null.
        Customer clientAtCounter = ClientsInQueue.FirstOrDefault();

        // Le mandamos a la UI SOLO al cliente de la posición 0
        // Los demás existen en la lista 'ClientsInQueue', pero la UI no los ve.
        _uiManager.UpdateCurrentClientPanel(clientAtCounter, _maxPatienceTime);
    }

    private void ProcessClientQueue()
    {
        List<Customer> customersToServe = new List<Customer>();
        List<Customer> customersToRemove = new List<Customer>();

        // A. Actualizar tiempo y buscar clientes frustrados
        foreach (var c in ClientsInQueue)
        {
            c.actualTimeInQueue += Time.deltaTime;

            // 1. LINQ G1 (I1) - Chequeo de clientes "enojados"
            if (_clientManager.AngryCustomer(ClientsInQueue, 30f).Contains(c)) // Umbral de 30s
            {
                var log = _clientManager.ClientLeaves(c, 20.00f); // Tipo Anónimo I1
                Debug.Log("Frustration Log: " + log);
                customersToRemove.Add(c);
            }
        }

        // B. Procesar Remociones
        foreach (var c in customersToRemove)
        {
            ClientsInQueue.Remove(c);

            // AGREGA ESTO: Devolver al Pool para reciclar el objeto
            _customerSpawner.ReturnCustomerToPool(c);

            // Limpiar el panel si el que se fue era el que estábamos viendo
            if (ClientsInQueue.Count == 0)
                _uiManager.UpdateCurrentClientPanel(null, _maxPatienceTime);
        }
    }

    public void ServeClient(Customer client)
    {
        // Suponemos que el cliente ya fue seleccionado y hay stock

        // 1. I1: Atender y obtener Tupla
        var logTuple = _clientManager.ToServeCustomer(client); // Tupla I1
        List<Product> productosVendidos = new List<Product>();
        decimal ingresoTotal = 0m;
        decimal costoTotal = 0m;
        // 2. I2: Descontar stock (Esta lógica debería estar en I2, pero la llamamos desde GM)
        foreach (var itemName in client.order)
        {
            var product = MasterInventory.Find(p => p.Name == itemName);
            if (product != null)
            {
                product.CurrentStock--; // Se actualiza el inventario maestro

                // Agregamos el producto a la lista temporal de esta venta
                productosVendidos.Add(product);

                // Calculamos totales ya que estamos aquí
                ingresoTotal += product.SellPrice;
                costoTotal += product.RestokeCost;
            }
        }

        // 3. I3: Registrar la Venta y Añadir a la cola pendiente (Time-Slicing)
        var newSale = new Sale
        {
            ID = CompletedSalesLog.Count + 1,
            Date = DateTime.Now,
            SoldItems = productosVendidos,
            TotalIncome = ingresoTotal,
            TotalCost = costoTotal
            // Lógica real de cálculo de costos/ingresos
        };

        PendingSalesQueue.Enqueue(newSale); // Se añade a la cola del Time-Slicing
        var saleLogObject = _financeManager.RegisterSale(newSale);
        _uiManager.AddToLog($"[VENTA] {saleLogObject}");

        ClientsInQueue.Remove(client);
        _customerSpawner.ReturnCustomerToPool(client);
        Debug.Log($"Client {client.customerName} served. Time: {logTuple.finalTime}");
    }


// C. Implementar Slices de Mantenimiento (I3 Time-Slicing)
    private void RunMaintenanceSlices()
    {
        // 1. I3: Ejecutar el Time-Slicing de transacciones
        if (PendingSalesQueue.Count > 0)
        {
            // Se procesan un máximo de 5 transacciones por frame/tick
            var processed = _financeManager.CloseBatchTransactions(PendingSalesQueue, 5); // ToList + Time-Slicing I3
            CompletedSalesLog.AddRange(processed);
            Debug.Log($"Processed {processed.Count} sales in this slice.");
        }

        // 2. I2: Revisar inventario con SkipWhile (Time-Slicing)
        var lowStockProducts = _inventoryManager.ObtenerProductosBajoStock(MasterInventory).ToList();
        if (lowStockProducts.Count > 0)
        {
            Debug.Log($"ALERT! Low stock products found: {lowStockProducts.First().Name}");
        }
    }

    private void UpdateUIPanels()
    {
        // 1. I1 Aggregate: Actualiza el reporte de salud
        string queueReport = _clientManager.GetQueueHealthReport(ClientsInQueue);
        _uiManager.UpdateQueueReport(queueReport); // <--- Delegación a UIManager

        // 2. I1 LINQ Generator: Actualiza la sugerencia de priori dad
        int priorityID = _clientManager.GetNextPriorityClientID(ClientsInQueue);
        _uiManager.HighlightPriorityClient(priorityID); // <--- Delegación a UIManager
        // Asume que tienes un TextMeshProUGUI para el Inventario en UIManager
        // _uiManager.UpdateInventoryReport(inventoryReport);

        // 3. I3 LINQ Generator (Transacciones Recientes)
        if (CompletedSalesLog.Any())
        {
            // LINQ G1 (Take + Generator)
            var topSales = _financeManager.GetTopNTransactions(CompletedSalesLog, 3);

            string salesLog = "--- TOP RECENT SALES ---\n";
            foreach (var sale in topSales)
            {
                salesLog += $"ID {sale.ID}: +{sale.TotalIncome:C} ({sale.Date.ToShortTimeString()})\n";
            }

            // Asume que tienes un TextMeshProUGUI para Ventas en UIManager
            // _uiManager.UpdateTopSalesLog(salesLog);
            Debug.Log(salesLog);
        }

        Customer currentClient = ClientsInQueue.FirstOrDefault();

        // Le pasamos el cliente Y el tiempo máximo de paciencia (ej. 30 segundos)
        _uiManager.UpdateCurrentClientPanel(currentClient, _maxPatienceTime);

        var balance = _financeManager.GetDaylyBalance(CompletedSalesLog, DailyExpenses);
        _uiManager.UpdateBalancePanel(balance.DaylyGain, balance.DaylySpending);

        // 2. Feed de Ventas (LINQ G1 + Generator)
        if (CompletedSalesLog.Any())
        {
            // Traemos las top 3 transacciones
            var topSales = _financeManager.GetTopNTransactions(CompletedSalesLog, 3);
            string feedString = "--- RECENT SALES ---\n";
            foreach (var s in topSales)
            {
                feedString += $"[{s.Date.ToShortTimeString()}] +{s.TotalIncome:C}\n";
            }

            _uiManager.UpdateSalesFeed(feedString);
        }

        // 3. Reporte de Rentabilidad (Aggregate)
        string profitReport = _financeManager.GetProfitabilityReportString(CompletedSalesLog);
        _uiManager.UpdateProfitabilityPanel(profitReport);

        string inventoryReport = _inventoryManager.GetInventoryDangerReport(MasterInventory);
        Debug.Log(inventoryReport); // Usamos Debug.Log para probar temporalmente

        // 2. Se lo pasamos al UIManager (Descomenta o agrega esta línea)
        _uiManager.UpdateInventoryReport(inventoryReport);
    }

    private void OnNewCustomerSpawned(Customer newCustomer)
    {
        // Este cliente es el objeto real del Pool
        ClientsInQueue.Add(newCustomer);
    }

    public void RestockProduct(Product productToRestock, int amount)
    {
        if (productToRestock == null || amount <= 0) return;

        // 1. I2: Solicitar Reposición y obtener Tupla (I2: RequestRestoke)
        var restockOrder = _inventoryManager.RequestRestoke(productToRestock, amount); // Tupla I2

        // 2. I2: Actualizar el Inventario Maestro
        productToRestock.CurrentStock += amount;

        // 3. I2: Registrar el evento (Tipo Anónimo I2: RegisterSuccessfullyRestoke)
        var successLog = _inventoryManager.RegisterSuccessfullyRestoke(productToRestock, amount);
        _uiManager.AddToLog($"[INVENTARIO] {successLog}"); // Usamos el UIManager

        // 4. I3: Registrar el Gasto (Necesitas una lista de gastos para I3)
        // Usamos el CostoTotal de la Tupla (Elemento 3)
        // Aunque I3 tiene la Tupla GetDaylyBalance, usaremos una lista simple de gastos para alimentarla.

        // Suponiendo que tienes una lista de gastos en GameManager:
        // Global: public List<decimal> DailyExpenses = new List<decimal>();
        // DailyExpenses.Add(restockOrder.TotalCost);

        DailyExpenses.Add(restockOrder.TotalCost); // Guardamos el costo en la lista

        _uiManager.AddToLog($"[FINANZAS] Gasto de reposición: {restockOrder.TotalCost:C}");

        // Opcional: Llamar al Dictionary Generator de I2 para ver el mapa de productos reabastecidos
        var map = _inventoryManager.GenerateRestokedMap(new List<Product> { productToRestock });
    }

    public void Button_AttendCurrentClient()
    {
        // 1. Buscamos al cliente que está en el mostrador (posición 0)
        Customer currentClient = ClientsInQueue.FirstOrDefault();

        // 2. Si existe alguien, lo atendemos
        if (currentClient != null)
        {
            ServeClient(currentClient);
            // La función ServeClient ya se encarga de sacarlo de la lista y devolverlo al pool
        }
        else
        {
            Debug.Log("No hay clientes para atender.");
        }
    }

    // Conecta este método al botón "Skip Client"
    public void Button_SkipCurrentClient()
    {
        // 1. Buscamos al cliente
        Customer currentClient = ClientsInQueue.FirstOrDefault();

        if (currentClient != null)
        {
            // 2. Lógica de saltar/rechazar

            // A. Registrar en el log (Opcional: puedes usar ClientLeaves del ClientManager)
            var log = _clientManager.ClientLeaves(currentClient, 0f); // 0 perdida por ser manual
            _uiManager.AddToLog($"[SKIPPED] {log}");

            // B. Sacarlo de la lista
            ClientsInQueue.Remove(currentClient);

            // C. Devolverlo al Pool (¡Muy importante para reciclar!)
            if (_customerSpawner != null)
            {
                _customerSpawner.ReturnCustomerToPool(currentClient);
            }

            // D. Forzar actualización de UI inmediata (opcional, el Update lo haría igual)
            // Esto evita que veas un frame del cliente viejo
            UpdateUIPanels();
        }
    }
    
    public void Button_ToggleInventory(bool show)
    {
        // Pasamos la lista maestra y una referencia a 'this' (GameManager)
        _uiManager.ToggleInventoryWindow(show, MasterInventory, this);
    }
}