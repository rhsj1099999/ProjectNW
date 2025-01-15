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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
