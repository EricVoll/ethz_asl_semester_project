using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FadeBase : MonoBehaviour
{

    public void Awake()
    {
        targetObject = this.gameObject;
    }

    GameObject targetObject;

    private void AssignTarget(GameObject go)
    {
        targetObject = go;
    }

    public void FadeIn(GameObject go = null)
    {
        if (go != null) AssignTarget(go);
        currentAlpha = 0;
        targetObject.SetActive(true);
        StartCoroutine(FadeInIterator(targetObject));
    }
    public void FadeOut(GameObject go = null)
    {
        if (go != null) AssignTarget(go);
        currentAlpha = 1;
        StartCoroutine(FadeOutIterator(targetObject));
    }

    public void ShowImmediate(GameObject go = null)
    {
        if (go != null) AssignTarget(go);
        SetAllAlpha(targetObject, 1);
        AnimationFinished();
    }

    public void HideImmediate(GameObject go = null)
    {
        if (go != null) AssignTarget(go);
        SetAllAlpha(targetObject, 0);
        AnimationFinished();
    }

    private void AnimationFinished()
    {
        //Remove the component after the animation is done
        Destroy(this);
    }


    double currentAlpha = 0;
    float deltaTime = .002f;
    double step = 0.008f;
    // seconds/deltaTime
    private IEnumerator FadeInIterator(GameObject go)
    {
        currentAlpha += step;

        SetAllAlpha(go, (float)currentAlpha);

        if (currentAlpha < 1f)
        {
            yield return new WaitForSeconds(deltaTime);
            yield return FadeInIterator(go);
        }
        else
        {
            AnimationFinished();
        }
    }
    private IEnumerator FadeOutIterator(GameObject go)
    {
        currentAlpha -= step;

        SetAllAlpha(go, (float)currentAlpha);

        if (currentAlpha > 0)
        {
            yield return new WaitForSeconds(deltaTime);
            yield return FadeOutIterator(go);
        }
        else
        {
            go.SetActive(false);
            AnimationFinished();
        }
    }
    private void SetAllAlpha(GameObject go, float alpha)
    {
        var renderers = go.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            if (!renderer.material.HasProperty("_Color"))
            {
                continue;
            }
            var color = renderer.material.color;
            renderer.material.color = new Color(color.r, color.g, color.b, Mathf.Max(Mathf.Min(alpha, 1f), 0f));
        }
        var textMeshPros = go.GetComponentsInChildren<TextMeshPro>();
        foreach (var textMesh in textMeshPros)
        {
            var color = textMesh.color;
            textMesh.color = new Color(color.r, color.g, color.b, Mathf.Max(Mathf.Min(alpha, 1f), 0f));
        }
    }


}
