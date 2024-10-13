using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    static CameraManager _instance = null;
    public static CameraManager GetInstance()
    {
        if (_instance == null)
        {
            _instance = new CameraManager();
        }

        return _instance;
    }

    [SerializeField] Dictionary<string, Camera> _cameras = new Dictionary<string, Camera>();
    public Camera GetCamera(string cameraName)
    {
        if (_cameras.ContainsKey(cameraName) == false)
        {
            //Crash;
            return null;
        }
        return _cameras[cameraName];
    }
}
