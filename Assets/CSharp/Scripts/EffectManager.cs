using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : SubManager<EffectManager>
{
    
    public class EffectCoroutineWrapper
    {
        public GameObject _createdEffect = null;
        public float _timeTarget = 0.0f;
        public float _time = 0.0f;
    }

    [Serializable]
    public class EffectWrapperInit
    {
        public string _name = "None";
        public GameObject _effectPrefab = null;
    }


    [SerializeField] private List<EffectWrapperInit> _effects_Init = new List<EffectWrapperInit>();
    private Dictionary<string, GameObject> _effects = new Dictionary<string, GameObject>();




    private void ReadyEffect()
    {
        foreach (var effect in _effects_Init) 
        {
            _effects.Add(effect._name, effect._effectPrefab);
        }
    }

    public void CreateEffect(string effectName, Vector3 dir, Vector3 postion)
    {
        if (_effects.ContainsKey(effectName) == false)
        {
            return;
        }

        GameObject createdEffect = Instantiate(_effects[effectName]);
        ParticleSystem particleSystem = createdEffect.GetComponent<ParticleSystem>();

        createdEffect.transform.position = postion;
        createdEffect.transform.rotation = Quaternion.LookRotation(dir);

        EffectCoroutineWrapper newWrapper = new EffectCoroutineWrapper();
        newWrapper._createdEffect = createdEffect;
        float maxDuration = Mathf.Max(particleSystem.main.duration, particleSystem.main.startLifetime.constantMax);
        newWrapper._timeTarget = maxDuration;
        StartCoroutine(EffectCoroutine(newWrapper));
    }

    private IEnumerator EffectCoroutine(EffectCoroutineWrapper wrapper)
    {
        while (true) 
        {
            wrapper._time += Time.deltaTime;

            if (wrapper._time >= wrapper._timeTarget)
            {
                Destroy(wrapper._createdEffect);
                break;
            }

            yield return null;
        }
    }


    public override void SubManagerFixedUpdate() {}
    public override void SubManagerInit() 
    {
        SingletonAwake();
        ReadyEffect();
    }
    public override void SubManagerLateUpdate() {}
    public override void SubManagerStart() {}
    public override void SubManagerUpdate() {}




}
