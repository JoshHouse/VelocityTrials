using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class saveSettings : MonoBehaviour
{

    // This function is called when the behaviour becomes disabled or inactive
    private void OnDisable()
    {
        // When Setting menu is left, store PlayerPrefs in registry
        // This can ensure proper saving in case of crashes or when exiting play mode in the Editor
        PlayerPrefs.Save();
    }

}
