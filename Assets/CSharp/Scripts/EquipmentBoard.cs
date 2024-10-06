using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentBoard : MonoBehaviour, IMoveItemStore
{
    [SerializeField] private GameObject _itemUIPrefab = null;
    [SerializeField] private GameObject _equipmentUIObjectPrefab = null;
    private GameObject _ownerCharacter = null;


    private RectTransform _myRectTransform = null;
    /*-------------
    ��� �ٲ� ������
    -------------*/
    private Dictionary<ItemInfo.EquipType, GameObject> _equipCellUIs = new Dictionary<ItemInfo.EquipType, GameObject>();
    private Dictionary<ItemInfo.EquipType, GameObject> _currEquippedMesh = new Dictionary<ItemInfo.EquipType, GameObject>();
    private Dictionary<ItemInfo.EquipType, GameObject> _currEquippedItemUIs = new Dictionary<ItemInfo.EquipType, GameObject>();

    public void DeleteOnMe(ItemStoreDesc storeDesc)
    {
        Debug.Assert(_currEquippedItemUIs.ContainsKey(storeDesc._info._equipType) != false, "�������� �ʾҴµ� �����Ϸ��մϴ�??");
        GameObject itemUI = _currEquippedItemUIs[storeDesc._info._equipType];
        Destroy(itemUI);
        _currEquippedItemUIs.Remove(storeDesc._info._equipType);
        UnEquipMesh(storeDesc);
    }


    private bool IsSameSkelaton(string meshGameObjectName)
    {
        return true;
    }

    private void Awake()
    {
        Debug.Assert(_equipmentUIObjectPrefab != null, "������ �Ҵ� ����");

        _myRectTransform = transform as RectTransform;

        _ownerCharacter = transform.root.gameObject;

        EquipmentCell[] components = GetComponentsInChildren<EquipmentCell>();

        Debug.Assert(components.Length != 0, "����ĭ�� �����ϴ�");

        EquipmentCellDesc desc;
        desc._owner = this;

        foreach (EquipmentCell component in components) 
        {
            Debug.Assert(_equipCellUIs.ContainsKey(component.GetCellType()) == false, "���� ����Ÿ���� �ߺ��˴ϴ�.");

            _equipCellUIs.Add(component.GetCellType(), component.gameObject);

            component.Initialize(desc);
        }

        _ownerCharacter = transform.root.gameObject;

    }

    public void EquipItem(ItemStoreDesc storeDesc, GameObject callerEquipCell)
    {

        if (_currEquippedItemUIs.ContainsKey(storeDesc._info._equipType) == true)
        {
            return;
            //�̹� �ش�ĭ�� �����ϰ� ������ �׳� ����
            /*---------------------------------------------------------------------------
            |TODO|  ���߿� ������ ���ұ�� ������ �̰��� �����ؾ��Ѵ�.
            ---------------------------------------------------------------------------*/
        }


        {
            storeDesc._owner.DeleteOnMe(storeDesc);

            GameObject equipmentUIObject = Instantiate(_equipmentUIObjectPrefab, callerEquipCell.transform);

            RectTransform equipmentUIRectTransform = equipmentUIObject.GetComponent<RectTransform>();
            RectTransform cellRectTransform = callerEquipCell.GetComponent<RectTransform>();
            equipmentUIRectTransform.sizeDelta = new Vector2(cellRectTransform.rect.width, cellRectTransform.rect.height);
            equipmentUIRectTransform.position = cellRectTransform.position;

            ItemBase itemBaseComponent = equipmentUIObject.GetComponent<ItemBase>();

            storeDesc._owner = this;

            itemBaseComponent.Initialize(this, storeDesc);

            _currEquippedItemUIs.Add(storeDesc._info._equipType, equipmentUIObject);
        }


        //�޽� ����
        {
            EquipItemMesh(storeDesc);
        }
    }



    private void UnEquipMesh(ItemStoreDesc storeDesc)
    {
        Debug.Assert(_currEquippedMesh.ContainsKey(storeDesc._info._equipType) != false, "�������� �ʾҴµ� ���������մϴ�??");
        GameObject mesh = _currEquippedMesh[storeDesc._info._equipType];
        Destroy(mesh);
        _currEquippedMesh.Remove(storeDesc._info._equipType);
    }


    private void EquipItemMesh(ItemStoreDesc storeDesc)
    {
        if (IsSameSkelaton(storeDesc._info._meshObjectName) == true)
        {
            List<GameObject> equipMeshes = ItemInfoManager.Instance.GetMeshes(storeDesc._info);

            GameObject originalAnimatorGameObject = _ownerCharacter.GetComponentInChildren<Animator>().gameObject;

            foreach (var item in equipMeshes)
            {
                //���� ���� ���̶� ������ �� �������, ���� ���� ������ �ִϸ����Ͷ� ������ ������
                GameObject equippedMesh = Instantiate(item, originalAnimatorGameObject.transform);
                SkinnedMeshRenderer skinnedMeshRenderer = equippedMesh.GetComponent<SkinnedMeshRenderer>();
                skinnedMeshRenderer.bones = _ownerCharacter.GetComponentInChildren<SkinnedMeshRenderer>().bones;
                _currEquippedMesh.Add(storeDesc._info._equipType, equippedMesh);
            }
        }
        else
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
