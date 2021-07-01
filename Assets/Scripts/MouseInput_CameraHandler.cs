using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;



public class MouseInput_CameraHandler : MonoBehaviour
{

    public Camera MainCam;
    public CinemachineVirtualCamera CvCam;
    public PolygonCollider2D Confiner;

    private Vector3 minBounds;
    private Vector3 maxBounds;
    private float cameraMarginX;
    private float cameraMarginY;
    private float cameraFieldProportion = (float) Screen.width / Screen.height; // 

    public float MouseDragSpeed;
    //
    private float currentDragSpeed;
    private bool zoomingIn;
    private Vector3 zoomWorldPoint = Vector3.zero;
    private Vector3 zoomScreenPoint = Vector3.zero;   
    




    /*-------------------------- Start / Update OVERRIDES -------------------*/

    void Start()
    {
        setupEdges();
        currentDragSpeed = MouseDragSpeed * CvCam.m_Lens.OrthographicSize;
    }

    void Update()
    {
        if(zoomingIn) finalizeZoomingIn();
        HandleMouse();
    }
    




    /*------------------------------- CORE STUFF ----------------------------*/

    void HandleMouse()
    {
        if(Input.GetAxis("MouseScrollWheel") != 0f) zoomCamera(Input.GetAxis("MouseScrollWheel"));
        if(Input.GetAxis("Fire1") != 0) panCamera(); // My Unity had "Fire1" set as Left Mousebutton 
    }

    void zoomCamera(float change_)
    {
        zoomingIn = change_ > 0f;   // Zomming in will be handled differently than zooming out
        // 
        zoomScreenPoint = zoomingIn ? Input.mousePosition : new Vector3(Screen.width*0.5f, Screen.height*0.5f, 0f);          
        zoomWorldPoint = MainCam.ScreenToWorldPoint(zoomScreenPoint);
        if(!zoomingIn) { // Zooming out is simple and can all be implemented in this function.
            transform.position = new Vector3(zoomWorldPoint.x, zoomWorldPoint.y, 0f);
            CvCam.ForceCameraPosition(transform.position, Quaternion.identity);   
        } // If zooming in we will have to wait until next Update to position camera so that it aligns with mouse pointer.
        CvCam.m_Lens.OrthographicSize -= change_;
        CvCam.m_Lens.OrthographicSize = Mathf.Clamp(CvCam.m_Lens.OrthographicSize, 3.0f, 10.0f); 
        MainCam.orthographicSize = CvCam.m_Lens.OrthographicSize;
        setupEdges(); // Distance between camera and edges have changed
    }

    void panCamera()
    {
        float moveInputX = Input.GetAxis("Mouse X");
        float moveInputY = Input.GetAxis("Mouse Y");
        if(Mathf.Abs(moveInputX) < 0.01f && Mathf.Abs(moveInputY) < 0.01f) return; // This might not be neccessary -- I just figured that it saves cpu power
        Vector3 move = (Vector3.left * moveInputX * currentDragSpeed) + (Vector3.down * moveInputY * currentDragSpeed);
        transform.Translate(move);
        clampPosition();    
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

    void finalizeZoomingIn()
    { // Find out how much world moved under mouse pointer and move it back so that the same point stays where mouse is
        Vector3 mouseWorldSpot = MainCam.ScreenToWorldPoint(zoomScreenPoint);
        transform.position += zoomWorldPoint - mouseWorldSpot;
        CvCam.ForceCameraPosition(transform.position, Quaternion.identity);   
        zoomingIn = false; // Finally done with zooming in.
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