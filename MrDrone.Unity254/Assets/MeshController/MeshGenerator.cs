using RosSharp;
using RosSharp.RosBridgeClient.MessageTypes.Cgal;
using RosSharp.RosBridgeClient.MessageTypes.Geometry;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public static class PointExtensions
{
    public static UnityEngine.Vector3 ToUnity(this Point p, bool ros2Unity = true)
    {
        UnityEngine.Vector3 v = new UnityEngine.Vector3();

        v.x = (float)p.x;
        v.y = (float)p.y;
        v.z = (float)p.z;

        if (ros2Unity)
            return TransformExtensions.Ros2Unity(v);
        else return v;
    }
}

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    public void ReportNewMesh(TriangleMeshStamped _mesh)
    {
        Debug.Log("Received a new mesh to generate!");

        vertices = new UnityEngine.Vector3[_mesh.mesh.vertices.Length];

        for (int i = 0; i < _mesh.mesh.vertices.Length; i++)
        {
            vertices[i] = _mesh.mesh.vertices[i].ToUnity();
        }

        // Unity takes the triangle indices as one array. Unravel it.
        triangles = new int[_mesh.mesh.triangles.Length * 3];

        for (int i = 0; i < _mesh.mesh.triangles.Length; i++)
        {
            triangles[3 * i + 0] = (int)_mesh.mesh.triangles[i].vertex_indices[2];
            triangles[3 * i + 1] = (int)_mesh.mesh.triangles[i].vertex_indices[1];
            triangles[3 * i + 2] = (int)_mesh.mesh.triangles[i].vertex_indices[0];
        }

        ui_thread.ExecuteOnMainThread(UpdateMesh);
    }

    public void Awake()
    {
        ui_thread = new UIThreadHandler();
    }

    public void Start()
    {
        mesh = new UnityEngine.Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        // The mesh collider has to be deactivated, wait for a few frames and then activated. Otherwise it won't work.
        GetComponent<MeshCollider>().sharedMesh = mesh;
        GetComponent<MeshCollider>().enabled = false;
    }
    float time;
    public void Update()
    {        
        ui_thread.ReportUpdate();
    }

    UnityEngine.Vector3[] vertices;
    int[] triangles;
    UIThreadHandler ui_thread;
    Mesh mesh;

    private void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        ui_thread.ExecuteOnMainThreadAfter(() => { GetComponent<MeshCollider>().enabled = true; }, 1f, this);
    }
}
