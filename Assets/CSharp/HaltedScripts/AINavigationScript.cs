using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

/*-------------------------------------------
 * |Halted|----------------------------------
-------------------------------------------*/

public class AINavigationScript : MonoBehaviour
{
}

//public class AINavigationScript : MonoBehaviour
//{
//    private const float _navmeshHitRadius = 1.0f;


//    private int _filterTarget = -1;
//    private float _minOffMeshSampleRadius = 0.05f;


    
    


//    private NavMeshHit _navMeshHit;
//    private NavMeshHit _navMeshHitFindOffMesh;

//    private int _targetPathPositionIndex = 0;
//    private NavMeshPath _navMeshPath = null;
//    private List<bool> _isOffMeshLinks = new List<bool>();
//    public NavMeshPath GetNavMeshPath() { return _navMeshPath; }
//    public List<bool> GetIsOffMeshLinkes() { return _isOffMeshLinks; }

//    [SerializeField] private GameObject _debuggingCornerSpherePrefab = null;
//    [SerializeField] private GameObject _debuggingCornerCapsulePrefab = null;
//    private List<GameObject> _createdDebuggingCornerSphere = new List<GameObject>();
//    private List<GameObject> _createdDebuggingCornerCapsule = new List<GameObject>();
//    private List<GameObject> _createdDebuggingCornerSphereReverse = new List<GameObject>();
//    private List<GameObject> _createdDebuggingCornerCapsuleReverse = new List<GameObject>();



//    private void Awake()
//    {
//        _navMeshPath = new NavMeshPath(); //��� �����̺��� �����ϸ� �ȵȴܴ�.
//    }



//    public void FindPath_Sphere(Vector3 startPosition, Vector3 targetPosition, float Radius)
//    {
//        _targetPathPositionIndex = 0;

//        NavMeshSurface shortestNav = NavigationManager.Instance.GetCurrStageNavMeshByPosition(transform.position);

//        NavigationManager.Instance.DeActiveAllNavMesh();

//        NavigationManager.Instance.ActiveNavMesh(shortestNav);

//        if (shortestNav == null)
//        {
//            Debug.Assert(shortestNav != null, "�ʿ� �׺���̼��� �������� �ֽ��ϴ�. ���� �ش������ ó������ �ʾҽ��ϴ�");
//        }

//        Vector3 navPosition = shortestNav.transform.position;

//        NavMeshQueryFilter filter = new NavMeshQueryFilter();
//        if (_filterTarget == -1)
//        {
//            filter.areaMask = NavMesh.AllAreas;
//        }
//        else
//        {
//            filter.areaMask = 1 << _filterTarget;
//        }

//        NavMesh.SamplePosition(targetPosition, out _navMeshHit, _navmeshHitRadius, filter);

//        FindPath(startPosition, _navMeshHit.position, filter);

//        switch (_navMeshPath.status)
//        {
//            case NavMeshPathStatus.PathComplete:
//                break;

//            case NavMeshPathStatus.PathPartial: //�Ųٷ� ����� �ѹ� �� ã�ƺ���
//                {
//                    NavMeshPath temp = _navMeshPath;
//                    FindPath(_navMeshHit.position, startPosition, filter);

//                    switch (_navMeshPath.status)
//                    {
//                        case NavMeshPathStatus.PathComplete:
//                            System.Array.Reverse(_navMeshPath.corners);
//                            break;

//                        case NavMeshPathStatus.PathPartial:
//                            break;

//                        case NavMeshPathStatus.PathInvalid:

//                        default:
//                            break;
//                    }
//                }
//                break;

//            case NavMeshPathStatus.PathInvalid:

//            default:
//                break;
//        }

//        CalculateOffMeshLinks();

//        //CreateDebugRoute();
//    }





//    private void CalculateOffMeshLinks()
//    {
//        _isOffMeshLinks.Clear();

//        _isOffMeshLinks.Add(false); //��ã�⸦ ������ ����, ���� ������ ������ ���� ����ִٰ� �����մϴ�.

//        for (int i = 1; i < _navMeshPath.corners.Length - 1; i++)
//        {
//            Vector3 startPosition = _navMeshPath.corners[i];
//            Vector3 endPosition = _navMeshPath.corners[i + 1];
//            Vector3 betweenPosition = (startPosition + endPosition) / 2.0f;

//            bool samplingSuccess = NavMesh.SamplePosition(betweenPosition, out _navMeshHitFindOffMesh, _minOffMeshSampleRadius, 1);

//            if (samplingSuccess == true) //�̾����ִ� �޽��Դϴ�
//            {
//                _isOffMeshLinks.Add(false);
//                continue;
//            }

