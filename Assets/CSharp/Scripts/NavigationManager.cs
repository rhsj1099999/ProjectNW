using System.Linq;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System;

[Serializable]
public struct TriangleDesc
{
    public TriangleDesc(int noUse = -1/*|TODO| C# ���� �÷���*/)
    {
        _positions = new Vector3[3];

        _isNeighbor0 = false;
        _isNeighbor1 = false;
        _isNeighbor2 = false;
    }

    public Vector3[] _positions;

    public bool _isNeighbor0;
    public bool _isNeighbor1;
    public bool _isNeighbor2;

    public bool isNeighbor(TriangleDesc another)
    {
        float customFloatEpsilon = 0.001f;

        int vertexSameCount = 0;

        for (int i = 0; i < 3; i++)
        {
            Vector3 myPosition = _positions[i];

            for (int j = 0; j < 3; j++)
            {
                if (Vector3.Distance(myPosition, another._positions[j]) <= customFloatEpsilon)
                {
                    //����
                    vertexSameCount++;
                    break;
                }
            }

            //sameCount�� 2�϶� ���⼭ ��� �����ص� ������ Ȥ�� �𸣴ϱ� 1,2,3 �϶� ���� �˻�
        }


        if (vertexSameCount == 0)
        {
            //�ٸ� �ﰢ���̴� (�ƿ� �������ִ�)
        }

        if (vertexSameCount == 1)
        {
            //�� �������� �پ��ִ�.
        }

        if (vertexSameCount == 2)
        {
            //�̿� �ﰢ���̴�
            return true;
        }

        if (vertexSameCount == 3)
        {
            Debug.Assert(false, "������ �Ȱ��� �ﰢ���� �����մϱ�?");
        }


        return false;
    }
}

[Serializable]
public struct SplittedNav
{
    public SplittedNav(int noUse = -1/*|TODO| C# ���� �÷���*/)
    {
        _triangles = new List<TriangleDesc>();
    }

    public void AddTriangle(TriangleDesc triangle)
    {
        _triangles.Add(triangle);
    }

    public List<TriangleDesc> _triangles;
}

public struct NavMeshObject
{
    public string _objectName; //Stage Map Name
    public List<NavMeshSurface> _navMeshSurfaceComponent;
}

public class NavigationManager : SubManager
{
    private static NavigationManager _instance = null;
    private Dictionary<string, HashSet<NavMeshSurface>> _stageNavMeshes = new Dictionary<string, HashSet<NavMeshSurface>>();
    private Dictionary<NavMeshSurface, NavMeshDataInstance> _activatedNavMeshIsntances = new Dictionary<NavMeshSurface, NavMeshDataInstance>();

    //NavTriangleDebuggingSection ... �ϳ��� NavComponent������ �����ϰ� �� �ִ� ��ĥ��
    [SerializeField] private GameObject _debuggingPlayer;
    [SerializeField] private GameObject _debuggingCornerSpherePrefab;
    [SerializeField] private GameObject _debuggingCornerCapsulePrefab;

    private List<GameObject> _createdDebuggingCornerSphere = new List<GameObject>();
    private List<GameObject> _createdDebuggingCornerCapsule = new List<GameObject>();
    private List<GameObject> _createdDebuggingCornerSphereReverse = new List<GameObject>();
    private List<GameObject> _createdDebuggingCornerCapsuleReverse = new List<GameObject>();
    private List<Mesh> triangleMeshes = new List<Mesh>();
    private List<Color> _colors = new List<Color>();
    private List<List<Vector3>> _totalVertices = new List<List<Vector3>>();
    private List<List<int>> _totalIndices = new List<List<int>>();
    private List<GameObject> _debuggingCapsules = new List<GameObject>();
    private bool _isDrawPlane = true;
    private bool _isDrawCapsules = true;
    [SerializeField] private int _debuggingTriangleCount = 2;

    //NavSplitSection
    private List<TriangleDesc> _triangleRecording = new List<TriangleDesc>();
    private List<SplittedNav> _splittedNavs = new List<SplittedNav>();
    private bool _isDrawSplittedNav = true;
    private List<GameObject> _debuggingPlaneVectorCapsules = new List<GameObject>();
    private bool _isDrawPlaneVectorCapsule = true;

    [SerializeField] private float _offMeshLinkRadius = 1.0f; //��ġ�� �����ϴٸ� �ּ� �ϳ��� �����ϵ� 2���̻���ʹ� �� �Ÿ���ŭ ��ġ�Ѵ�
    [SerializeField] private float _offMeshLinkOffset = 1.0f; //�� �Ÿ���ŭ [x-z]���������� Ray Casting�Ͽ� ��ġ�Ѵ�.
    [SerializeField] private float _offMeshLinkJumpDistance = 1.0f; //�� �Ÿ���ŭ ���� y�� ����Ѵ�
    [SerializeField] private float _offMeshLinkDropHeight = 1.0f; //�� �Ÿ���ŭ ���� y�� ����Ѵ�




