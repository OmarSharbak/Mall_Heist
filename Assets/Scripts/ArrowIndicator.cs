using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using EmeraldAI;
using EmeraldAI.Utility;

public class ArrowIndicator : MonoBehaviour
{
    public Transform player;
    public List<Transform> guards = new List<Transform>();
    public Image arrowPrefab;
    public float circleRadius = 100f;  // The radius of the circle around the player on the screen where the arrows are placed.

    private Camera mainCamera;
    private List<Image> arrows = new List<Image>();

    private void Start()
    {
        mainCamera = Camera.main;
        CreateArrows();
    }

    private void CreateArrows()
    {
        foreach (var guard in guards)
        {
            Image newArrow = Instantiate(arrowPrefab, transform);
            newArrow.enabled = false;
            arrows.Add(newArrow);
        }
    }

    private void Update()
    {
        for (int i = 0; i < guards.Count; i++)
        {
            if (IsGuardFollowing(guards[i]))
            {
                arrows[i].enabled = true;
                PointArrowToGuard(arrows[i], guards[i]);
            }
            else
            {
                arrows[i].enabled = false;
            }
        }
    }

    private bool IsGuardFollowing(Transform guard)
    {
        EmeraldAIDetection emeraldAIDetection = guard.GetComponent<EmeraldAIDetection>();
        if (emeraldAIDetection.EmeraldComponent.CurrentTarget != null)
            return true;

        return false;
    }

    private void PointArrowToGuard(Image arrow, Transform guard)
    {
        Vector3 dirToGuard = guard.position - player.position;
        float angle = GetAngleFromDirection(dirToGuard);
        arrow.rectTransform.rotation = Quaternion.Euler(90, 0, angle);

        Vector2 arrowPosition = CalculatePositionOnCircle(angle, circleRadius);
        arrow.rectTransform.anchoredPosition = arrowPosition;  // This will place the arrow at a specific position around the player.
    }

    private float GetAngleFromDirection(Vector3 dir)
    {
        dir = mainCamera.transform.InverseTransformDirection(dir);
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        return angle - 90;
    }

    private Vector2 CalculatePositionOnCircle(float angle, float radius)
    {
        float x = radius * Mathf.Cos(angle * Mathf.Deg2Rad);
        float y = radius * Mathf.Sin(angle * Mathf.Deg2Rad);
        return new Vector2(x, y);
    }
}
