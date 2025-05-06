using UnityEngine;

public class TitleCameraTurn : MonoBehaviour
{

    public float turnSpeed;
    private bool rightHit; 

    // Start is called before the first frame update
    void Start() {
        rightHit = false; // Start off turning to the right
    }

    private void FixedUpdate()
    {
        if (rightHit)
        { // Turns the camera to the left till it is at a point of -20 degrees
            transform.Rotate(Vector3.down * turnSpeed * Time.deltaTime);
            if (transform.eulerAngles.y < 340 && transform.eulerAngles.y >= 20)
            {
                rightHit = false; // Flip turn angle to right
            }
        }
        else
        { // Turns the camera to the right till it is at a point of 20 degrees.
            transform.Rotate(Vector3.up * turnSpeed * Time.deltaTime);
            if (transform.eulerAngles.y > 20 && transform.eulerAngles.y <= 340)
            {
                rightHit = true; // Flip turn angle to left
            }
        }
            
    }

}
