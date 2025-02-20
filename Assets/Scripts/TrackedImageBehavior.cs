// Peter Hu, 2025
// https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@6.1/manual/features/image-tracking/artrackedimagemanager.html
using UnityEngine;
using UnityEngine.XR.ARFoundation;
// TrackableId
using UnityEngine.XR.ARSubsystems;


public class TrackedImageBehavior : MonoBehaviour
{

    private ARRaycastPlace arRaycastPlace;
   
    [SerializeField]
    ARTrackedImageManager m_ImageManager;

    void Start()
    {
        // Find the ARRaycastPlace script in the scene
        arRaycastPlace = FindFirstObjectByType<ARRaycastPlace>();  
    }


    void OnEnable() => m_ImageManager.trackablesChanged.AddListener(OnChanged);

    void OnDisable() => m_ImageManager.trackablesChanged.RemoveListener(OnChanged);

    void OnChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        foreach (var newImage in eventArgs.added)
        {
            // Handle added event
            Debug.Log("Image added");
            // arRaycastPlace.SwitchObjects();
        }

        foreach (var updatedImage in eventArgs.updated)
        {
            // Handle updated event
            Debug.Log("Image updated");
            arRaycastPlace.SwitchObjects();
        }

        foreach (var removed in eventArgs.removed)
        {
            // Handle removed event
            Debug.Log("Image removed");
            TrackableId removedImageTrackableId = removed.Key;
            ARTrackedImage removedImage = removed.Value;
        }
    }

    
}
