using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    public Camera MainCamera => mainCamera;
    [SerializeField] private CinemachineBrain cinemachineBrain;
    public CinemachineBrain CinemachineBrain => cinemachineBrain;
    [SerializeField] private Dictionary<CameraType, CinemachineCamera> cameras = new();
    
    public Transform CameraTransform => mainCamera.transform;

    public void ChangeCamera(CameraType cameraType)
    {
        // Get current camera
        CinemachineCamera currentCamera = cinemachineBrain.ActiveVirtualCamera as CinemachineCamera;
        
        // Make sure not changing to current camera
        if (currentCamera != null && cameras.TryGetValue(cameraType, out CinemachineCamera cam) && currentCamera == cam)
            return;

        // If this camera isn't set
        if (!cameras.TryGetValue(cameraType, out CinemachineCamera newCamera)) return;
        
        int currentPriority = 0;
        if (currentCamera)
            currentPriority = currentCamera.Priority;

        newCamera.Priority = currentPriority + 1;
    }
    
    public enum CameraType
    {
        Main,
        Aim,
        Climbing,
        Parachute
    }
}
