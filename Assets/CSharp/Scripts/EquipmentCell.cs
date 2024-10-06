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
        Debug.Assert(_equipType != EquipType.None, "장착셀에 None이 설정돼있으면 안된다");
        Debug.Assert(_equipmentUIObjectPrefab != null, "equipment Prefab이 null이여서는 안된다");

        _ownerCharacter = transform.root.gameObject;
    }

    public void DeleteOnMe(ItemStoreDesc storeDesc) // : IMoveItemStore
    {
        Debug.Assert(_equippedItemUI != null, "장착하지 않았는데 지우려합니다??");
        Destroy(_equippedItemUI);
        _equippedItemUI = null;
    }

    public void EquipItem(ItemStoreDesc storedDesc)
    {
        if (_equippedItemUI != null) 
        {
            return;
            //이미 해당칸에 장착하고 있으면 그냥 종료
            /*---------------------------------------------------------------------------
            |TODO|  나중에 아이템 스왑기능 구현시 이곳을 수정해야한다.
            ---------------------------------------------------------------------------*/
        }

        if (storedDesc._info._equipType != _equipType)
        {
            //착용 부위가 다른템이다
            return;
        }
        
        {
            storedDesc._owner.DeleteOnMe(storedDesc);//삭제하고

            //장착한다

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


        //_ownerCharacter -> 모델 바꾸기

        EquipItemMesh(storedDesc);
    }

    private void EquipItemMesh(ItemStoreDesc storeDesc)
    {
        //if (false/*원본 모델과 같은 뼈 구조이다*/) 
        //{

        //}
        //else
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
