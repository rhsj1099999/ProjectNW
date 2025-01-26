using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerSender_BattleCollision : TriggerSender
{
    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        Debug.Log("Enter");
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerEnter(other);
        Debug.Log("Exit");
    }

    protected override void OnTriggerStay(Collider other)
    {
        base.OnTriggerEnter(other);
        Debug.Log("Stay");
    }
}
