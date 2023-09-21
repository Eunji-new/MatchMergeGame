using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject readyScreen;
    public GameObject gameScreen;
    public GameObject endScreen;
    //public GameObject RankingPanel;
    //public GameObject InputNamePanel;
    public GameObject handle;
    public Button startBtn;
    public Button restartBtn;
    public Button exitBtn;
    //public Text score;
    //public Text scoreFinal;
    ///public Text scoreRank;
    public Slider timeSlider;
    public Coroutine blinkCoroutine;

    // public Button saveRankBtn;
    // public Button rankingBtn;
    // public Button showNameInputBtn;
    // public Button closeRankBtn;
    // public TMP_InputField nameInputField;

    private void Awake() {
        startBtn.onClick.AddListener(() => {GameController.instance.StartGame();});
        restartBtn.onClick.AddListener(() => {GameController.instance.ReadyGame();});
        exitBtn.onClick.AddListener(() => {Application.Quit();});
        //saveRankBtn.onClick.AddListener(() => {CreateRanking();});
        //rankingBtn.onClick.AddListener(() => {SetActivePanel(RankingPanel, true);});
        //showNameInputBtn.onClick.AddListener(() => {SetActivePanel(InputNamePanel, true);});
        //closeRankBtn.onClick.AddListener(() => {
        //    SetActivePanel(RankingPanel, false);
        //    SetActivePanel(InputNamePanel, false); });
    }
    private void Start() {
        timeSlider.maxValue = GameController.instance.gameTime;
    }

    public void SetText(Text text, int value)
    {
        text.text = value.ToString();
    }

    public void SetSliderValue(float value)
    {
        timeSlider.value = value;
    }

    public void BlinkHandle()
    {
        if(blinkCoroutine == null)
        {
            blinkCoroutine = StartCoroutine(CoBlinkHandle());
        }
        // else
        // {
        //     StopCoroutine(blinkCoroutine);
        //     blinkCoroutine = StartCoroutine(CoBlinkHandle());
        // }
    }

    public void StopBlink()
    {
        if(blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
    }

    IEnumerator CoBlinkHandle()
    {
        //Debug.Log("핸들 깜박임");
        yield return null;

        Image handleImg = handle.GetComponent<Image>();
        handleImg.color = new Color(236/255.0f, 28/255.0f, 67/255.0f);

        while (true) {
            float fadeCnt = 1;
            while(fadeCnt > 0)
            {
                fadeCnt -= 0.1f;
                handleImg.color = new Color(handleImg.color.r, handleImg.color.g, handleImg.color.b, fadeCnt);
			    yield return new WaitForSeconds (.05f);
            }
            fadeCnt = 0;
            while(fadeCnt < 1)
            {
                fadeCnt += 0.1f;
                handleImg.color = new Color(handleImg.color.r, handleImg.color.g, handleImg.color.b, fadeCnt);
			    yield return new WaitForSeconds (.05f);
            }
		}
    }

    public void ShowScreen(GameObject screen)
    {
        handle.GetComponent<Image>().color = Color.white;
        readyScreen.SetActive(false);
        gameScreen.SetActive(false);
        endScreen.SetActive(false);

        screen.SetActive(true);
    }

    public void SetActivePanel(GameObject panel, bool isActive)
    {
        panel.SetActive(isActive);
    }

    public void CreateRanking()
    {
        // bool isSuccess = RankingManager.instance.CreateRanking(nameInputField.text, GameController.instance.score);
        // if(isSuccess)
        // {
        //     SetActivePanel(InputNamePanel, false);
        //     showNameInputBtn.interactable = false;
        // }
    }
}
