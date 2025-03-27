using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public static AudioManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this; // Instantiate this instance as the only instance of Audio Manager
            DontDestroyOnLoad(gameObject); // Prevent this instance from being destroyed on load
        }
        else
        {
            Destroy(instance); // If another instance of Audio Manager exists, destroy this instance
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
