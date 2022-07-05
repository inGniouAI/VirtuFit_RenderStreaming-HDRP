using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;
using Gyroscope = UnityEngine.InputSystem.Gyroscope;
using Unity.RenderStreaming;

    using UnityInputSystem = UnityEngine.InputSystem.InputSystem;
    static class TouchScreenExtension
    {
        public static IEnumerable<EnhancedTouch> GetTouches(this Touchscreen screen)
        {
            return EnhancedTouch.activeTouches.Where(touch => touch.screen == screen);
        }
    }

    [RequireComponent(typeof(InputChannelReceiverBase))]
    class CameraController : MonoBehaviour
    {

        [SerializeField] private InputChannelReceiverBase receiver;
        private List<Mouse> listMouse = new List<Mouse>();
        private List<Touchscreen> listScreen = new List<Touchscreen>();

        void Awake()
        {
            if (receiver == null)
                receiver = GetComponent<InputChannelReceiverBase>();
            receiver.onDeviceChange += OnDeviceChange;
           /// receiver.on

            EnhancedTouchSupport.Enable();
            currentZoom = renderCamera.fieldOfView;
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

        void SetDevice(InputDevice device, bool add=true)
        {
          //  uiController?.SetDevice(device, add);

            switch (device)
            {
                case Mouse mouse:

                    if (add){
                        listMouse.Add(mouse);
                        Debug.Log("mouce device added"+listMouse.Count);

                    }
                    else{
                        listMouse.Remove(mouse);
                        Debug.Log("mouce device removed"+listMouse.Count);

                    }
                    return;
               
                case Touchscreen screen:

                    if(add){
                        listScreen.Add(screen);
                        Debug.Log("Touchscreen device added"+listScreen.Count);

                    }
                    else{
                        listScreen.Remove(screen);
                        Debug.Log("Touchscreen device removed"+listScreen.Count);

                    }
                    return;
            }

        }

        void FixedUpdate()
        {

            // Rotation an zoom,pan by Mouse
            foreach (var mouse in listMouse)
            {
                if (IsMouseDragged(mouse, true)){
                    LookRotationCamerabyMouse(mouse.delta.ReadValue());
                }
               // else if(IsMouseDragged(mouse, false) && renderCamera.fieldOfView < ZoomPan){
                 //   Panning(mouse.delta.ReadValue().y);
               // }
                else{
                    ZoomCameraByMouse(mouse.scroll.ReadValue());
                    LookRotation();
                }
            }
             // Rotation an zoom,pan by touch
            foreach (var screen in listScreen)
            {
                var touches = screen.GetTouches();
                if (touches.Count() == 3)
                {
                    var activeTouches = touches.ToArray();
                    PanningByTouch( activeTouches[0], activeTouches[1]);
                }
                else if (touches.Count() == 2)
                {
                    var activeTouches = touches.ToArray();
                    ZoomCameraByTouch(activeTouches[0],activeTouches[1]);
                }else if (touches.Count() == 1)
                {
                    var activeTouches = touches.ToArray();
                    LookRotationCameraByTouch(activeTouches[0].delta);
                }
            }
        
        }

        // void ResetCamera()
        // {
        //     m_InitialCameraState.UpdateTransform(transform);
        //     m_TargetCameraState.SetFromTransform(transform);
        //     m_InterpolatingCameraState.SetFromTransform(transform);

        // }

        static bool IsMouseDragged(Mouse m, bool useLeftButton) {
            if (null == m)
                return false;

            if (Screen.safeArea.Contains(m.position.ReadValue())) {
                //check left/right click
                if ((useLeftButton && m.leftButton.isPressed) || (!useLeftButton && m.rightButton.isPressed)) {
                    return true;
                }
            }
            return false;
        }
// TODO FROM HETAl
    [SerializeField] Camera renderCamera;    
    public float RotationsSpeedForMouse = 5.0f;
    public float RotationsSpeedForTouch = 5.0f;

    void Start () {
        GameManager.Instance.ModelLoadedEvent.AddListener(ModelLoaded);
        targetZoom = renderCamera.fieldOfView;

	}
    private bool isModel = false;
    private float inputValue ;

	void ModelLoaded(){
        if(GameManager.Instance.MyTwin!=null) {
            Debug.Log("Model loaded ");
            isModel = true;
        }
    }

private void LookRotationCamerabyMouse(Vector2 input){
     if(isModel) {
       
        inputValue = input.x;
        if(Mathf.Abs(input.x)> Mathf.Abs(input.y)){
            transform.RotateAround( GameManager.Instance.MyTwin.transform.position, Vector3.up, inputValue * RotationsSpeedForMouse);
            LookRotation();
        }else if(renderCamera.fieldOfView < ZoomPan){
            Panning(input.y);
        }
      
     }
}
private void LookRotationCameraByTouch(Vector2 input){
     if(isModel) {
        inputValue = input.x;
        transform.RotateAround( GameManager.Instance.MyTwin.transform.position, Vector3.up, inputValue * RotationsSpeedForTouch);
       LookRotation();
     }
}
private void LookRotation(){
     if(GameManager.Instance.MyTwin!=null) {

     Vector3 lookPos = GameManager.Instance.MyTwin.transform.position - transform.position;
        Quaternion lookRot = Quaternion.LookRotation(lookPos, Vector3.up);
        float eulerY = lookRot.eulerAngles.y;
        Quaternion rotation = Quaternion.Euler (0, eulerY, 0);
        transform.rotation = rotation;

    }
}
    private float targetZoom;
    public float zoomFactor = 0.01f;
        public float zoomFactorForTouch = 0.01f;

    public float ZoomMax = 60f;
    public float ZoomMin = 5f;
    public float ZoomPan = 30f;

    [SerializeField] private float zoomLerpSpeed = 5;  
        [SerializeField] private float zoomLerpSpeedByTouch = 5;  

     float currentZoom;
  
   private void ZoomCameraByMouse(Vector2 vector2){
        if(vector2.y == 0){
            return;
        }
        float scrollData = vector2.y;
        targetZoom -= scrollData * zoomFactor;
        targetZoom = Mathf.Clamp(targetZoom, ZoomMin, ZoomMax);
        renderCamera.fieldOfView = Mathf.Lerp(renderCamera.fieldOfView, targetZoom, Time.deltaTime * zoomLerpSpeed);
    }
    private float targetPan;
    public float PanUpperLimit = 0f;
    public float PanLowerLimit = 1.5f;
    private Vector3 targetPanV3;
    [SerializeField] private float PanLerpSpeed = 5;
    public float panFactor = 0.01f;

    private void Panning(float vector2y){
        
        float DragData = vector2y;
        targetPan -= DragData * panFactor;
        targetPan = Mathf.Clamp(targetPan, PanUpperLimit, PanLowerLimit);
        targetPanV3 = new Vector3(transform.position.x,targetPan,transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPanV3, Time.deltaTime * PanLerpSpeed);
    }
    private float lastMultiTouchDistance;
    float diff;
     private void ZoomCameraByTouch(EnhancedTouch firstTouch, EnhancedTouch secondTouch)
    {
             var midpoint = GetMidpoint(firstTouch, secondTouch);

        if (firstTouch.phase == TouchPhase.Began || secondTouch.phase == TouchPhase.Began)
        {
            lastMultiTouchDistance = Vector2.Distance(firstTouch.screenPosition, secondTouch.screenPosition);
              startPoint = midpoint;
        }

        // Ensure that remaining logic only executes if either finger is actively moving
        if (firstTouch.phase != TouchPhase.Moved || secondTouch.phase != TouchPhase.Moved)
        {
            return;
        }

        //Calculate if fingers are pinching togethehar or apart
        float newMultiTouchDistance = Vector2.Distance(firstTouch.screenPosition, secondTouch.screenPosition);
         diff = newMultiTouchDistance-lastMultiTouchDistance;

    if (diff <=1 && diff >=-1){
      //  

    }else{
        //Call the zoom method on the camera, specifying if it's zooming in our out
       Zoom(newMultiTouchDistance < lastMultiTouchDistance);
    }
        // Set the last distance calculation
        lastMultiTouchDistance = newMultiTouchDistance;
    }
   
    private const float ZoomAmountbyTouch = 0.5f;
   void Zoom(bool zoomOut){
       currentZoom = Mathf.Clamp(currentZoom + (zoomOut ? ZoomAmountbyTouch : -ZoomAmountbyTouch), 5f, 90f);
        renderCamera.fieldOfView = Mathf.Lerp(renderCamera.fieldOfView, currentZoom, Time.deltaTime * zoomLerpSpeedByTouch);
        Camera.main.fieldOfView = renderCamera.fieldOfView;
   }
float MAX_DIFFERENCE = 10f;
 Vector2 startPoint = Vector2.zero;

   void PanningByTouch(EnhancedTouch firstTouch, EnhancedTouch secondTouch){
         if (renderCamera.fieldOfView < ZoomPan){
            var midpoint = GetMidpoint(firstTouch, secondTouch);
        if (firstTouch.phase == TouchPhase.Began || secondTouch.phase == TouchPhase.Began)
        {
            lastMultiTouchDistance = Vector2.Distance(firstTouch.screenPosition, secondTouch.screenPosition);
              startPoint = midpoint;
        }

        // Ensure that remaining logic only executes if either finger is actively moving
        if (firstTouch.phase != TouchPhase.Moved || secondTouch.phase != TouchPhase.Moved)
        {
            return;
        }
  // get the difference between the two points.
     var difference = startPoint - midpoint;
     
     // now, get either x or y here. change this line to use x or y to your liking. 
     // makes it so that if x = MAX_DIFFERENCE, then result = 1
     var result = difference.x / MAX_DIFFERENCE;
     
     // optional: make sure it never gets bigger than 1 or smaller than -1
     result = Mathf.Clamp(result, -1.0f, 1.0f);

     Panning(result);
         }
   }
   Vector2 GetMidpoint(EnhancedTouch touch1, EnhancedTouch touch2)
     {
         return Vector2.Lerp (touch1.screenPosition, touch2.screenPosition, 0.5f);
     }
 // END FROM HETAL

    }

