using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using Unity.VisualScripting;

public class ItemUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IBeginDragHandler
{
    /*-------------------------------------------------------
    �������� �κ��丮, ���â �� �����ɶ� �̰��� ����� �����Ѵ�.
    �� UI ��ü�� �����Ѵ�.
    �ʿ� �ѷ��� Item�� �̰��� �ƴѵ� �̰Ŷ� ������ �����Ŵ�
    -------------------------------------------------------*/
    private RectTransform _myRectTransform = null;
    private ItemStoreDescBase _itemStoreDesc;

    /*-----------------
    ��� ���ึ�� �ٲ� ������
     ----------------*/
    private Vector2 _myOriginalPosition = Vector2.zero;
    private Quaternion _myOriginalRotation = Quaternion.identity;
    private Vector2 _myOriginalSize = Vector2.zero;
    private Transform _beforeDragTransform = null;

    private bool _additionalRotating_Dynamic = false;
    private bool _isDragging = false;
    private bool _isInitted = false;


    public virtual void OverlapItemWork(ItemUI itemUI) {}

    public void Initialize(ItemStoreDescBase storeDesc)
    {
        {
            //�̹��� ������Ʈ ����

            //Debug.Assert(false, "�̰��� �����ؾ��մϴ�");
            //Debug.Break();
            //if (info._sprite != null)
            //{
            //    itemUI.GetComponent<Image>().sprite = info._sprite;
            //}
        }
        _myRectTransform = GetComponent<RectTransform>();
        _itemStoreDesc = storeDesc;
        _isInitted = true;
    }

