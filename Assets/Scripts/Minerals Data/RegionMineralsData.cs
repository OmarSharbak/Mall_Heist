using System;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.InventoryEngine;

namespace JayMountains
{
    [CreateAssetMenu(fileName = "RegionMineralsData", menuName = "JayMountains/RegionMineralsData", order = 1)]
    [Serializable]
    public class RegionMineralsData : ScriptableObject
    {
        public string Region;
        public List<MineralData> Minerals = new();
    }


    [Serializable]
    public class MineralData
    {
        public int Index;
        public MineralItem MineralItem;

        // This is basically a temp (not supposed to be used in actual production) state where I'm waiting for Chris's reply for the excel sheet.
        public enum ExcelState
        {
            Good,      // Green.
            Undecided, // Yellow.
            NotInGame, // Red.
        }
        public ExcelState ChrisCheck = MineralData.ExcelState.Undecided;
    }
}