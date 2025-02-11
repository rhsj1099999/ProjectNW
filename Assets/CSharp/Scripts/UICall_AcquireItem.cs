using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using static ItemUI;

public class UICall_AcquireItem : UICallScript
{
    public class UICall_AcquireItemDesc
    {
        public GameObject _itemTarget = null;
        public ItemStoreDescBase _itemStoreDesc = null;
        public Collider _offCollider = null;
    }

    private UICall_AcquireItemDesc _desc = null;

    public void Init(UICall_AcquireItemDesc desc)
    {
        _desc = desc;

        _uiData = new InteractionUIData();
        _uiData._sprite = desc._itemStoreDesc._itemAsset._ItemImage;
        _uiData._message = "줍기 : " + desc._itemStoreDesc._itemAsset._ItemName;
    }

    private void Awake() {}
    private void Start() {}

    public override void UICall(InteractionUIListScript caller)
    {
        //주우려고 할때 불립니다.
        {
            GameActorScript controllingScript = caller._Owner.GetUIControllingComponent();
            CharacterScript characterScript = controllingScript as CharacterScript;
            if (characterScript == null) 
            {
                return;
            }
            
            List<InventoryBoard> ownerInventoryBoards = characterScript.GetMyInventoryBoards();

            if (ownerInventoryBoards.Count <= 0)
            {
                return;
            }

            bool isSuccess = false;
            foreach (InventoryBoard inventoryBoard in ownerInventoryBoards)
            {
                int targetX = -1;
                int targetY = -1;
                bool isRotated = false;
                bool isAquireable = inventoryBoard.CheckInventorySpace_MustOpt(_desc._itemStoreDesc._itemAsset, ref targetX, ref targetY, ref isRotated);

                if (isAquireable == false)
                {
                    continue;
                }

                _desc._itemStoreDesc._isRotated = isRotated;
                inventoryBoard.AddItemUsingForcedIndex(_desc._itemStoreDesc, targetX, targetY, null);
                isSuccess = true;
                caller.RemoveList(this);
                Destroy(_desc._itemTarget);
                break;
            }

            if (isSuccess == false) 
            {
                return;
            }
        }

    }

    public override void UICall_Off(InteractionUIListScript caller)
    {
    }
}   
