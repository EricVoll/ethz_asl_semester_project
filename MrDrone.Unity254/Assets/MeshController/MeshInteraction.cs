using Microsoft.MixedReality.Toolkit.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshInteraction : MonoBehaviour, IMixedRealityPointerHandler
{
    public Action<Vector3> VertexClicked;
    public GameObject PointClickedPrefab;
    public GameObject TrajectoryHolder;
    public LineRenderer lineRenderer;

    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    {
        GameObject go = GameObject.Instantiate(PointClickedPrefab);
        go.transform.parent = this.transform;
        go.transform.position = eventData.Pointer.Result.Details.Point;
        Vector3 relativePos = transform.InverseTransformPoint(go.transform.position);
        //GameObject.Destroy(go);
        VertexClicked?.Invoke(relativePos);

        ReportNewLinePoint(relativePos);
    }

    List<Vector3> points = new List<Vector3>();
    private void ReportNewLinePoint(Vector3 v)
    {
        points.Add(v);
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }

    public void OnPointerDown(MixedRealityPointerEventData eventData) { }

    public void OnPointerDragged(MixedRealityPointerEventData eventData) { }
    public void OnPointerUp(MixedRealityPointerEventData eventData) { }
}
