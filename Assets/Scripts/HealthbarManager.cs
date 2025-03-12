using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;

namespace JayMountains
{
    public class HealthbarManager : MMSingleton<HealthbarManager>
    {
        [SerializeField] private MMProgressBar healthBar;

        public MMProgressBar LinkToProgressBar()
        {
            return healthBar;
        }
    }
}