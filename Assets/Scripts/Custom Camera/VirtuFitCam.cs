using System;
using System.Collections;
using System.Collections.Generic;
using Unity.RenderStreaming;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(InputChannelReceiverBase))]
public class VirtuFitCam : MonoBehaviour
{
    #region RenderStreaming
    [SerializeField] private InputChannelReceiverBase receiver;
    private List<Mouse> listMouse = new List<Mouse>();
    private List<Touchscreen> listScreen = new List<Touchscreen>();
    private Mouse mouseDevice;
    private Touchscreen touchscreenDevice;
    #endregion

    #region Input Variables
    private CameraControl Control;
    private Vector3 previousPosition;
    private Coroutine OrbitPanCoroutine;
    private Coroutine ZoomCoroutine;
    #endregion

    #region Camera Properties
    [SerializeField]private Camera VirtuFitCamera;
    [SerializeField]private GameObject Focus;
    [SerializeField] private bool isLoaded;
    public string FocusTag = "Focus";
    [SerializeField] [Range(0, 5)] private float distanceToTarget = 1;
    [SerializeField] private Vector3 StartPosition = new Vector3(0, 1.25f);
    #endregion

    #region Camera Orbit Properties
    [SerializeField] [Range(0, 360)] private int maxRotationInOneSwipe = 180;
    #endregion

    #region Camera Pan Properties
    [SerializeField] [Range(0, 1)] private float maxPanInOneSwipe = 0.25f;
    [SerializeField]private float[] PanBounds = new float[] {0, 2};
    [SerializeField] private bool DevPan = false;
    #endregion

    #region Camera Zoom Properties
    [SerializeField] [Range(0, 1)] private float ZoomSpeed = 0.5f;
    [SerializeField]private float[] ZoomBounds = new float[] { 10f, 60f };
    #endregion

