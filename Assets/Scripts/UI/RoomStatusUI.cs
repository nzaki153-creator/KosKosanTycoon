using TMPro;
using UnityEngine;

public class RoomStatusUI : MonoBehaviour
{
    [SerializeField] private string roomId;
    [SerializeField] private TMP_Text statusText;

    private void Update()
    {
        if (!RoomManager.Instance.IsRoomUnlocked(roomId))
        {
            statusText.text = "Locked";
        }
        else if (RoomManager.Instance.HasTenant(roomId))
        {
            statusText.text = "Occupied";
        }
        else
        {
            statusText.text = "Empty";
        }
    }
}