using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CharacterColliderScript;
using static AnimationFrameDataAsset;


public class CharacterModelDataInitializer : MonoBehaviour
{
    [SerializeField] private List<WeaponColliderScript> _basicModelColliders = new List<WeaponColliderScript>();
    private Dictionary<ColliderAttachType, WeaponColliderScript> _basicColliders = new Dictionary<ColliderAttachType, WeaponColliderScript>();
    public Dictionary<ColliderAttachType, WeaponColliderScript> _BasicColliders => _basicColliders;


    private CharacterScript _owner = null;

    public void Init(CharacterScript owner)
    {
        _owner = owner;
    }

    public List<WeaponColliderScript> GetModelBasicColliders() { return _basicModelColliders; }
    public CharacterScript GetOwner() { return _owner; }



    /*---------------------------------------------------
    |NOTI| ���� ����/����� ���� ������ �����ϴ� �ڵ��Դϴ�.
    ���°� �ٲ���, �� �ڵ�� �ɸ� �������� �����Ǿ��մϴ�.
    ---------------------------------------------------*/

    private void Awake()
    {
        foreach (var collider in _basicModelColliders)
        {
            if (_basicColliders.ContainsKey(collider.GetAttachType()) == true)
            {
                Debug.Assert(false, "�⺻ �浹ü�� ����Ÿ���� �ߺ��˴ϴ�.");
                Debug.Break();
            }

            _basicColliders.Add(collider.GetAttachType(), collider);
        }
    }

    public void AnimationEvent_InvincibleOn()
    {
        StatScript ownerStatScript = _owner.GCST<StatScript>();

        StateContoller ownerStateController = _owner.GCST<StateContoller>();
    }

    public void AnimationEvent_InvincibleOff()
    {
        StatScript ownerStatScript = _owner.GCST<StatScript>();

        StateContoller ownerStateController = _owner.GCST<StateContoller>();
    }








}
