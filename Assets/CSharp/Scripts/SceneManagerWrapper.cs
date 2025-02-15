using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerWrapper : SubManager<SceneManagerWrapper>
{
    /*--------------------------------------------
    |NOTI| �� �ε�, �� ��ȯ ���� �����ϴ� �Ŵ���
    ���� ����� ����Ƽ���� �����ϴµ� ���Ѵ� (Ŀư������)
    --------------------------------------------*/

    public List<GameObject> _curtainCallPrefabList = new List<GameObject>();
    public List<GameObject> GetCurtainCallList() { return _curtainCallPrefabList; }
    private Dictionary<CurtainCallType, GameObject> _curtainCallPrefabs = new Dictionary<CurtainCallType, GameObject>();

    public override void SubManagerInit()
    {
        SingletonAwake();
        ReadyCurtainCallPrefabs();
    }

    public override void SubManagerUpdate()
    {
        if (Input.GetKeyDown(KeyCode.F1) == true)
        {
            SceneManagerWrapper.Instance.ChangeSceneDirectly("StageScene_1");
        }

        if (Input.GetKeyDown(KeyCode.F2) == true)
        {
            SceneManagerWrapper.Instance.ChangeSceneDirectly("StageScene_2");
        }

        if (Input.GetKeyDown(KeyCode.F3) == true)
        {
            SceneManagerWrapper.Instance.ChangeSceneDirectly("StageScene_0_Debugging");
        }

        if (Input.GetKeyDown(KeyCode.F4) == true)
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

    public Coroutine CurtainCall(CurtainCallType type, CurtainCallControlDesc desc)
    {
        Canvas canvasObject = UIManager.Instance.Get2DCanvs();

        GameObject simpleColorFadeInOut = _curtainCallPrefabs[type];

        GameObject newCurtainCall = Instantiate(simpleColorFadeInOut, canvasObject.transform);

        CurtainCallBase component = newCurtainCall.GetComponent<CurtainCallBase>();

        return component.Active(desc);
    }


    public void ChangeSceneDirectly(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }


    public Coroutine ChangeScene(string sceneName, CurtainCallType typeOn, CurtainCallControlDesc descOn, CurtainCallType typeOff, CurtainCallControlDesc descOff, bool isRevive = false)
    {
        return StartCoroutine(ChangeSceneCoroutine(sceneName, typeOn, descOn, typeOff, descOff, isRevive));
    }

    private IEnumerator ChangeSceneCoroutine(string sceneName, CurtainCallType typeOn, CurtainCallControlDesc descOn, CurtainCallType typeOff, CurtainCallControlDesc descOff, bool isRevive = false)
    {
        if (descOn != null)
        {
            yield return CurtainCall(typeOn, descOn);
        }

        //---------------------------------
        SceneManager.LoadScene(sceneName);
        //---------------------------------

        if (isRevive == true)
        {
            PlayerScript playerScript = FindFirstObjectByType<PlayerScript>();
            playerScript.CharacterRevive();
        }

        if (descOff != null)
        {
            yield return CurtainCall(typeOff, descOff);
        }

        yield return null;
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

    public override void SubManagerFixedUpdate()
    {
    }

    public override void SubManagerLateUpdate()
    {
    }
}
