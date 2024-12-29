using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static ItemInfo;

public class EquipmentBoard : MonoBehaviour, IMoveItemStore
{
    //[SerializeField] private GameObject _itemUIPrefab = null;
    [SerializeField] private GameObject _equipmentUIObjectPrefab = null;


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

        EquipmentCell[] components = GetComponentsInChildren<EquipmentCell>();

        Debug.Assert(components.Length != 0, "����ĭ�� �����ϴ�");

        EquipmentCellDesc desc;
        desc._owner = this;

        foreach (EquipmentCell component in components) 
        {
            ItemInfo.EquipType cellType = component.GetCellType();

            if (cellType != ItemInfo.EquipType.Weapon && cellType != ItemInfo.EquipType.UseAndComsumeableByCharacter)
            {
                Debug.Assert(_equipCellUIs.ContainsKey(component.GetCellType()) == false, "���� ����Ÿ���� �ߺ��˴ϴ�.");

                _equipCellUIs.Add(component.GetCellType(), component.gameObject);
            }

            component.Initialize(desc);
        }
    }


    public void EquipItem_Weapon(ItemStoreDesc storeDesc, GameObject callerEquipCell)
    {

    }


    public void UnEquipItem_Weapon(ItemStoreDesc storeDesc, GameObject callerEquipCell)
    {

    }





    public bool EquipItem(ItemStoreDesc storeDesc, GameObject callerEquipCell)
    {
        if (storeDesc._info._equipType == EquipType.Weapon)
        {
            return true;
        }

        List<GameObject> cells = GetCells(storeDesc);

        foreach (var cell in cells)
        {
            EquipType needType = cell.GetComponent<EquipmentCell>().GetCellType();

            GameObject currEquippedObject = null;
            _currEquippedItemUIs.TryGetValue(needType, out currEquippedObject);
            if (currEquippedObject != null)
            {
                return false;
            }
        }



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

            _currEquippedItemUIs.Add(cell.GetComponent<EquipmentCell>().GetCellType(), equipmentUIObject);

        }

        //�޽� ����
        {
            EquipItemMesh(storeDesc);
        }

        return true;
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
        Debug.Assert(_currEquippedMesh.ContainsKey(storeDesc._info._equipType) != false, "�������� �ʾҴµ� ���������մϴ�??");

        if (IsSameSkelaton(storeDesc._info) == true)
        {
            GameObject mesh = _currEquippedMesh[storeDesc._info._equipType];
            _currEquippedMesh.Remove(storeDesc._info._equipType);
            Destroy(mesh);
        }
        else
        {
            UIComponent myUIComponent = GetComponentInParent<UIComponent>();
            GameObject uiReturnOwner = myUIComponent.GetReturnObject();
            CharacterAnimatorScript ownerCharacterAnimatorScript = uiReturnOwner.GetComponentInChildren<CharacterAnimatorScript>();
            _currEquippedMesh.Remove(storeDesc._info._equipType);
            ownerCharacterAnimatorScript.ResetCharacterModel();
        }
    }


    private void EquipItemMesh(ItemStoreDesc storeDesc)
    {
        /*--------------------------------------------------------------
        |NOTI| �������� �и��͸��ǰ� �ٸ��� ������ ���Ű���(��Ų) ����Դϴ�.
        --------------------------------------------------------------*/
        UIComponent myUIComponent = GetComponentInParent<UIComponent>();

        GameObject uiReturnOwner = myUIComponent.GetReturnObject();

        CharacterAnimatorScript ownerCharacterAnimatorScript = uiReturnOwner.GetComponentInChildren<CharacterAnimatorScript>();

        GameObject originalAnimatorGameObject = ownerCharacterAnimatorScript.GetCurrActivatedModelObject();

        if (IsSameSkelaton(storeDesc._info) == true)
        {
            List<GameObject> equipMeshes = ItemInfoManager.Instance.GetMeshes(storeDesc._info);

            foreach (var item in equipMeshes) //���� �ϳ���
            {
                GameObject equippedMesh = Instantiate(item, originalAnimatorGameObject.transform);
                SkinnedMeshRenderer skinnedMeshRenderer = equippedMesh.GetComponent<SkinnedMeshRenderer>();
                skinnedMeshRenderer.bones = originalAnimatorGameObject.GetComponentInChildren<SkinnedMeshRenderer>().bones;

                /*-------------------------------------------------------------
                |TODO| �ִ��� �ø��� �Ǹ� �ȵǴµ�, �� �Ǿ��ϴ��� �˾Ƴ����Ѵ�.
                -------------------------------------------------------------*/
                skinnedMeshRenderer.updateWhenOffscreen = true;
                _currEquippedMesh.Add(storeDesc._info._equipType, equippedMesh);
            }
        }
        else
        {
            GameObject newModel = ownerCharacterAnimatorScript.ModelChange(ItemInfoManager.Instance.GetEquipmentPrefab(storeDesc._info._meshObjectName));
            _currEquippedMesh.Add(storeDesc._info._equipType, newModel);
        }
    }

    private bool IsSameSkelaton(ItemInfo itemInfo)
    {
        UIComponent myUIComponent = GetComponentInParent<UIComponent>();

        if (myUIComponent == null)
        {
            Debug.Assert(false, "��� UI�� �θ� UIComponent 1�� ���� �������־���մϴ�");
            Debug.Break();
        }

        CharacterAnimatorScript ownerCharacterAnimatorScript = myUIComponent.GetReturnObject().GetComponentInChildren<CharacterAnimatorScript>();

        if (ownerCharacterAnimatorScript == null) 
        {
            Debug.Assert(false, "�ٸ� �� �������� Ȯ���ϱ� ���ؼ� CharacterAnimatorScript�� �ʿ��մϴ�");
            Debug.Break();
        }

        GameObject prefabObject = ItemInfoManager.Instance.GetEquipmentPrefab(itemInfo._meshObjectName);
        Animator prefabAnimator = prefabObject.GetComponent<Animator>();
        Debug.Assert(prefabAnimator != null, "�������� ���Prefab�� �ݵ�� Animator�� ������ �־�� �մϴ�");

        Avatar prefabAvatar = prefabAnimator.avatar;

        return ownerCharacterAnimatorScript.IsSameSkeleton(prefabAvatar);
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
