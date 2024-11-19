using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class CopyChildrenName : MonoBehaviour
{
    public GameObject targetObject; // 이름을 저장할 기준이 되는 게임 오브젝트
    public string fileName = "ChildNames.txt"; // 저장할 파일 이름

    private void Start()
    {
        // 경로 설정 (현재 실행 경로에 저장됨)
        string path = Path.Combine(Application.dataPath, fileName);

        // StringBuilder를 통해 텍스트를 작성
        StringBuilder content = new StringBuilder();

        // 깊이와 함께 이름 작성
        WriteChildNames(targetObject.transform, 0, content);

        // 파일 저장
        File.WriteAllText(path, content.ToString());

        Debug.Log($"Names saved to {path}");
    }

    private void WriteChildNames(Transform obj, int depth, StringBuilder content)
    {
        // Depth에 맞는 탭 추가 후 이름 작성
        content.AppendLine(new string('\t', depth) + obj.name);

        // 자식들에 대해 재귀적으로 이름 작성
        foreach (Transform child in obj)
        {
            WriteChildNames(child, depth + 1, content);
        }
    }
}