    private void OnDrawGizmos()
    {
        if (_isDrawPlane == true)
        {
            if (triangleMeshes.Count <= 0) { return; }
            if (_colors.Count <= 0) { return; }

            int index = 0;
            foreach (var mesh in triangleMeshes)
            {
                Gizmos.color = _colors[index];
                Gizmos.DrawMesh(mesh, Vector3.zero);
                index++;
                if (index >= _colors.Count)
                {
                    index = (index % _colors.Count);
                }
            }
        }
    }

    public override void SubManagerUpdate()
    {
        DrawPlane();
        DrawCapsule();
    }

    private void DrawPlane()
    {
        if (Input.GetKeyDown(KeyCode.LeftBracket) == true)
        {
            _isDrawPlane = !_isDrawPlane;
        }
    }

    private void DrawCapsule()
    {
        if (Input.GetKeyDown(KeyCode.RightBracket) == true)
        {
            _isDrawCapsules = !_isDrawCapsules;

            foreach (var capsule in _debuggingCapsules)
            {
                capsule.SetActive(_isDrawCapsules);
            }
        }
    }

    private void DrawPlaneVectorCapsules()
    {
        if (Input.GetKeyDown(KeyCode.Slash) == true)
        {
            _isDrawPlaneVectorCapsule = !_isDrawPlaneVectorCapsule;

            foreach (var capsule in _debuggingPlaneVectorCapsules)
            {
                capsule.SetActive(_isDrawPlaneVectorCapsule);
            }
        }
    }

    private void OnDestroy()
    {
        foreach (KeyValuePair<NavMeshSurface, NavMeshDataInstance> item in _activatedNavMeshIsntances)
        {
            NavMesh.RemoveNavMeshData(item.Value);
        }
    }

