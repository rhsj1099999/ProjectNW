//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class Functions : MonoBehaviour
//{
//    // Start is called before the first frame update
//    void Start()
//    {

//    }

//    // Update is called once per frame
//    void Update()
//    {

//    }
//}
public static class Functions
{
    public static void ForceCrash()
    {
        // ������ ���Ḧ �����ϴ� ����� �� �ϳ�
        // 1. NullReferenceException�� �߻����� ũ���� ����
        string crash = null;
        crash.ToString(); // NullReferenceException

        // 2. ���� ����
        // Application.Quit();

        // 3. �ɰ��� �ý��� ���� �߻�
        // System.Environment.FailFast("Force crash.");
    }
}
