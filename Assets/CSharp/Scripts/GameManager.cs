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
                GameManager component = gameObject.AddComponent<GameManager>();
                _instance = component;
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

        Application.targetFrameRate = -1; // 제한 없음

        foreach (var subManager in _subManagers)
        {
            subManager.SubManagerAwake();
        }

        foreach(var FRAMEDROPOBJECT in _FIRSTINTANTIATE)
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
