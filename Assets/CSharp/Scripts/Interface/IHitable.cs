using UnityEngine;

public interface IHitable
{
    public void DealMe_Final
        (
            DamageDesc damage,
            bool isCriticalHit,
            CharacterScript attacker,
            CharacterScript victim,
            ref Vector3 closetPoint,
            ref Vector3 hitNormal
        );
}
