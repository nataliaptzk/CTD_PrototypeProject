using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using TriangleNet.Topology;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using TriangleNet.Topology.DCEL;
using TriangleNet.Voronoi;
using UnityEditor;
using Debug = UnityEngine.Debug;
using Vertex = TriangleNet.Geometry.Vertex;

// ReSharper disable All


[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider))]
public class TerrainGenerator : MonoBehaviour
{
    [Header("Poisson Sampling Generation")] [Range(10, 150)] [SerializeField]
    private float _minDistancePerPoint;

    [Range(50, 150)] [SerializeField] private int _minDistancePerPointForVoronoi;

    private List<Vector2> _poissonPointsForMainMesh = new List<Vector2>();
    private List<Vector2> _poissonPointsForVoronoiDiagram = new List<Vector2>();


    [Range(5, 50)] [SerializeField] private int _rejectionSamples; // optimal is 30


    [Header("Terrain Size")] [Range(1, 1000)] [SerializeField]
    private int _sizeX;

    [Range(1, 1000)] [SerializeField] private int _sizeY;
    private UnityEngine.Mesh _terrainMesh;

    //   private List<TrianglesState> _trianglesStates;

    private TriangleNet.Mesh _triangulatedMesh;

    private Polygon _polygon;
    private Polygon _polygonForVoronoi;

    private StandardVoronoi _voronoi;

    private TriangleNet.Mesh _voronoiMesh;

    public Material material;

    private CellularAutomaton _cellularAutomaton;

    [Header("Dummy stats")] public int killedEnemiesDummy;
    public int gatheredObjectsDummy;
    private int terrainValue;
    [Header("Other")] [SerializeField] private GameObject _loadingImage;
    private Stopwatch stopwatch;

    private void Awake()
    {
        stopwatch = new Stopwatch();
    }

    public void Initiate()
    {
        terrainValue = 0;
        _polygon = new Polygon();
        _cellularAutomaton = gameObject.GetComponent<CellularAutomaton>();

        _poissonPointsForMainMesh = PoissonDiscSampling.GeneratePoints(_minDistancePerPoint, new Vector2(_sizeX, _sizeY), _rejectionSamples);
        for (int i = 0; i < _poissonPointsForMainMesh.Count; i++)
        {
            _polygon.Add(new Vertex(_poissonPointsForMainMesh[i].x, _poissonPointsForMainMesh[i].y));
        }

        _polygon.Add(new Vertex(0, 0));
        _polygon.Add(new Vertex(0, _sizeY));
        _polygon.Add(new Vertex(_sizeX, 0));
        _polygon.Add(new Vertex(_sizeX, _sizeY));

        ConstraintOptions constraints = new ConstraintOptions();
        constraints.ConformingDelaunay = true;
        constraints.SegmentSplitting = 0;

        _triangulatedMesh = _polygon.Triangulate(constraints) as TriangleNet.Mesh;

        GenerateVoronoiDiagram();

        GenerateMesh();

        _cellularAutomaton.DetermineCellsState(_triangulatedMesh.triangles, true);


        Time.timeScale = 0;
        _loadingImage.SetActive(false);
    }

    public void Revaluate()
    {
        _cellularAutomaton.DetermineCellsState(_triangulatedMesh.triangles, false);
    }

    public void DummyAmounts()
    {
        for (int i = 0; i < killedEnemiesDummy; i++)
        {
            DataCollection.AddInfo(DataCollection.DataType.ENEMY_KILL);
        }

        Debug.Log(DataCollection._killedEnemies);


        for (int i = 0; i < gatheredObjectsDummy; i++)
        {
            DataCollection.AddInfo(DataCollection.DataType.GATHERED_ITEM);
        }

        Debug.Log(DataCollection._gatheredItems);
    }

    private void GenerateVoronoiDiagram()
    {
        _polygonForVoronoi = new Polygon();

        _poissonPointsForMainMesh = PoissonDiscSampling.GeneratePoints(_minDistancePerPointForVoronoi, new Vector2(_sizeX, _sizeY), _rejectionSamples);
        for (int i = 0; i < _poissonPointsForMainMesh.Count; i++)
        {
            _polygonForVoronoi.Add(new Vertex(_poissonPointsForMainMesh[i].x, _poissonPointsForMainMesh[i].y));
        }

        _polygonForVoronoi.Add(new Vertex(0, 0));
        _polygonForVoronoi.Add(new Vertex(0, _sizeY));
        _polygonForVoronoi.Add(new Vertex(_sizeX, 0));
        _polygonForVoronoi.Add(new Vertex(_sizeX, _sizeY));

        ConstraintOptions constraints = new ConstraintOptions();
        constraints.ConformingDelaunay = true;
        constraints.SegmentSplitting = 0;

        _voronoiMesh = _polygonForVoronoi.Triangulate(constraints) as TriangleNet.Mesh;

        _voronoi = new StandardVoronoi(_voronoiMesh);
    }

    private List<Vector2> GetVerticesList(Face face)
    {
        List<Vector2> verticesList = new List<Vector2>();
        foreach (var edge in face.EnumerateEdges())
        {
            verticesList.Add(new Vector2((float) edge.origin.x, (float) edge.origin.y));
        }

        return verticesList;
    }


