using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [SerializeField]
    TMP_Text scoreTxt, timerTxt;

    [SerializeField]
    GameObject endOfEpisodePanel;

    [SerializeField]
    RawImage background;

    public float x, y;

    int remainingTime = 60;

    public bool episodeEnded;

    [HideInInspector]
    public int score;

    private void Awake()
    {
        episodeEnded = false;
        instance = this;
        endOfEpisodePanel.GetComponent<RectTransform>().localScale = Vector3.zero;
    }

    private void Update()
    {
        background.uvRect = new Rect(background.uvRect.position + new Vector2(x, y) * Time.deltaTime, background.uvRect.size);
    }

    private void Start()
    {
        StartCoroutine(TimerCoroutine());
    }

    IEnumerator TimerCoroutine()
    {
        while (remainingTime > 0)
        {
            if (remainingTime < 10)
            {
                timerTxt.text = "0" + remainingTime.ToString();
            }
            else
            {
                timerTxt.text = remainingTime.ToString();
            }

            yield return new WaitForSeconds(1);
            remainingTime--;
        }
        timerTxt.text = remainingTime.ToString();
        endOfEpisodePanel.GetComponent<RectTransform>().DOScale(1, .5f).SetEase(Ease.OutBack);
        episodeEnded = true;
    }

    public void IncreaseScoreFonx(int incomingScore)
    {
        score += incomingScore;

        scoreTxt.text = score.ToString();
    }

}
