using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorBreak : MonoBehaviour
{
    [SerializeField] GameObject floorBase; // The base of the platform
    [SerializeField] AudioClip breakSFX; // The SFX that plays when the platform breaks

    private void OnCollisionEnter(Collision collision)
    {
        // If the player touches the platform's base while the platform is stable
        if (collision.gameObject.CompareTag("Player"))
        {
            // Platform is no longer "stable" and is falling. So the collider gets turned off to prevent anymore triggers.
            gameObject.GetComponent<BoxCollider>().enabled = false;
            StartCoroutine(BreakPlatform()); // Run the coroutine to break the platform
        }
    }

    /// <summary>
    /// Simulates the base of the platform breaking roughtly a second after the player lands on it.
    /// </summary>
    /// <returns></returns>
    private IEnumerator BreakPlatform()
    {
        float currTime = 0f, moveTime = 1f;
        Vector3 startV3 = floorBase.transform.position, endV3 = floorBase.transform.position + new Vector3(0, -100, 0);

        AudioManager.instance.PlaySFXClip(breakSFX, transform);

        yield return new WaitForSeconds(1f);
        
        while (currTime < moveTime)
        {
            floorBase.transform.position = Vector3.Lerp(startV3, endV3, currTime / moveTime);
            currTime += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.25f);
        Destroy(floorBase); // Floor should be out of sight, so destroy the game object
    }

}
