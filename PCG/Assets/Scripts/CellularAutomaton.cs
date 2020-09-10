using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;
using TriangleNet.Topology;
using UnityEngine;
using GD.MinMaxSlider;
using TriangleNet;
using Debug = UnityEngine.Debug;

// ReSharper disable All

public class CellularAutomaton : MonoBehaviour
{
    [Header("Cell Conversion to type: required edges")]
    //[Range(0, 3)] [SerializeField] private int _enemyConversion;

    //  [Range(0, 3)] [SerializeField] private int _gatherableConversion;
    [Range(0, 3)]
    [SerializeField]
    private int _treeToEmptyConversion;

    [Range(0, 3)] [SerializeField] private int _emptyToTreeConversion;
    [Range(0, 3)] [SerializeField] private int _emptyToItemConversion;

    [Header("Spawn chance")] [Range(0, 100)] [SerializeField]
    private int _treeChance;

    private float _currentEnemyChance;
    private float _currentGatherableChance;

    [Header("MIN/MAX Spawn chance")] [MinMaxSlider(0, 100)] [SerializeField]
    private Vector2Int _enemyMinMaxChance;

    [MinMaxSlider(0, 100)] [SerializeField]
    private Vector2Int _gatherableMinMaxChance;


    [Header("MIN/MAX iterations per type")] [MinMaxSlider(0, 4)] [SerializeField]
    private Vector2Int _enemyMinMaxIterations;

    [MinMaxSlider(0, 4)] [SerializeField] private Vector2Int _gatherableMinMaxIterations;

    private int _currentEnemyIteration;
    private int _currentGatherableIteration;


    // private List<SpawnLocations> _spawnLocations;
    [Header("Counts")] public int enemyCount;
    public int treeCount;
    public int baseCount;
    public int gatherableCount;


    [Header("Other")] [SerializeField] private ObjectPooler _objectPooler;

    private Stopwatch _stopwatch;

    public enum ObjectTypes
    {
        UNSPAWNABLE,
        ENEMY,
        GATHERABLE,
        TREE,
        BASE,
        EMPTY
    }

    private void Awake()
    {
        _stopwatch = new Stopwatch();

        _objectPooler = gameObject.GetComponent<ObjectPooler>();
    }

    public void DetermineCellsState(TrianglePool trianglePool, bool isInitialRun)
    {
        _stopwatch.Start();

        _objectPooler.CleanUpSpawned();
        SetupChancesAndIterations(isInitialRun);


        if (isInitialRun)
        {
            ExcludeIslandEdgesFromSpawn(trianglePool);
            // find triangle to spawn the base at
            int baseId = FindBaseLocation(trianglePool);


            ScatterTrees(trianglePool, baseId);

            SmoothOutTrees(trianglePool);
        }
        else
        {
            foreach (var triangle in trianglePool)
            {
                if (triangle.type == ObjectTypes.GATHERABLE || triangle.type == ObjectTypes.ENEMY)
                {
                    triangle.type = ObjectTypes.EMPTY;
                }
            }
        }


        ScatterGatherableObjects(trianglePool);


        SmoothOutGatherables(trianglePool);


        ScatterEnemies(trianglePool);


        SmoothOutEnemies(trianglePool);

        _stopwatch.Stop();
        Debug.Log(_stopwatch.ElapsedMilliseconds);
        SpawnObjects(trianglePool, isInitialRun);
    }


    private static void ExcludeIslandEdgesFromSpawn(TrianglePool trianglePool)
    {
        foreach (var triangle in trianglePool)
        {
            if (triangle.isAssignedTerrain)
            {
                for (int j = 0; j < 3; j++) // 3 vertices in the triangle
                {
                    if (!triangle.neighbors[j].Triangle.isAssignedTerrain)
                    {
                        triangle.type = ObjectTypes.UNSPAWNABLE;
                    }
                }
            }
        }
    }

    private void ScatterTrees(TrianglePool trianglePool, int baseId)
    {
        foreach (var triangle in trianglePool)
        {
            bool baseNeighbour = false;

            if (triangle.id == baseId)
            {
                triangle.type = ObjectTypes.BASE;
            }

            for (int j = 0; j < 3; j++) // 3 vertices in the triangle
            {
                if (triangle.neighbors[j].Triangle.type == ObjectTypes.BASE)
                {
                    baseNeighbour = true;
                }
            }

            if (triangle.isAssignedTerrain && triangle.type != ObjectTypes.BASE && triangle.type != ObjectTypes.UNSPAWNABLE && !baseNeighbour)
            {
                int randomCheck = UnityEngine.Random.Range(0, 100);
                triangle.type = randomCheck < _treeChance ? ObjectTypes.TREE : ObjectTypes.EMPTY;
            }
        }
    }

