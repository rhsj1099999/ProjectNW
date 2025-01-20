using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardCellDesc
{
    public BoardUIBaseScript _owner = null;
}

public abstract class BoardUICellBase : MonoBehaviour
{
    protected BoardUIBaseScript _owner = null;

    public BoardUIBaseScript GetOwner() { return _owner; }
    public void Initialize(BoardCellDesc desc) {_owner = desc._owner;}
    public abstract bool TryMoveItemDropOnCell(ItemStoreDesc storedDesc, ref int startX, ref int startY, bool grabRotation);
}