    // https://wiki.unity3d.com/index.php/PolyContainsPoint
    public bool FaceContainsPoint(Face face, Vertex[] vertices)
    {
        List<Vector2> facePoints = GetVerticesList(face);

        var j = facePoints.Count - 1;
        var inside = false;

        for (int a = 0; a < vertices.Length; a++)
        {
            for (int i = 0; i < facePoints.Count; j = i++)
            {
                var pi = facePoints[i];
                var pj = facePoints[j];
                if (((pi.y <= vertices[a].y && vertices[a].y < pj.y) || (pj.y <= vertices[a].y && vertices[a].y < pi.y)) &&
                    (vertices[a].x < (pj.x - pi.x) * (vertices[a].y - pi.y) / (pj.y - pi.y) + pi.x))
                    inside = !inside;
            }

            if (inside) break;
        }


        return inside;
    }

    private void AssignTerrainTriangles()
    {
        foreach (var face in _voronoi.Faces)
        {
            bool proceed = true;
            foreach (var edge in face.EnumerateEdges())
            {
                if (!edge.twin.face.bounded)
                {
                    proceed = false;
                }
            }

            if (face.bounded && proceed)
            {
                foreach (var triangle in _triangulatedMesh.triangles)
                {
                    if (!triangle.isAssignedTerrain)
                    {
                        if (FaceContainsPoint(face, triangle.vertices))
                        {
                            triangle.isAssignedTerrain = true;
                            triangle.voronoiRegionID = face.id;
                            triangle.color = new Color(0, 0.7f, 0.2f, 1);
                            terrainValue++;
                        }
                    }
                }
            }
        }
    }

    private void AssignTriangles()
    {
        foreach (var triangle in _triangulatedMesh.triangles)
        {
            triangle.color = Color.cyan;
            triangle.isAssignedTerrain = false;
            triangle.type = CellularAutomaton.ObjectTypes.EMPTY;
        }
    }


    private void OnDrawGizmos()
    {
        if (_voronoi != null)
        {
            foreach (var face in _voronoi.Faces)
            {
                HalfEdge e = null;
                Vector3 c1;
                Vector3 c2;


                foreach (var edge in face.EnumerateEdges())
                {
                    e = edge;


                    if (face.Bounded)
                    {
                        c1 = new Vector3((float) e.Origin.x, 0, (float) e.Origin.y);
                        c2 = new Vector3((float) e.Next.origin.x, 0, (float) e.Next.origin.y);
                        Gizmos.DrawLine(c1, c2);
                    }
                }

                if (!face.Bounded)
                {
                    c1 = new Vector3((float) e.Twin.Origin.x, 0, (float) e.Twin.Origin.y);
                    c2 = new Vector3((float) e.Twin.Next.origin.x, 0, (float) e.Twin.Next.origin.y);
                    Gizmos.DrawLine(c1, c2);
                }
            }
        }
    }


    private void GenerateMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<Color> colors = new List<Color>();
        List<int> triangles = new List<int>();

        IEnumerator<Triangle> triangleEnum = _triangulatedMesh.Triangles.GetEnumerator();

        AssignTerrainTriangles();

        for (int i = 0; i < _triangulatedMesh.Triangles.Count; i++)
        {
            if (!triangleEnum.MoveNext())
            {
                break;
            }

            Triangle currentTriangle = triangleEnum.Current;

            Vector3 v0 = new Vector3((float) currentTriangle.vertices[2].x, 0, (float) currentTriangle.vertices[2].y);
            Vector3 v1 = new Vector3((float) currentTriangle.vertices[1].x, 0, (float) currentTriangle.vertices[1].y);
            Vector3 v2 = new Vector3((float) currentTriangle.vertices[0].x, 0, (float) currentTriangle.vertices[0].y);

            triangles.Add(vertices.Count);
            triangles.Add(vertices.Count + 1);
            triangles.Add(vertices.Count + 2);

            vertices.Add(v0);
            vertices.Add(v1);
            vertices.Add(v2);

            var normal = Vector3.Cross(v1 - v0, v2 - v0);

            Color triangleColor = Color.gray;


            triangleColor = currentTriangle.color;

            for (int x = 0; x < 3; x++)
            {
                normals.Add(normal);
                uvs.Add(Vector3.zero);
                colors.Add(triangleColor);
            }
        }

        _terrainMesh = new UnityEngine.Mesh();
        _terrainMesh.vertices = vertices.ToArray();
        _terrainMesh.uv = uvs.ToArray();
        _terrainMesh.triangles = triangles.ToArray();
        _terrainMesh.colors = colors.ToArray();
        _terrainMesh.normals = normals.ToArray();
        transform.GetComponent<MeshFilter>().mesh = _terrainMesh;
        transform.GetComponent<MeshCollider>().sharedMesh = _terrainMesh;
        transform.GetComponent<MeshRenderer>().material = material;
    }

    public void SaveMesh()
    {
        if (transform.GetComponent<MeshFilter>() != null)
        {
            var path = "Assets/GeneratedMesh" + ".asset";
            AssetDatabase.CreateAsset(transform.GetComponent<MeshFilter>().sharedMesh, path);
        }
    }
}
/*
public class TrianglesState
{
    public bool isAssigned; // true = terrain, false = water
    public Color triangleColor;
    public Triangle triangleCopy;
    public int voronoiRegionId;

    public TrianglesState(Color triangleColor, Triangle triangleCopy)
    {
        this.triangleCopy = triangleCopy;
        this.triangleColor = triangleColor;
        isAssigned = false;
        voronoiRegionId = -1;
    }
}*/