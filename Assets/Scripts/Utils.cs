using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

//#if DG
//using DG.Tweening;
//#endif

public static class Utils
{
    public static string CurrencyFormat(float value)
    {
        string str = value.ToString();

        if (value < 1000)
            str = value.ToString("F0");
        else if (value >= 1000 && value < 1000000)
        {
            float _value = value / (float)1000;
            str = $"{_value:F1}K";
        }
        else if (value >= 1000000)
        {
            float _value = value / (float)1000000;
            str = $"{_value:F2}M";
        }

        return str;
    }

    public static string FormatTime(float seconds)
    {
        int hours = Mathf.FloorToInt(seconds / 3600);
        int minutes = Mathf.FloorToInt((seconds % 3600) / 60);
        int remainingSeconds = Mathf.FloorToInt(seconds % 60);
        if (hours > 0)
            return string.Format("{0:D2}:{1:D2}:{2:D2}", hours, minutes, remainingSeconds);
        return string.Format("{0:D2}:{1:D2}", minutes, remainingSeconds);
    }

    //#if DG
    //public static void WaitAndPerform(float wait, System.Action action)
    //{
    //    DOTween.Sequence()
    //       .AppendInterval(wait)
    //       .AppendCallback(() => action?.Invoke());
    //}

    //#endif

    public static bool IsPointerOverUI()
    {
        return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() ||
            Input.touchCount > 0 && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
    }

    public static void AlignChildren(Transform parent, float spacing)
    {
        float currentXPosition = 0f;

        foreach (Transform child in parent)
        {
            if (child.gameObject.activeInHierarchy)
            {
                Renderer renderer = child.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    Vector3 childSize = renderer.bounds.size;

                    // Position the child
                    child.localPosition = new Vector3(currentXPosition + childSize.x / 2, 0f, 0f);

                    // Update the current position for the next child
                    currentXPosition += childSize.x + spacing;
                }
            }
        }
    }

    public static Vector3 GetPointInBound(BoxCollider boxCollider, Vector3 objectSize)
    {
        Bounds bounds = boxCollider.bounds;

        // Generate a random position within the BoxCollider while considering the object's size
        Vector3 randomPosition = new Vector3(
            Random.Range(bounds.min.x + objectSize.x / 2, bounds.max.x - objectSize.x / 2),
            Random.Range(bounds.min.y + objectSize.y / 2, bounds.max.y - objectSize.y / 2),
            Random.Range(bounds.min.z + objectSize.z / 2, bounds.max.z - objectSize.z / 2)
        );
        return randomPosition;
    }

    public static Bounds CalcBoundsFromBox(BoxCollider collider)
    {

        Vector3 localCenter = collider.center;
        Vector3 localSize = collider.size;

        // Transform the local center into world space
        Vector3 worldCenter = collider.transform.TransformPoint(localCenter);

        // Scale the local size by the transform's scale to get the size in world space
        Vector3 worldSize = Vector3.Scale(localSize, collider.transform.lossyScale);

        return new Bounds(worldCenter, worldSize);

    }

    public static Vector3 GetMouseWorldPosition(out RaycastHit hit, float distance = float.MaxValue)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, distance))
        {
#if UNITY_EDITOR

            Debug.DrawRay(ray.origin, ray.direction * Vector3.Distance(hit.point, ray.origin), Color.red, 5);
#endif
            return hit.point;
        }
        return ray.GetPoint(10);  // Default distance if no hit
    }

    public static Vector3 GetMouseWorldPosition(LayerMask layerMask, out RaycastHit hit, float distance = float.MaxValue)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, distance, layerMask))
        {
#if UNITY_EDITOR
            Debug.DrawRay(ray.origin, ray.direction * Vector3.Distance(hit.point, ray.origin), Color.blue, 5);
#endif
            return hit.point;
        }
        return ray.GetPoint(10);  // Default distance if no hit
    }
}

public static class Extension
{
    public static void SetGameObjectActive(this Component component, bool value)
    {
        if (component == null)
        {
            Debug.LogError("Component is null");
            return;
        }

        component.gameObject.SetActive(value);
    }

    public static void CurrencySetText(this TextMeshProUGUI textMesh, float value)
    {
        if (value < 1000)
            textMesh.text = value.ToString("F0");
        else if (value >= 1000 && value < 1000000)
        {
            float _value = value / (float)1000;
            textMesh.text = $"{_value:F1}K";
        }
        else if (value >= 1000000)
        {
            float _value = value / (float)1000000;
            textMesh.text = $"{_value:F2}M";
        }
    }

    public static void Shuffle<T>(this System.Random random, List<T> ts)
    {
        int n = ts.Count;
        while (n > 1)
        {
            int randIndex = random.Next(n--);
            T val = ts[randIndex];
            T temp = ts[n];
            ts[n] = val;
            ts[randIndex] = temp;
        }
    }

    public static void Shuffle<T>(this System.Random random, T[] ts)
    {
        int n = ts.Length;
        while (n > 1)
        {
            int randIndex = random.Next(n--);
            T val = ts[randIndex];
            T temp = ts[n];
            ts[n] = val;
            ts[randIndex] = temp;
        }
    }

    public static bool FirePlaneRay(this ref Plane plane, Camera camera, out Vector3 point)
    {
        point = Vector3.zero;
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        float distanceToPlane;

        if (plane.Raycast(ray, out distanceToPlane))
        {
            point = ray.GetPoint(distanceToPlane);
            return true;
        }
        return false;
    }

    public static void ClearChildren(this Transform transform, int index = 0)
    {
        var count = transform.childCount;
        for (int i = index; i < count; i++)
        {
            var child = transform.GetChild(index);
            child.parent = null;

            if (Application.isPlaying)
                Object.Destroy(child.gameObject);
            else
                Object.DestroyImmediate(child.gameObject);
        }
    }

    public static string TimeTranslate(this System.TimeSpan timeSpace)
    {
        if (timeSpace.Days > 0)
        {
            return string.Format("{0:D1}d {1:D2}h {2:D2}m {3:D2}s", timeSpace.Days, timeSpace.Hours, timeSpace.Minutes, timeSpace.Seconds);
        }
        return string.Format("{0:D2}h {1:D2}m {2:D2}s", timeSpace.Hours, timeSpace.Minutes, timeSpace.Seconds);
    }

    public static T GetOrAddComponent<T>(this GameObject go) where T : Component
    {
        T comp = go.GetComponent<T>();
        if (!comp)
            comp = go.AddComponent<T>();
        return comp;
    }

    public static bool TryGetComponentInParent<T>(this GameObject go, out T component) where T : Component
    {
        component = go.GetComponentInParent<T>();
        return component != null;
    }

}
public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }

    public static string ToJson<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper);
    }

    public static string ToJson<T>(T[] array, bool prettyPrint)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}
