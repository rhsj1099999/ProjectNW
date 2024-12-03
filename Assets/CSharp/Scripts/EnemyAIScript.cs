using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAIScript : MonoBehaviour
{
    //StateSection
    [SerializeField] private float _detectRange = 3.0f;
    [SerializeField] private float _detectRayCastTick = 1.0f;
    [SerializeField] private float _detectHorizontalDeg = 60.0f;
    [SerializeField] private float _stateAcc = 0.0f;

    //--------------IdleState Vars
    [SerializeField] private float _idleStateTarget = 3.0f;
    //--------------PatrolState Vars
    [SerializeField] private float _patrolSpeedRatio = 0.5f;
    [SerializeField] private float _patrolRotateSpeedRatio = 0.5f;
    //--------------ChaseState Vars
    [SerializeField] private float _maxChasingDistance = 20.0f;
    [SerializeField] private float _chaseSpeedRatio = 2.0f;
    [SerializeField] private float _chaseRotateSpeedRatio = 2.0f;
    //--------------NormalAttack Vars
    [SerializeField] private float _normalAttackRange = 2.0f;
    //--------------Battle Vars
    private List<string> _enemyTags = new List<string>();
    [SerializeField] private GameObject _targetEnemy = null;



    private void Awake()
    {
        
    }
}
