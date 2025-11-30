using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI queueReportText; // Para Aggregate I1
    [SerializeField] private TextMeshProUGUI priorityClientLabel; // Para Generator I1
    [SerializeField] private TextMeshProUGUI logText; // Para los Tipos Anónimos

    [Header("Estación Central - Cliente Actual")] [SerializeField]
    private TextMeshProUGUI currentClientNameText; // Nombre/ID

    [SerializeField] private TextMeshProUGUI currentClientOrderText; // Lista de productos
    [SerializeField] private Slider currentClientTimerSlider;
    [SerializeField] private Image sliderFillImage;

    [Header("Panel de Finanzas")] [SerializeField]
    private TextMeshProUGUI balanceText; // "Ganancia: $100 | Gasto: $50"

    [SerializeField] private TextMeshProUGUI topSalesFeedText; // Lista de últimas ventas
    [SerializeField] private TextMeshProUGUI profitReportText; // Reporte del Aggregate

    [Header("Panel de Inventario")] [SerializeField]
    private TextMeshProUGUI inventoryReportText; // Referencia al texto en Unity
    
    [Header("Sistema de Inventario")]
    [SerializeField] private GameObject inventoryWindow; // El panel completo (para abrir/cerrar)
    [SerializeField] private Transform inventoryContainer; // El objeto que tiene el Layout Group (donde van los items)
    [SerializeField] private GameObject inventorySlotPrefab; // El prefab que creamos en el paso 2

    public void ToggleInventoryWindow(bool isOpen, List<Product> allProducts, GameManager gm)
    {
        if (inventoryWindow != null) 
            inventoryWindow.SetActive(isOpen);

        if (isOpen)
        {
            GenerateInventoryList(allProducts, gm);
        }
    }
    private void GenerateInventoryList(List<Product> products, GameManager gm)
    {
        // 1. Limpiar lista anterior (Borrar hijos viejos)
        foreach (Transform child in inventoryContainer)
        {
            Destroy(child.gameObject);
        }

        // 2. Crear nuevos items
        foreach (Product p in products)
        {
            GameObject newSlot = Instantiate(inventorySlotPrefab, inventoryContainer);
            InventorySlotUI slotScript = newSlot.GetComponent<InventorySlotUI>();
            
            if (slotScript != null)
            {
                slotScript.Setup(p, gm);
            }
        }
    }
    public void UpdateCurrentClientPanel(Customer client, float maxWaitTime)
    {
        // CASO A: HAY UN CLIENTE EN LA POSICIÓN 0
        if (client != null)
        {
            // Actualizar Textos
            if (currentClientNameText != null) 
                currentClientNameText.text = client.customerName; // Ej: "Anna"
            
            if (currentClientOrderText != null) 
                currentClientOrderText.text = string.Join("\n", client.order); // Ej: "Milk\nBread"

            // Actualizar Slider (Matemática simple)
            if (currentClientTimerSlider != null)
            {
                // Calculamos porcentaje (0 a 1) basado en su tiempo actual
                float progress = client.actualTimeInQueue / maxWaitTime;
                currentClientTimerSlider.value = progress; 
                
                // Opcional: Cambiar color de la barra
                if (sliderFillImage != null)
                    sliderFillImage.color = Color.Lerp(Color.green, Color.red, progress);
            }
        }
        // CASO B: LA LISTA ESTÁ VACÍA (client llega null)
        else
        {
            if (currentClientNameText != null) currentClientNameText.text = "Esperando...";
            if (currentClientOrderText != null) currentClientOrderText.text = "";
            if (currentClientTimerSlider != null) currentClientTimerSlider.value = 0f;
        }
    }

    public void UpdateQueueReport(string reportString)
    {
        if (queueReportText != null)
        {
            queueReportText.text = reportString;
        }
    }

    public void HighlightPriorityClient(int clientID)
    {
        if (priorityClientLabel != null)
        {
            if (clientID != -1)
            {
                priorityClientLabel.text = $"PRIORIDAD: ID {clientID}";
                // Aquí podrías cambiar el color de fondo del cliente en la cola
            }
            else
            {
                priorityClientLabel.text = "Cola tranquila";
            }
        }
    }

    public void AddToLog(string message)
    {
        if (logText != null)
        {
            logText.text = message + "\n" + logText.text;
        }
    }

    public void UpdateBalancePanel(decimal income, decimal expenses)
    {
        if (balanceText != null)
        {
            decimal net = income - expenses;
            // Muestra Ingresos, Gastos y el Neto. El Neto en verde si es positivo, rojo si negativo.
            string color = net >= 0 ? "green" : "red";
            balanceText.text = $"Income: {income:C} | Expenses: {expenses:C}\nNet: <color={color}>{net:C}</color>";
        }
    }

    public void UpdateSalesFeed(string feed)
    {
        if (topSalesFeedText != null)
        {
            topSalesFeedText.text = feed;
        }
    }

    public void UpdateProfitabilityPanel(string report)
    {
        if (profitReportText != null)
        {
            profitReportText.text = report;
        }
    }

    public void UpdateInventoryReport(string report)
    {
        if (inventoryReportText != null)
        {
            inventoryReportText.text = report;
        }
        else
        {
            Debug.Log("Error: Inventory Report Not Found");
        }
    }
}