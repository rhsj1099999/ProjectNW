using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ItemInfo;

public struct EquipmentDesc
{
    public ItemInfo _equipmentInfo;
}

public class EquipmentCell : MonoBehaviour, IMoveItemStore
{
    [SerializeField] private EquipType _equipType = EquipType.None;
    [SerializeField] private GameObject _equipmentUIObjectPrefab;

    private GameObject _ownerEquipBoard = null;
    private GameObject _ownerCharacter = null;
    private GameObject _equippedItemUI = null;
    private ItemInfo _equipmentItemInfo;

    private void Awake()
    {
        Debug.Assert(_equipType != EquipType.None, "�������� None�� ������������ �ȵȴ�");
        Debug.Assert(_equipmentUIObjectPrefab != null, "equipment Prefab�� null�̿����� �ȵȴ�");

        _ownerCharacter = transform.root.gameObject;
    }

    public void DeleteOnMe(ItemStoreDesc storeDesc) // : IMoveItemStore
    {
        Debug.Assert(_equippedItemUI != null, "�������� �ʾҴµ� ������մϴ�??");
        Destroy(_equippedItemUI);
        _equippedItemUI = null;
    }

    public void EquipItem(ItemStoreDesc storedDesc)
    {
        if (_equippedItemUI != null) 
        {
            return;
            //�̹� �ش�ĭ�� �����ϰ� ������ �׳� ����
            /*---------------------------------------------------------------------------
            |TODO|  ���߿� ������ ���ұ�� ������ �̰��� �����ؾ��Ѵ�.
            ---------------------------------------------------------------------------*/
        }

        if (storedDesc._info._equipType != _equipType)
        {
            //���� ������ �ٸ����̴�
            return;
        }
        
        {
            storedDesc._owner.DeleteOnMe(storedDesc);//�����ϰ�

            //�����Ѵ�

            GameObject equipmentUIObject = Instantiate(_equipmentUIObjectPrefab, this.gameObject.transform);

            RectTransform equipmentUIRectTransform = equipmentUIObject.GetComponent<RectTransform>();

            RectTransform myRectTransform = GetComponent<RectTransform>();

            equipmentUIRectTransform.sizeDelta = new Vector2(myRectTransform.rect.width, myRectTransform.rect.height);

            equipmentUIRectTransform.position = myRectTransform.position;

            _equipmentItemInfo = storedDesc._info;

            ItemBase itemBaseComponent = equipmentUIObject.GetComponent<ItemBase>();

            storedDesc._owner = null;

            itemBaseComponent.Initialize(null, storedDesc);

            _equippedItemUI = equipmentUIObject;
        }


        //_ownerCharacter -> �� �ٲٱ�

        EquipItemMesh(storedDesc);
    }

    private void EquipItemMesh(ItemStoreDesc storeDesc)
    {
        //if (false/*���� �𵨰� ���� �� �����̴�*/) 
        //{

        //}
        //else
        {
            ////1. �� �����ܰ�
            //GameObject emptyGameObject = new GameObject("EquipmentModelDummy"); // "MyChildObject"�� ������ ������Ʈ�� �̸�
            //emptyGameObject.transform.SetParent(this.transform);
            //emptyGameObject.transform.localPosition = Vector3.zero;

            ////2. �ִϸ����� ���� �ܰ�
            //GameObject modelObject = Instantiate(_equipmentItemModelPrefab_MustDel, emptyGameObject.transform);
            //Animator modelAnimator = modelObject.GetComponent<Animator>();
            //if (modelAnimator == null)
            //{
            //    modelAnimator = modelObject.AddComponent<Animator>();
            //}
            //RuntimeAnimatorController ownerController = _AnimController.GetAnimator().runtimeAnimatorController;
            //RuntimeAnimatorController newController = Instantiate<RuntimeAnimatorController>(ownerController);
            //modelAnimator.runtimeAnimatorController = newController;
            //modelAnimator.avatar = _equipmentItemModelAvatar_MustDel;

            ////3. ��ε�ĳ���� ����ܰ�
            //AnimPropertyBroadCaster animpropertyBroadCaster = GetComponent<AnimPropertyBroadCaster>();
            //animpropertyBroadCaster.AddAnimator(modelObject);

            ////4. ������ ������Ʈ�� ���� �ܰ�
            //RiggingPublisher ownerRigPublisher = gameObject.GetComponent<RiggingPublisher>();
            //ownerRigPublisher.PublishRigging(modelObject, modelAnimator);

            ////5. Skinned Mesh Renderer ��Ȱ��ȭ �ܰ� (���� ��� �����ֱ� �����̴�)
            ////GameObject itemMeshObject = null;
        }
    }
}
