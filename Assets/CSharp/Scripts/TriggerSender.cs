using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerSender : MonoBehaviour
{
    public enum TriggerType
    {
        Enter,
        Stay,
        Exit,
    }

    public void SubscribeMe(TriggerType type, Action<Collider> action)
    {
        switch (type) 
        {
            default:
                Debug.Assert(false, "타입이 지정되지 않았습니다");
                Debug.Break();
                break;

                case TriggerType.Enter:
                OnTriggerEnterSender += action;
                break;

            case TriggerType.Stay:
                OnTriggerStaySender += action;
                break;

            case TriggerType.Exit:
                OnTriggerExitSender += action;
                break;
        }
    }

    public System.Action<Collider> OnTriggerEnterSender;
    public System.Action<Collider> OnTriggerStaySender;
    public System.Action<Collider> OnTriggerExitSender;

    protected virtual void OnTriggerEnter(Collider other)
    {
        OnTriggerEnterSender?.Invoke(other);
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        OnTriggerExitSender?.Invoke(other);
    }

    protected virtual void OnTriggerStay(Collider other)
    {
        OnTriggerStaySender?.Invoke(other);
    }

}
