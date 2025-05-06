using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameManager;

public class TitleManager : MonoBehaviour
{

    public static TitleManager instance;
    [Header("Title Screen Objects")]
    [SerializeField] private GameObject titleCovering;
    [SerializeField] private GameObject centerDoor;
    
    private Transform whiteBG;
    private Image whiteBGImage;
    private Transform blackL;
    private Transform blackR;

    [Header("Level Select Screen Objects")]
    [SerializeField] private GameObject levelsPanel;
    [SerializeField] private GameObject backButton;

    [Header("Audio Objects")]
    [SerializeField] private AudioClip[] voiceClips;
    [SerializeField] private AudioClip titleBGM;

    private Vector3 blackLStartPos;
    private Vector3 blackRStartPos;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this; // Instantiate this instance as the only instance of Title Manager
        }
        else
        {
            Destroy(gameObject); // If another instance of Title Manager exists, destroy this instance
        }

        whiteBG = titleCovering.transform.GetChild(0);
        whiteBGImage = whiteBG.GetComponent<Image>();
        blackL = titleCovering.transform.GetChild(1);
        blackR = titleCovering.transform.GetChild(2);

        // Set the covering's positions at the start of the opening
        SetupOpening(Screen.width, Screen.height); // Pass in the width and height as parameters to avoid repeatedly calling the Screen to get the width and height
    }

    private void Start()
    {
        if (GameManager.instance.gameState == (int)GameManager.GameStates.OPENING)
        {
            StartCoroutine(OpeningAnimation(1f));
        }
    }

    public void ExitGame()
    {
        if (TitleManager.instance != null)
        {
            StartCoroutine(ExitAnimation(1f));
        }
        else
        {
            GameManager.instance.ChangeGameState(GameStates.EXIT);
        }
    }

    private void SetupOpening(float w, float h)
    {

        RawImage blackLImage = blackL.GetComponent<RawImage>();
        RawImage blackRImage = blackR.GetComponent<RawImage>();

        whiteBGImage.rectTransform.sizeDelta = new Vector3(w, h); // Changes the scale of the white covering to match the screen's width and height

        // Make the left black image match the screen's height and cover half the width of the screen on the left
        blackLImage.rectTransform.sizeDelta = new Vector3(w/2, h);
        blackLImage.rectTransform.position = new Vector3(blackLImage.rectTransform.sizeDelta.x - (w/4), blackLImage.rectTransform.position.y, 0);

        // Same as the left image, but for the right
        blackRImage.rectTransform.sizeDelta = new Vector3(w/2, h);
        blackRImage.rectTransform.position = new Vector3(blackRImage.rectTransform.sizeDelta.x + (w/4), blackRImage.rectTransform.position.y, 0);

        blackLStartPos = blackL.transform.localPosition;
        blackRStartPos = blackR.transform.localPosition;
    }

    public IEnumerator OpeningAnimation(float moveTime)
    {

        if (GameManager.instance.gameState == (int)GameManager.GameStates.OPENING)
        {
            AudioManager.instance.PlayBGM(titleBGM, transform);


            Vector3 blackLV3 = blackLStartPos;
            Vector3 blackRV3 = blackRStartPos;
            Vector3 moveTo = new Vector3(Screen.width, 0, 0);
            Color startColor = whiteBGImage.color;
            Color transparentWhite = new Color(1, 1, 1, 0);
            yield return new WaitForSeconds(0.5f);

            AudioManager.instance.PlayVoiceClip(voiceClips[0], transform);

            StartCoroutine(MovementLERP(blackL, moveTime, blackLV3, -moveTo));
            StartCoroutine(MovementLERP(blackR, moveTime, blackRV3, moveTo));

            yield return new WaitForSeconds(0.5f);

            float currTime = 0f;

            while (currTime < 1f)
            {
                whiteBGImage.color = Color.Lerp(startColor, transparentWhite, currTime / 1f);
                currTime += Time.deltaTime;
                yield return null;
            }


            yield return new WaitForSeconds(0.25f);

            GameManager.instance.ChangeGameState(GameManager.GameStates.MENU);
            titleCovering.SetActive(false);
        }
        
        yield return null;
    }

    public IEnumerator EnterLevel(int levelIndex, GameManager.GameStates gameState)
    {
        titleCovering.SetActive(true);
        StartCoroutine(MovementLERP(centerDoor.transform, 1.5f, centerDoor.transform.localPosition, centerDoor.transform.localPosition + new Vector3(0, 10, 0)));
        StartCoroutine(MovementLERP(levelsPanel.transform, 0.6f, levelsPanel.transform.localPosition, new Vector3(-(Screen.width), 0, 0)));
        StartCoroutine(MovementLERP(backButton.transform, 1f, backButton.transform.localPosition, new Vector3(0, -(Screen.height), 0)));

        float clipLength = voiceClips[1].length;
        AudioManager.instance.PlayVoiceClip(voiceClips[1], transform);
        yield return new WaitForSeconds(clipLength);

        GameManager.instance.ChangeGameState(gameState);
        SceneManager.LoadScene(levelIndex);
        yield return null;
    }

    private IEnumerator MovementLERP(Transform objToMove, float moveTime, Vector3 objStart, Vector3 moveTo)
    {
        float currTime = 0f;

        while (currTime < moveTime)
        {
            objToMove.transform.localPosition = Vector3.Lerp(objStart, moveTo, currTime/moveTime);
            currTime += Time.deltaTime;
            yield return null;
        }

        yield return null;
    }

    public IEnumerator ExitAnimation(float moveTime)
    {
        titleCovering.SetActive(true);
        Color transparentWhite = new Color(1, 1, 1, 0);
        float currTime = 0f;

        StartCoroutine(MovementLERP(blackL, moveTime, blackL.transform.localPosition, blackLStartPos));
        StartCoroutine(MovementLERP(blackR, moveTime, blackR.transform.localPosition, blackRStartPos));

        while (currTime < moveTime)
        {
            whiteBGImage.color = Color.Lerp(transparentWhite, Color.white, currTime / 1f);
            currTime += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        GameManager.instance.ChangeGameState(GameManager.GameStates.EXIT);
    }

}
