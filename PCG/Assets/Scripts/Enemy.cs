using System;
using UnityEngine;
using UnityEngine.AI;

// ReSharper disable All

public class Enemy : MonoBehaviour
{
    private Vector3 _initialPosition;
    private Target _pursueTarget; // player
    private NavMeshAgent _agent;
    [SerializeField] private int _energy;
    [SerializeField] private int _bitsGain;
    private PlayerEnergy _playerEnergy;
    private GameManager _gameManager;

    [SerializeField] private GameObject _myCanvas;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _gameManager = FindObjectOfType<GameManager>();
        _playerEnergy = FindObjectOfType<PlayerEnergy>();
        _myCanvas.gameObject.SetActive(false);
    }

    void Start()
    {
        _initialPosition = transform.position;

        SetTargetToNone();
    }

    private void SetTargetToNone()
    {
        _pursueTarget = new Target(_initialPosition, TargetTypes.None);
        _agent.isStopped = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_pursueTarget.targetType == TargetTypes.Player && !_agent.isStopped)
        {
            PursuePlayer();
        }
        else if (_pursueTarget.targetType == TargetTypes.Self && !_agent.isStopped)
        {
            ResetSelfTransform();
        }
    }

    private void PursuePlayer()
    {
        _agent.destination = _pursueTarget.targetPosition;

        if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
        {
            _agent.isStopped = true;
        }
    }

    private void ResetSelfTransform()
    {
        _agent.destination = _pursueTarget.targetPosition;

        if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
        {
            SetTargetToNone();
        }
    }

    private void AbandonPursue()
    {
        _agent.isStopped = false;
        _pursueTarget.targetPosition = _initialPosition;
        _pursueTarget.targetType = TargetTypes.Self;
    }

    public void Die()
    {
        _playerEnergy.AdjustEnergy(_energy);
        DataCollection.AddInfo(DataCollection.DataType.ENEMY_KILL);
        _gameManager.AddBits(_bitsGain);
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _agent.isStopped = false;
            _pursueTarget.targetPosition = other.gameObject.transform.position;
            _pursueTarget.targetType = TargetTypes.Player;
            _myCanvas.gameObject.SetActive(true);
            
            _gameManager.PushMessage("Shoot by pressing Left Mouse Button");

        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _pursueTarget.targetPosition = other.gameObject.transform.position;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            AbandonPursue();
            _myCanvas.gameObject.SetActive(false);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        // if the collision is the projectile Die()
        if (other.gameObject.CompareTag("Projectile"))
        {
            Die();
        }
    }

    enum TargetTypes
    {
        None,
        Player,
        Self
    }

    [Serializable]
    class Target
    {
        public Vector3 targetPosition;
        public TargetTypes targetType;

        public Target(Vector3 targetPosition, TargetTypes targetType)
        {
            this.targetPosition = targetPosition;
            this.targetType = targetType;
        }
    }

    private void OnEnable()
    {
        _initialPosition = transform.position;

        SetTargetToNone();
    }

    private void OnDisable()
    {
        _initialPosition = transform.position;

        SetTargetToNone();
    }
}