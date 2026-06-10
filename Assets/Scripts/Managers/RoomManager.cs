using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    // Singleton
    public static RoomManager Instance { get; private set; }

    // SAVE KEY
    private const string SAVE_KEY = "GAME_SAVE";

    [Header("Room Database")]
    [SerializeField] private List<RoomData> rooms = new();

    // runtime state
    private HashSet<string> unlockedRooms = new();
    private HashSet<string> occupiedRooms = new();
    private HashSet<string> pendingTenants = new();
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        LoadGame();
        StartTenantTimers();
        Debug.Log("=== ROOM LIST ===");

        foreach (var room in rooms)
        {
            Debug.Log(
                $"ID: {room.roomId} | " +
                $"Name: {room.roomName} | " +
                $"Income: {room.incomePerCycle}"
            );
        }

        StartCoroutine(IncomeLoop());
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveGame();
        }
    }

    // =========================
    // BUY ROOM
    // =========================
    public void BuyRoom(string roomId)
    {
        Debug.Log("BuyRoom dipanggil: " + roomId);
        if (unlockedRooms.Contains(roomId))
        {
            Debug.Log("Room sudah dibeli: " + roomId);
            return;
        }

        RoomData room = GetRoom(roomId);

        if (room == null)
        {
            Debug.LogWarning("Room tidak ditemukan: " + roomId);
            return;
        }

        unlockedRooms.Add(roomId);

        StartCoroutine(TenantMoveIn(roomId));

        Debug.Log("ROOM DIBELI: " + room.roomName);
    }

    public void SaveGame()
    {
        SaveData data = new SaveData();

        data.money = MoneyManager.Instance.GetMoney();
        data.unlockedRooms = new List<string>(unlockedRooms);
        data.occupiedRooms = new List<string>(occupiedRooms);

        string json = JsonUtility.ToJson(data);

        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();

        Debug.Log("Game Saved");
    }

    public void LoadGame()
    {
        if (!PlayerPrefs.HasKey(SAVE_KEY))
        {
            Debug.Log("No Save Found");
            return;
        }

        string json = PlayerPrefs.GetString(SAVE_KEY);

        SaveData data = JsonUtility.FromJson<SaveData>(json);

        unlockedRooms = new HashSet<string>(data.unlockedRooms);
        occupiedRooms = data.occupiedRooms != null
    ? new HashSet<string>(data.occupiedRooms)
    : new HashSet<string>();

        MoneyManager.Instance.SetMoney(data.money);

        Debug.Log("Game Loaded");
    }

    // =========================
    // INCOME LOOP
    // =========================
    private IEnumerator IncomeLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            GenerateIncome();
        }
    }

    private IEnumerator TenantMoveIn(string roomId)
    {
        if (pendingTenants.Contains(roomId))
            yield break;

        pendingTenants.Add(roomId);

        yield return new WaitForSeconds(5f);

        AddTenant(roomId);

        pendingTenants.Remove(roomId);

        SaveGame();

        Debug.Log("Tenant otomatis masuk: " + roomId);
    }
    private void GenerateIncome()
    {
        long totalIncome = 0;

        foreach (var room in rooms)
        {
            if (occupiedRooms.Contains(room.roomId))
            {
                totalIncome += room.incomePerCycle;
            }
        }

        if (totalIncome > 0 && MoneyManager.Instance != null)
        {
            MoneyManager.Instance.AddMoney(totalIncome);
        }

        Debug.Log("Income + " + totalIncome);
    }

    // =========================
    // HELPER
    // =========================
    private RoomData GetRoom(string roomId)
    {
        foreach (var room in rooms)
        {
            if (room.roomId == roomId)
                return room;
        }

        return null;
    }

    public bool IsRoomUnlocked(string roomId)
    {
        return unlockedRooms.Contains(roomId);
    }

    public bool HasTenant(string roomId)
    {
        return occupiedRooms.Contains(roomId);
    }

    public void AddTenant(string roomId)
    {
        Debug.Log("AddTenant dipanggil: " + roomId);
        if (!unlockedRooms.Contains(roomId))
        {
            Debug.LogWarning("Room belum dibeli: " + roomId);
            return;
        }

        if (occupiedRooms.Contains(roomId))
        {
            Debug.Log("Room sudah memiliki tenant: " + roomId);
            return;
        }

        occupiedRooms.Add(roomId);

        Debug.Log("Tenant masuk ke room: " + roomId);
    }

    private void StartTenantTimers()
    {
        foreach (string roomId in unlockedRooms)
        {
            if (!occupiedRooms.Contains(roomId))
            {
                StartCoroutine(TenantMoveIn(roomId));
            }
        }
    }
    public void RemoveTenant(string roomId)
    {
        occupiedRooms.Remove(roomId);

        Debug.Log("Tenant keluar dari room: " + roomId);
    }

    [ContextMenu("DELETE SAVE")]
    private void DeleteSave()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();

        Debug.Log("SAVE DIHAPUS");
    }
}