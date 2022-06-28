using System.Collections.Generic;
using Unity.RenderStreaming;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

[RequireComponent(typeof(InputChannelReceiverBase))]
public class CameraControl : MonoBehaviour
{
    [SerializeField] private InputChannelReceiverBase receiver;

    private List<Mouse> listMouse = new List<Mouse>();
    private List<Touchscreen> listScreen = new List<Touchscreen>();

    [SerializeField] private Camera cam;
    [SerializeField] private Transform target;
    [SerializeField][Range(0, 5)] private float distanceToTarget = 1;
    [SerializeField][Range(0, 360)] private float maxRotationInOneSwipe = 180;
    [SerializeField][Range(0, 1)] private float maxPanInOneSwipe = 0.25f;
    [SerializeField][Range(0, 10)] private float MouseZoomSpeed = 5f;
    [SerializeField][Range(0, 10)] private float TouchZoomSpeed = 5f;
    [SerializeField] private float[] ZoomBounds = new float[] { 10f, 85f };
    [SerializeField][Range(0, 10f)] public float touchSpeed = 10f;
    private Vector3 previousPosition;
    private Touch touch1;
    private Touch touch2;

    private bool isModel = false;

    private void Start()
    {
        GameManager.Instance.ModelLoadedEvent.AddListener(ModelLoaded);
    }
    void ModelLoaded()
    {
        if (GameManager.Instance.MyTwin != null)
        {
            Debug.Log("Model loaded ");
            isModel = true;
            target = GameManager.Instance.MyTwin.transform;
        }
    }

    private void Awake()
    {
        if (receiver == null)
            receiver = GetComponent<InputChannelReceiverBase>();
        receiver.onDeviceChange += OnDeviceChange;

        EnhancedTouchSupport.Enable();
    }

    void FixedUpdate()
    {
        foreach (var mouse in listMouse)
        {
            MouseInput();
        }
        foreach (var screen in listScreen)
        {
            TouchInput();
        }
    }

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
                    Debug.Log("mouce device added");

                    if (add)
                        listMouse.Add(mouse);
                    else
                        listMouse.Remove(mouse);
                    return;
                case Touchscreen screen:
                    Debug.Log("Touchscreen device added");

                    if (add)
                        listScreen.Add(screen);
                    else
                        listScreen.Remove(screen);
                    return;
            }
        }


        void MouseInput()
        {
            var mouse = Mouse.current; //Gets Current input type
            if (mouse == null)
                return;

            if (mouse.leftButton.wasPressedThisFrame) //Left Click Intializes Camera Orbiting
            {
                previousPosition = cam.ScreenToViewportPoint(mouse.position.ReadValue());
            }
            else if (mouse.leftButton.IsPressed())
            {
                maxRotationInOneSwipe = 180;
                CameraOrbit(mouse.position.ReadValue()); //Executes Camera Orbiting
            }
            if (mouse.rightButton.wasPressedThisFrame) //Right Click Initializes Camera Panning
            {
                previousPosition = cam.ScreenToViewportPoint(mouse.position.ReadValue());
            }
            else if (mouse.rightButton.IsPressed())
            {
                maxPanInOneSwipe = 0.25f;
                CameraPan(mouse.position.ReadValue()); //Executes Camera Panning
            }
            ZoomCamera(-mouse.scroll.ReadValue().y);   //Scroll Initializes Camera Zoom And Executes it
        }

        void TouchInput()
        {
            if (Touch.activeFingers.Count == 1)
            {
                touch1 = Touch.activeFingers[0].currentTouch;
                previousPosition = cam.ScreenToViewportPoint(touch1.startScreenPosition);
                maxRotationInOneSwipe = 10f;
                CameraOrbit(touch1.screenPosition);
            }
            if (Touch.activeFingers.Count == 2)
            {
                touch1 = Touch.activeFingers[0].currentTouch;
                touch2 = Touch.activeFingers[1].currentTouch;

                Vector2 touch1Previous = touch1.startScreenPosition - touch1.screen.delta.ReadValue();
                Vector2 touch2Previous = touch2.startScreenPosition - touch2.screen.delta.ReadValue();

                float oldTouchDistance = Vector2.Distance(touch1Previous, touch2Previous);
                float currentTouchDistance = Vector2.Distance(touch1.screenPosition, touch2.screenPosition);

                float DeltaDistance = oldTouchDistance - currentTouchDistance;

                ZoomCamera(DeltaDistance * TouchZoomSpeed);
            }
            if (Touch.activeFingers.Count == 3)
            {
                touch1 = Touch.activeFingers[0].currentTouch;
                previousPosition = cam.ScreenToViewportPoint(touch1.startScreenPosition);
                maxPanInOneSwipe = 0.15f;
                CameraPan(touch1.screenPosition);
            }
        }

        void CameraOrbit(Vector2 MousePosition)
        {
            Vector3 newPosition = cam.ScreenToViewportPoint(MousePosition);
            Vector3 direction = previousPosition - newPosition;

            float rotationAroundYAxis = -direction.x * maxRotationInOneSwipe; // camera moves horizontally

            cam.transform.position = new Vector3(target.position.x, Mathf.Clamp((cam.transform.position.y), 0.2f, 1.25f), target.position.z);

            cam.transform.Rotate(new Vector3(0, 1, 0), rotationAroundYAxis, Space.World); //  This is what makes it move Horizontally

            cam.transform.Translate(new Vector3(0, 0, -distanceToTarget));

            previousPosition = newPosition;
        }

        void CameraPan(Vector2 MousePosition)
        {
            Vector3 newPosition = cam.ScreenToViewportPoint(MousePosition);
            Vector3 direction = previousPosition - newPosition;

            float moveAlongYAxis = -direction.y * maxPanInOneSwipe; // camera moves Vertically

            cam.transform.position = new Vector3(target.position.x, Mathf.Clamp((cam.transform.position.y), 0.2f, 1.25f), target.position.z);

            cam.transform.Translate(new Vector3(0, moveAlongYAxis, 0)); // This is what makes it move Vertically

            cam.transform.Translate(new Vector3(0, 0, -distanceToTarget));

            previousPosition = newPosition;
        }

        void ZoomCamera(float ZoomValue)
        {
            cam.fieldOfView = Mathf.Clamp(cam.fieldOfView - (ZoomValue * MouseZoomSpeed), ZoomBounds[0], ZoomBounds[1]); //Reads the float value and adjusts Camera FOV for Zoom accordingly
        }
    }
