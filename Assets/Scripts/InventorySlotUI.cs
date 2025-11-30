using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI stockText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button restockButton;

    private Product _myProduct;
    private GameManager _gameManager;
    private int _restockAmount = 10; // Cantidad fija a reponer por click

    // Este método se llamará cuando creemos la fila
    public void Setup(Product product, GameManager gm)
    {
        _myProduct = product;
        _gameManager = gm;

        RefreshData();

        // Configurar el botón
        restockButton.onClick.RemoveAllListeners(); // Limpiar eventos viejos
        restockButton.onClick.AddListener(OnRestockClicked);
    }

    public void RefreshData()
    {
        if (_myProduct != null)
        {
            nameText.text = _myProduct.Name;
            
            // Cambiar color si es crítico (Visual feedback)
            if (_myProduct.CurrentStock <= _myProduct.MaxStock * 0.1f)
                stockText.text = $"<color=red>{_myProduct.CurrentStock}</color> / {_myProduct.MaxStock}";
            else
                stockText.text = $"{_myProduct.CurrentStock} / {_myProduct.MaxStock}";

            costText.text = $"${_myProduct.RestokeCost * _restockAmount} (x{_restockAmount})";
        }
    }

    private void OnRestockClicked()
    {
        if (_gameManager != null && _myProduct != null)
        {
            // Llamamos al método del GameManager para reponer
            _gameManager.RestockProduct(_myProduct, _restockAmount);
            
            // Actualizamos visualmente el texto del stock inmediatamente
            RefreshData();
        }
    }
}
