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
            Debug.Assert(false, "������ ���� ������ �������� �����ϼ���");
            Debug.Break();
        }

        _myReectTransform = (RectTransform)transform;
    }


    public void RemoveBuff(RuntimeBuffAsset asset)
    {
        BuffIconWrapper buffIconWrapper = _cuffBuffs[asset];
        //������ �ȵ˴ϴ�.

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
            //�̹� UIâ���� �ִ°Ŵ�
            {
                //������ ������Ű�ų�, ���̴� �ʱ�ȭ �ϰų� UI ������ �۾��Ѵ�
            }
        }
        else
        {
            GameObject createUIObject = Instantiate(_buffIconPrefab, transform);
            {
                //�̹��� ����
                {
                    BuffIconScript iconScript = createUIObject.GetComponent<BuffIconScript>();
                    iconScript.SetImage(asset._fromAsset._BuffUIImage);
                }

                //��ġ����
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
