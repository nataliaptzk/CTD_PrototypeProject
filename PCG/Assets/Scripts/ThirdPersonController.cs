using System;
using System.Collections.ObjectModel;
using UnityEngine;

// ReSharper disable All

public class ThirdPersonController : MonoBehaviour
{
    [SerializeField] private float _walkSpeed;
    [SerializeField] private float _backWalkSpeed;
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _fallSpeed;
    [SerializeField] private float _rotationSpeed;

    [SerializeField] private GameObject _projectile;
    [SerializeField] private GameObject _projectileSpawnPoint;

    private Gatherable _currentGatherable;
    private PlayerEnergy _playerEnergy;

    [SerializeField] private bool _grounded;
    [SerializeField] private bool _isJumping;
    [SerializeField] private bool _isFalling;

    private float _verticalLookRotation;
    private Vector3 _moveAmount;
    private Vector3 _smoothMoveVelocity;
    private Rigidbody _rigidbody;

    private float _horizontal, _vertical;
    public bool canPlay;

    public bool Grounded
    {
        get => _grounded;
        set => _grounded = value;
    }

    public bool IsFalling
    {
        get => _isFalling;
        set => _isFalling = value;
    }

    void Awake()
    {
        canPlay = false;
        _rigidbody = GetComponent<Rigidbody>();
        _playerEnergy = GetComponent<PlayerEnergy>();

        DataCollection.ResetDataCollection();
    }

    private void Start()
    {
        Grounded = false;
        _isJumping = false;
        IsFalling = false;
    }

    void Update()
    {
        _horizontal = Input.GetAxisRaw("Horizontal");
        _vertical = Input.GetAxisRaw("Vertical");

        transform.Translate(_walkSpeed * Time.deltaTime * _vertical * Vector3.forward);

        transform.Rotate(0, Time.deltaTime * _rotationSpeed * _horizontal, 0);


        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Shoot();
        }

        if (Input.GetKeyDown(KeyCode.C) && canPlay)
        {
            if (_currentGatherable != null)
            {
                GatherObject(_currentGatherable);
                _currentGatherable = null;
            }
        }
    }

    private void Shoot()
    {
        GameObject projectile;
        projectile = Instantiate(_projectile, _projectileSpawnPoint.transform.position, _projectileSpawnPoint.transform.rotation);

        projectile.GetComponent<Rigidbody>().velocity = _projectileSpawnPoint.transform.TransformDirection(Vector3.up * 10);
        _playerEnergy.AdjustEnergy(-1);
    }

    private void GatherObject(Gatherable gatherable)
    {
        gatherable.GetCollected();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Gatherable"))
        {
            _currentGatherable = other.GetComponent<Gatherable>();
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Gatherable"))
        {
            _currentGatherable = null;
        }
    }
}