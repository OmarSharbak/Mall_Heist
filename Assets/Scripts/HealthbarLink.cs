using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;

namespace JayMountains
{
    public class HealthbarLink : MonoBehaviour
    {
        private void Awake()
        {
            this.GetComponent<MMHealthBar>().TargetProgressBar = HealthbarManager.Instance.LinkToProgressBar();
        }
    }
}