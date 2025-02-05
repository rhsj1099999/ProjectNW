using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using static StatScript;

public class BuffDisplayScript : MonoBehaviour
{
    public class BuffIconWrapper
    {
        public BuffIconWrapper(RuntimeBuffAsset buffAsset, int index, GameObject createdUI)
        {
            _buffAsset = buffAsset;
            _createdUI = createdUI;
            _myIndex = index;
        }
        public RuntimeBuffAsset _buffAsset = null;
        public GameObject _createdUI = null;
        public int _myIndex = -1;
    }

    [SerializeField] private GameObject _buffIconPrefab = null;

    private Dictionary<RuntimeBuffAsset, BuffIconWrapper> _cuffBuffs = new Dictionary<RuntimeBuffAsset, BuffIconWrapper>();
    private List<BuffIconWrapper> _createdUIList = new List<BuffIconWrapper>();
    private RectTransform _myReectTransform = null;

    private void Awake()
    {
        if (_buffIconPrefab == null)
        {
            Debug.Assert(false, "생성될 버프 아이콘 프리팹을 설정하세요");
            Debug.Break();
        }

        _myReectTransform = (RectTransform)transform;
    }


    public void RemoveBuff(RuntimeBuffAsset asset)
    {
        BuffIconWrapper buffIconWrapper = _cuffBuffs[asset];
        //없으면 안됩니다.

        int startIndex = buffIconWrapper._myIndex + 1;

        for (int i = startIndex; i < _createdUIList.Count; i++)
        {
            RectTransform iconRectTransform = (RectTransform)_buffIconPrefab.transform;
            Vector3 delta = new Vector3(iconRectTransform.rect.width, 0.0f, 0.0f);
            _createdUIList[i]._createdUI.transform.position -= delta;
            _createdUIList[i]._myIndex -= 1;
        }

        Destroy(buffIconWrapper._createdUI);

        _createdUIList.RemoveAt(buffIconWrapper._myIndex);

        _cuffBuffs.Remove(asset);
    }


    public void AddBuff(RuntimeBuffAsset asset)
    {
        if (_cuffBuffs.ContainsKey(asset) == true)
        {
            //이미 UI창에서 있는거다
            {
                //수량을 증가시키거나, 쉐이더 초기화 하거나 UI 로직을 작업한다
            }
        }
        else
        {
            GameObject createUIObject = Instantiate(_buffIconPrefab, transform);
            {
                //이미지 세팅
                {
                    BuffIconScript iconScript = createUIObject.GetComponent<BuffIconScript>();
                    iconScript.SetImage(asset._fromAsset._BuffUIImage);
                }

                //위치세팅
                {
                    RectTransform iconRectTransform = (RectTransform)createUIObject.transform;
                    Vector3 delta = new Vector3(iconRectTransform.rect.width * _cuffBuffs.Count, 0.0f, 0.0f);
                    iconRectTransform.position = _myReectTransform.position + delta;
                }
            }

            BuffIconWrapper newIconWrapper = new BuffIconWrapper(asset, _cuffBuffs.Count, createUIObject);
            _createdUIList.Add(newIconWrapper);
            _cuffBuffs.Add(asset, newIconWrapper);
        }
    }
}
