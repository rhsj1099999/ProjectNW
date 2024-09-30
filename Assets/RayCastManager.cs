using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RayCastManager : MonoBehaviour
{
    [SerializeField] private EventSystem _eventSystem = null;

    private static RayCastManager _instance;
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }

        _instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
    void Start()
    {

    }

    void Update()
    {

    }

    public static RayCastManager Instance
    {
        get 
        {
            if (_instance == null)
            {
                _instance = new RayCastManager();
                DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
    }

    public void RayCastAll(ref List<RaycastResult> results, bool isReverse = false)
    {
        GameObject rootObject = transform.root.gameObject;

        GraphicRaycaster[] raycasters = rootObject.GetComponentsInChildren<GraphicRaycaster>();

        Vector2 mousePosition = Input.mousePosition;
        PointerEventData pointerEventData = new PointerEventData(_eventSystem);
        pointerEventData.position = mousePosition;

        List<RaycastResult> eachResult = new List<RaycastResult>();
        
        if (isReverse == true) 
        {
            foreach (var raycaster in raycasters.AsEnumerable().Reverse())
            {
                eachResult.Clear();
                raycaster.Raycast(pointerEventData, eachResult);
                eachResult.Reverse();

                foreach (var result in eachResult)
                {
                    results.Add(result);
                }
            }
        }
        else
        {
            foreach (var raycaster in raycasters)
            {
                eachResult.Clear();
                raycaster.Raycast(pointerEventData, eachResult);
                eachResult.Reverse();

                foreach (var result in eachResult)
                {
                    results.Add(result);
                }
            }
        }

    }
}
