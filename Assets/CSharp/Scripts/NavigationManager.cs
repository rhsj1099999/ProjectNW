using System.Linq;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public struct NavMeshObject
{
    public string _objectName; //Stage Map Name
    public List<NavMeshSurface> _navMeshSurfaceComponent;
}

public class NavigationManager : MonoBehaviour
{
    private static NavigationManager _instance = null;
    private Dictionary<string, HashSet<NavMeshSurface>> _stageNavMeshes = new Dictionary<string, HashSet<NavMeshSurface>>();
    private Dictionary<NavMeshSurface, NavMeshDataInstance> _activatedNavMeshIsntances = new Dictionary<NavMeshSurface, NavMeshDataInstance>();

    private void OnDestroy()
    {
        foreach (KeyValuePair<NavMeshSurface, NavMeshDataInstance> item in _activatedNavMeshIsntances)
        {
            NavMesh.RemoveNavMeshData(item.Value);
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(_instance.gameObject);
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        NavMesh.RemoveAllNavMeshData();
    }

    

    private void Start()
    {
        //���� �� �׺� Ȱ��ȭ
        //NavMesh.RemoveAllNavMeshData();
    }

    public static NavigationManager Instance
    {
        get 
        {
            if (_instance == null)
            {
                GameObject gameObject = new GameObject("NavManager_AfterCreated");
                DontDestroyOnLoad (gameObject);
                NavigationManager component = gameObject.AddComponent<NavigationManager>();
                _instance = component;
            }
            return _instance;
        }
    }

    public void DeActiveAllNavMesh()
    {
        NavMesh.RemoveAllNavMeshData();
        _activatedNavMeshIsntances.Clear();
    }

    public void DeActiveNavMesh(NavMeshSurface target)
    {
        if (_activatedNavMeshIsntances.ContainsKey(target) == false)
        {
            return; //�̹� ������.
        }

        NavMeshDataInstance targetInstance = _activatedNavMeshIsntances[target];

        NavMesh.RemoveNavMeshData(targetInstance);

        _activatedNavMeshIsntances.Remove(target);
    }

    public void ActiveNavMesh(NavMeshSurface target)
    {
        if (_activatedNavMeshIsntances.ContainsKey(target) == true)
        {
            return; //�̹� �ߵ� �ƴ�.
        }

        NavMeshDataInstance dataInstance = NavMesh.AddNavMeshData(target.navMeshData);

        _activatedNavMeshIsntances.Add(target, dataInstance);
    }

    public void ActiveAllSceneNavMesh(string SceneName)
    {
        HashSet<NavMeshSurface> hashSet = _stageNavMeshes[SceneName];

        foreach (NavMeshSurface surface in hashSet)
        {
            ActiveNavMesh(surface);
        }
    }

    public void ActiveAllNavMesh()
    {
        foreach (KeyValuePair<string, HashSet<NavMeshSurface>> item in _stageNavMeshes)
        {
            HashSet<NavMeshSurface> hashSet = item.Value;

            foreach (NavMeshSurface surface in hashSet)
            {
                ActiveNavMesh(surface);
            }
        }
    }

    public void AddStaticNavMesh(GameObject caller)
    {
        string objectName = caller.name;

        int sceneCount = SceneManager.sceneCount;

        Debug.Assert(sceneCount < 2, "��Ƽ�� �ε� �ý����� ���Եƽ��ϱ�?");

        string sceneName = SceneManager.GetActiveScene().name;

        NavMeshSurface component = caller.GetComponent<NavMeshSurface>();

        Debug.Assert(component != null, "�� �Լ��� ȣ�������� �ش� ������Ʈ�� null�̿����� �ȵȴ�");

        if (_stageNavMeshes.ContainsKey(sceneName) == false)
        {
            _stageNavMeshes.Add(sceneName, new HashSet<NavMeshSurface>());
        }

        HashSet<NavMeshSurface> mapNavMesh = _stageNavMeshes[sceneName];

        Debug.Assert(mapNavMesh.Contains(component) == false, "�̹� �ߺ� �׺���̼��� �ֽ��ϴ�");

        Debug.Assert(component.navMeshData != null, "������� ���� �׺���̼� ������Ʈ�Դϴ�");

        mapNavMesh.Add(component);

        NavMeshDataInstance dataInstance = NavMesh.AddNavMeshData(component.navMeshData);

        _activatedNavMeshIsntances.Add(component, dataInstance);
    }


    public NavMeshSurface GetCurrStageNavMeshByPosition(Vector3 position)
    {
        string currSceneName = SceneManager.GetActiveScene().name;

        //Debug.Assert(_stageNavMeshes.ContainsKey(currSceneName) == true, "�׺���̼� �޽��� �� �ϳ��� �����ϱ�?");

        if (_stageNavMeshes.ContainsKey(currSceneName) == false)
        {
            return null;
        }

        SortedDictionary<float, NavMeshSurface> characterIn = new SortedDictionary<float, NavMeshSurface>();

        foreach (var navMesh in _stageNavMeshes[currSceneName])
        {
            Vector3 size = navMesh.size;
            Vector3 center = navMesh.center;

            // NavMeshSurface�� ���� ���� ��ǥ������ �ּ� �� �ִ� �� ���
            Vector3 worldCenter = navMesh.transform.TransformPoint(center);  // ���� ��ǥ�� ��ȯ�� �߽���
            Vector3 halfSize = size * 0.5f;

            Vector3 minBounds = worldCenter - halfSize;  // AABB�� �ּ� �� (���ϴ� �𼭸�)
            Vector3 maxBounds = worldCenter + halfSize;  // AABB�� �ִ� �� (���� �𼭸�)

            // �������� NavMeshSurface�� AABB ���� �ִ��� üũ
            bool isIn = (position.x >= minBounds.x && position.x <= maxBounds.x &&
            position.y >= minBounds.y && position.y <= maxBounds.y &&
            position.z >= minBounds.z && position.z <= maxBounds.z);

            if (isIn == true)
            {
                float distance = Vector3.Distance(position, center);
                characterIn.Add(distance, navMesh);
            }
        }

        if (characterIn.Count == 0)
        {
            return null;
        }

        return characterIn.First().Value;
    }

}