    #region RenderStreaming
    void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        switch (change)
        {
            case InputDeviceChange.Added:
                SetDevice(device);
                return;
            case InputDeviceChange.Removed:
                SetDevice(device, false);
                return;
        }
    }
    void SetDevice(InputDevice device, bool add = true)
    {
        switch (device)
        {
            case Mouse mouse:
                if (add)
                {
                    listMouse.Add(mouse);
                    mouseDevice = mouse;
                }
                else
                {
                    listMouse.Remove(mouse);
                }
                return;

            case Touchscreen screen:

                if (add)
                {
                    listScreen.Add(screen);
                    touchscreenDevice = screen;
                }
                else
                {
                    listScreen.Remove(screen);
                }
                return;
        }

    }
    #endregion

    #region Private Unity Functions
    private void Awake()
    {
        if (receiver == null)
            receiver = GetComponent<InputChannelReceiverBase>();

        VirtuFitCamera = Camera.main;
        //Focus = GameObject.FindGameObjectWithTag($"{FocusTag}");
        Focus = GameManager.Instance.MyTwin;
        Control = new CameraControl();
                    receiver.onDeviceChange += OnDeviceChange;
    }

    private void OnEnable()
    {
        Control.Enable();
    }

    private void OnDisable()
    {
        Control.Disable();
    }

    private void Start()
    {
        GameManager.Instance.ModelLoadedEvent.AddListener(ModelLoaded);
    }
    #endregion

    #region Function Trigger States
    private void StartOrbit()
    {
        Debug.Log("Orbit Started" + Control.Camera.PrimaryPosition.ReadValue<Vector2>());
        if (Control.Camera.PrimaryTrigger.WasPressedThisFrame())
        {
            previousPosition = VirtuFitCamera.ScreenToViewportPoint(Control.Camera.PrimaryPosition.ReadValue<Vector2>());
        }
        OrbitPanCoroutine = StartCoroutine(CameraOrbit());
    }
    private void EndOrbit()
    {
        Debug.Log("Orbit Ended" + Control.Camera.PrimaryPosition.ReadValue<Vector2>());
        StopCoroutine(OrbitPanCoroutine);
    }

    private void StartZoom()
    {
        Debug.Log("Zoom Detected, Disabling Orbiting and Panning");
        StopCoroutine(OrbitPanCoroutine);
        ZoomCoroutine = StartCoroutine(ZoomDetection());
    }

    private void EndZoom()
    {
        Debug.Log("Zoom Ended");
        StopCoroutine(ZoomCoroutine);
    }

    private void StartScrollZoom()
    {
        Debug.Log("Mouse Scroll Initialized");
        ZoomCamera(Control.Camera.MouseScroll.ReadValue<float>(), ZoomSpeed * 100);
    }

    private void EndScrollZoom()
    { 
        Debug.Log("Mouse Scroll Disabled");
    }
    #endregion

    #region Camera Functions
    IEnumerator CameraOrbit()
    {
        while (true)
        {
            if (Control.Camera.PrimaryTrigger.IsPressed())
            {
                Vector3 newPosition = VirtuFitCamera.ScreenToViewportPoint(Control.Camera.PrimaryPosition.ReadValue<Vector2>());
                Vector3 direction = previousPosition - newPosition;

                float rotationAroundYAxis = -direction.x * maxRotationInOneSwipe; // camera moves horizontally
                float moveAlongYAxis = -direction.y * maxPanInOneSwipe;

                VirtuFitCamera.transform.position = new Vector3(Focus.transform.position.x, Mathf.Clamp((VirtuFitCamera.transform.position.y), PanBounds[0], PanBounds[1]), Focus.transform.position.z);
        
                VirtuFitCamera.transform.Rotate(new Vector3(0, 1, 0), rotationAroundYAxis, Space.World); // <— This is what makes it work!
                if (VirtuFitCamera.fieldOfView <= 30)
                {
                    VirtuFitCamera.transform.Translate(new Vector3(0, moveAlongYAxis, 0));
                }
#if UNITY_EDITOR
                if (DevPan == true)
                {
                    VirtuFitCamera.transform.Translate(new Vector3(0, moveAlongYAxis, 0));
                }
#endif

                VirtuFitCamera.transform.Translate(new Vector3(0, 0, -distanceToTarget));

                previousPosition = newPosition;
                yield return null;
            }
        }
    }

    IEnumerator ZoomDetection()
    {
        float previousDistance = 0f, distance = 0f;
        while (true)
        {
            distance = Vector2.Distance(Control.Camera.PrimaryPosition.ReadValue<Vector2>(), Control.Camera.SecondaryPosition.ReadValue<Vector2>());

            if (distance > previousDistance)
            {
                ZoomCamera(distance, ZoomSpeed);
            }
            else if (distance < previousDistance)
            {
                ZoomCamera(-distance, ZoomSpeed);
            }
            previousDistance = distance;

            yield return null;
        }
    }

    void ZoomCamera(float offset, float speed)
    {
        if (offset == 0)
        {
            return;
        }
        VirtuFitCamera.fieldOfView = Mathf.Clamp(VirtuFitCamera.fieldOfView - (offset * (speed * Time.deltaTime)), ZoomBounds[0], ZoomBounds[1]);
    }

    private void ModelLoaded()
    {
        if (GameManager.Instance.MyTwin != null)
        {
            Debug.Log("Model loaded ");
            isLoaded = true;
            if (isLoaded == true)
            {
                Focus = GameManager.Instance.MyTwin;
                Control.Camera.PrimaryTrigger.started += _ => StartOrbit();
                Control.Camera.PrimaryTrigger.canceled += _ => EndOrbit();
                Control.Camera.SecondaryTrigger.started += _ => StartZoom();
                Control.Camera.SecondaryTrigger.canceled += _ => EndZoom();
                Control.Camera.MouseScroll.started += _ => StartScrollZoom();
                Control.Camera.MouseScroll.canceled += _ => EndScrollZoom();
                StartPosition = new Vector3(StartPosition.x, StartPosition.y, distanceToTarget);
                VirtuFitCamera.transform.position = StartPosition;
            }
        }
    }
    #endregion
}
