using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public struct NavVerticalDesc
{
    public float _jumpDistances;
    public float _dropHeights;
}

[Serializable]
public struct NavVerticalDescWrapper
{
    public string _descName;
    public NavVerticalDesc _desc;
}

public class StaticNavMeshScript : MonoBehaviour
{
    /*--------------------------------------------------------------------------------------------------------------------------
    |TODO| �̰� �ϴ� ������ Jump, Drop �� �������� Bake �Ϸ��°���. ������ OffMesh���� ���� �߰��Ѵ��� �����ϴ� ����� �����غ���
    --------------------------------------------------------------------------------------------------------------------------*/
    [SerializeField] private List<int> _additionalAgents = new List<int>();
    private HashSet<int> _additionalAgentsSet = new HashSet<int>();

    private void Start()
    {
        NavigationManager navManager = NavigationManager.Instance;

        NavMeshSurface originalComponent = GetComponent<NavMeshSurface>();

        navManager.AddStaticNavMesh(gameObject, originalComponent);
    }

    private void Awake()
    {


        //foreach (var type in _additionalAgents)
        //{
        //    Debug.Assert(originalComponent.agentTypeID != type, "���� ������Ʈ�� �̹� �������ϴ�");
        //    Debug.Assert(_additionalAgentsSet.Contains(type) == false, "�ߺ����� �����մϴ�");

        //    NavMeshSurface newComponent = gameObject.AddComponent<NavMeshSurface>();

        //    CopyNavMeshSurfaceSettings(originalComponent, newComponent);

        //    newComponent.agentTypeID = NavMesh.GetSettingsByIndex(type).agentTypeID;

        //    newComponent.BuildNavMesh();

        //    navManager.AddStaticNavMesh(gameObject, newComponent);

        //    _additionalAgentsSet.Add(type);
        //}

    }


    private void CopyNavMeshSurfaceSettings(NavMeshSurface source, NavMeshSurface target)
    {
        // NavMeshSurface�� �ֿ� �Ӽ� ����
        target.agentTypeID = source.agentTypeID;
        target.collectObjects = source.collectObjects;
        target.layerMask = source.layerMask;
        target.useGeometry = source.useGeometry;
        target.defaultArea = source.defaultArea;
        target.ignoreNavMeshAgent = source.ignoreNavMeshAgent;
        target.ignoreNavMeshObstacle = source.ignoreNavMeshObstacle;
        target.overrideTileSize = source.overrideTileSize;
        target.tileSize = source.tileSize;
        target.overrideVoxelSize = source.overrideVoxelSize;
        target.voxelSize = source.voxelSize;
        target.buildHeightMesh = source.buildHeightMesh;
        target.center = source.center;
        target.size = source.size;
    }
}
