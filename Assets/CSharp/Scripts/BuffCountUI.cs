using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BuffCountUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textMeshPro = null;

    public void SetCount(int count)
    {
        if (count <= 1)
        {
            _textMeshPro.enabled = false;
            return;
        }

        _textMeshPro.enabled = true;
        _textMeshPro.text = count.ToString();
    }
}