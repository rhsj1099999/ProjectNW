using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerWrapper : SubManager
{
    /*--------------------------------------------
    |NOTI| �� �ε�, �� ��ȯ ���� �����ϴ� �Ŵ���
    ���� ����� ����Ƽ���� �����ϴµ� ���Ѵ� (Ŀư������)
    --------------------------------------------*/

    private static SceneManagerWrapper _instance = null;

    public List<GameObject> _curtainCallPrefabList = new List<GameObject>();
    public List<GameObject> GetCurtainCallList() { return _curtainCallPrefabList; }
    private Dictionary<CurtainCallType, GameObject> _curtainCallPrefabs = new Dictionary<CurtainCallType, GameObject>();

    public static SceneManagerWrapper Instance 
    {
        get
        {
            if (_instance == null) 
            {
                GameObject newSceneManagerWrapper = new GameObject("SceneManagerWrapper");
                DontDestroyOnLoad(newSceneManagerWrapper);
                _instance = newSceneManagerWrapper.AddComponent<SceneManagerWrapper>();
            }

            return _instance;
        }
    }

    public override void SubManagerAwake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        _instance = this;

        DontDestroyOnLoad(gameObject);

        ReadyCurtainCallPrefabs();
    }

    public override void SubManagerUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) == true)
        {
            SceneManagerWrapper.Instance.ChangeSceneDirectly("StageScene_1");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) == true)
        {
            SceneManagerWrapper.Instance.ChangeSceneDirectly("StageScene_2");
        }

        if (Input.GetKeyDown(KeyCode.Alpha0) == true)
        {
            SceneManagerWrapper.Instance.ChangeSceneDirectly("StageScene_0");
        }
    }


    public override void SubManagerStart()
    {
        /*--------------------------------------------------------------------------------
        |TODO| ���� ���۽� ù ȭ���� �Ʒ� �Լ��� �����Ǵµ� �̰͵� �ۿ��� �����ϰ�ʹ�
        --------------------------------------------------------------------------------*/

        GameStartWhiteScene();
    }

    private void ReadyCurtainCallPrefabs()
    {
        if (_curtainCallPrefabList.Count <= 0)
        {
            Debug.Assert(false, "���� �ϳ��� �����ϱ�?");
            Debug.Break();
        }

        foreach (var prefab in _curtainCallPrefabList)
        {
            CurtainCallBase component = prefab.GetComponent<CurtainCallBase>();

            CurtainCallType type = component.GetCurtainCallType();

            if (_curtainCallPrefabs.ContainsKey(type) == true)
            {
                Debug.Assert(false, "�̹� Ÿ���� �ֽ��ϴ�");
                Debug.Break();
                continue;
            }

            _curtainCallPrefabs.Add(type, prefab);
        }
    }

    public void CurtainCall(CurtainCallType type, CurtainCallControlDesc desc)
    {
        GameObject mainCanvasObject = UIManager.Instance.GetMainCanvasObject();

        GameObject simpleColorFadeInOut = _curtainCallPrefabs[type];

        GameObject newCurtainCall = Instantiate(simpleColorFadeInOut, mainCanvasObject.transform);

        CurtainCallBase component = newCurtainCall.GetComponent<CurtainCallBase>();

        component.Active(desc);
    }


    public void ChangeSceneDirectly(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }


    public void ChangeScene(string sceneName, CurtainCallType type, CurtainCallControlDesc desc)
    {
        SceneManager.LoadScene(sceneName);
    }


    private void GameStartWhiteScene()
    {
        CurtainCallControl_SimpleColor newDesc = new CurtainCallControl_SimpleColor();

        newDesc._runningTime = 2.0f;
        newDesc._target = true;
        newDesc._color = new Vector3(1.0f, 1.0f, 1.0f);

        CurtainCall(CurtainCallType.SimpleColorFadeInOut, newDesc);
    }



    public class CurtainCallDesc
    {
        public GameObject _curtainCallPrefab = null;
        public float _targetTime = 1.0f;
        public bool _state = false;
    }

    public GameObject GetCurtainCallPrefab(CurtainCallType type)
    {
        return _curtainCallPrefabs[type];
    }

    public void CurtainCall(CurtainCallDesc desc)
    {

    }
}
