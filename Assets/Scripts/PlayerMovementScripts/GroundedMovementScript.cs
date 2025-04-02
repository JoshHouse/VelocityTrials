using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundedMovementScript : MonoBehaviour
{
    [Header("Movement Manager Script")]
    public PlayerMovementManager playerMovementManager;

    [Header("Movement Mechanic Scripts")]
    public SlidingScript slidingScript;
    public MantlingScript mantlingScript;
    public WallClimbingScript wallClimbingScript;
    public GrapplingScript grapplingScript;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void handleGroundedMovement()
    {

    }
}
