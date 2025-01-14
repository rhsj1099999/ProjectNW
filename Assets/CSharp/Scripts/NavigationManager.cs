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
    public TriangleDesc(int noUse = -1/*|TODO| C# 버전 올려라*/)
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
                    //같음
                    vertexSameCount++;
                    break;
                }
            }

            //sameCount가 2일때 여기서 즉시 리턴해도 되지만 혹시 모르니까 1,2,3 일때 전부 검사
        }


        if (vertexSameCount == 0)
        {
            //다른 삼각형이다 (아예 떨어져있다)
        }

        if (vertexSameCount == 1)
        {
            //한 꼭지점만 붙어있다.
        }

        if (vertexSameCount == 2)
        {
            //이웃 삼각형이다
            return true;
        }

        if (vertexSameCount == 3)
        {
            Debug.Assert(false, "완전히 똑같은 삼각형이 존재합니까?");
        }


        return false;
    }
}

[Serializable]
public struct SplittedNav
{
    public SplittedNav(int noUse = -1/*|TODO| C# 버전 올려라*/)
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

    //NavTriangleDebuggingSection ... 하나의 NavComponent에서만 동작하게 돼 있다 고칠것
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

    [SerializeField] private float _offMeshLinkRadius = 1.0f; //설치가 가능하다면 최소 하나를 보장하되 2개이상부터는 이 거리만큼 설치한다
    [SerializeField] private float _offMeshLinkOffset = 1.0f; //이 거리만큼 [x-z]떨어진곳에 Ray Casting하여 설치한다.
    [SerializeField] private float _offMeshLinkJumpDistance = 1.0f; //이 거리만큼 점프 y를 고려한다
    [SerializeField] private float _offMeshLinkDropHeight = 1.0f; //이 거리만큼 낙하 y를 고려한다




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
        //현재 씬 네비만 활성화
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
            return; //이미 꺼졌다.
        }

        NavMeshDataInstance targetInstance = _activatedNavMeshIsntances[target];

        NavMesh.RemoveNavMeshData(targetInstance);

        _activatedNavMeshIsntances.Remove(target);
    }

    public void ActiveNavMesh(NavMeshSurface target)
    {
        if (_activatedNavMeshIsntances.ContainsKey(target) == true)
        {
            return; //이미 발동 됐다.
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

        Debug.Assert(sceneCount < 2, "멀티씬 로딩 시스템이 도입됐습니까?");

        string sceneName = SceneManager.GetActiveScene().name;

        NavMeshSurface component = callerComponent;

        Debug.Assert(component != null, "이 함수를 호출했을때 해당 컴포넌트가 null이여서는 안된다");

        if (_stageNavMeshes.ContainsKey(sceneName) == false)
        {
            _stageNavMeshes.Add(sceneName, new HashSet<NavMeshSurface>());
        }

        HashSet<NavMeshSurface> mapNavMesh = _stageNavMeshes[sceneName];

        if (component.navMeshData == null)
        {
            return;
        }
        //Debug.Assert(component.navMeshData != null, "빌드되지 않은 네비게이션 컴포넌트입니다");

        Debug.Assert(mapNavMesh.Contains(component) == false, "이미 중복 네비게이션이 있습니다");

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

        //Debug.Assert(_stageNavMeshes.ContainsKey(currSceneName) == true, "네비게이션 메쉬가 단 하나라도 없습니까?");

        if (_stageNavMeshes.ContainsKey(currSceneName) == false)
        {
            return null;
        }

        SortedDictionary<float, NavMeshSurface> characterIn = new SortedDictionary<float, NavMeshSurface>();

        foreach (var navMesh in _stageNavMeshes[currSceneName])
        {
            Vector3 size = navMesh.size;
            Vector3 center = navMesh.center;

            // NavMeshSurface의 실제 월드 좌표에서의 최소 및 최대 값 계산
            Vector3 worldCenter = navMesh.transform.TransformPoint(center);  // 월드 좌표로 변환된 중심점
            Vector3 halfSize = size * 0.5f;

            Vector3 minBounds = worldCenter - halfSize;  // AABB의 최소 값 (좌하단 모서리)
            Vector3 maxBounds = worldCenter + halfSize;  // AABB의 최대 값 (우상단 모서리)

            // 포지션이 NavMeshSurface의 AABB 내에 있는지 체크
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
            //삼각형이 하나 완성됐다.

            //이 삼각형은 기존에 있던 삼각형들과 이웃인가?

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

            if (neighborNavIndices.Count == 1) //하나만 있다
            {
                List<TriangleDesc> navTriangles = _splittedNavs[neighborNavIndices[0]]._triangles;
                navTriangles.Add(currTriangle);
            }

            if (neighborNavIndices.Count > 1) //두개 이상이다
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
            Debug.Assert(false, "삼각형 개수가 다를수도 있습니까?");
        }

        if (debugCount != (triangulation.areas.Length))
        {
            Debug.Assert(false, "삼각형 개수가 다를수도 있습니까?");
        }
    }

    private void PlaceOffMeshLinks()
    {
        //생성된 삼각형 전부를 대상으로 한다

        for (int i = 0; i < 1/*무언가의 정보 개수(점프 거리들)*/; i++)
        {
            foreach (var splittedNav in _splittedNavs)
            {
                foreach (var triangle in splittedNav._triangles)
                {
                    TriangleDesc currTriangle = triangle;

                    //각 변의 중점,수직한방향으로 위 스펙만큼 떨어져서 레이캐스팅 한다

                    //결과가 나온다면 

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
        //생성된 삼각형 전부를 대상으로 한다
    }

}
