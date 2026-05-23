using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class PlaneDebugLogger : MonoBehaviour
{
    [SerializeField]
    private ARPlaneManager planeManager;

    [SerializeField]
    private float logIntervalSeconds = 1f;

    private float nextLogTime;
    private int lastTrackableCount = -1;

    private void Awake()
    {
        if (planeManager == null)
        {
            planeManager = GetComponent<ARPlaneManager>();
        }
    }

    private void Update()
    {
        if (planeManager == null || Time.unscaledTime < nextLogTime)
        {
            return;
        }

        nextLogTime = Time.unscaledTime + logIntervalSeconds;

        var trackableCount = 0;
        foreach (var plane in planeManager.trackables)
        {
            trackableCount++;
        }

        if (trackableCount == lastTrackableCount)
        {
            return;
        }

        lastTrackableCount = trackableCount;
        Debug.Log(
            $"[AR Plane Debug] enabled={planeManager.enabled}, trackables={trackableCount}, " +
            $"requested={planeManager.requestedDetectionMode}, current={planeManager.currentDetectionMode}");
    }
}