    private void ScatterEnemies(TrianglePool trianglePool)
    {
        foreach (var triangle in trianglePool)
        {
            bool baseNeighbour = false;
            for (int j = 0; j < 3; j++) // 3 vertices in the triangle
            {
                if (triangle.neighbors[j].Triangle.type == ObjectTypes.BASE)
                {
                    baseNeighbour = true;
                }
            }

            if (triangle.isAssignedTerrain && triangle.type == ObjectTypes.EMPTY && !baseNeighbour)
            {
                int randomCheck = UnityEngine.Random.Range(0, 100);
                triangle.type = randomCheck < _currentEnemyChance ? ObjectTypes.ENEMY : ObjectTypes.EMPTY;
            }
        }
    }

    private void ScatterGatherableObjects(TrianglePool trianglePool)
    {
        foreach (var triangle in trianglePool)
        {
            bool baseNeighbour = false;
            for (int j = 0; j < 3; j++) // 3 vertices in the triangle
            {
                if (triangle.neighbors[j].Triangle.type == ObjectTypes.BASE)
                {
                    baseNeighbour = true;
                }
            }

            if (triangle.isAssignedTerrain && triangle.type == ObjectTypes.EMPTY && !baseNeighbour)
            {
                int randomCheck = UnityEngine.Random.Range(0, 100);
                triangle.type = randomCheck < _currentGatherableChance ? ObjectTypes.GATHERABLE : ObjectTypes.EMPTY;
            }
        }
    }

    private void SmoothOutTrees(TrianglePool trianglePool)
    {
        for (int i = 0; i < 2; i++)
        {
            List<ObjectTypes> tempList = new List<ObjectTypes>();

            foreach (var triangle in trianglePool)
            {
                int treeTypeNeighbours = 0;
                int emptyTypeNeighbours = 0;
                tempList.Add(triangle.type);

                if (triangle.isAssignedTerrain && triangle.type != ObjectTypes.UNSPAWNABLE)
                {
                    for (int j = 0; j < 3; j++) // 3 vertices in the triangle
                    {
                        if (triangle.neighbors[j].Triangle.type == ObjectTypes.TREE)
                        {
                            treeTypeNeighbours++;
                        }
                        else if (triangle.neighbors[j].Triangle.type == ObjectTypes.EMPTY || triangle.neighbors[j].Triangle.type == ObjectTypes.UNSPAWNABLE)
                        {
                            emptyTypeNeighbours++;
                        }
                    }


                    if (triangle.type == ObjectTypes.TREE)
                    {
                        tempList[triangle.id] = emptyTypeNeighbours >= _treeToEmptyConversion ? ObjectTypes.EMPTY : ObjectTypes.TREE;
                    }
                    else if (triangle.type == ObjectTypes.EMPTY)
                    {
                        tempList[triangle.id] = treeTypeNeighbours >= _emptyToTreeConversion ? ObjectTypes.TREE : ObjectTypes.EMPTY;
                    }
                }
            }

            foreach (var triangle in trianglePool)
            {
                triangle.type = tempList[triangle.id];
            }
        }
    }

    private void SmoothOutGatherables(TrianglePool trianglePool)
    {
        for (int i = 0; i < _currentGatherableIteration; i++)
        {
            List<ObjectTypes> tempList = new List<ObjectTypes>();

            foreach (var triangle in trianglePool)
            {
                int gatherableTypeNeighbours = 0;
                tempList.Add(triangle.type);

                if (triangle.isAssignedTerrain && triangle.type == ObjectTypes.EMPTY)
                {
                    for (int j = 0; j < 3; j++) // 3 vertices in the triangle
                    {
                        if (triangle.neighbors[j].Triangle.type == ObjectTypes.GATHERABLE)
                        {
                            gatherableTypeNeighbours++;
                        }
                    }


                    if (triangle.type == ObjectTypes.EMPTY)
                    {
                        if (gatherableTypeNeighbours >= _emptyToItemConversion)
                        {
                            int randomCheck = UnityEngine.Random.Range(0, 100);

                            if (randomCheck < _emptyToItemConversion * _currentGatherableIteration * 10)
                            {
                                tempList[triangle.id] = ObjectTypes.GATHERABLE;
                            }
                            else
                            {
                                tempList[triangle.id] = ObjectTypes.EMPTY;
                            }
                        }
                    }
                }
            }

            foreach (var triangle in trianglePool)
            {
                triangle.type = tempList[triangle.id];
            }
        }
    }

