using UnityEngine;
using UnityEngine.UI;

public class UpgradeButtonUI : MonoBehaviour
{
    [SerializeField] private string roomId;
    [SerializeField] private Button button;

    private void Start()
    {
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        RoomManager.Instance.UpgradeRoom(roomId);
    }
}