using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    public Slider zoomSlider;
    public Camera playerCamera;

    [Header("Settings")]
    public float minZoom = 30f;
    public float maxZoom = 60f;
    public float zoomSpeed = 5f;

    private float targetZoom;

    void Start()
    {
        zoomSlider.onValueChanged.AddListener(UpdateZoom);
        targetZoom = playerCamera.fieldOfView;
    }

    void Update()
    {
        if (Mathf.Abs(playerCamera.fieldOfView - targetZoom) > 0.1f)
        {
            playerCamera.fieldOfView = Mathf.Lerp(
                playerCamera.fieldOfView,
                targetZoom,
                zoomSpeed * Time.deltaTime
            );
        }
    }

    void UpdateZoom(float value) => targetZoom = Mathf.Lerp(minZoom, maxZoom, value);
}
