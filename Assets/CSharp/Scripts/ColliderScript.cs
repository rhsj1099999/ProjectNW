using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AnimationAttackFrameAsset;

public class ColliderScript : MonoBehaviour
{
    [SerializeField] protected ColliderAttachType _attachType = ColliderAttachType.ENEND;
    protected Collider _attackCollider = null;
    protected CharacterColliderScript _ownerCharacterColliderScript = null;

    public virtual ColliderAttachType GetAttachType() { return _attachType; }

    private void Awake()
    {
        _attackCollider = GetComponent<Collider>();

        if (_attackCollider == null)
        {
            Debug.Assert(false, "AttackCollider가 반드시 있어야합니다");
            Debug.Break();
        }
    }
}
