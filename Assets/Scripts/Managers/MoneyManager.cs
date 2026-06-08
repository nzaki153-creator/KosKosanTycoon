using UnityEngine;
using System;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance { get; private set; }

    public event Action<long> OnMoneyChanged;

    private const string MONEY_KEY = "PlayerMoney";
    private const long START_MONEY = 5000000;

    private long currentMoney;

    public long CurrentMoney => currentMoney;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        LoadMoney();

        Debug.Log("Money Loaded: " + currentMoney);
    }
    public void AddMoney(long amount)
    {
        if (amount <= 0) return;

        currentMoney += amount;

        SaveMoney();

        OnMoneyChanged?.Invoke(currentMoney);
    }

    public bool SpendMoney(long amount)
    {
        if (amount <= 0) return false;

        if (currentMoney < amount)
            return false;

        currentMoney -= amount;

        SaveMoney();

        OnMoneyChanged?.Invoke(currentMoney);

        return true;
    }

    private void SaveMoney()
    {
        PlayerPrefs.SetString(MONEY_KEY, currentMoney.ToString());
        PlayerPrefs.Save();
    }

    private void LoadMoney()
    {
        string savedMoney =
            PlayerPrefs.GetString(MONEY_KEY, START_MONEY.ToString());

        if (long.TryParse(savedMoney, out long loadedMoney))
        {
            currentMoney = loadedMoney;
        }
        else
        {
            currentMoney = START_MONEY;
        }
    }
    [ContextMenu("Reset Money")]
    private void ResetMoney()
    {
        PlayerPrefs.DeleteKey(MONEY_KEY);
    }
    [ContextMenu("Add 1 Juta")]
    private void DebugAddMoney()
    {
        AddMoney(1000000);
    }

    [ContextMenu("Spend 500 Ribu")]
    private void DebugSpendMoney()
    {
        SpendMoney(500000);
    }
}