//            /*--------------------------------------------------------------
//            |TODO| �������Ϸ� ������ �� �ִ� �������� �Ǻ��ϴ� ���� �����غ���
//            --------------------------------------------------------------*/
//            //if (false /*�������Ϸ� ������ �� �ֽ��ϱ�?*/) //�������� �ʾƵ� ������ �� �ֽ��ϴ�.
//            //{
//            //    _isOffMeshLinks.Add(false);
//            //    continue;
//            //}


//            //�� ���� OffMeshLink�̱⿡ 2���� �߰��մϴ� //true true true 3���� true�� ������ ���ٰ� �����մϴ�. �׳� �ڳʰ� �׷�
//            _isOffMeshLinks.Add(true); _isOffMeshLinks.Add(true);
//            i++;
//        }

//        _isOffMeshLinks.Add(false); //��ã�⸦ ���� ������ ������ ���� ����ִٰ� �����մϴ�.
//    }





//    private void FindPath(Vector3 startPosition, Vector3 endPosition, NavMeshQueryFilter filter)
//    {
//        _targetPathPositionIndex = 0;

//        NavMeshSurface shortestNav = NavigationManager.Instance.GetCurrStageNavMeshByPosition(transform.position);

//        NavigationManager.Instance.DeActiveAllNavMesh();

//        NavigationManager.Instance.ActiveNavMesh(shortestNav);

//        if (shortestNav == null)
//        {
//            Debug.Assert(shortestNav != null, "�ʿ� �׺���̼��� �������� �ֽ��ϴ�. ���� �ش������ ó������ �ʾҽ��ϴ�");
//            return;
//        }

//        NavMesh.SamplePosition(endPosition, out _navMeshHit, 1.0f, filter);

//        NavMesh.CalculatePath(startPosition, endPosition, filter, _navMeshPath);
//    }





//    /*-----------------------------------
//    * Debugging Section
//   -----------------------------------*/
//    //private void FindAndDrawPathOnlyJump()
//    //{
//    //    NavMeshQueryFilter filter = new NavMeshQueryFilter();
//    //    if (_filterTarget == -1)
//    //    {
//    //        filter.areaMask = NavMesh.AllAreas;
//    //    }
//    //    else
//    //    {
//    //        filter.areaMask = 1 << _filterTarget;
//    //    }

//    //    NavMeshSurface shortestNav = NavigationManager.Instance.GetCurrStageNavMeshByPosition(transform.position);

//    //    NavigationManager.Instance.DeActiveAllNavMesh();

//    //    NavigationManager.Instance.ActiveNavMesh(shortestNav);

//    //    Vector3 randomedPosition = _debuggingPlayer.transform.position;


//    //    NavMesh.SamplePosition(randomedPosition, out _navMeshHit, 30.0f, filter);

//    //    Vector3 patrolPosition = _navMeshHit.position;
//    //    NavMesh.CalculatePath(transform.position, patrolPosition, filter, _navMeshPath);

//    //    DebugLog_PathFindingResult(true);
//    //}

//    //private void FindClosetEdge()
//    //{
//    //    NavMeshQueryFilter filter = new NavMeshQueryFilter();
//    //    if (_filterTarget == -1)
//    //    {
//    //        filter.areaMask = NavMesh.AllAreas;
//    //    }
//    //    else
//    //    {
//    //        filter.areaMask = 1 << _filterTarget;
//    //    }

//    //    NavMeshSurface shortestNav = NavigationManager.Instance.GetCurrStageNavMeshByPosition(transform.position);

//    //    NavigationManager.Instance.DeActiveAllNavMesh();

//    //    NavigationManager.Instance.ActiveNavMesh(shortestNav);

//    //    NavMesh.FindClosestEdge(transform.position, out _navMeshHit, filter);


//    //    GameObject createdSphere = Instantiate(_debuggingCornerSpherePrefab);
//    //    createdSphere.transform.position = _navMeshHit.position;
//    //    _createdDebuggingCornerSphere.Add(createdSphere);
//    //}

//    //private void CalculateTriangulation()
//    //{
//    //    NavMeshSurface shortestNav = NavigationManager.Instance.GetCurrStageNavMeshByPosition(transform.position);

//    //    NavigationManager.Instance.DeActiveAllNavMesh();

//    //    NavigationManager.Instance.ActiveNavMesh(shortestNav);

//    //    NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
//    //}

//    //private void FindAndDrawPathReverse()
//    //{
//    //    NavMeshQueryFilter filter = new NavMeshQueryFilter();
//    //    if (_filterTarget == -1)
//    //    {
//    //        filter.areaMask = NavMesh.AllAreas;
//    //    }
//    //    else
//    //    {
//    //        filter.areaMask = 1 << _filterTarget;
//    //    }

//    //    NavMeshSurface shortestNav = NavigationManager.Instance.GetCurrStageNavMeshByPosition(transform.position);

//    //    NavigationManager.Instance.DeActiveAllNavMesh();

