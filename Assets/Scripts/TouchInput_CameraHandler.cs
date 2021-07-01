using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;



public class TouchInput_CameraHandler : MonoBehaviour
{

    public Camera MainCam;
    public CinemachineVirtualCamera CvCam;
    public PolygonCollider2D Confiner;
    
    public float PanSpeed;
    public bool UseDeltaPan;
    public float DeltaPanSpeed;
    //
    private Vector3 lastPanPosition;
    private int panFingerId; 
    
    private Vector3 minBounds;
    private Vector3 maxBounds;
    private float cameraMarginX;
    private float cameraMarginY;
    private float cameraFieldProportion = (float) Screen.width / Screen.height; // 
    private bool wasZoomingLastFrame; 
    private Vector2[] lastZoomPositions; 

    



    /*---------------------------- Start / Update OVERRIDES -------------------------*/
    void Start()
    {
        setupEdges();
    }

    void Update()
    {
        HandleTouch();
        // if(Input.touchSupported && Application.platform != RuntimePlatform.WebGLPlayer) HandleTouch(); // What the internet told me was the way to detect and handle Touch systems, but it didn't work for me
    }





    /*------------------------------- CORE STUFF ----------------------------*/

    void HandleTouch()
    {
        switch(Input.touchCount) {

        case 1: // Panning
            // If the touch began, capture its position and its finger ID.
            // Otherwise, if the finger ID of the touch doesn't match, skip it.
            Touch touch = Input.GetTouch(0);
            if(touch.phase == TouchPhase.Began) {
                lastPanPosition = touch.position;
                panFingerId = touch.fingerId;
            } else if(touch.fingerId == panFingerId && touch.phase == TouchPhase.Moved) {
                PanCamera(touch);
            }
            break;
    
        case 2: // Zooming
            Vector2[] newPositions = new Vector2[]{Input.GetTouch(0).position, Input.GetTouch(1).position};
            if (!wasZoomingLastFrame) {
                lastZoomPositions = newPositions;
                wasZoomingLastFrame = true;
            } else {
                // Zoom based on the distance between the new positions compared to the 
                // distance between the previous positions.
                float newDistance = Vector2.Distance(newPositions[0], newPositions[1]);
                float oldDistance = Vector2.Distance(lastZoomPositions[0], lastZoomPositions[1]);
                float offset = newDistance - oldDistance;
                Vector2 center = newPositions[1] + ((newPositions[0] - newPositions[1]) * 0.5f);
                //
                zoomCamera(offset, center);
                lastZoomPositions = newPositions;
            }
            break;
            
        default: 
            wasZoomingLastFrame = false;
            break;
        }
    }

    void PanCamera(Touch touch_) {
        //
        // Determine how much to move the camera
        Vector3 move = new Vector3();
        if(UseDeltaPan) {
            move = -touch_.deltaPosition*touch_.deltaTime*DeltaPanSpeed*CvCam.m_Lens.OrthographicSize;
        } else {
            Vector3 offset = MainCam.ScreenToViewportPoint(lastPanPosition - (Vector3)touch_.position);
            move = new Vector3(offset.x*cameraFieldProportion, offset.y, 0f) * PanSpeed * CvCam.m_Lens.OrthographicSize;
        }
        //        
        transform.Translate(move);          // Perform the movement
        lastPanPosition = touch_.position;  // Cache the position
        //
        clampPosition();                    // Stay within certain borders
    }
    
    void zoomCamera(float change_, Vector2 position_)
    {
        if(Mathf.Approximately(change_, 0f)) return;
        // Change the camera ortho projection
        CvCam.m_Lens.OrthographicSize -= change_ * 0.01f;
        CvCam.m_Lens.OrthographicSize = Mathf.Clamp(CvCam.m_Lens.OrthographicSize, 3.0f, 10.0f);   
        // Re-work the dimension variables
        cameraMarginY = CvCam.m_Lens.OrthographicSize;
        cameraMarginX = cameraMarginY * cameraFieldProportion;
    }





    /*------------------------------- HELPER STUFF ----------------------------*/
    
    void clampPosition()
    {
        /* 
            Keep us from moving towards edges if camera is confined and can't follow 
            -- this avoids follow-object and camera being forced apart.
            The "cameraMargin(X|Y)" variables gives the distance between camera and edge of screen.
        */
        float clampX = Mathf.Clamp(transform.position.x, minBounds.x + cameraMarginX, maxBounds.x - cameraMarginX);
        float clampY = Mathf.Clamp(transform.position.y, minBounds.y + cameraMarginY, maxBounds.y - cameraMarginY);
        transform.position = new Vector3(clampX, clampY, transform.position.z);
    }

    void setupEdges()
    {
        // For keeping follow object away from edges -- see function "clampPosition"
        minBounds = Confiner.bounds.min;
        maxBounds = Confiner.bounds.max;
        cameraMarginY = CvCam.m_Lens.OrthographicSize;
        cameraMarginX = cameraMarginY * cameraFieldProportion;
    }

}
