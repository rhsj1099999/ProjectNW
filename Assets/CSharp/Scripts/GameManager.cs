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
            if (_instance == null)
            {
                GameObject gameObject = new GameObject("GameManager");
                _instance = gameObject.AddComponent<GameManager>();
                DontDestroyOnLoad(gameObject);
            }

            return _instance;
        }
    }

    [SerializeField] private List<SubManager> _subManagers = new List<SubManager>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        
        DontDestroyOnLoad(gameObject);

        foreach (var subManager in _subManagers)
        {
            subManager.SubManagerInit();
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


    private void OnDestroy()
    {
        //foreach (var subManager in _subManagers)
        //{
        //    Destroy(subManager.gameObject);
        //}

        _subManagers.Clear();
    }
}
