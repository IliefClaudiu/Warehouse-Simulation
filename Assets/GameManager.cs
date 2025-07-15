using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("References")]
    public PlayerMovement playerMovement;
    public ForkliftController forklift;
    public CameraController cameraController;
    public Transform dropZone;

    private GameObject selectedItem;

    [Header("UI & Cameras")]
    public GameObject manualControlsUI;
    public Camera mainCamera;
    public Camera manualCamera;
    public GameObject joystickUI;

    [Header("Mode")]
    public bool isManualMode = false;


    void Awake() => Instance = this;

    public void SelectItem(GameObject item)
    {
        Debug.Log("Selected object: " + item.name);
        Debug.Log("World position: " + item.transform.position);
        Debug.Log("Parent: " + item.transform.parent.name);
        selectedItem = item;
        forklift.StartTransportSequence(item.transform);
    }

    public void CompleteTransport()
    {
        Vector3 originalScale = selectedItem.transform.lossyScale;
        selectedItem.transform.SetParent(dropZone,true);
        selectedItem.transform.localScale = originalScale;
        selectedItem = null;
    }
    public void SetManualMode(bool manual)
    {
        isManualMode = manual;

        if (mainCamera != null && manualCamera != null)
        {
            if (manual)
            {
                // Manual mode: manual camera full screen, main camera off
                manualCamera.enabled = true;
                manualCamera.rect = new Rect(0, 0, 1, 1);  // full screen
                mainCamera.enabled = false;
            }
            else
            {
                // Automatic mode: main camera full screen, manual camera small rectangle
                mainCamera.enabled = true;
                manualCamera.enabled = true;  // still enabled to show small rect
                manualCamera.rect = new Rect(0.7f, 0.7f, 0.3f, 0.3f);  // small rectangle
            }
        }

        // UI toggle
        if (manualControlsUI != null)
            manualControlsUI.SetActive(manual);

        if (joystickUI != null)  // add joystick UI reference!
            joystickUI.SetActive(!manual);
        forklift.SetManualMode(manual);
        Debug.Log("Toggling mode to: " + isManualMode);
    }

    public void ToggleMode()
    {
        SetManualMode(!isManualMode);
    }
}
