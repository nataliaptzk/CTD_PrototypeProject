using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable All

public class PlayerEnergy : MonoBehaviour
{
    private float _energy;
    [SerializeField] private Image _energySlider;
    [SerializeField] private GameObject _plus;
    [SerializeField] private GameObject _minusOne;
    private GameManager _gameManager;

    private void Awake()
    {
        _plus.SetActive(false);
        _minusOne.SetActive(false);
        _gameManager = FindObjectOfType<GameManager>();
    }

    void Start()
    {
        _energy = 10;
    }

    void Update()
    {
        DecayEnergyOverTime();
        ManageSlider();

        if (_energy <= 0)
        {
            _gameManager.GameOver();
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

    public IEnumerator ShowMinusIcon()
    {
        _minusOne.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        _minusOne.SetActive(false);
    }

    public void AdjustEnergy(float amount)
    {
        if (amount > 0)
        {
            StartCoroutine(ShowPlusIcon(amount));
        }
        else if (amount < 0)
        {
            StartCoroutine(ShowMinusIcon());
        }

        _energy += amount;
    }

    private void DecayEnergyOverTime()
    {
        _energy = (float) (_energy - Time.deltaTime * 0.2f);
        _energy = Mathf.Clamp(_energy, 0, 10);
    }

    private void ManageSlider()
    {
        _energySlider.fillAmount = _energy / 10f;
    }

    public void RecoverEnergy()
    {
        _energy = 10;
    }
    
}