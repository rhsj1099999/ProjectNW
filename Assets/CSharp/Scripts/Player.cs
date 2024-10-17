using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (_AnimController == null)
        { 
            //Crash
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void Awake()
    {
        SkinnedMeshRenderer[] originalModelSkinnedRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach (var skinnedMeshRenderer in originalModelSkinnedRenderers)
        {
            /*-------------------------------------------------------------
            |TODO| 애당초 컬링이 되면 안되는데, 왜 되야하는지 알아내야한다.
            -------------------------------------------------------------*/
            skinnedMeshRenderer.updateWhenOffscreen = true;
        }
    }
    [SerializeField] private AnimContoller _AnimController = null;
}
