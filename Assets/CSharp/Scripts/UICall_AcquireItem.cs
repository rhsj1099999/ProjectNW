using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class UICall_AcquireItem : UICallScript
{
    public class UICall_AcquireItemDesc
    {
        public GameObject _itemTarget = null;
        public ItemStoreDesc _itemStoreDesc = null;
        public Collider _offCollider = null;
    }

    private UICall_AcquireItemDesc _desc = null;

    public void Init(UICall_AcquireItemDesc desc)
    {
        _desc = desc;
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
            
            GameObject inventoryObject = characterScript.GetInventory();

            if (inventoryObject == null) 
            {
                return;
            }

            InventoryBoard[] inventories = inventoryObject.GetComponentsInChildren<InventoryBoard>();

            if (inventories.Length <= 0)
            {
                return;
            }

            foreach (InventoryBoard inventoryBoard in inventories)
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

            }

        }
        caller.RemoveList(this);
        Destroy(_desc._itemTarget);
    }

    public override void UICall_Off(InteractionUIListScript caller)
    {
    }
}   
