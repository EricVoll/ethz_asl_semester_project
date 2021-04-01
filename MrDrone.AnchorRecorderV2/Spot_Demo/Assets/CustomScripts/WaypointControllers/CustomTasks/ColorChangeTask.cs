using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WaypointControl.Task.Interfaces;

public class ColorChangeTask : ITask
{
    public ColorChangeTask(GameObject targetObject, Material material)
    {
        this.targetObject = targetObject;
        this.materialToSet = material;
    }

    private GameObject targetObject;
    private Material materialToSet;

    public void ExecuteAsync(Action<ITaskResult> Callback)
    {
        var renderer = targetObject.GetComponentsInChildren<Renderer>().Where(x => x.CompareTag("WaypointBody"));
        if(renderer != null)
        {
            foreach (var item in renderer)
            {
                item.material = materialToSet;
            }
        }
        
        Callback(null);
    }
}
