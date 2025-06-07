using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

/// <summary>
/// This script detects a specified AR image marker and spawns a GameObject prefab on it.
/// It uses the latest AR Foundation 6.0+ API for trackablesChanged events.
/// Attach this script to your AR Session Origin GameObject.
/// </summary>
public class ARLabyrinthManager : MonoBehaviour
{
    [Header("AR Setup")]
    [Tooltip("The ARTrackedImageManager responsible for detecting images. " +
             "Should be on the same GameObject as this script (AR Session Origin).")]
    [SerializeField]
    private ARTrackedImageManager m_ARTrackedImageManager;

    [Tooltip("The Prefab to spawn when the target marker is detected. " +
             "This should be a 3D model or an empty GameObject containing your virtual content.")]
    [SerializeField]
    private GameObject m_PrefabToSpawn;

    [Tooltip("The exact name of the reference image (marker) in your " +
             "AR Reference Image Library that you want to track.")]
    public string targetMarkerName = "MyTargetMarker"; // IMPORTANT: Change this to your marker's name!

    // Dictionary to keep track of instantiated prefabs per tracked image,
    // to manage their lifecycle (update position/rotation, destroy when lost).
    private Dictionary<string, GameObject> m_SpawnedObjects = new Dictionary<string, GameObject>();

    void OnEnable()
    {
        // Attempt to get the ARTrackedImageManager if it's not assigned in the Inspector.
        if (m_ARTrackedImageManager == null)
        {
            m_ARTrackedImageManager = GetComponent<ARTrackedImageManager>();
            if (m_ARTrackedImageManager == null)
            {
                Debug.LogError("MarkerSpawner: ARTrackedImageManager not found or assigned on AR Session Origin! " +
                               "Please ensure it's on this GameObject and assigned in the Inspector.");
                enabled = false; // Disable this script if the manager isn't available
                return;
            }
        }

        // Check if the prefab to spawn is assigned.
        if (m_PrefabToSpawn == null)
        {
            Debug.LogError("MarkerSpawner: Prefab To Spawn is not assigned! Please assign a prefab in the Inspector.");
            enabled = false; // Disable this script if there's no prefab
            return;
        }

        // Subscribe to the AR Foundation 6.0+ trackablesChanged event for ARTrackedImage.
        m_ARTrackedImageManager.trackablesChanged.AddListener(OnTrackablesChanged);
        Debug.Log("MarkerSpawner: Subscribed to ARTrackedImageManager.trackablesChanged event.");
    }

    void OnDisable()
    {
        // Unsubscribe from the event to prevent memory leaks and ensure proper cleanup.
        if (m_ARTrackedImageManager != null)
        {
            m_ARTrackedImageManager.trackablesChanged.AddListener(OnTrackablesChanged);
            Debug.Log("MarkerSpawner: Unsubscribed from ARTrackedImageManager.trackablesChanged event.");
        }
    }

    /// <summary>
    /// Event handler for ARTrackedImageManager.trackablesChanged.
    /// This method is called by AR Foundation whenever the state of tracked images changes.
    /// </summary>
    /// <param name="eventArgs">Contains lists of added, updated, and removed ARTrackedImage instances.</param>
    private void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        // Handle newly added (detected) images
        foreach (var addedImage in eventArgs.added)
        {
            Debug.Log($"MarkerSpawner: Detected new image: {addedImage.referenceImage.name} " +
                      $"(Tracking State: {addedImage.trackingState}) at position {addedImage.transform.position}");

            // Check if the detected image's name matches our target marker name
            if (addedImage.referenceImage.name == targetMarkerName)
            {
                // If we've already spawned an object for this marker (e.g., due to re-detection after a brief loss),
                // avoid spawning a duplicate.
                if (m_SpawnedObjects.ContainsKey(addedImage.referenceImage.name))
                {
                    Debug.LogWarning(
                        $"MarkerSpawner: Object already spawned for {addedImage.referenceImage.name}. Skipping duplicate spawn.");
                    continue;
                }

                // Instantiate the prefab. Parent it to the 'addedImage.transform' so it moves and rotates with the real-world marker.
                GameObject spawnedObject = Instantiate(m_PrefabToSpawn, addedImage.transform);
                spawnedObject.name =
                    $"Spawned_{addedImage.referenceImage.name}"; // Give it a descriptive name in the Hierarchy

                // Store the spawned object in our dictionary for future management.
                m_SpawnedObjects.Add(addedImage.referenceImage.name, spawnedObject);

                Debug.Log(
                    $"MarkerSpawner: Successfully spawned '{spawnedObject.name}' on marker '{addedImage.referenceImage.name}'.");
            }
        }

        // Handle updated images (e.g., marker moved, tracking quality changed)
        foreach (var updatedImage in eventArgs.updated)
        {
            // If the spawned object is parented to updatedImage.transform, its position/rotation
            // will automatically update. You can add logic here if you need to react to changes,
            // for example, based on updatedImage.trackingState.
            if (m_SpawnedObjects.ContainsKey(updatedImage.referenceImage.name))
            {
                // Debug.Log($"MarkerSpawner: Marker '{updatedImage.referenceImage.name}' updated. Tracking State: {updatedImage.trackingState}");
                // Example: If tracking becomes limited, you might want to adjust visuals or notify the user.
                // if (updatedImage.trackingState == TrackingState.Limited)
                // {
                //     Debug.LogWarning($"Marker '{updatedImage.referenceImage.name}' tracking is limited. Content might flicker.");
                // }
            }
        }

        // // Handle removed (lost) images
        // foreach (var removedImage in eventArgs.removed)
        // {
        //     Debug.Log($"MarkerSpawner: Lost image: {removedImage.referenceImage.name}");
        //
        //     // If we have an object spawned for this lost marker, destroy it and remove from our dictionary.
        //     if (m_SpawnedObjects.TryGetValue(removedImage.referenceImage.name, out GameObject spawnedInstance))
        //     {
        //         Destroy(spawnedInstance); // Remove the virtual object from the scene
        //         m_SpawnedObjects.Remove(removedImage.referenceImage.name); // Clean up the dictionary
        //
        //         Debug.Log($"MarkerSpawner: Destroyed spawned object for lost marker '{removedImage.referenceImage.name}'.");
        //     }
        // }
        // Also disable instantiated prefabs if they have been removed.

        // ARCore doesn't seem to remove these at all; if it does, it would delete our child GameObject
        // as well. So we don't really need to do so here.
        foreach (var trackedImage in eventArgs.removed)
        {
            m_PrefabToSpawn.SetActive(false);
        }
    }
}