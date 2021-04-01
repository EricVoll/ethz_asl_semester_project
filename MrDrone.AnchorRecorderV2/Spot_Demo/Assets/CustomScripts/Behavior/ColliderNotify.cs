using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderNotify : MonoBehaviour
{
    public List<string> Tags = new List<string>()
    {

    };

    public void Start()
    {
        impl = InterfaceImplementer.GetComponent<IHaveColliderNotifyer>();
    }

    public Component InterfaceImplementer;
    private IHaveColliderNotifyer impl;

    public WaypointController controller;

    private void OnCollisionEnter(Collision collision)
    {
        if (Tags.Contains(collision.gameObject.tag))
        {
            impl.NotifyCollision(collision);
        }
    }
}
