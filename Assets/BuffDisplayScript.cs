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

    private Dictionary<BuffAsset, BuffIconWrapper> _cuffBuffs = new Dictionary<BuffAsset, BuffIconWrapper>();
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
        BuffIconWrapper buffIconWrapper = null;
        _cuffBuffs.TryGetValue(asset._fromAsset, out buffIconWrapper);
        if (buffIconWrapper == null) 
        {
            Debug.Assert(false, "�ش� ������ ã���� ������ " + asset._fromAsset.name);
            Debug.Break();
            return;
        }
        //������ �ȵ˴ϴ�.

        int startIndex = buffIconWrapper._myIndex + 1;

        for (int i = startIndex; i < _createdUIList.Count; i++)
        {
            RectTransform iconRectTransform = (RectTransform)_buffIconPrefab.transform;
            Vector3 delta = new Vector3(iconRectTransform.rect.width, 0.0f, 0.0f);
            Vector2 delta_Vector2 = new Vector2(iconRectTransform.rect.width, 0.0f);

            ((RectTransform)_createdUIList[i]._createdUI.transform).anchoredPosition -= delta_Vector2;
            _createdUIList[i]._myIndex -= 1;
        }

        Destroy(buffIconWrapper._createdUI);

        _createdUIList.RemoveAt(buffIconWrapper._myIndex);

        _cuffBuffs.Remove(asset._fromAsset);
    }


    public void AddBuff(RuntimeBuffAsset asset)
    {
        if (_cuffBuffs.ContainsKey(asset._fromAsset) == true)
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
                    iconScript.SetImage(asset._fromAsset._BuffUIImage, asset);
                }

                //��ġ����
                {
                    RectTransform iconRectTransform = (RectTransform)createUIObject.transform;
                    Vector3 delta = new Vector3(iconRectTransform.rect.width * _cuffBuffs.Count, 0.0f, 0.0f);
                    Vector2 delta_Vector2 = new Vector2(iconRectTransform.rect.width * _cuffBuffs.Count, 0.0f);
                    iconRectTransform.anchoredPosition = _myReectTransform.anchoredPosition + delta_Vector2;
                }
            }

            BuffIconWrapper newIconWrapper = new BuffIconWrapper(asset, _cuffBuffs.Count, createUIObject);
            _createdUIList.Add(newIconWrapper);
            _cuffBuffs.Add(asset._fromAsset, newIconWrapper);
        }
    }
}
