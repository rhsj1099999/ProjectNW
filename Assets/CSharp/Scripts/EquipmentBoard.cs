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
    계속 바뀔 변수들
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


        //메쉬 장착
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
            Debug.Assert(_currEquippedMesh.ContainsKey(storeDesc._info._equipType) != false, "장착하지 않았는데 장착해제합니다??");
            GameObject mesh = _currEquippedMesh[storeDesc._info._equipType];
            Destroy(mesh);
            _currEquippedMesh.Remove(storeDesc._info._equipType);
        }
        else
        {
            Debug.Assert(_currEquippedMesh.ContainsKey(storeDesc._info._equipType) != false, "장착하지 않았는데 장착해제합니다??");

            GameObject mesh = _currEquippedMesh[storeDesc._info._equipType];

            //파라메터 브로드 캐스터 링크 해제단계
            AnimPropertyBroadCaster broadCaster = _ownerCharacter.GetComponentInChildren<AnimPropertyBroadCaster>();
            Debug.Assert(broadCaster != null, "broadCaster null이여서는 안된다");
            broadCaster.RemoveAnimator(mesh);


            //원본 캐릭터 메쉬 TurnOn단계
            GameObject hasOriginalAnimatorObject = _ownerCharacter.GetComponentInChildren<Animator>().gameObject;
            if (storeDesc._info._equipType == ItemInfo.EquipType.All)
            {
                SkinnedMeshRenderer[] originalModelSkinnedRenderers = hasOriginalAnimatorObject.GetComponentsInChildren<SkinnedMeshRenderer>();

                foreach (var skinnedMeshRenderer in originalModelSkinnedRenderers)
                {
                    skinnedMeshRenderer.enabled = true;
                }
            }
            else { /*다른뼈구조인데 전신갑빠가 아니면 생각해봐야할것*/}

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
                //만약 원본 모델이랑 동일한 뼈 구조라면, 가장 먼저 만나는 애니메이터랑 동일한 상태임
                GameObject equippedMesh = Instantiate(item, originalAnimatorGameObject.transform);
                SkinnedMeshRenderer skinnedMeshRenderer = equippedMesh.GetComponent<SkinnedMeshRenderer>();
                skinnedMeshRenderer.bones = _ownerCharacter.GetComponentInChildren<SkinnedMeshRenderer>().bones;
                _currEquippedMesh.Add(storeDesc._info._equipType, equippedMesh);
            }
        }
        else
        {
            //1. 모델 생성단계
            //
            GameObject emptyGameObject = new GameObject("EquipmentModelDummy"); // "MyChildObject"는 생성된 오브젝트의 이름
            emptyGameObject.transform.SetParent(_ownerCharacter.transform);
            emptyGameObject.transform.localPosition = Vector3.zero;
            //

            //2. 애니메이터 세팅 단계
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
            Debug.Assert(prefabAnimator != null, "입으려는 장비Prefab은 반드시 Animator를 가지고 있어야 합니다");
            modelAnimator.avatar = prefabAnimator.avatar;
            //

            ////3. 브로드캐스터 연결단계
            //
            AnimPropertyBroadCaster animpropertyBroadCaster = _ownerCharacter.GetComponentInChildren<AnimPropertyBroadCaster>();
            animpropertyBroadCaster.AddAnimator(modelObject);
            //

            ////4. 생성된 오브젝트의 리깅 단계
            //
            RiggingPublisher ownerRigPublisher = _ownerCharacter.GetComponentInChildren<RiggingPublisher>();
            ownerRigPublisher.PublishRigging(modelObject, modelAnimator);
            //


            //5. 장착한 장비의 Skinned Mesh Renderer 비활성화 단계 (입은 장비만 보여주기 위함이다)
            //
            if (storeDesc._info._equipType == ItemInfo.EquipType.All)
            {
                //pass 전부다 보여준다
            }
            else {}
            /*---------------------------------------------------------------------------
            |TODO|  All Equipment 말고 다른구조는 어떻게 대응할지 생각해볼것.
            ---------------------------------------------------------------------------*/
            //


            //6. 원본 모델의  Skinned Mesh Renderer 비활성화 단계 (입은 장비만 보여주기 위함이다)
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
            |TODO|  All Equipment 말고 다른구조는 어떻게 대응할지 생각해볼것.
            ---------------------------------------------------------------------------*/
            //


            //7. 장착한 장비의 IK 단계
            //
            //


            //8. 장착한 장비에 무기 붙여주기 단계
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
        Debug.Assert(prefabAnimator != null, "입으려는 장비Prefab은 반드시 Animator를 가지고 있어야 합니다");

        Avatar prefabAvatar = prefabAnimator.avatar;

        return prefabAvatar == originalAvatar; //만약 아바타가 다르다면 서로 같은 뼈구조가 아니다.
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
