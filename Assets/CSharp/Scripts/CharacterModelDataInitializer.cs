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
    |NOTI| 상태 진입/벗어남에 의한 버프를 조작하는 코드입니다.
    상태가 바뀔경우, 이 코드로 걸린 버프들은 해제되야합니다.
    ---------------------------------------------------*/

    private void Awake()
    {
        foreach (var collider in _basicModelColliders)
        {
            if (_basicColliders.ContainsKey(collider.GetAttachType()) == true)
            {
                Debug.Assert(false, "기본 충돌체의 부착타입이 중복됩니다.");
                Debug.Break();
            }

            _basicColliders.Add(collider.GetAttachType(), collider);
        }
    }
}
