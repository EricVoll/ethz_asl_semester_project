using MrDrone.Control;
using RosSharp.RosBridgeClient.MessageTypes.Cgal;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    public void ReportNewMesh(TriangleMeshStamped _mesh)
    {
        Debug.Log("Received a new mesh to generate!");

        vertices = new Vector3[_mesh.mesh.vertices.Length];

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

    public void Start()
    {
        mesh = new UnityEngine.Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        // The mesh collider has to be deactivated, wait for a few frames and then activated. Otherwise it won't work.
        GetComponent<MeshCollider>().sharedMesh = mesh;
        GetComponent<MeshCollider>().enabled = false;
        ui_thread = new UIThreadHandler();
    }
    float time;
    public void Update()
    {        
        ui_thread.ReportUpdate();
    }

    Vector3[] vertices;
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
