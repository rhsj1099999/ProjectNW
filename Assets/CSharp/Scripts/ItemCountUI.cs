using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemCountUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textMeshPro = null;

    public void SetCount(int count)
    {
        _textMeshPro.text = count.ToString();
    }
}
