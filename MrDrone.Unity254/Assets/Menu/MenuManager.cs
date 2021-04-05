using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public GameObject RobotModel;
    public GameObject ManifoldModel;
    public GameObject TooltipModel;
    public GameObject[] Cubes;
    public Material MeshMaterial;
    public void ToggleRobotModel()
    {
        Toggle(RobotModel);
    }

    public void ToggleManifold()
    {
        Toggle(ManifoldModel);
    }

    public void ToggleTooltip()
    {
        Toggle(TooltipModel);
    }

    private void Toggle(GameObject obj)
    {
        obj.SetActive(!obj.activeInHierarchy);
    }

    public void ToggleCubes()
    {
        foreach (var item in Cubes)
        {
            Toggle(item);
        }
    }

    public void SliderChanged(SliderEventData data)
    {
        var col = ManifoldModel.GetComponent<Renderer>().material.GetColor("_BaseColor");
        col.a = data.NewValue;
        ManifoldModel.GetComponent<Renderer>().material.SetColor("_BaseColor", col);
    }

}