    private void Awake()
    {
        if (_instance != this && _instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        _instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public override void SubManagerInit()
    {
        NavMesh.RemoveAllNavMeshData();


        Color color = Color.red;
        _colors.Add(color);

        color = Color.yellow;
        _colors.Add(color);

        color = Color.black;
        _colors.Add(color);

        color = Color.green;
        _colors.Add(color);
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

    public void AddStaticNavMesh(GameObject caller, NavMeshSurface callerComponent)
    {
        string objectName = caller.name;

        int sceneCount = SceneManager.sceneCount;

        Debug.Assert(sceneCount < 2, "��Ƽ�� �ε� �ý����� ���Եƽ��ϱ�?");

        string sceneName = SceneManager.GetActiveScene().name;

        NavMeshSurface component = callerComponent;

        Debug.Assert(component != null, "�� �Լ��� ȣ�������� �ش� ������Ʈ�� null�̿����� �ȵȴ�");

        if (_stageNavMeshes.ContainsKey(sceneName) == false)
        {
            _stageNavMeshes.Add(sceneName, new HashSet<NavMeshSurface>());
        }

        HashSet<NavMeshSurface> mapNavMesh = _stageNavMeshes[sceneName];

        if (component.navMeshData == null)
        {
            return;
        }
        //Debug.Assert(component.navMeshData != null, "������� ���� �׺���̼� ������Ʈ�Դϴ�");

        Debug.Assert(mapNavMesh.Contains(component) == false, "�̹� �ߺ� �׺���̼��� �ֽ��ϴ�");

        mapNavMesh.Add(component);

        NavMeshDataInstance dataInstance = NavMesh.AddNavMeshData(component.navMeshData);
        //OffMeshLink meshLink;
        _activatedNavMeshIsntances.Add(component, dataInstance);



        //NavMeshTriangulation tri = NavMesh.CalculateTriangulation();
        //for (int i = 0; i < tri.vertices.Length; i++)
        //{
        //    tri.vertices[i].y += 0.05f;
        //}

        //CreateTriangleMesh(tri, _debuggingTriangleCount);

        //for (int i = 0; i < tri.vertices.Length; i++)
        //{
        //    GameObject createdSphere = Instantiate(_debuggingCornerSpherePrefab);
        //    createdSphere.transform.position = tri.vertices[i];
        //    _createdDebuggingCornerSphere.Add(createdSphere);
        //    MeshRenderer meshRenderer = createdSphere.GetComponent<MeshRenderer>();
        //    meshRenderer.material.color = Color.red;
        //}

        //SplitMesh(tri);

        //PlaceDebuggingPlaneVector();
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


    bool isSamePosition(Vector3 sour, Vector3 dest)
    {
        float customFloatEpsilon = 0.01f;

        float deltaX = Mathf.Abs(sour.x - dest.x);
        float deltaY = Mathf.Abs(sour.y - dest.y);
        float deltaZ = Mathf.Abs(sour.z - dest.z);

        if (deltaX <= customFloatEpsilon && deltaY <= customFloatEpsilon && deltaZ <= customFloatEpsilon)
        {
            return true;
        }

        return false;
    }



    void CreateTriangleMesh(NavMeshTriangulation triangualtion, int triangleCount = -1)
    {
        if (triangleCount == -1)
        {
            triangleMeshes.Add(new Mesh());
            triangleMeshes[0].vertices = triangualtion.vertices;
            triangleMeshes[0].triangles = triangualtion.indices;
        }
        else
        {

            for (int i = 0; i < triangleCount; i++)
            {
                if (i >= triangualtion.areas.Length)
                {
                    break;
                }
                List<Vector3> vertices = new List<Vector3>();
                vertices.Add(new Vector3());
                vertices.Add(new Vector3());
                vertices.Add(new Vector3());
                _totalVertices.Add(vertices);
                List<int> indices = new List<int>();
                indices.Add(0);
                indices.Add(0);
                indices.Add(0);
                _totalIndices.Add(indices);

                triangleMeshes.Add(new Mesh());
                indices[0] = triangualtion.indices[(i * 3) + 0];
                indices[1] = triangualtion.indices[(i * 3) + 1];
                indices[2] = triangualtion.indices[(i * 3) + 2];

                vertices[0] = triangualtion.vertices[indices[0]];
                vertices[1] = triangualtion.vertices[indices[1]];
                vertices[2] = triangualtion.vertices[indices[2]];

                indices[0] = 0;
                indices[1] = 1;
                indices[2] = 2;

                Vector3[] dirs = new Vector3[3];
                Vector3[] betweens = new Vector3[3];
                float[] distances = new float[3];

                dirs[0] = (vertices[1] - vertices[0]).normalized;
                dirs[1] = (vertices[2] - vertices[1]).normalized;
                dirs[2] = (vertices[0] - vertices[2]).normalized;

                betweens[0] = (vertices[1] + vertices[0]) / 2.0f;
                betweens[1] = (vertices[2] + vertices[1]) / 2.0f;
                betweens[2] = (vertices[0] + vertices[2]) / 2.0f;

                distances[0] = Vector3.Distance(vertices[1],vertices[0]) / 2.0f;
                distances[1] = Vector3.Distance(vertices[2],vertices[1]) / 2.0f;
                distances[2] = Vector3.Distance(vertices[0],vertices[2]) / 2.0f;

                GameObject debuggingCapsule = new GameObject();

                for (int j = 0; j < 3; j++)
                {
                    GameObject createdCapsule = Instantiate(_debuggingCornerCapsulePrefab);

                    createdCapsule.transform.position = betweens[j];
                    Vector3 targetUp = dirs[j];

                    createdCapsule.transform.up = targetUp;
                    Vector3 localScale = createdCapsule.transform.localScale;
                    localScale.y = distances[j];
                    createdCapsule.transform.localScale = localScale;
                    createdCapsule.GetComponent<MeshRenderer>().material.color = Color.green;

                    _debuggingCapsules.Add(createdCapsule);
                }


                triangleMeshes[i].vertices = _totalVertices[i].ToArray();
                triangleMeshes[i].triangles = _totalIndices[i].ToArray();
            }
        }

        foreach (Mesh mesh in triangleMeshes) 
        {
            mesh.RecalculateNormals();
        }
    }


    private void SplitMesh(NavMeshTriangulation triangulation)
    {
        int currTriangles = 0;

        int[] currIndices = new int[3];
        Vector3[] currVertices = new Vector3[3];

        List<int> neighborNavIndices = new List<int>();

        for (int i = 0; i < triangulation.areas.Length; i++)
        {
            neighborNavIndices.Clear();

            TriangleDesc currTriangle = new TriangleDesc();
            currIndices[0] = triangulation.indices[(i * 3) + 0];
            currIndices[1] = triangulation.indices[(i * 3) + 1];
            currIndices[2] = triangulation.indices[(i * 3) + 2];
            currTriangle._positions = new Vector3[3];
            currTriangle._positions[0] = triangulation.vertices[currIndices[0]];
            currTriangle._positions[1] = triangulation.vertices[currIndices[1]];
            currTriangle._positions[2] = triangulation.vertices[currIndices[2]];
            //�ﰢ���� �ϳ� �ϼ��ƴ�.

            //�� �ﰢ���� ������ �ִ� �ﰢ����� �̿��ΰ�?

            for (int j = 0; j < _splittedNavs.Count; j++)
            {
                List<TriangleDesc> navTriangles = _splittedNavs[j]._triangles;

                for (int k = 0; k < navTriangles.Count; k++)
                {
                    TriangleDesc existed = navTriangles[k];

                    //if (existed._isNeighbor0 == true && existed._isNeighbor1 == true && existed._isNeighbor2 == true)
                    //{
                    //    continue;
                    //}

                    bool isNeighbor = currTriangle.isNeighbor(navTriangles[k]);

                    if (isNeighbor == true)
                    {
                        neighborNavIndices.Add(j);
                        break;
                    }
                }
            }

            if (neighborNavIndices.Count == 0)
            {
                SplittedNav newSplittedNav = new SplittedNav();
                newSplittedNav._triangles = new List<TriangleDesc>();
                newSplittedNav.AddTriangle(currTriangle);
                _splittedNavs.Add(newSplittedNav);
            }

            if (neighborNavIndices.Count == 1) //�ϳ��� �ִ�
            {
                List<TriangleDesc> navTriangles = _splittedNavs[neighborNavIndices[0]]._triangles;
                navTriangles.Add(currTriangle);
            }

            if (neighborNavIndices.Count > 1) //�ΰ� �̻��̴�
            {
                List<TriangleDesc> navTriangles = _splittedNavs[neighborNavIndices[0]]._triangles;
                navTriangles.Add(currTriangle);

                for (int x = 1; x < neighborNavIndices.Count; x++)
                {
                    List<TriangleDesc> navTrianglesCopy = _splittedNavs[neighborNavIndices[x]]._triangles;

                    for (int y = 0; y < navTrianglesCopy.Count; y++)
                    {
                        navTriangles.Add(navTrianglesCopy[y]);
                    }
                }

                int removeCount = 0;
                for (int x = 1; x < neighborNavIndices.Count; x++)
                {
                    _splittedNavs.Remove(_splittedNavs[neighborNavIndices[x] - removeCount]);
                    removeCount++;
                }
            }

            currTriangles++;
        }

        int debugCount = 0;
        foreach (var tris in _splittedNavs)
        {
            debugCount += tris._triangles.Count;
        }



        if (currTriangles != (triangulation.areas.Length))
        {
            Debug.Assert(false, "�ﰢ�� ������ �ٸ����� �ֽ��ϱ�?");
        }

        if (debugCount != (triangulation.areas.Length))
        {
            Debug.Assert(false, "�ﰢ�� ������ �ٸ����� �ֽ��ϱ�?");
        }
    }

    private void PlaceOffMeshLinks()
    {
        //������ �ﰢ�� ���θ� ������� �Ѵ�

        for (int i = 0; i < 1/*������ ���� ����(���� �Ÿ���)*/; i++)
        {
            foreach (var splittedNav in _splittedNavs)
            {
                foreach (var triangle in splittedNav._triangles)
                {
                    TriangleDesc currTriangle = triangle;

                    //�� ���� ����,�����ѹ������� �� ���常ŭ �������� ����ĳ���� �Ѵ�

                    //����� ���´ٸ� 

                }
            }
        }
    }

    private void PlaceDebuggingPlaneVector()
    {
        Vector3[] betweens = new Vector3[3];
        Vector3[] dirs = new Vector3[3];

        foreach (var splittedNav in _splittedNavs)
        {
            foreach (var triangle in splittedNav._triangles)
            {
                TriangleDesc currTriangle = triangle;
                
                betweens[0] = (currTriangle._positions[0] + currTriangle._positions[1]) / 2.0f;
                betweens[1] = (currTriangle._positions[1] + currTriangle._positions[2]) / 2.0f;
                betweens[2] = (currTriangle._positions[2] + currTriangle._positions[0]) / 2.0f;

                dirs[0] = currTriangle._positions[0] - currTriangle._positions[1];
                dirs[1] = currTriangle._positions[1] - currTriangle._positions[2];
                dirs[2] = currTriangle._positions[2] - currTriangle._positions[0];

                Vector3 normal = Vector3.Cross(dirs[0], dirs[1]);
                Vector3 centor = (betweens[0] + betweens[1] + betweens[2]) / 3.0f;



                for (int j = 0; j < 3; j++)
                {
                    GameObject createdCapsule = Instantiate(_debuggingCornerCapsulePrefab);

                    createdCapsule.transform.position = betweens[j];
                    Vector3 targetUp = Vector3.Cross(normal, dirs[j]);

                    createdCapsule.transform.up = targetUp;
                    Vector3 localScale = createdCapsule.transform.localScale;
                    createdCapsule.transform.position += targetUp.normalized * localScale.y;


                    localScale.y = 0.25f;
                    createdCapsule.transform.localScale = localScale;


                    createdCapsule.GetComponent<MeshRenderer>().material.color = Color.green;

                    _debuggingPlaneVectorCapsules.Add(createdCapsule);
                }
            }
        }
        //������ �ﰢ�� ���θ� ������� �Ѵ�
    }

}
