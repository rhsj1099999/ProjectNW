using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BoardUIBaseScript : GameUISubComponent
{
    public abstract void DeleteOnMe(ItemStoreDesc storedDesc);
    public abstract bool CheckItemDragDrop(ItemStoreDesc storedDesc, ref int startX, ref int startY, bool grabRotation, BoardUICellBase caller);
    public abstract void AddItemUsingForcedIndex(ItemStoreDesc storedDesc, int targetX, int targetY, BoardUICellBase caller);
}
