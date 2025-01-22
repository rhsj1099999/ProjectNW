using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ItemUI;

public abstract class BoardUIBaseScript : GameUISubComponent
{
    public abstract void DeleteOnMe(ItemStoreDescBase storedDesc);
    public abstract bool CheckItemDragDrop(ItemStoreDescBase storedDesc, ref int startX, ref int startY, bool grabRotation, BoardUICellBase caller);
    public abstract void AddItemUsingForcedIndex(ItemStoreDescBase storedDesc, int targetX, int targetY, BoardUICellBase caller);
    public abstract List<GameObject> GetItemUIs(ItemStoreDescBase storeDesc);
}
