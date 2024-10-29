using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    //��ǥ ������Ʈ
    private InputController _inputController = null;
    private CharacterMoveScript2 _characterMoveScript2 = null;
    private StateContoller _stateContoller = null;


    //Weapon Section -> �̰� �ٸ� ������Ʈ�� ������(���� ��������ִ°� EquipmentBoard Ȥ�� Inventory)
    private WeaponScript _currEquipWeapon = null;
    private ItemInfo _currEquipWeaponItem = null;
    public WeaponScript GetWeapon() { return _currEquipWeapon; }
    public ItemInfo GetWeaponItem() { return _currEquipWeaponItem; }
    private WeaponScript[] _tempLeftWeapons;
    private int _currLeftWeaponIndex = 0;
    private WeaponScript _tempCurrLeftWeapon = null;
    private WeaponScript[] _tempRightWeapons;
    private int _currRightWeaponIndex = 0;
    private WeaponScript _tempCurrTightWeapon = null;
    //����� ĳ���͸޽��� �ִϸ����͸� �����ֱ� ������ �ִϸ����͸� �����ִ� ���ӿ�����Ʈ�� ĳ���� �޽���
    private GameObject _characterModelObject = null; 



    //Aim System
    private AimScript2 _aimScript = null;




    //Animator Secton -> �̰� �ٸ� ������Ʈ�� ������
    private Animator _animator = null;
    private AnimatorOverrideController _overrideController = null;
    private bool _corutineStarted = false;
    private float _blendTarget = 0.0f;
    private string _targetName1 = "Human@Idle01";
    private string _targetName2 = "UseThisToChange1";
    private int _currentLayerIndex = 0;
    private int _maxLayer = 2;
    private AnimationClip _currAnimClip = null;
    [SerializeField] private float _transitionSpeed = 10.0f;
    private float _currAnimationSeconds = 0.0f;
    private int _currAnimationLoopCount = 0;
    public float GetCurrAnimationClipFrame() { return _currAnimClip.frameRate * _currAnimationSeconds; }
    public float GetCurrAnimationClipSecond() { return _currAnimationSeconds; }
    public int GetCurrAnimationLoopCount() { return _currAnimationLoopCount; }

    private void Awake()
    {
        _inputController = GetComponent<InputController>();
        Debug.Assert( _inputController != null, "��ǲ ��Ʈ�ѷ��� ����");

        _characterMoveScript2 = GetComponent<CharacterMoveScript2>();
        Debug.Assert(_characterMoveScript2 != null, "CharacterMove ������Ʈ ����");

        _stateContoller = GetComponent<StateContoller>();
        Debug.Assert(_stateContoller != null, "StateController�� ����");

        _animator = GetComponentInChildren<Animator>();
        Debug.Assert(_animator != null, "StateController�� ����");
        _overrideController = new AnimatorOverrideController(_animator.runtimeAnimatorController);
        _animator.runtimeAnimatorController = _overrideController;
        _characterModelObject = _animator.gameObject;

        /*------------------------
        |TODO| �ӽ��ڵ��̴�. ������
        --------------------------*/
        {
            _tempLeftWeapons = new WeaponScript[3];
            _tempLeftWeapons[0] = null;
            _tempLeftWeapons[1] = null;
            _tempLeftWeapons[2] = null;

            _tempRightWeapons = new WeaponScript[3];

            _tempRightWeapons[0] = new WeaponScript();
            _tempRightWeapons[0]._weaponType = ItemInfo.WeaponType.LargeSword;

            _tempRightWeapons[1] = new WeaponScript();
            _tempRightWeapons[1]._weaponType = ItemInfo.WeaponType.MediumGun;

            _tempRightWeapons[2] = null;
        }
    }

    private void Start()
    {
        State currState = _stateContoller.GetCurrState();
        AnimationClip currAnimationClip = currState.GetStateDesc()._stateAnimationClip;
        _currAnimClip = currAnimationClip;
    }

    public void StateChanged()
    {
        /*-----------------------------------------------------------------------------------------
        |TODO| ���� �� �ڵ带 ���� ���ߵǳ�? Root ����� Late Tick �� ������ ���Ǽ� �ʿ��ϱ� �ѵ�
        -----------------------------------------------------------------------------------------*/
        _currAnimationSeconds = 0.0f;
        _currAnimationLoopCount = 0;
    }

    private void ReadyAimSystem()
    {
        if (_aimScript == null)
        {
            _aimScript = transform.gameObject.AddComponent<AimScript2>();
        }
        _aimScript.enabled = true;
    }

    private void Update()
    {
        //Ÿ�ӵ����
        {
            if (Input.GetKeyDown(KeyCode.L))  // SŰ�� ������ ���� �ӵ��� ������ ��
            {
                Time.timeScale = 0.1f;
                Time.fixedDeltaTime = Time.timeScale * 0.02f;  // ������ �ð� ������Ʈ
            }

            if (Input.GetKeyDown(KeyCode.O))  // RŰ�� ������ ���� �ӵ��� �������� ����
            {
                Time.timeScale = 1.0f;
                Time.fixedDeltaTime = Time.timeScale * 0.02f;
            }
        }

        /*-----------------------------------------------------------------------------------------------------------------
        |TODO| �� ������ �־�� �ϳ�? ���������� ���̴� ������Ʈ�� ���ؼ�, ��������ִ� ������Ʈ�鿡�� �̾���̴°� �����Ƽ� ��������
        -----------------------------------------------------------------------------------------------------------------*/
        //Weapon Change Check
        {
            WeaponScript nextWeapon = null;

            bool weaponChangeTry = false;

            if (Input.GetKeyDown(KeyCode.R))
            {
                //�޼� ���� �������� ��ȯ
                weaponChangeTry = true;

                _currLeftWeaponIndex++;
                if (_currLeftWeaponIndex >= 3)
                {
                    _currLeftWeaponIndex = _currLeftWeaponIndex % 3;
                }
                nextWeapon = _tempLeftWeapons[_currLeftWeaponIndex];
            }
            else if (Input.GetKeyDown(KeyCode.T))
            {
                //������ ���� �������� ��ȯ
                weaponChangeTry = true;

                _currRightWeaponIndex++;
                if (_currRightWeaponIndex >= 3)
                {
                    _currRightWeaponIndex = _currRightWeaponIndex % 3;
                }
                nextWeapon = _tempRightWeapons[_currRightWeaponIndex];
            }

            //���� ��ȯ�� �õ��ߴ�
            if (weaponChangeTry == true) 
            {
                //���� ���Ⱑ ����.
                if (nextWeapon == null)
                {
                    if (_aimScript != null)
                    {
                        _aimScript.enabled = false;
                    }
                }
                else
                {
                    //���� ���Ⱑ �ִ�.

                    //1. ���� �ý��� Ȱ��/��Ȱ��ȭ
                    {
                        if (nextWeapon._weaponType >= ItemInfo.WeaponType.NotWeapon && nextWeapon._weaponType <= ItemInfo.WeaponType.LargeSword)
                        {
                            if (_aimScript != null)
                            {
                                _aimScript.enabled = false;
                            }
                        }

                        if (nextWeapon._weaponType >= ItemInfo.WeaponType.SmallGun && nextWeapon._weaponType <= ItemInfo.WeaponType.LargeGun)
                        {
                            ReadyAimSystem();
                        }
                    }

                    //2. ������ ���� ����(�޽� ���̱�) -> �ʿ��ϴٸ� IK ����
                    {
                        //������ ������ ����
                        {
                            GameObject prefab = Resources.Load<GameObject>(nextWeapon._itemPrefabRoute);
                            GameObject newObject = Instantiate(prefab);
                        }

                        //���� ã��
                        {
                            Debug.Assert(_characterModelObject != null, "���⸦ ���̷��µ� ���� ����� �ȵȴ�");
                            WeaponSocketScript[] weaponSockets = _characterModelObject.GetComponentsInChildren<WeaponSocketScript>();

                            Debug.Assert(weaponSockets.Length > 0, "���⸦ ���̷��µ� �𵨿� ������ ����");

                            foreach (var socketComponent in weaponSockets)
                            {

                            }

                            Transform correctSocket = null;
                        }


                        

                        //newObject.transform.position = _aimSatellite.transform.position;
                        //newObject.transform.SetParent(_aimSatellite.transform);
                    }


                }
            }
        }


        /*-----------------------------------------------------------------------------------------------------------------
        |TODO| ����� �� ������Ʈ�� ����� ���¸� ���� ����ִ�. ���ݻ����� ��쿡�� ���Ⱑ ���¸� ����־�� ���� ������?
        -----------------------------------------------------------------------------------------------------------------*/
        //Weapon Change Check
        {
            //���⼭ ������ȯ�� �õ������� -----|
        }                                   //|
        {//���� ������Ʈ                     //|
            _stateContoller.DoWork(); // <----| �̰� �þ���Ѵ�
        }


        {//�⺻������ �߷��� ��� ������Ʈ �Ѵ�
            _characterMoveScript2.GravityUpdate();
        }

        _characterMoveScript2.ClearLatestVelocity();
    }

    private void LateUpdate()
    {
        {//�ִϸ��̼� ������Ʈ
            State currState = _stateContoller.GetCurrState();
            AnimationClip currAnimationClip = currState.GetStateDesc()._stateAnimationClip;

            if (_currAnimClip != currAnimationClip)
            {
                _currAnimClip = currAnimationClip;
                ChangeAnimation(currAnimationClip);
            }

            _currAnimationSeconds += Time.deltaTime * _animator.speed;

            if (_currAnimationSeconds > currAnimationClip.length)
            {
                _currAnimationSeconds -= currAnimationClip.length;
                _currAnimationLoopCount++;
            }
        }
    }

    private IEnumerator SwitchingBlendCoroutine()
    {
        //Layer �ε��� ����
        while (true)
        {
            float blendDelta = (_currentLayerIndex == 0) //0�� ���̾ ����ؾ��ϳ�
                ? Time.deltaTime * -_transitionSpeed   //0�� ���̾ ����ؾ��Ѵٸ� ��ǥ���� 0.0
                : Time.deltaTime * _transitionSpeed;  //1�� ���̾ ����ؾ��Ѵٸ� ��ǥ���� 1.0

            _blendTarget += blendDelta;

            /*-----------------------------------------------------------------
            |TODO| �� �ڵ�� ����
            -----------------------------------------------------------------*/

            if (_blendTarget > 1.0f)
            {
                _blendTarget = 1.0f - float.Epsilon;
                _animator.SetLayerWeight(1, _blendTarget);
                break;
            }
            else if (_blendTarget < 0.0f)
            {
                _blendTarget = 0.0f;
                _animator.SetLayerWeight(1, _blendTarget);
                break;
            }

            _animator.SetLayerWeight(1, _blendTarget);

            yield return null;
        }

        _corutineStarted = false;
    }


    private void ChangeAnimation(AnimationClip targetClip)
    {
        /*----------------------------------------------------------------------------------------------------------
        |TODO| Jump -> Sprint ���� ��ȯ��
        �����Ƿ� ���� Jump -> Move -> Idle�� ��ȯ�� �̷������.
        �̶� �ʹ����� ��ȯ���� ���� ��� �ڷ�ƾ�� ����Ǹ鼭 ����� �ڷ���Ʈ�Ѵ� �����ض�
        ----------------------------------------------------------------------------------------------------------*/

        {
            AnimatorClipInfo[] currentClipInfos = _animator.GetCurrentAnimatorClipInfo(0);
            Debug.Assert((currentClipInfos.Length > 0), "������� �ִϸ��̼��� �Ҿ���Ƚ��ϴ�");
        }
        
        _currentLayerIndex++;
        if (_currentLayerIndex >= _maxLayer)
        {
            _currentLayerIndex = _currentLayerIndex % _maxLayer;
        }

        string nextNodeName = "None";

        if (_currentLayerIndex == 0)
        {
            _overrideController[_targetName1] = targetClip;
            nextNodeName = "State1";
        }
        else
        {
            _overrideController[_targetName2] = targetClip;
            nextNodeName = "State2";
        }

        _animator.Play(nextNodeName, _currentLayerIndex, 0.0f);

        //Start Coroutine
        if (_corutineStarted == false)
        {
            _corutineStarted = true;
            StartCoroutine("SwitchingBlendCoroutine");
        }
    }
}
