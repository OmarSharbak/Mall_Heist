using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public int activeSaveSlot = 1;
    public List<LevelData> levelsData = new List<LevelData>();
}

[System.Serializable]
public class LevelData
{
    public string levelName;
    public bool isUnlocked = false;
    public float bestTime = float.MaxValue;
    public string bestTier = "None";
    public List<bool> achievements = new List<bool>();
    public bool finished = false;
}