    private void SmoothOutEnemies(TrianglePool trianglePool)
    {
        for (int i = 0; i < _currentEnemyIteration; i++)
        {
            List<ObjectTypes> tempList = new List<ObjectTypes>();

            foreach (var triangle in trianglePool)
            {
                int enemyTypeNeighbours = 0;
                tempList.Add(triangle.type);

                if (triangle.isAssignedTerrain && triangle.type == ObjectTypes.EMPTY)
                {
                    for (int j = 0; j < 3; j++) // 3 vertices in the triangle
                    {
                        if (triangle.neighbors[j].Triangle.type == ObjectTypes.ENEMY)
                        {
                            enemyTypeNeighbours++;
                        }
                    }


                    if (triangle.type == ObjectTypes.EMPTY)
                    {
                        if (enemyTypeNeighbours >= _emptyToItemConversion)
                        {
                            int randomCheck = UnityEngine.Random.Range(0, 100);

                            if (randomCheck < _emptyToItemConversion * _currentEnemyIteration * 10)
                            {
                                tempList[triangle.id] = ObjectTypes.ENEMY;
                            }
                            else
                            {
                                tempList[triangle.id] = ObjectTypes.EMPTY;
                            }
                        }
                    }
                }
            }

            foreach (var triangle in trianglePool)
            {
                triangle.type = tempList[triangle.id];
            }
        }
    }

    private void SetupChancesAndIterations(bool initial)
    {
        if (initial)
        {
            DataCollection.SetMinMaxIterations(_enemyMinMaxIterations.x, _enemyMinMaxIterations.y, _gatherableMinMaxIterations.x, _gatherableMinMaxIterations.y);
            _currentEnemyChance = _enemyMinMaxChance.x;
            _currentGatherableChance = _gatherableMinMaxChance.x;

            _currentEnemyIteration = _enemyMinMaxIterations.x;
            _currentGatherableIteration = _gatherableMinMaxIterations.x;
        }
        else
        {
            var chances = DataCollection.GetChanceAdjustments();
            _currentEnemyChance = Mathf.Clamp(_currentEnemyChance + chances.enemyChanceAdjustment, _enemyMinMaxChance.x, _enemyMinMaxChance.y);
            _currentGatherableChance = Mathf.Clamp(_currentGatherableChance + chances.gatherableChanceAdjustment, _gatherableMinMaxChance.x, _gatherableMinMaxChance.y);

            var iterations = DataCollection.GetSmoothingIterations();
            _currentEnemyIteration = Mathf.Clamp((int) iterations.enemyIterations, _enemyMinMaxIterations.x, _enemyMinMaxIterations.y);
            _currentGatherableIteration = Mathf.Clamp((int) iterations.gatherableIterations, _gatherableMinMaxIterations.x, _gatherableMinMaxIterations.y);
        }
    }

    private void SpawnObjects(TrianglePool trianglePool, bool isInitialRun)
    {
        foreach (var triangle in trianglePool)
        {
            var centre = triangle.GetCentroid();

            if (triangle.type == ObjectTypes.BASE && isInitialRun)
            {
                _objectPooler.GetPooledObject(ObjectTypes.BASE, centre);

                baseCount++;
            }
            else if (triangle.type == ObjectTypes.TREE & isInitialRun)
            {
                _objectPooler.GetPooledObject(ObjectTypes.TREE, centre);

                treeCount++;
            }
            else if (triangle.type == ObjectTypes.ENEMY)
            {
                _objectPooler.GetPooledObject(ObjectTypes.ENEMY, centre);

                enemyCount++;
            }
            else if (triangle.type == ObjectTypes.GATHERABLE)
            {
                _objectPooler.GetPooledObject(ObjectTypes.GATHERABLE, centre);

                gatherableCount++;
            }
        }
    }

    private int FindBaseLocation(TrianglePool triangles)
    {
        bool repeat = false;
        int id = -1;
        do
        {
            int randomTriangleIndex = UnityEngine.Random.Range(0, triangles.Count);
            var temp = triangles.Sample(1, new System.Random());

            foreach (var t in temp)
            {
                if (t.isAssignedTerrain && t.type != ObjectTypes.UNSPAWNABLE)
                {
                    t.type = ObjectTypes.BASE;
                    id = t.id;
                    repeat = true;
                }
                else
                {
                    repeat = false;
                }
            }
        } while (!repeat);

        return id;
    }
}