using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AnimationFrameDataAsset;

public class ColliderScript : MonoBehaviour
{
    [SerializeField] protected ColliderAttachType _attachType = ColliderAttachType.ENEND;
    protected Collider _attackCollider = null;
    protected CharacterColliderScript _ownerCharacterColliderScript = null;
    protected bool _isActive = true;
    public bool _IsActive => _isActive;

    public virtual ColliderAttachType GetAttachType() { return _attachType; }

    private void Awake()
    {
        _attackCollider = GetComponent<Collider>();

        if (_attackCollider == null)
        {
            Debug.Assert(false, "AttackCollider�� �ݵ�� �־���մϴ�");
            Debug.Break();
        }
    }
}
