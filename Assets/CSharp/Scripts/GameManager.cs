using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    static private GameManager _instance = null;

    [SerializeField] private List<GameObject> _FIRSTINTANTIATE = new List<GameObject>();

    public GameManager Instance 
    {
        get 
        {
            return _instance;
        }
    }

    private List<ISubManager> _subManagers = new List<ISubManager>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        
        DontDestroyOnLoad(gameObject);

        ISubManager[] components = GetComponents<ISubManager>();
        foreach (ISubManager sub in components) 
        {
            sub.SubManagerInit();
            _subManagers.Add(sub);
        }

        foreach (var FRAMEDROPOBJECT in _FIRSTINTANTIATE)
        {
            GameObject newGameObject = Instantiate(FRAMEDROPOBJECT);
            Destroy(newGameObject);
        }
    }

    private void Start()
    {
        foreach (var subManager in _subManagers)
        {
            subManager.SubManagerStart();
        }
    }

    private void FixedUpdate()
    {
        foreach (var subManager in _subManagers)
        {
            subManager.SubManagerFixedUpdate();
        }
    }

    private void Update()
    {
        foreach (var subManager in _subManagers)
        {
            subManager.SubManagerUpdate();
        }
    }

    private void LateUpdate()
    {
        foreach (var subManager in _subManagers)
        {
            subManager.SubManagerLateUpdate();
        }
    }
}
