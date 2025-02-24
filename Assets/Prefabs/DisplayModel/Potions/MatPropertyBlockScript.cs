using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MatPropertyBlockScript : MonoBehaviour
{
    [SerializeField] Color _color = Color.white;
    [SerializeField] private Renderer _renderer;
    private MaterialPropertyBlock _block;

    private void OnValidate()
    {
        if (_renderer == null)
        {
            return;
        }

        MatPropertyWork();
    }

    private void MatPropertyWork()
    {
        _renderer = GetComponent<Renderer>();
        _block = new MaterialPropertyBlock();

        _renderer.GetPropertyBlock(_block);
        _block.SetColor("_BaseColor", _color);
        _renderer.SetPropertyBlock(_block);
    }

    private void Awake()
    {
        if (_renderer == null)
        {
            Debug.Assert(false, "타겟 렌더러를 설정하세여");
            Debug.Break();
        }

        MatPropertyWork();
    }
}
