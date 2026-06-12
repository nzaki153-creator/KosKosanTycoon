using TMPro;
using UnityEngine;

public class OfflineIncomeUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI text;

    private void Start()
    {
        long income = RoomManager.Instance.TakeOfflineIncome();

        if (income <= 0)
        {
            panel.SetActive(false);
            return;
        }

        panel.SetActive(true);
        text.text = "+ " + income.ToString("N0");
    }
}