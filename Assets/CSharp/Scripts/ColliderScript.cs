using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AnimationFrameDataAsset;

public class ColliderScript : MonoBehaviour
{
    [SerializeField] protected ColliderAttachType _attachType = ColliderAttachType.ENEND;
    protected Collider _attackCollider = null;
    protected CharacterColliderScript _ownerCharacterColliderScript = null;
    protected Vector3 _movedDir = Vector3.zero;
    protected Vector3 _prevPosition = Vector3.zero;

    public virtual ColliderAttachType GetAttachType() { return _attachType; }

    public Vector3? GetMoveDir()
    {
        return _movedDir;
    }

    private void OnEnable()
    {
        _prevPosition = transform.position;
    }

    private void LateUpdate()
    {
        _movedDir = (transform.position - _prevPosition).normalized;
        _prevPosition = transform.position;
    }

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
