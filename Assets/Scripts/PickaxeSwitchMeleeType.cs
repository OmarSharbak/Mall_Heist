using UnityEngine;
using MoreMountains.TopDownEngine;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;

public class PickaxeSwitchMeleeType : MonoBehaviour
{
    [SerializeField] private MeleeWeapon weapon;
    [SerializeField] private DamageOnTouch damageAreaEnemies;
    [SerializeField] private DamageOnTouch damageAreaMineralObjects;
    [SerializeField] private MMFeedbacks dAreaEnemiesFeedbacks;
    [SerializeField] private MMFeedbacks dAreaMineralObjectsFeedbacks;


    //----------------------------------------------------------------
    private void Start()
    {
        damageAreaEnemies.DamageTakenHealth = weapon.Owner.CharacterHealth;
        damageAreaMineralObjects.DamageTakenHealth = weapon.Owner.CharacterHealth;
    }


    //----------------------------------------------------------------
    [Button]
    public void SwitchTypeForEnemies()
    {
        weapon._damageArea = null;
        weapon.ExistingDamageArea = damageAreaEnemies;
        weapon.Initialization();
        weapon.TargetLayerMask = 0; // nothing
        weapon.TargetLayerMask += LayerMask.GetMask("Enemies");
        weapon.WeaponUsedMMFeedback = dAreaEnemiesFeedbacks;

        // Weird bug fix..
        damageAreaMineralObjects.gameObject.GetComponent<BoxCollider2D>().enabled = false;
    }


    //----------------------------------------------------------------
    [Button]
    public void SwitchTypeForMineralObstacles()
    {
        weapon._damageArea = null;
        weapon.ExistingDamageArea = damageAreaMineralObjects;
        weapon.Initialization();
        weapon.TargetLayerMask = 0; // nothing
        weapon.TargetLayerMask += LayerMask.GetMask("Obstacles");
        weapon.TargetLayerMask += LayerMask.GetMask("ObstaclesMineable");
        weapon.WeaponUsedMMFeedback = dAreaMineralObjectsFeedbacks;

        // Weird bug fix..
        damageAreaEnemies.gameObject.GetComponent<BoxCollider2D>().enabled = false;
    }


    //----------------------------------------------------------------
    private void OnEnable()
    {
        //SwitchTypeForMineralObstacles();
    }
}