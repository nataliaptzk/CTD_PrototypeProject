using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base : MonoBehaviour
{
    private GameManager _gameManager;

    private void Awake()
    {
        _gameManager = FindObjectOfType<GameManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (_gameManager.CheckIfEnoughBitsGathered() && _gameManager.HasDayFinished())
            {
                _gameManager.NextDay();
            }
            else if (!_gameManager.CheckIfEnoughBitsGathered() && _gameManager.HasDayFinished())
            {
                _gameManager.PushMessage("Not enough bits! Find more and then come back!");
            }
            else if (!_gameManager.HasDayFinished())
            {
                _gameManager.PushMessage("The day has not finished yet! Come when it is night time.");

            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
        }
    }
}