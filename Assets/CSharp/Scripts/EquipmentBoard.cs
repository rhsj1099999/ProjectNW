using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        UnEquipUI(storeDesc);
        UnEquipMesh(storeDesc);
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

            List<GameObject> cells = GetCells(storeDesc);

            foreach (var cell in cells)
            {
                GameObject equipmentUIObject = Instantiate(_equipmentUIObjectPrefab, callerEquipCell.transform);

                RectTransform equipmentUIRectTransform = equipmentUIObject.GetComponent<RectTransform>();
                RectTransform cellRectTransform = cell.GetComponent<RectTransform>();
                equipmentUIRectTransform.sizeDelta = new Vector2(cellRectTransform.rect.width, cellRectTransform.rect.height);
                equipmentUIRectTransform.position = cellRectTransform.position;

                ItemBase itemBaseComponent = equipmentUIObject.GetComponent<ItemBase>();

                storeDesc._owner = this;

                itemBaseComponent.Initialize(this, storeDesc);

                EquipmentCell cellComponent = cell.GetComponent<EquipmentCell>();

                _currEquippedItemUIs.Add(cellComponent.GetCellType(), equipmentUIObject);
            }
        }


        //�޽� ����
        {
            EquipItemMesh(storeDesc);
        }
    }

    private void UnEquipUI(ItemStoreDesc storeDesc)
    {
        var keys = _currEquippedItemUIs.Keys.ToList();
        foreach (var key in keys)
        {
            if ((key & storeDesc._info._equipType) == ItemInfo.EquipType.None)
            {
                continue;
            }
            Destroy(_currEquippedItemUIs[key]);
            _currEquippedItemUIs.Remove(key);
        }
    }

    private void UnEquipMesh(ItemStoreDesc storeDesc)
    {
        if (IsSameSkelaton(storeDesc._info) == true)
        {
            Debug.Assert(_currEquippedMesh.ContainsKey(storeDesc._info._equipType) != false, "�������� �ʾҴµ� ���������մϴ�??");
            GameObject mesh = _currEquippedMesh[storeDesc._info._equipType];
            Destroy(mesh);
            _currEquippedMesh.Remove(storeDesc._info._equipType);
        }
        else
        {
            Debug.Assert(_currEquippedMesh.ContainsKey(storeDesc._info._equipType) != false, "�������� �ʾҴµ� ���������մϴ�??");

            GameObject mesh = _currEquippedMesh[storeDesc._info._equipType];

            //�Ķ���� ��ε� ĳ���� ��ũ �����ܰ�
            AnimPropertyBroadCaster broadCaster = _ownerCharacter.GetComponentInChildren<AnimPropertyBroadCaster>();
            Debug.Assert(broadCaster != null, "broadCaster null�̿����� �ȵȴ�");
            broadCaster.RemoveAnimator(mesh);


            //���� ĳ���� �޽� TurnOn�ܰ�
            GameObject hasOriginalAnimatorObject = _ownerCharacter.GetComponentInChildren<Animator>().gameObject;
            if (storeDesc._info._equipType == ItemInfo.EquipType.All)
            {
                SkinnedMeshRenderer[] originalModelSkinnedRenderers = hasOriginalAnimatorObject.GetComponentsInChildren<SkinnedMeshRenderer>();

                foreach (var skinnedMeshRenderer in originalModelSkinnedRenderers)
                {
                    skinnedMeshRenderer.enabled = true;
                }
            }
            else { /*�ٸ��������ε� ���Ű����� �ƴϸ� �����غ����Ұ�*/}

            Destroy(mesh.transform.parent.gameObject);
            _currEquippedMesh.Remove(storeDesc._info._equipType);
        }
    }


    private void EquipItemMesh(ItemStoreDesc storeDesc)
    {
        if (IsSameSkelaton(storeDesc._info) == true)
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
            //1. �� �����ܰ�
            //
            GameObject emptyGameObject = new GameObject("EquipmentModelDummy"); // "MyChildObject"�� ������ ������Ʈ�� �̸�
            emptyGameObject.transform.SetParent(_ownerCharacter.transform);
            emptyGameObject.transform.localPosition = Vector3.zero;
            //

            //2. �ִϸ����� ���� �ܰ�
            //
            GameObject modelObjectPrefab = ItemInfoManager.Instance.GetEquipmentPrefab(storeDesc._info._meshObjectName);
            GameObject modelObject = Instantiate(modelObjectPrefab, emptyGameObject.transform);
            Animator modelAnimator = modelObject.GetComponent<Animator>();
            if (modelAnimator == null)
            {
                modelAnimator = modelObject.AddComponent<Animator>();
            }

            GameObject hasOriginalAnimatorObject = _ownerCharacter.GetComponentInChildren<Animator>().gameObject;
            Animator originalAnimatorComponent = hasOriginalAnimatorObject.GetComponent<Animator>();

            RuntimeAnimatorController ownerController = originalAnimatorComponent.runtimeAnimatorController;
            RuntimeAnimatorController newController = Instantiate<RuntimeAnimatorController>(ownerController);
            modelAnimator.runtimeAnimatorController = newController;

            Animator prefabAnimator = modelObjectPrefab.GetComponent<Animator>();
            Debug.Assert(prefabAnimator != null, "�������� ���Prefab�� �ݵ�� Animator�� ������ �־�� �մϴ�");
            modelAnimator.avatar = prefabAnimator.avatar;
            //

            ////3. ��ε�ĳ���� ����ܰ�
            //
            AnimPropertyBroadCaster animpropertyBroadCaster = _ownerCharacter.GetComponentInChildren<AnimPropertyBroadCaster>();
            animpropertyBroadCaster.AddAnimator(modelObject);
            //

            ////4. ������ ������Ʈ�� ���� �ܰ�
            //
            RiggingPublisher ownerRigPublisher = _ownerCharacter.GetComponentInChildren<RiggingPublisher>();
            ownerRigPublisher.PublishRigging(modelObject, modelAnimator);
            //


            //5. ������ ����� Skinned Mesh Renderer ��Ȱ��ȭ �ܰ� (���� ��� �����ֱ� �����̴�)
            //
            if (storeDesc._info._equipType == ItemInfo.EquipType.All)
            {
                //pass ���δ� �����ش�
            }
            else {}
            /*---------------------------------------------------------------------------
            |TODO|  All Equipment ���� �ٸ������� ��� �������� �����غ���.
            ---------------------------------------------------------------------------*/
            //


            //6. ���� ����  Skinned Mesh Renderer ��Ȱ��ȭ �ܰ� (���� ��� �����ֱ� �����̴�)
            //
            if (storeDesc._info._equipType == ItemInfo.EquipType.All)
            {
                SkinnedMeshRenderer[] originalModelSkinnedRenderers = hasOriginalAnimatorObject.GetComponentsInChildren<SkinnedMeshRenderer>();

                foreach (var skinnedMeshRenderer in originalModelSkinnedRenderers)
                {
                    skinnedMeshRenderer.enabled = false;
                }
            }
            else { }
            /*---------------------------------------------------------------------------
            |TODO|  All Equipment ���� �ٸ������� ��� �������� �����غ���.
            ---------------------------------------------------------------------------*/
            //


            //7. ������ ����� IK �ܰ�
            //
            //


            //8. ������ ��� ���� �ٿ��ֱ� �ܰ�
            //
            //

            _currEquippedMesh.Add(storeDesc._info._equipType, modelObject);
        }
    }

    private bool IsSameSkelaton(ItemInfo itemInfo)
    {
        GameObject hasOriginalAnimatorObject = _ownerCharacter.GetComponentInChildren<Animator>().gameObject;
        Animator originalAnimatorComponent = hasOriginalAnimatorObject.GetComponent<Animator>();
        Avatar originalAvatar = originalAnimatorComponent.avatar;

        GameObject prefabObject = ItemInfoManager.Instance.GetEquipmentPrefab(itemInfo._meshObjectName);
        Animator prefabAnimator = prefabObject.GetComponent<Animator>();
        Debug.Assert(prefabAnimator != null, "�������� ���Prefab�� �ݵ�� Animator�� ������ �־�� �մϴ�");

        Avatar prefabAvatar = prefabAnimator.avatar;

        return prefabAvatar == originalAvatar; //���� �ƹ�Ÿ�� �ٸ��ٸ� ���� ���� �������� �ƴϴ�.
    }

    private List<GameObject> GetCells(ItemStoreDesc storeDesc)
    {
        List<GameObject> retCells = new List<GameObject>();

        foreach (KeyValuePair<ItemInfo.EquipType, GameObject> cell in _equipCellUIs)
        {
            if ((cell.Key & storeDesc._info._equipType) != ItemInfo.EquipType.None)
            {
                retCells.Add(cell.Value);
            }
        }

        return retCells;
    }
}