    private void Start()
    {
        if (_isInitted == false)
        {
            Debug.Assert(false, "Init�� ȣ����� �ʾҽ��ϴ�");
            Debug.Break();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) == true && _isDragging == true) 
        {
            RotateInGrab();
        }
    }

    public IEnumerator DestroyCoroutine()
    {
        transform.localScale = Vector3.zero;

        yield return new WaitForNextFrameUnit();
        yield return new WaitForNextFrameUnit();

        if (EventSystem.current.currentSelectedGameObject == gameObject)
        {
            Debug.Log("�̷��� �ȵȴ�");
            Debug.Break();
            EventSystem.current.SetSelectedGameObject(null);
        }

        Destroy(gameObject);
    }


    public void OnPointerDown(PointerEventData eventData) {}

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_isDragging == false)
        {
            //Ŭ�������� �巡���� ���ߴ�;
            return;
        }

        //�巡�� ����
        {
            _isDragging = false;
            _myRectTransform.rotation = _myOriginalRotation;
            _myRectTransform.position = _myOriginalPosition;
            _myRectTransform.sizeDelta = _myOriginalSize;
            transform.SetParent(_beforeDragTransform);
        }


        List<RaycastResult> uiRayCastResult = new List<RaycastResult>();

        UIManager.Instance.RayCastAll(ref uiRayCastResult);

        if (uiRayCastResult.Count <= 0)
        {
            if (_itemStoreDesc._itemAsset._FieldExistAble == true)
            {
                //������ ����
                UIComponent myUIComponent = _itemStoreDesc._owner.GetComponentInParent<UIComponent>();
                GameObject ownerCharacter = myUIComponent.GetUIControllingComponent().gameObject;

                GameObject dropItemGameObject = new GameObject(_itemStoreDesc._itemAsset._ItemName);
                dropItemGameObject.transform.position = Vector3.zero;
                dropItemGameObject.transform.rotation = Quaternion.identity;

                GameObject dropItemModel = Instantiate(_itemStoreDesc._itemAsset._ItemModel, dropItemGameObject.transform);
                dropItemModel.transform.localPosition = Vector3.zero;
                dropItemModel.transform.localRotation = Quaternion.identity;

                Bounds itemBounds = new Bounds();
                GetActivatedRenderers(dropItemModel, ref itemBounds, ownerCharacter);

                Rigidbody addRigidBody = dropItemGameObject.AddComponent<Rigidbody>();
                {
                    addRigidBody.drag = 0.5f;
                    addRigidBody.angularDrag = 0.5f;
                    addRigidBody.interpolation = RigidbodyInterpolation.Interpolate;
                    addRigidBody.collisionDetectionMode = CollisionDetectionMode.Continuous;
                    addRigidBody.includeLayers = 0;
                    addRigidBody.excludeLayers = ~ (LayerMask.GetMask("StaticNavMeshLayer") | LayerMask.GetMask("Player"));
                }

                CapsuleCollider addCapsuleCollider = dropItemGameObject.AddComponent<CapsuleCollider>();
                {
                    Vector3 lengths = new Vector3(itemBounds.size.x, itemBounds.size.y, itemBounds.size.z);
                    int heightIndex = 0;
                    float maxVal = 0.0f;
                    for (int i = 0; i < 3; i++)
                    {
                        if (maxVal <= lengths[i])
                        {
                            maxVal = lengths[i];
                            heightIndex = i;
                        }
                    }

                    addCapsuleCollider.direction = heightIndex;
                    addCapsuleCollider.includeLayers = 0;
                    addCapsuleCollider.excludeLayers = ~LayerMask.GetMask("StaticNavMeshLayer");
                    addCapsuleCollider.center = itemBounds.center;
                    addCapsuleCollider.height = lengths[heightIndex];
                    lengths[heightIndex] = 0.0f;
                    addCapsuleCollider.radius = lengths.magnitude / 2.0f;
                }



                GameObject dropItemInteraction = new GameObject("Interaction");
                dropItemInteraction.SetActive(false);
                dropItemInteraction.layer = LayerMask.NameToLayer("InteractionableCollider");
                dropItemInteraction.transform.SetParent(dropItemGameObject.transform);
                dropItemInteraction.transform.position = Vector3.zero;
                dropItemInteraction.transform.rotation = Quaternion.identity;

                CapsuleCollider interactionCollider = dropItemInteraction.AddComponent<CapsuleCollider>();
                {
                    interactionCollider.direction = addCapsuleCollider.direction;
                    interactionCollider.includeLayers = addCapsuleCollider.includeLayers;
                    interactionCollider.excludeLayers = ~LayerMask.GetMask("Player");
                    interactionCollider.center = addCapsuleCollider.center;
                    interactionCollider.height = addCapsuleCollider.height;
                    interactionCollider.radius = addCapsuleCollider.radius;
                    interactionCollider.isTrigger = true;
                }

                UICall_AcquireItem interactionUIComponent = dropItemInteraction.AddComponent<UICall_AcquireItem>();
                UICall_AcquireItem.UICall_AcquireItemDesc newDesc = new UICall_AcquireItem.UICall_AcquireItemDesc();
                newDesc._itemStoreDesc = _itemStoreDesc;
                newDesc._itemTarget = dropItemGameObject;
                newDesc._offCollider = interactionCollider;
                interactionUIComponent.Init(newDesc);
                dropItemInteraction.SetActive(true);



                dropItemGameObject.transform.position = ownerCharacter.transform.position + Vector3.up * 1.5f;
                dropItemGameObject.transform.rotation = ownerCharacter.transform.rotation;

                {
                    float itemThrowForce = 1.0f;

                    addRigidBody.position = ownerCharacter.transform.position + Vector3.up * 1.5f;
                    Vector3 initialForceVector = Quaternion.AngleAxis(-30.0f, ownerCharacter.transform.right) * ownerCharacter.transform.forward * itemThrowForce;
                    addRigidBody.AddForce(initialForceVector, ForceMode.Impulse);
                }
            }

            _itemStoreDesc._owner.DeleteOnMe(_itemStoreDesc);
            StartCoroutine(DestroyCoroutine());

            return;
        }

        GameObject topObject = uiRayCastResult.First().gameObject;

        if (topObject == gameObject) 
        {
            if (uiRayCastResult.Count <= 1)
            {
                return;
            }

            topObject = uiRayCastResult[1].gameObject;
        }

        if (topObject == null)
        {
            return;
        }


        //�ֻ���� ItemUI�̴�.
        ItemUI itemBaseComponent = topObject.GetComponent<ItemUI>();
        if (itemBaseComponent != null)
        {
            bool isDelete = false;
            itemBaseComponent._itemStoreDesc.OverlapItem(_itemStoreDesc, ref isDelete);
            if (isDelete == true) 
            {
                _itemStoreDesc._owner.DeleteOnMe(_itemStoreDesc);
                StartCoroutine(DestroyCoroutine());
            }
            return;
        }


        //�ֻ���� BoardUICellBase �̴�
        BoardUICellBase cellComponent = topObject.GetComponent<BoardUICellBase>();
        if (cellComponent != null)
        {
            int startX = -1;
            int startY = -1;

            if (cellComponent.TryMoveItemDropOnCell(_itemStoreDesc, ref startX, ref startY, _additionalRotating_Dynamic) == false)
            {
                return;
            }

            /*--------------------------------------------------------------------------------------------------------
            |NOTI| ���ο� ItemStoreDescBase �ʱ�ȭ ������尡 ������ ������ �����ϰ� �ִ� ������ ���մϴ�.
            ���� owner �� Equip�ϼ����ְ�, Inventory �ϼ��� �־� ���ϼ��� ���ؼ��Դϴ�.
            --------------------------------------------------------------------------------------------------------*/


            //�����Ҷ� _owner = ������ �־��� ���� �����̳�
            _itemStoreDesc._owner.DeleteOnMe(_itemStoreDesc);
            _itemStoreDesc._isRotated = _additionalRotating_Dynamic;
            cellComponent.GetOwner().AddItemUsingForcedIndex(_itemStoreDesc, startX, startY, cellComponent);
            StartCoroutine(DestroyCoroutine());

            return;
        }

        return;
    }

    public void OnDrag(PointerEventData eventData)
    {
        _myRectTransform.anchoredPosition += eventData.delta;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //�巡�� �غ�
        {
            //�巡���� Ȱ��ȭ
            _isDragging = true;

            //�׷� �η��̼� �ʱ�ȭ
            _additionalRotating_Dynamic = _itemStoreDesc._isRotated;

            //���콺�� ������ ���� ���ư��� ����
            _myOriginalPosition = _myRectTransform.position;

            _myOriginalRotation = _myRectTransform.rotation;

            //���� ���콺�� ���δ�
            _myRectTransform.position = Input.mousePosition;

            //���콺�� ������ ���ư� ũ�� ����
            _myOriginalSize = _myRectTransform.sizeDelta;

            //���� ũ��� �������´�
            _myRectTransform.sizeDelta = new Vector2(_itemStoreDesc._itemAsset._SizeX * 20, _itemStoreDesc._itemAsset._SizeY * 20);

            _beforeDragTransform = transform.parent;

            //���� ���� �������� �׷���
            UIManager.Instance.SetMeFinalZOrder(gameObject);
        }

        int sizeX = (_itemStoreDesc._isRotated == false) ? _itemStoreDesc._itemAsset._SizeX * 20: _itemStoreDesc._itemAsset._SizeY * 20;
        int sizeY = (_itemStoreDesc._isRotated == false) ? _itemStoreDesc._itemAsset._SizeY * 20 : _itemStoreDesc._itemAsset._SizeX * 20;

        _myRectTransform.position = Input.mousePosition;
        _myRectTransform.position += new Vector3(sizeX / 2, -sizeY / 2, 0);
        _myRectTransform.position += new Vector3(-10, 10, 0); //��������
    }

    private void RotateInGrab()
    {
        _additionalRotating_Dynamic = !_additionalRotating_Dynamic;

        _myRectTransform.position = Input.mousePosition;

        int sizeX = (_additionalRotating_Dynamic == false) ? _itemStoreDesc._itemAsset._SizeX * 20 : _itemStoreDesc._itemAsset._SizeY * 20;
        int sizeY = (_additionalRotating_Dynamic == false) ? _itemStoreDesc._itemAsset._SizeY * 20 : _itemStoreDesc._itemAsset._SizeX * 20;

        _myRectTransform.position = Input.mousePosition;
        _myRectTransform.position += new Vector3(sizeX / 2, -sizeY / 2, 0);
        _myRectTransform.position += new Vector3(-10, 10, 0); //��������


        Vector3 axis = new Vector3(0.0f, 0.0f, 1.0f);
        float angle = (_additionalRotating_Dynamic == true) ? 90.0f : -90.0f;

        _myRectTransform.RotateAround(_myRectTransform.position, axis, angle);
    }


    private void GetActivatedRenderers(GameObject targetObject, ref Bounds ret, GameObject fromOwner)
    {
        Renderer[] renderers = targetObject.GetComponentsInChildren<Renderer>();


        if (renderers.Length <= 0)
        {
            //�⺻ ������ ����
            return;
        }

        Bounds firstBound = renderers[0].bounds;
        ret = firstBound;

        foreach (var renderer in renderers)
        {
            if (renderer.enabled == false)
            {
                continue;
            }

            Bounds diffBound = renderer.bounds;
            ret.Encapsulate(diffBound);
        }
    }

}
