using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public interface IMoveItemStore
{
    public void DeleteOnMe(ItemStoreDesc storedDesc);
    public bool CheckItemDragDrop(ItemStoreDesc storedDesc, ref int startX, ref int startY, bool grabRotation, BoardUICellBase caller);
    public void AddItemUsingForcedIndex(ItemStoreDesc storedDesc, int targetX, int targetY, BoardUICellBase caller);

}