//    //    NavigationManager.Instance.ActiveNavMesh(shortestNav);

//    //    Vector3 randomedPosition = transform.position;

//    //    NavMesh.SamplePosition(randomedPosition, out _navMeshHit, 1.0f, filter);

//    //    Vector3 patrolPosition = _navMeshHit.position;

//    //    NavMesh.CalculatePath(_debuggingPlayer.transform.position, patrolPosition, filter, _navMeshPath);

//    //    DebugLog_PathFindingResult(true, true);
//    //}

//    //private void DebugLog_PathFindingResult(bool drawRoute = false, bool color = false)
//    //{
//    //    if (_navMeshPath.status == NavMeshPathStatus.PathInvalid)
//    //    {
//    //        Debug.Log("���ã�� ����");
//    //        return;
//    //    }

//    //    if (_navMeshPath.status == NavMeshPathStatus.PathComplete)
//    //    {
//    //        Debug.Log("���ã�� ����");
//    //    }

//    //    if (_navMeshPath.status == NavMeshPathStatus.PathPartial)
//    //    {
//    //        Debug.Log("���ã�� �ɰ���");
//    //    }

//    //    if (drawRoute == true)
//    //    {
//    //        CreateDebugRoute(color);
//    //    }
//    //}

//    //private void FindAndDrawPath()
//    //{
//    //    NavMeshQueryFilter filter = new NavMeshQueryFilter();
//    //    if (_filterTarget == -1)
//    //    {
//    //        filter.areaMask = NavMesh.AllAreas;
//    //    }
//    //    else
//    //    {
//    //        filter.areaMask = 1 << _filterTarget;
//    //    }


//    //    //��ΰ�� -> ���������� �� ��ġ
//    //    NavMeshSurface shortestNav = NavigationManager.Instance.GetCurrStageNavMeshByPosition(transform.position);

//    //    NavigationManager.Instance.DeActiveAllNavMesh();

//    //    NavigationManager.Instance.ActiveNavMesh(shortestNav);

//    //    Vector3 randomedPosition = _debuggingPlayer.transform.position;

//    //    NavMesh.SamplePosition(randomedPosition, out _navMeshHit, 2.0f, filter);

//    //    Vector3 patrolPosition = _navMeshHit.position;

//    //    NavMesh.CalculatePath(transform.position, patrolPosition, filter, _navMeshPath);

//    //    DebugLog_PathFindingResult(true);
//    //}

//    private void CreateDebugRoute(bool color = false)
//    {
//        for (int i = 0; i < _navMeshPath.corners.Length; i++)
//        {
//            GameObject createdSphere = Instantiate(_debuggingCornerSpherePrefab);
//            createdSphere.transform.position = _navMeshPath.corners[i];
//            _createdDebuggingCornerSphere.Add(createdSphere);
//            MeshRenderer meshRenderer = createdSphere.GetComponent<MeshRenderer>();
//            if (color == true)
//            {
//                meshRenderer.material.color = Color.yellow;
//            }


//            if (i == _navMeshPath.corners.Length - 1)
//            {
//                break;
//            }

//            GameObject createdCapsule = Instantiate(_debuggingCornerCapsulePrefab);
//            Vector3 firstPosition = _navMeshPath.corners[i];
//            Vector3 secondPosition = _navMeshPath.corners[i + 1];
//            createdCapsule.transform.position = (firstPosition + secondPosition) / 2.0f;

//            Vector3 targetUp = (secondPosition - firstPosition).normalized;
//            createdCapsule.transform.up = targetUp;
//            Vector3 localScale = createdCapsule.transform.localScale;
//            localScale.y = Vector3.Distance(firstPosition, secondPosition) / 2.0f;
//            createdCapsule.transform.localScale = localScale;
//            _createdDebuggingCornerCapsule.Add(createdCapsule);
//            meshRenderer = createdCapsule.GetComponent<MeshRenderer>();
//            if (color == true)
//            {
//                meshRenderer.material.color = Color.yellow;
//            }
//        }
//    }

//    private void DeletePath()
//    {
//        foreach (var item in _createdDebuggingCornerSphere)
//        {
//            Destroy(item);
//        }
//        foreach (var item in _createdDebuggingCornerCapsule)
//        {
//            Destroy(item);
//        }
//        foreach (var item in _createdDebuggingCornerSphereReverse)
//        {
//            Destroy(item);
//        }
//        foreach (var item in _createdDebuggingCornerCapsuleReverse)
//        {
//            Destroy(item);
//        }

//        _createdDebuggingCornerSphere.Clear();
//        _createdDebuggingCornerCapsule.Clear();
//        _createdDebuggingCornerSphereReverse.Clear();
//        _createdDebuggingCornerCapsuleReverse.Clear();
//    }
//}
