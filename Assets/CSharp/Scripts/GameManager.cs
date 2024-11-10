using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    static private GameManager _instance = null;

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
        _instance = this;
        DontDestroyOnLoad(gameObject);

        if (_instance != this)
        {
            Destroy(this.gameObject);
        }

        foreach (var subManager in _subManagers)
        {
            subManager.SubManagerAwake();
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
}
