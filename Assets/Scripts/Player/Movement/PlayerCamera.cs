using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    public Camera MainCamera => mainCamera;
    [SerializeField] private CinemachineBrain cinemachineBrain;
    public CinemachineBrain CinemachineBrain => cinemachineBrain;
    [SerializeField] private SerializedDictionary<CameraType, CinemachineCamera> cameras = new();
    
    [Header("Attributes")]
    [SerializeField] private CameraType defaultCamera = CameraType.Main;

    private CinemachineCamera currentCamera;
    
    public Transform CameraTransform => mainCamera.transform;

    private void Start()
    {
        foreach (CinemachineCamera cam in cameras.Values)
            cam.Priority = 0;
        
        print("start");
        
        ChangeCamera(defaultCamera);
    }

    public void ChangeCamera(CameraType cameraType)
    {
        // If this camera type isn't set
        if (!cameras.TryGetValue(cameraType, out CinemachineCamera newCamera)) return;
        
        // Make sure not changing to current camera
        if (currentCamera != null && cameras.TryGetValue(cameraType, out CinemachineCamera cam) && currentCamera == cam)
            return;
        
        // If the current camera has orbital follow
        if (currentCamera)
        {
            var orbitalFollow = currentCamera.GetComponent<CinemachineOrbitalFollow>();
            if (orbitalFollow)
                orbitalFollow.HorizontalAxis.TriggerRecentering();
        }
        
        // Switch priorities
        if (currentCamera)
            currentCamera.Priority = 0;
        newCamera.Priority = 1;

        currentCamera = newCamera;
    }
    
    public enum CameraType
    {
        Main,
        Aim,
        Climbing,
        Parachute
    }
}
