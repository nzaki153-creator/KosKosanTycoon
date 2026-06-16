using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeButtonUI : MonoBehaviour
{
    [SerializeField] private string roomId;
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text buttonText;

    private void Start()
    {
        button.onClick.AddListener(OnClick);
    }

    private void Update()
    {
        int level = RoomManager.Instance.GetRoomLevel(roomId);
        long cost = level * 100;

        buttonText.text =
            $"Upgrade\nRp {cost}";

        button.interactable =
            MoneyManager.Instance.GetMoney() >= cost;
    }

    private void OnClick()
    {
        RoomManager.Instance.UpgradeRoom(roomId);
    }
}