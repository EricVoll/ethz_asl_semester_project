using MrDrone.Control;
using RosSharp.RosBridgeClient.MessageTypes.Cgal;
using System.Collections;
using System.Collections.Generic;
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
        ui_thread = new UIThreadHandler();
    }

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
        //vertices = new Vector3[]
        //{
        //    new Vector3(0,0,0),
        //    new Vector3(0,0,1),
        //    new Vector3(0,1,0),
        //};
        //triangles = new int[]
        //{
        //    0,1,2
        //};
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

}
