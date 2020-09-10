using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ReSharper disable All

public class Gatherable : MonoBehaviour
{
    [SerializeField] private int _energy;
    private PlayerEnergy _playerEnergy;
    [SerializeField] private int _bitsGain;

    [SerializeField] private GameObject _myCanvas;

    private GameManager _gameManager;

    private void Awake()
    {
        _playerEnergy = FindObjectOfType<PlayerEnergy>();
        _myCanvas.SetActive(false);
        _gameManager = FindObjectOfType<GameManager>();
    }

    public void GetCollected()
    {
        _playerEnergy.AdjustEnergy(_energy);
        DataCollection.AddInfo(DataCollection.DataType.GATHERED_ITEM);
        _gameManager.AddBits(_bitsGain);

        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _myCanvas.gameObject.SetActive(true);

            _gameManager.PushMessage("Collect Item by pressing C");
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _myCanvas.gameObject.SetActive(false);
        }
    }
}