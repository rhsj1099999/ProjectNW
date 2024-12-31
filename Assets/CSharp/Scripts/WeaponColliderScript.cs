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
            Debug.Assert(false, "End Type �� ����������");
            Debug.Break();
        }

        _attackCollider = GetComponent<Collider>();
        
        if (_attackCollider == null )
        {
            Debug.Assert(false, "AttackCollider�� �ݵ�� �־���մϴ�");
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
                Debug.Assert(false, "�θ��߿� �� ������Ʈ�� �ݵ�� �־���մϴ�");
                Debug.Break();
            }
        }

        _ownerCharacterColliderScript.ChangeCollider(_attachType, gameObject);
    }
}
