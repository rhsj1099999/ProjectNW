using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardCellDesc
{
    public IMoveItemStore _owner = null;
}

public abstract class BoardUICellBase : MonoBehaviour
{
    protected IMoveItemStore _owner = null;

    public IMoveItemStore GetOwner() { return _owner; }

    public void Initialize(BoardCellDesc desc)
    {
        _owner = desc._owner;
    }

    public abstract bool TryMoveItemDropOnCell(ItemStoreDesc storedDesc, ref int startX, ref int startY, bool grabRotation);
}
