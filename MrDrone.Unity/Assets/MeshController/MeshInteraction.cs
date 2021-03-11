using Microsoft.MixedReality.Toolkit.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshInteraction : MonoBehaviour, IMixedRealityPointerHandler
{
    public Action<Vector3> VertexClicked;

    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.parent = this.transform;
        go.transform.position = eventData.Pointer.Result.Details.Point;
        Vector3 relativePos = transform.InverseTransformPoint(go.transform.position);
        GameObject.Destroy(go);
        VertexClicked?.Invoke(relativePos);
    }

    public void OnPointerDown(MixedRealityPointerEventData eventData) { }

    public void OnPointerDragged(MixedRealityPointerEventData eventData) { }
    public void OnPointerUp(MixedRealityPointerEventData eventData) { }
}
