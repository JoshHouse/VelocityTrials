using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{

    public static TitleManager instance;

    public GameObject titleCovering;
    private Image whiteBG;
    private Image blackL;
    private Image blackR;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this; // Instantiate this instance as the only instance of Title Manager
        }
        else
        {
            Destroy(instance); // If another instance of Title Manager exists, destroy this instance
        }

        whiteBG = titleCovering.transform.GetChild(0).GetComponent<Image>();
        blackL = titleCovering.transform.GetChild(1).GetComponent<Image>();
        blackR = titleCovering.transform.GetChild(2).GetComponent<Image>();

        // Set the covering's positions at the start of the opening
        SetupOpening(Screen.width, Screen.height); // Pass in the width and height as parameters to avoid repeatedly calling the Screen to get the width and height
    }

    private void SetupOpening(float w, float h)
    {
        whiteBG.rectTransform.sizeDelta = new Vector3(w, h); // Changes the scale of the white covering to match the screen's width and height

        // Make the left black image match the screen's height and cover half the width of the screen on the left
        blackL.rectTransform.sizeDelta = new Vector3(w/2, h);
        blackL.rectTransform.position = new Vector3(blackL.rectTransform.sizeDelta.x - (w/4), blackL.rectTransform.position.y, 0);

        // Same as the left image, but for the right
        blackR.rectTransform.sizeDelta = new Vector3(w/2, h);
        blackR.rectTransform.position = new Vector3(blackR.rectTransform.sizeDelta.x + (w/4), blackR.rectTransform.position.y, 0);
    }

    public IEnumerator OpeningAnimation(float moveTime)
    {
        float currTime = 0f;
        Vector3 blackLV3 = blackL.rectTransform.localPosition;
        Vector3 blackRV3 = blackR.rectTransform.localPosition;
        Vector3 moveTo = new Vector3(1000, 0, 0);
        Color startColor = whiteBG.color;
        Color transparentWhite = new Color(1, 1, 1, 0);
        yield return new WaitForSeconds(0.5f);

        while (currTime < moveTime)
        {
            blackL.rectTransform.localPosition = Vector3.Lerp(blackLV3, -moveTo, currTime/moveTime);
            blackR.rectTransform.localPosition = Vector3.Lerp(blackRV3, moveTo, currTime/moveTime);
            currTime += Time.deltaTime;
            yield return null; // Movement should not wait in between loops
        }

        currTime = 0f;
        yield return new WaitForSeconds(0.25f);

        while (currTime < moveTime)
        {
            whiteBG.color = Color.Lerp(startColor, transparentWhite, currTime/moveTime);
            currTime += Time.deltaTime;
            yield return null;
        }

        GameManager.instance.ChangeGameState(GameManager.GameStates.MENU);
        titleCovering.SetActive(false);
        yield return null;
    }

    // Sets gamemode to testing then loads the testing scene
    public void GoToTestingGrounds()
    {
        GameManager.instance.ChangeGameState(GameManager.GameStates.TESTING);
        SceneManager.LoadSceneAsync("DefaultScene");
    }

}
