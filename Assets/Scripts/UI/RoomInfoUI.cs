using TMPro;
using UnityEngine;

public class RoomInfoUI : MonoBehaviour
{
    [SerializeField] private string roomId;
    [SerializeField] private TMP_Text infoText;

    private void Update()
    {
        int level =
            RoomManager.Instance.GetRoomLevel(roomId);

        long income =
            RoomManager.Instance.GetRoomIncome(roomId);

        infoText.text =
            $"Level {level}\nIncome {income} / 5s";
    }
}