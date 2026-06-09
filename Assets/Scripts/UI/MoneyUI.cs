using TMPro;
using UnityEngine;

public class MoneyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;

    private void Start()
    {
        MoneyManager.Instance.OnMoneyChanged += UpdateUI;
        UpdateUI(MoneyManager.Instance.GetMoney());
    }

    private void UpdateUI(long value)
    {
        moneyText.text = "Money: " + value.ToString();
    }
}