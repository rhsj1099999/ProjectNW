using UnityEngine;

public class AbsoluteHandling : MonoBehaviour, IDebugRender
{
    private void Awake()
    {
        ((IDebugRender)this).AddMe(gameObject);
    }

    private void OnDestroy()
    {
        ((IDebugRender)this).RemoveMe(gameObject);
    }
}
