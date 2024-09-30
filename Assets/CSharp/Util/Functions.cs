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
        // 비정상 종료를 유도하는 방법들 중 하나
        // 1. NullReferenceException을 발생시켜 크래시 유도
        string crash = null;
        crash.ToString(); // NullReferenceException

        // 2. 강제 종료
        // Application.Quit();

        // 3. 심각한 시스템 오류 발생
        // System.Environment.FailFast("Force crash.");
    }
}
