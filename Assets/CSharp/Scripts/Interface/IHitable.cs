using UnityEngine;

public interface IHitable
{
    public void DealMe(int damage, GameObject caller);
}
