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

    private Dictionary<string, int> roomLevels = new();

    private const int MAX_ROOM_LEVEL = 10;

    private long pendingOfflineIncome = 0;

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
        if (pauseStatus && MoneyManager.Instance != null)
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

        if (!roomLevels.ContainsKey(roomId))
        {
            roomLevels.Add(roomId, 1);
        }

        StartCoroutine(TenantMoveIn(roomId));

        Debug.Log("ROOM DIBELI: " + room.roomName);

        SaveGame();
    }

    public void SaveGame()
    {
        SaveData data = new SaveData();

        if (MoneyManager.Instance != null)
        {
            data.money = MoneyManager.Instance.GetMoney();
        }
        else
        {
            data.money = 0;
        }
        data.unlockedRooms = new List<string>(unlockedRooms);
        data.occupiedRooms = new List<string>(occupiedRooms);

        data.roomLevelIds = new List<string>();
        data.roomLevels = new List<int>();

        foreach (var pair in roomLevels)
        {
            data.roomLevelIds.Add(pair.Key);
            data.roomLevels.Add(pair.Value);
        }

        data.lastSaveTime = System.DateTime.UtcNow.ToString("o");

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

        roomLevels.Clear();

        if (data.roomLevelIds != null &&
            data.roomLevels != null)
        {
            for (int i = 0; i < Mathf.Min(data.roomLevelIds.Count, data.roomLevels.Count); i++)
            {
                roomLevels[data.roomLevelIds[i]] = data.roomLevels[i];
            }
        }

        Debug.Log("Game Loaded");

        foreach (string roomId in unlockedRooms)
        {
            if (!roomLevels.ContainsKey(roomId))
            {
                roomLevels.Add(roomId, 1);
            }
        }

        CalculateOfflineIncome(data.lastSaveTime);
    }

    private void CalculateOfflineIncome(string lastSaveTime)
    {
        if (string.IsNullOrEmpty(lastSaveTime))
            return;

        if (!System.DateTime.TryParse(lastSaveTime, null,
            System.Globalization.DateTimeStyles.RoundtripKind,
            out System.DateTime savedTime))
            return;

        double offlineSeconds = (System.DateTime.UtcNow - savedTime).TotalSeconds;

        if (offlineSeconds < 5f)
            return;

        // cap 8 jam
        double cappedSeconds = System.Math.Min(offlineSeconds, 28800);

        long totalIncome = 0;

        foreach (var room in rooms)
        {
            if (unlockedRooms.Contains(room.roomId))
            {
                int level = GetRoomLevel(room.roomId);
                totalIncome += room.incomePerCycle * level;
            }
        }

        // hitung cycles (pakai interval 5 detik)
        long cycles = (long)(cappedSeconds / 5f);

        pendingOfflineIncome = totalIncome * cycles;

        if (pendingOfflineIncome > 0)
        {
            MoneyManager.Instance.AddMoney(pendingOfflineIncome);
        }

        Debug.Log("Offline Income: " + pendingOfflineIncome);
    }

    public long TakeOfflineIncome()
    {
        long value = pendingOfflineIncome;
        pendingOfflineIncome = 0;
        return value;
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
                int level = GetRoomLevel(room.roomId);

                totalIncome += room.incomePerCycle * level;
            }
        }

        if (totalIncome > 0 && MoneyManager.Instance != null)
        {
            MoneyManager.Instance.AddMoney(totalIncome);
        }

        //Debug.Log("Income + " + totalIncome);
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

    public int GetRoomLevel(string roomId)
    {
        if (roomLevels.ContainsKey(roomId))
            return roomLevels[roomId];

        return 1;
    }

    public long GetRoomIncome(string roomId)
    {
        RoomData room = GetRoom(roomId);

        if (room == null)
            return 0;

        int level = GetRoomLevel(roomId);

        return room.incomePerCycle * level;
    }

    public int GetOccupiedRoomCount()
    {
        return occupiedRooms.Count;
    }

    public int GetTotalRoomCount()
    {
        return rooms.Count;
    }

    public long GetTotalIncome()
    {
        long total = 0;

        foreach (string roomId in occupiedRooms)
        {
            total += GetRoomIncome(roomId);
        }

        return total;
    }

    public bool UpgradeRoom(string roomId)
    {
        Debug.Log("UpgradeRoom dipanggil: " + roomId);

        if (!unlockedRooms.Contains(roomId))
        {
            Debug.Log("Room belum dibeli");
            return false;
        }

        int currentLevel = GetRoomLevel(roomId);

        if (currentLevel >= MAX_ROOM_LEVEL)
        {
            Debug.Log("Room sudah max level");
            return false;
        }

        long upgradeCost = currentLevel * 100;

        if (!MoneyManager.Instance.SpendMoney(upgradeCost))
        {
            Debug.Log("Uang tidak cukup");
            return false;
        }

        roomLevels[roomId] = currentLevel + 1;

        SaveGame();

        Debug.Log(
            $"Upgrade {roomId} " +
            $"Level {currentLevel} -> {currentLevel + 1}"
        );

        return true;
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

    [ContextMenu("PRINT ROOM LEVELS")]
    private void PrintRoomLevels()
    {
        foreach (var pair in roomLevels)
        {
            Debug.Log($"{pair.Key} = Level {pair.Value}");
        }
    }

    [ContextMenu("DEBUG UPGRADE ROOM 1")]
    private void DebugUpgradeRoom1()
    {
        foreach (var room in rooms)
        {
            UpgradeRoom(room.roomId);
            break;
        }
    }

    [ContextMenu("DELETE SAVE")]
    private void DeleteSave()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();

        Debug.Log("SAVE DIHAPUS");
    }
}