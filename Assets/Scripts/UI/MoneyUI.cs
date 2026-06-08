using UnityEngine;
using TMPro;

public class MoneyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;

    private void Start()
    {
        UpdateMoney(MoneyManager.Instance.CurrentMoney);

        MoneyManager.Instance.OnMoneyChanged += UpdateMoney;
    }

    private void OnDestroy()
    {
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.OnMoneyChanged -= UpdateMoney;
        }
    }

    private void UpdateMoney(long amount)
    {
        moneyText.text = "Rp " + amount.ToString();
    }
}