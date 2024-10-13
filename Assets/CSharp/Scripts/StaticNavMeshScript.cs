using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticNavMeshScript : MonoBehaviour
{
    private void Awake()
    {
        NavigationManager navManager = NavigationManager.Instance;

        navManager.AddStaticNavMesh(gameObject);
    }
}
