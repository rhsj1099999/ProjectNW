using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonUpClose : MonoBehaviour, IPointerUpHandler
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_closeTargetGameObject != null) 
        {
            _closeTargetGameObject.SetActive(false);
        }
    }


    [SerializeField] private GameObject _closeTargetGameObject = null;
}
