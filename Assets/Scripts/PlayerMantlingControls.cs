using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMantlingControls : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] PlayerMovement movementScript;
    [SerializeField] BoxCollider playerCollider;


    // References not gotten by the editor
    private int mantleLayer; // The layer to detect mantling points on

    private KeyCode mantlingKey; // The key code for mantling
    private float mantlingDistance = 1f; // How far away a mantling point can be from the player
    private float mantlingHeight = 0.6f; // How high the player mantles up
    private float mantlingTime = 0.5f; // How long to spend mantling
   

    // Start is called before the first frame update
    void Start()
    {
        // Get the necessary references
        mantlingKey = movementScript.jumpKey; // Set the mantling key to the jump key as seen in the PlayerMovement script

        mantleLayer = LayerMask.NameToLayer("MantlingPoint"); // Get a reference to the layer used for mantling from the Unity Engine
        mantleLayer = ~mantleLayer; // Prevents Raycasting from ignoring the mantling layer
    }

    // Update is called once per frame
    void Update()
    {
        DetectMantling(); // Calls the function for seeing if you can mantle
    }

    /**
     * Function for the player attempting to mantle, or climb and jump over an obstacle while near a ledge, across a mantlable object. 
     * When the player first approaches the object while in midair, the function below will be used to detect if the player is near a mantling 
     * point. If so, they will allow the player to mantle across the ledge to whatever is above.
     */ 
    public void DetectMantling()
    {
        

        // Mantling should only happen if the player is airborne (i.e, during a jump), and they press the jump key a second time
        if (Input.GetKeyDown(mantlingKey))
        {
            /* Attempts to spawn a raycast at the mantling point, provided the player is facing the mantling point and is in range
             * playerCamera.transform.position - Origin point for the ray
             * playerCamera.transform.forward - direction the ray should move
             * out var hit1 - information on the Raycast hit
             * mantlingDistance - The distance the raycast should be spawned
             * mantleLayer - The layer defined in the Unity Project for mantling.*/
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out var hit1, mantlingDistance, mantleLayer))
            {
                Debug.Log("A mantling point is in front."); // Debug message to know that a mantling point was found successful

                /* Attempts to spawn a raycast at the landing place using the first hit point as a starting point
                 * Starting position: 
                    * hit1.point - start with the point taken from the first Raycast
                    * playerCamera.transform.forward * playerCollider.size.y - add hit1.point to the product of the camera facing forward times the player's collider's size
                    * Vector3.up * mantlingHeight * playerTransform.localScale.y - Product of moving up times the maximum mantling height, times the player's transform scale
                 * Direction:
                    * Vector3.down - Moves the player down towards the landing area over the mantle point
                 * out var hit2 - information on the second raycast hit
                 * Max Distance:
                    * playerTransform.localScale.y - Max distance is calculated based on the player's scale
                 * */
                if (Physics.Raycast(hit1.point + (playerCamera.transform.forward * playerCollider.size.y) + (Vector3.up * mantlingHeight * transform.localScale.y), Vector3.down, 
                    out var hit2, (transform.localScale.y / 2)))
                {
                    Debug.Log("A landing point has been found."); // Debug messgae to know that a landing point was found via the second raycast
                    StartCoroutine(Mantle(hit2.point, mantlingTime)); // Start the coroutine for mantling
                }
            }

        }

    }

    /**
     * Coroutine to actually perform the mantling.
     */ 
    private IEnumerator Mantle(Vector3 moveTo, float mantleTime)
    {
        float currTime = 0f; // Current time tracker
        Vector3 startPos = transform.position; // Represents the starting position at the player's current location

        // Create a loop to move the player to the mantling point over time.
        while (currTime < mantleTime)
        {
            transform.position = Vector3.Lerp(startPos, moveTo, currTime / mantleTime); // Use Lerp to create the transition from the start point to the end point.
            currTime += Time.deltaTime; // Increment currTime on each loop
            yield return null; // Player should not wait in between loops
        }

        transform.position = moveTo; // The player's position is set to the end point at the end to ensure the player is at this point.
    }

}
