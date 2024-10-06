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
    계속 바뀔 변수들
    -------------*/
    private Dictionary<ItemInfo.EquipType, GameObject> _equipCellUIs = new Dictionary<ItemInfo.EquipType, GameObject>();
    private Dictionary<ItemInfo.EquipType, GameObject> _currEquippedMesh = new Dictionary<ItemInfo.EquipType, GameObject>();
    private Dictionary<ItemInfo.EquipType, GameObject> _currEquippedItemUIs = new Dictionary<ItemInfo.EquipType, GameObject>();

    public void DeleteOnMe(ItemStoreDesc storeDesc)
    {
        Debug.Assert(_currEquippedItemUIs.ContainsKey(storeDesc._info._equipType) != false, "장착하지 않았는데 해제하려합니다??");
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
        Debug.Assert(_equipmentUIObjectPrefab != null, "프리팹 할당 안함");

        _myRectTransform = transform as RectTransform;

        _ownerCharacter = transform.root.gameObject;

        EquipmentCell[] components = GetComponentsInChildren<EquipmentCell>();

        Debug.Assert(components.Length != 0, "장착칸이 없습니다");

        EquipmentCellDesc desc;
        desc._owner = this;

        foreach (EquipmentCell component in components) 
        {
            Debug.Assert(_equipCellUIs.ContainsKey(component.GetCellType()) == false, "셀의 장착타입이 중복됩니다.");

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
            //이미 해당칸에 장착하고 있으면 그냥 종료
            /*---------------------------------------------------------------------------
            |TODO|  나중에 아이템 스왑기능 구현시 이곳을 수정해야한다.
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


        //메쉬 장착
        {
            EquipItemMesh(storeDesc);
        }
    }



    private void UnEquipMesh(ItemStoreDesc storeDesc)
    {
        Debug.Assert(_currEquippedMesh.ContainsKey(storeDesc._info._equipType) != false, "장착하지 않았는데 장착해제합니다??");
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
                //만약 원본 모델이랑 동일한 뼈 구조라면, 가장 먼저 만나는 애니메이터랑 동일한 상태임
                GameObject equippedMesh = Instantiate(item, originalAnimatorGameObject.transform);
                SkinnedMeshRenderer skinnedMeshRenderer = equippedMesh.GetComponent<SkinnedMeshRenderer>();
                skinnedMeshRenderer.bones = _ownerCharacter.GetComponentInChildren<SkinnedMeshRenderer>().bones;
                _currEquippedMesh.Add(storeDesc._info._equipType, equippedMesh);
            }
        }
        else
        {
            ////1. 모델 생성단계
            //GameObject emptyGameObject = new GameObject("EquipmentModelDummy"); // "MyChildObject"는 생성된 오브젝트의 이름
            //emptyGameObject.transform.SetParent(this.transform);
            //emptyGameObject.transform.localPosition = Vector3.zero;

            ////2. 애니메이터 세팅 단계
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

            ////3. 브로드캐스터 연결단계
            //AnimPropertyBroadCaster animpropertyBroadCaster = GetComponent<AnimPropertyBroadCaster>();
            //animpropertyBroadCaster.AddAnimator(modelObject);

            ////4. 생성된 오브젝트의 리깅 단계
            //RiggingPublisher ownerRigPublisher = gameObject.GetComponent<RiggingPublisher>();
            //ownerRigPublisher.PublishRigging(modelObject, modelAnimator);

            ////5. Skinned Mesh Renderer 비활성화 단계 (입은 장비만 보여주기 위함이다)
            ////GameObject itemMeshObject = null;
        }
    }
}
