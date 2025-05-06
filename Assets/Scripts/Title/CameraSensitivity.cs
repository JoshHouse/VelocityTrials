using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CameraSensitivity : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI minValue;
    [SerializeField] TextMeshProUGUI maxValue;
    [SerializeField] TextMeshProUGUI currValue;
    [SerializeField] Slider slider;

    // Start is called before the first frame update
    void Start()
    {
        minValue.text = slider.minValue.ToString();
        maxValue.text = slider.maxValue.ToString();
        slider.value = PlayerPrefs.GetFloat("Sensitivity", 800);
    }

    // Update is called once per frame
    void Update()
    {
        currValue.text = slider.value.ToString();
    }

    public void SetSensitivity()
    {
        GameManager.instance.UpdateSensitivity(slider.value);
    }

    public void ResetSensitivity()
    {
        slider.value = 800f;
        GameManager.instance.UpdateSensitivity(800f);
    }

}
