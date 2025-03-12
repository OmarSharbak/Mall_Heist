using UnityEngine;
using UnityEngine.Events;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;

public class BossDeadLogic : MonoBehaviour
{
    [SerializeField] private UnityEvent OnDeathEvent;

    private Health health;


    //----------------------------------------------------------------
    private void Awake()
    {
        health = this.GetComponent<Health>(); Debug.Log(health);
    }


    //----------------------------------------------------------------
    public void BossKilled()
    {
        AIBrain brain = transform.GetComponent<AIBrain>();
        if (brain == null) return;

        brain.BrainActive = false;
    }


    //----------------------------------------------------------------
    protected virtual void OnEnable()
    {
        health.OnDeath += OnDeathEvent.Invoke;
        health.OnDeath += BossKilled;
    }


    //----------------------------------------------------------------
    protected virtual void OnDisable()
    {
        health.OnDeath -= OnDeathEvent.Invoke;
        health.OnDeath -= BossKilled;
    }
}