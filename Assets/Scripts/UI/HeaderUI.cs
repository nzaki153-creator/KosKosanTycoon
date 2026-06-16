using TMPro;
using UnityEngine;

public class HeaderUI : MonoBehaviour
{
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI incomeText;
    public TextMeshProUGUI roomCountText;

    private void Update()
    {
        moneyText.text =
            $"Rp {MoneyManager.Instance.GetMoney()}";

        incomeText.text =
            $"Income: Rp {CalculateIncome()} / 5s";

        roomCountText.text =
            $"Kamar: {GetOccupiedRooms()} / {GetTotalRooms()}";
    }

    private long CalculateIncome()
    {
        return RoomManager.Instance.GetTotalIncome();
    }

    private int GetOccupiedRooms()
    {
        return RoomManager.Instance.GetOccupiedRoomCount();
    }

    private int GetTotalRooms()
    {
        return RoomManager.Instance.GetTotalRoomCount();
    }
}