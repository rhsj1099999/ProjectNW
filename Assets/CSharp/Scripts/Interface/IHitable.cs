using UnityEngine;

public interface IHitable
{
    public void DealMe(DamageDesc damage, GameObject caller);
}
