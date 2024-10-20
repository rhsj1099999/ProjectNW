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
    |TODO| 이걸 하는 이유가 Jump, Drop 의 종류별로 Bake 하려는거임. 툴만들어서 OffMesh따로 빨리 추가한다음 관리하는 방법을 생각해볼것
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
        //    Debug.Assert(originalComponent.agentTypeID != type, "원본 컴포넌트가 이미 구웠습니다");
        //    Debug.Assert(_additionalAgentsSet.Contains(type) == false, "중복값이 존재합니다");

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
        // NavMeshSurface의 주요 속성 복사
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
