using System;
using System.Collections.Generic;

public class ObjectPool<T>
{
    public delegate T FactoryMethod();
    private FactoryMethod _factory = default;

    private Action<T, bool> _turnOnOff = default;

    private List<T> _stock = new();

    private bool _dynamic = false;

    public ObjectPool(FactoryMethod factory, Action<T, bool> TurnOnOff, int initialCount = 5, bool dynamic = true)
    {
        _factory = factory;
        _turnOnOff = TurnOnOff;

        for (int i = 0; i < initialCount; i++)
        {
            var obj = _factory();

            _turnOnOff(obj, false);

            _stock.Add(obj);
        }
        _dynamic = dynamic;
    }

    public T Get()
    {
        T obj = default;

        if (_stock.Count > 0)
        {
            obj = _stock[0];

            _stock.RemoveAt(0);
        }
        else if (_dynamic)
        {
            obj = _factory();
        }

        if (obj != null) _turnOnOff(obj, true);

        return obj;
    }

    public void StockAdd(T obj)//ReturnStock
    {
        _turnOnOff(obj, false);

        _stock.Add(obj);
    }
}