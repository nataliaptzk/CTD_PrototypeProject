using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ObjectPooler : MonoBehaviour
{
    [Header("Spawnables")] [SerializeField]
    private GameObject _spawnablesStaticPool;

    [SerializeField] private GameObject _spawnablesEnemiesPool;
    [SerializeField] private GameObject _spawnablesGatherablesPool;
    private List<GameObject> _nonStaticPoolHolders;
    [SerializeField] private GameObject _player;

    [Header("Prefabs")] [SerializeField] private GameObject _enemyPrefab;

    [SerializeField] private GameObject _gatherablePrefab;
    [SerializeField] private GameObject _treePrefab;
    [SerializeField] private GameObject _basePrefab;


    private void Awake()
    {
        _nonStaticPoolHolders = new List<GameObject>();

        // not adding static objects here
        _nonStaticPoolHolders.Add(_spawnablesGatherablesPool);
        _nonStaticPoolHolders.Add(_spawnablesEnemiesPool);
    }


    public void GetPooledObject(CellularAutomaton.ObjectTypes objectType, Vector2 centre)
    {
        if (objectType == CellularAutomaton.ObjectTypes.ENEMY)
        {
            for (int i = 0; i < _spawnablesEnemiesPool.transform.childCount; i++)
            {
                if (!_spawnablesEnemiesPool.transform.GetChild(i).gameObject.activeInHierarchy)
                {
                    _spawnablesEnemiesPool.transform.GetChild(i).gameObject.transform.position = new Vector3(centre.x, _enemyPrefab.gameObject.transform.localScale.y / 2, centre.y);
                    _spawnablesEnemiesPool.transform.GetChild(i).gameObject.SetActive(true);
                    return;
                }
            }

            Instantiate(_enemyPrefab, new Vector3(centre.x, _enemyPrefab.gameObject.transform.localScale.y / 2, centre.y), Quaternion.identity, _spawnablesEnemiesPool.transform);
        }
        else if (objectType == CellularAutomaton.ObjectTypes.GATHERABLE)
        {
            for (int i = 0; i < _spawnablesGatherablesPool.transform.childCount; i++)
            {
                if (!_spawnablesGatherablesPool.transform.GetChild(i).gameObject.activeInHierarchy)
                {
                    _spawnablesGatherablesPool.transform.GetChild(i).gameObject.transform.position = new Vector3(centre.x, _gatherablePrefab.gameObject.transform.localScale.y / 2, centre.y);
                    _spawnablesGatherablesPool.transform.GetChild(i).gameObject.SetActive(true);

                    return;
                }
            }

            Instantiate(_gatherablePrefab, new Vector3(centre.x, _gatherablePrefab.gameObject.transform.localScale.y / 2, centre.y), Quaternion.identity, _spawnablesGatherablesPool.transform);
        }
        else if (objectType == CellularAutomaton.ObjectTypes.BASE)
        {
            for (int i = 0; i < _spawnablesStaticPool.transform.childCount; i++)
            {
                if (!_spawnablesStaticPool.transform.GetChild(i).gameObject.activeInHierarchy)
                {
                    _spawnablesStaticPool.transform.GetChild(i).gameObject.SetActive(true);
                    return;
                }
            }

            Instantiate(_basePrefab, new Vector3(centre.x, _gatherablePrefab.gameObject.transform.localScale.y + 3, centre.y), Quaternion.identity, _spawnablesStaticPool.transform);
            _player.transform.position = new Vector3(centre.x, 3, centre.y);
        }
        else if (objectType == CellularAutomaton.ObjectTypes.TREE)
        {
            for (int i = 0; i < _spawnablesStaticPool.transform.childCount; i++)
            {
                if (!_spawnablesStaticPool.transform.GetChild(i).gameObject.activeInHierarchy)
                {
                    _spawnablesStaticPool.transform.GetChild(i).gameObject.SetActive(true);
                    return;
                }
            }

            Instantiate(_treePrefab, new Vector3(centre.x, _gatherablePrefab.gameObject.transform.localScale.y / 2, centre.y), Quaternion.identity, _spawnablesStaticPool.transform);
        }
    }

    public void CleanUpSpawned()
    {
        foreach (var poolHolder in _nonStaticPoolHolders)
        {
            for (int i = 0; i < poolHolder.transform.childCount; i++)
            {
                poolHolder.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }
}