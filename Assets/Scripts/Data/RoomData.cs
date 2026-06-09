using UnityEngine;

[CreateAssetMenu(
    fileName = "RoomData",
    menuName = "KosTycoon/Room Data"
)]
public class RoomData : ScriptableObject
{
    [Header("Identity")]
    public string roomId;
    public string roomName;

    [Header("Economy")]
    public long purchaseCost;
    public long incomePerCycle;

    [Header("Visual")]
    public Sprite roomSprite;
}