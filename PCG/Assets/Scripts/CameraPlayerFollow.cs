using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPlayerFollow : MonoBehaviour
{
    [SerializeField] private Transform _cameraTargetToFollow;
    [SerializeField] private float _smoothSpeed = 0.125f;
    private Vector3 _velocity = Vector3.zero;

    private Vector3 _offset;

    //   private Vector3 _mouseScrollOffset;
    [SerializeField] private float _offsetA, _offsetB, _mouseSensitivity;

    [NonSerialized] public bool isFollowing;


    private void Start()
    {
        isFollowing = true;
    }

    private void Update()
    {
        var i = Input.GetAxis("Mouse ScrollWheel"); // -0.4 scroll down 0.4 scroll up
        _offsetA += i * _mouseSensitivity * -1;
        _offsetA = Mathf.Clamp(_offsetA, 1f, 25f);
    }

    void LateUpdate()
    {
        if (isFollowing)
        {
            Transform myTransform = transform;

            // _mouseScrollOffset = new Vector3();

            _offset = (_cameraTargetToFollow.transform.up.normalized * _offsetA + -_cameraTargetToFollow.transform.forward.normalized * _offsetB);
            Vector3 newPos = _cameraTargetToFollow.position + _offset;
            transform.position = Vector3.SmoothDamp(myTransform.position, newPos, ref _velocity, _smoothSpeed);

            transform.LookAt(_cameraTargetToFollow, _cameraTargetToFollow.transform.up);
        }
        else
        {
        }
    }
}