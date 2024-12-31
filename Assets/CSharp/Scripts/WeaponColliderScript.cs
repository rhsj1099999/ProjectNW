using System.Collections;
using System.Collections.Generic;
using static AnimationAttackFrameAsset;
using UnityEngine;

public class WeaponColliderScript : MonoBehaviour
{
    [SerializeField] private ColliderAttachType _attachType = ColliderAttachType.ENEND;
    private Collider _attackCollider = null;
    private CharacterColliderScript _ownerCharacterColliderScript = null;

    private void Awake()
    {
        if (_attachType == ColliderAttachType.ENEND)
        {
            Debug.Assert(false, "End Type 은 쓰지마세요");
            Debug.Break();
        }

        _attackCollider = GetComponent<Collider>();
        
        if (_attackCollider == null )
        {
            Debug.Assert(false, "AttackCollider가 반드시 있어야합니다");
            Debug.Break();
        }
    }

    public void ChangeColliderCall()
    {
        if ( _ownerCharacterColliderScript == null )
        {
            _ownerCharacterColliderScript = GetComponentInParent<CharacterColliderScript>();

            if (_ownerCharacterColliderScript == null)
            {
                Debug.Assert(false, "부모중에 이 컴포넌트는 반드시 있어야합니다");
                Debug.Break();
            }
        }

        _ownerCharacterColliderScript.ChangeCollider(_attachType, gameObject);
    }
}
