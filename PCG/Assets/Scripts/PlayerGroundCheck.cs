using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundCheck : MonoBehaviour
{
    private ThirdPersonController _player;

    private void Awake()
    {
        _player = transform.parent.GetComponent<ThirdPersonController>();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            _player.Grounded = true;
            _player.IsFalling = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            _player.Grounded = false;
        }
    }
}