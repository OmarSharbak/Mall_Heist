using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.TopDownEngine;

namespace JayMountains
{
    /// <summary>
    /// Attach this to a game object that has the WeaponAutoAim2D component. <br />
    /// Remember to attach the IdentifiedTarget component on the other game objects. <br />
    /// <i>Used with third party asset store packages:</i> <b>TopDown Engine</b>
    /// </summary>
    public class IdentifyTarget : MonoBehaviour
    {
        private WeaponAutoAim2D _weaponAutoAim2D = null;
        private Transform _currentTarget = null;
        private PickaxeSwitchMeleeType _weaponTypeSwitch = null;

        private void Awake()
        {
            _weaponAutoAim2D = this.transform.GetComponent<WeaponAutoAim2D>();
            if (_weaponAutoAim2D == null)
                Debug.LogError("No WeaponAutoAim2D component found on the game object.");

            _weaponTypeSwitch = this.transform.GetComponent<PickaxeSwitchMeleeType>();
        }

        private void Update()
        {
            if (_weaponAutoAim2D != null)
            {
                var target = _weaponAutoAim2D.Target;
                if (_currentTarget != target)
                {
                    // Check if the previous target was null. If not null, inform it that it is no longer a target.
                    if (_currentTarget != null)
                    {
                        var notifier = _currentTarget.GetComponent<IdentifiedTarget>();
                        if (notifier) notifier.IsNotTarget();

                        _weaponTypeSwitch.SwitchTypeForEnemies();
                    }

                    if (target != null)
                    {
                        var notifier = target.GetComponent<IdentifiedTarget>();
                        if (notifier) notifier.IsTarget();

                        if (LayerMask.LayerToName(target.gameObject.layer) == "Obstacles" ||
                            LayerMask.LayerToName(target.gameObject.layer) == "ObstaclesMineable")
                        {
                            _weaponTypeSwitch.SwitchTypeForMineralObstacles();
                        }

                        if (LayerMask.LayerToName(target.gameObject.layer) == "Enemies")
                        {
                            _weaponTypeSwitch.SwitchTypeForEnemies();
                        }
                    }

                    _currentTarget = target;
                }
            }
        }
    }
}