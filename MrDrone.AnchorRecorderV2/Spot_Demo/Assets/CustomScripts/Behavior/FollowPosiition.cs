using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPosiition : MonoBehaviour
{

    public Transform targetTransform;
    public bool X;
    public bool Y;
    public bool Z;

    // Update is called once per frame
    void Update()
    {
        this.transform.position = new Vector3(get(X).x, get(Y).y, get(Z).z);
    }

    private Vector3 get(bool selector) => selector ? targetTransform.position : transform.position;
}
