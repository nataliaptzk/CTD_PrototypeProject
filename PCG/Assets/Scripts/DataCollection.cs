using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using TriangleNet.Voronoi;
using UnityEngine;

// ReSharper disable All

public static class DataCollection
{
    public enum DataType
    {
        NONE,
        ENEMY_KILL,
        GATHERED_ITEM
    }

    public static float _killedEnemies;
    public static float _gatheredItems;

    private static float _enemyRate;
    private static float _gatheredRate;

    private static float _bothSources;

    private static float _enemyChanceAdjustment;
    private static float _gatherableChanceAdjustment;

    private static int _enemyIterations;
    private static int _gatherableIterations;

    private static int _enemyMinIteration;
    private static int _enemyMaxIteration;
    private static int _gatherableMinIteration;
    private static int _gatherableMaxIteration;


    public static void AddInfo(DataType type)
    {
        if (type == DataType.ENEMY_KILL)
        {
            _killedEnemies++;
            CalculateEnemyRate();
        }
        else if (type == DataType.GATHERED_ITEM)
        {
            _gatheredItems++;
            CalculateGatherableRate();
        }
    }


    private static void CalculateBoth()
    {
        _bothSources = _killedEnemies + _gatheredItems;
    }

    private static void CalculateEnemyRate()
    {
        CalculateBoth();
        _enemyRate = _killedEnemies / _bothSources;
    }

    private static void CalculateGatherableRate()
    {
        CalculateBoth();
        _gatheredRate = _gatheredItems / _bothSources;
    }

    public static (float enemyChanceAdjustment, float gatherableChanceAdjustment) GetChanceAdjustments()
    {
        _enemyChanceAdjustment = 0;
        _gatherableChanceAdjustment = 0;

        _enemyChanceAdjustment = _enemyRate - _gatheredRate;
        _gatherableChanceAdjustment = _gatheredRate - _enemyRate;

        return (_enemyChanceAdjustment, _gatherableChanceAdjustment);
    }

    public static (float enemyIterations, float gatherableIterations) GetSmoothingIterations()
    {
        var threshold  = 10;
        _enemyIterations = 0;
        _gatherableIterations = 0;

        _enemyIterations = Mathf.Clamp((int) (_killedEnemies / threshold), _enemyMinIteration, _enemyMaxIteration);

        _gatherableIterations = Mathf.Clamp((int) (_gatheredItems / threshold), _gatherableMinIteration, _gatherableMaxIteration);

        return (_enemyIterations, _gatherableIterations);
    }

    public static void ResetDataCollection()
    {
        _killedEnemies = 0;
        _gatheredItems = 0;
    }

    public static void SetMinMaxIterations(int minE, int maxE, int minG, int maxG)
    {
        _enemyMinIteration = minE;
        _enemyMaxIteration = maxE;
        _gatherableMinIteration = minG;
        _gatherableMaxIteration = maxG;
    }
}