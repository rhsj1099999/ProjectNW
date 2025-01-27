using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CharacterColliderScript;
    
public class CharacterModelDataInitializer : MonoBehaviour
{
    [SerializeField] private List<WeaponColliderScript> _basicModelColliders = new List<WeaponColliderScript>();

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

    public void AnimationEvent_InvincibleOn()
    {
        StatScript ownerStatScript = _owner.GCST<StatScript>();

        StateContoller ownerStateController = _owner.GCST<StateContoller>();

        //ownerStatScript.TryStateBuff
        

        //ownerStatScript._invincible = true;
    }

    public void AnimationEvent_InvincibleOff()
    {
        StatScript ownerStatScript = _owner.GCST<StatScript>();

        StateContoller ownerStateController = _owner.GCST<StateContoller>();

        //ownerStatScript._invincible = false;
    }








}
