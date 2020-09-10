using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// ReSharper disable All

public class GameManager : MonoBehaviour
{
    // need timer for the day
    // need the timer for energy loss

    public bool _canUsePushMessage;

    [SerializeField] private TextMeshProUGUI _messageTextBox;
    [SerializeField] private GameObject _messageTextBoxParent;


    [Header("Managable")] [SerializeField] private GameObject _player;

    [SerializeField] private TerrainGenerator _terrainGenerator;
    [SerializeField] private TextMeshProUGUI _dayCount;
    [SerializeField] private TextMeshProUGUI _untilNightTimer;
    [SerializeField] private TextMeshProUGUI _bitsCount;
    [SerializeField] private TextMeshProUGUI _gameOverText;
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private GameObject _instructionsPanel;
    [SerializeField] private GameObject _newDayPanel;
    [SerializeField] private GameObject _light;

    [SerializeField] private GameObject _plus;

    private int _daysIngame;
    private int _timeLeft;
    private int _bits;
    private int _bitsRequired;


    private void Awake()
    {
        _messageTextBoxParent.SetActive(false);
        _plus.SetActive(false);

        _canUsePushMessage = false;
        _daysIngame = 1;
        _bitsRequired = 10;
        _bits = 0;
    }

    private void Start()
    {
        _terrainGenerator.Initiate();
    }

    public void ActivateGame()
    {
        Time.timeScale = 1;
        _instructionsPanel.SetActive(false);
        _player.GetComponent<ThirdPersonController>().canPlay = true;
        _canUsePushMessage = true;
        StartCoroutine(Countdown(60));
        UpdateUI();
    }

    public void AddBits(int amount)
    {
        _bits += amount;
        UpdateUI();
        StartCoroutine(ShowPlusIcon(amount));
    }

    private void UpdateUI()
    {
        _bitsCount.text = _bits + " / " + _bitsRequired;
        _dayCount.text = _daysIngame.ToString();
    }

    public bool CheckIfEnoughBitsGathered()
    {
        if (_bits >= _bitsRequired)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void NextDay()
    {
        Time.timeScale = 0;
        _terrainGenerator.Revaluate();
        StartCoroutine(StartNewDay());
        _daysIngame++;
        _bits = 0;
        _bitsRequired = _daysIngame * 10;
        _player.GetComponent<PlayerEnergy>().RecoverEnergy();
        UpdateUI();
        _player.GetComponent<ThirdPersonController>().canPlay = true;
        Time.timeScale = 1;

    }

    private IEnumerator StartNewDay()
    {
        _newDayPanel.SetActive(true);
        yield return new WaitForSeconds(3f);
        _newDayPanel.SetActive(false);
        _light.gameObject.SetActive(true);
        StartCoroutine(Countdown(60));

    }

    public void GameOver()
    {
        Time.timeScale = 0;

        _gameOverPanel.SetActive(true);
        _gameOverText.text = "You survived " + _daysIngame + " days.";
    }

    public void Reload()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void PushMessage(string message)
    {
        if (_canUsePushMessage)
        {
            StartCoroutine(PushMessageEnumerator(message, 2));
        }
    }

    public bool HasDayFinished()
    {
        if (_timeLeft > 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private IEnumerator ShowPlusIcon(float amount)
    {
        int temp = (int) amount;
        _plus.GetComponent<TextMeshProUGUI>().text = "+" + temp;
        _plus.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        _plus.SetActive(false);
    }

    public IEnumerator Countdown(float duration)
    {
        float totalTime = duration;
        while (totalTime >= 0)
        {
            totalTime -= Time.deltaTime;
            _timeLeft = (int) totalTime;

            int min = Mathf.FloorToInt(_timeLeft / 60);
            int sec = Mathf.FloorToInt(_timeLeft % 60);

            _untilNightTimer.text = min.ToString("00") + ":" + sec.ToString("00");
            yield return null;
        }

        _player.GetComponent<ThirdPersonController>().canPlay = false;
        _light.gameObject.SetActive(false);
        StartCoroutine(PushMessageEnumerator("You can't gather at night! But you can kill enemies! Head to the base, otherwise you die!", 5));
    }

    IEnumerator PushMessageEnumerator(string message, int time)
    {
        _canUsePushMessage = false;
        _messageTextBoxParent.SetActive(true);
        _messageTextBox.text = message;
        yield return new WaitForSeconds(time);
        _messageTextBox.text = "";
        _messageTextBoxParent.SetActive(false);
        _canUsePushMessage = true;
    }
}