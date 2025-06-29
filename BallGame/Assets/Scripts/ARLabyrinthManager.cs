using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class ARLabyrinthManager : MonoBehaviour
{
    [SerializeField] private ARTrackedImageManager ARTrackedImageManager;

    [SerializeField] private GameObject PrefabToSpawn;

    public string targetMarkerName = "MyTargetMarker";

    private Dictionary<string, GameObject> spawnedObjects = new Dictionary<string, GameObject>();

    void OnEnable()
    {
        if (ARTrackedImageManager == null)
        {
            ARTrackedImageManager = GetComponent<ARTrackedImageManager>();
            if (ARTrackedImageManager == null)
            {
                enabled = false;
                return;
            }
        }

        if (PrefabToSpawn == null)
        {
            enabled = false;
            return;
        }

        ARTrackedImageManager.trackablesChanged.AddListener(OnTrackablesChanged);
    }

    void OnDisable()
    {
        // unsubscribe from the event to prevent memory leaks and ensure proper cleanup.
        if (ARTrackedImageManager != null)
        {
            ARTrackedImageManager.trackablesChanged.AddListener(OnTrackablesChanged);
        }
    }

    private void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        // handle newly added (detected) images
        foreach (var addedImage in eventArgs.added)
        {
            if (addedImage.referenceImage.name == targetMarkerName)
            {
                if (spawnedObjects.ContainsKey(addedImage.referenceImage.name))
                {
                    continue;
                }

                GameObject spawnedObject = Instantiate(PrefabToSpawn, addedImage.transform);
                spawnedObject.name = $"Spawned_{addedImage.referenceImage.name}";

                spawnedObjects.Add(addedImage.referenceImage.name, spawnedObject);

                // add ball logic to game manager
                BallLogic spawnedBallLogic = spawnedObject.GetComponentInChildren<BallLogic>();
                if (spawnedBallLogic != null)
                {
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.SetBallLogic(spawnedBallLogic);
                        Debug.Log("Passed BallLogic to GameManager.");
                    }
                    else
                    {
                        Debug.LogError("GameManager not found.");
                    }
                }
                else
                {
                    Debug.LogError("BallLogic script not found.");
                }
            }
        }

        // handle updated images (e.g., marker moved, tracking quality changed)
        foreach (var updatedImage in eventArgs.updated)
        {
            if (spawnedObjects.ContainsKey(updatedImage.referenceImage.name))
            {
                if (updatedImage.trackingState == TrackingState.Limited)
                {
                    // Debug.LogWarning(
                    //     $"Marker '{updatedImage.referenceImage.name}' tracking is limited. Content might flicker.");
                }
            }
        }
    }
}