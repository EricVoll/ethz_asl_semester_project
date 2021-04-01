using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHaveColliderNotifyer
{
    void NotifyCollision(Collision collision);
}
