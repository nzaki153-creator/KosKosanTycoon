using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public long money;

    public List<string> unlockedRooms = new();

    public List<string> occupiedRooms = new();

    public List<string> roomLevelIds = new();

    public List<int> roomLevels = new();
}