using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using PDollarGestureRecognizer;

public class MovementRecognizer : MonoBehaviour
{
    public XRNode inputsource;
    public InputHelpers.Button inputButton;
    public float inputThreshold = 0.1f;
    public Transform movementSource;

    public float newPositionThresholdDistance = 0.03f;
    public GameObject debugCubePrefab;

    private bool isMoving = false;
    private List<Vector3> positionsList = new List<Vector3>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        InputHelpers.IsPressed(InputDevices.GetDeviceAtXRNode(inputsource), inputButton, out bool isPressed, inputThreshold );

        //Start the Movement Gesture
        if(!isMoving && isPressed)
        {
            StartMovement();
        }
        // Ending the Movement Gesture
        else if (isMoving && !isPressed)
        {
            EndMovement();
        }
        // Updating the Movement Gesture
        else if (isMoving && isPressed)
        {
            UpdateMovement();
        }
    }

    void StartMovement()
    {
        Debug.Log( "Start Curl" );
        isMoving = true;
        positionsList.Clear();
        positionsList.Add( movementSource.position );
        if (debugCubePrefab)
        {
            Destroy( Instantiate(debugCubePrefab, movementSource.position, Quaternion.identity), 3 );
        }
    }

    void EndMovement()
    {
        Debug.Log( "End Curl" );
        isMoving = false;

        // Create the Gesture from the position List
        Point[] pointArray = new Point[positionsList.Count];

        for(int i = 0; i < positionsList.Count; i++)
        {
            // pointArray[i] = positionsList[i];
        }
    }

    void UpdateMovement()
    {
        Debug.Log( "Update Movement" );
        Vector3 lastPosition = positionsList[positionsList.Count - 1];
        if ( Vector3.Distance( movementSource.position, lastPosition ) > newPositionThresholdDistance )
        {
            positionsList.Add( movementSource.position );
            if (debugCubePrefab)
            {
                Destroy(Instantiate(debugCubePrefab, movementSource.position, Quaternion.identity), 3);
            }
        }
    }    
}
