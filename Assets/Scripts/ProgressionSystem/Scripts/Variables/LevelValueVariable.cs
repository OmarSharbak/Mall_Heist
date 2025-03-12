using System.Collections.Generic;
using UnityEngine;

namespace ProgressionSystem.Scripts.Variables
{
    [CreateAssetMenu(fileName = "LevelValue", menuName = "Progression/Variables/LevelValue")]
    public class LevelValueVariable : ScriptableObject
    {
        public List<int> LevelValue;
        public string ValueRepresents = "XP";
    }
}