using System;
using System.Collections;
using UnityEngine;


public class ZombieScript : CharacterScript, IHitable
{
    [SerializeField] private BattleUIScript _battleUIInstanced = null;
    [SerializeField] private float _battleUICounterTarget = 5.0f;
    [SerializeField] private float _battleUICounterACC = 0.0f;
    private Coroutine _battleUICoroutine = null;

    protected override void Awake()
    {
        base.Awake();

        if (_battleUIInstanced == null)
        {
            Debug.Assert(false, "전투중 표시될 UI 프리팹을 설정하세요");
            Debug.Break();
        }
    }

    public override LayerMask CalculateWeaponColliderIncludeLayerMask()
    {
        int ret = LayerMask.GetMask("Player");
        return ret;
    }

    public override void DeadCall()
    {
        base.DeadCall();

        int dropItemCount = UnityEngine.Random.Range(2, 5);

        if (dropItemCount == 0) 
        {
            return;
        }

        for (int i = 0; i < dropItemCount; i++)
        {
            ItemAsset randomItemAsset = ItemInfoManager.Instance.GetItemInfo(UnityEngine.Random.Range(0, ItemInfoManager.Instance.GetMaxItemCount()));
            ItemStoreDescBase newStoreDescBase = ItemInfoManager.Instance.CreateItemStoreDesc(randomItemAsset, 1, 0, false, null);

            int randomDeg_X = UnityEngine.Random.Range(0, 90);
            int randomDeg_Y = UnityEngine.Random.Range(0, 360);

            Vector3 dir = Vector3.up;
            Quaternion itemThrowRotation = Quaternion.Euler(randomDeg_X, randomDeg_Y, 0);
            dir = itemThrowRotation * dir;

            int randomForce = UnityEngine.Random.Range(2, 4);
            dir *= randomForce;

            ItemInfoManager.Instance.DropItemToField(transform, newStoreDescBase, dir);
        }
    }

    public override void DealMe_Final(DamageDesc damage, bool isWeakPoint, CharacterScript attacker, CharacterScript victim, ref Vector3 closetPoint, ref Vector3 hitNormal)
    {
        base.DealMe_Final(damage, isWeakPoint, attacker, victim, ref closetPoint, ref hitNormal);

        AfterDealMe();
    }

    public override void AfterDealMe()
    {
        if (_dead == false)
        {
            if (_battleUICoroutine == null)
            {
                _battleUICoroutine = StartCoroutine(ShowBattleUICoroutine());
            }
            else
            {
                _battleUICounterACC = 0.0f;
                UIManager.Instance.SetMeFinalZOrder(_battleUIInstanced.gameObject, UIManager.LayerOrder.EnemyInBattle);
            }
        }
    }

    protected override void ZeroHPCall(CharacterScript killedBy)
    {
        base.ZeroHPCall(killedBy);

        if (_battleUICoroutine != null)
        {
            StopCoroutine(_battleUICoroutine);
            TurnOffBattleUI();
        }
    }

    public override void YouKillThisObject(GameObject killObject)
    {
        TurnOffBattleUI();
    }

    private IEnumerator ShowBattleUICoroutine()
    {
        _battleUICounterACC = 0.0f;
        UIManager.Instance.TurnOnUI(_battleUIInstanced.gameObject, UIManager.LayerOrder.EnemyInBattle);

        while (true) 
        {
            _battleUICounterACC += Time.deltaTime;

            if (_battleUICounterACC >= _battleUICounterTarget)
            {
                TurnOffBattleUI();
                break;
            }

            yield return null;
        }
    }

    private void TurnOffBattleUI()
    {
        UIManager.Instance.TurnOffUI(_battleUIInstanced.gameObject);
        _battleUICoroutine = null;
    }
}
