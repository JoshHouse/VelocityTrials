using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleCameraTurn : MonoBehaviour
{

    public float turnSpeed;
    private float lim = 20;
    private bool hitRight;

    // Start is called before the first frame update
    void Start() { 
        StartCoroutine(CameraTurn());
        hitRight = false;
    }

    IEnumerator CameraTurn()
    {
        float y = 0;
        while (true)
        {
            while (!hitRight)
            {
                if (transform.eulerAngles.y > lim)
                {
                    hitRight = true;
                }
                else
                {
                    y += turnSpeed * Time.deltaTime;
                    y = Mathf.Clamp(y, -21, 21);
                    transform.rotation = Quaternion.Euler(0, y, 0);
                }
                yield return null;
            }

            while (hitRight) //TODO: Make it so camera can actually loop back to right
            {
                if (transform.eulerAngles.y < -lim)
                {
                    hitRight = false;
                }
                else
                {
                    y += -turnSpeed * Time.deltaTime;
                    y = Mathf.Clamp(y, -21, 21);
                    transform.rotation = Quaternion.Euler(0, y, 0);
                }
                yield return null;
            }
            yield return null;
        }
    }

}
