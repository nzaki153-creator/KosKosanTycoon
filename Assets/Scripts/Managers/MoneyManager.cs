using System;
using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance { get; private set; }

    public event Action<long> OnMoneyChanged;

    [SerializeField] private long money = 0;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AddMoney(long amount)
    {
        money += amount;
        OnMoneyChanged?.Invoke(money);
    }

    public bool SpendMoney(long amount)
    {
        if (money < amount) return false;

        money -= amount;
        OnMoneyChanged?.Invoke(money);
        return true;
    }

    public long GetMoney()
    {
        return money;
    }

    public void SetMoney(long value)
    {
        money = value;
        OnMoneyChanged?.Invoke(money);
    }

}