using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class CopyChildrenName : MonoBehaviour
{
    public GameObject targetObject; // �̸��� ������ ������ �Ǵ� ���� ������Ʈ
    public string fileName = "ChildNames.txt"; // ������ ���� �̸�

    private void Start()
    {
        // ��� ���� (���� ���� ��ο� �����)
        string path = Path.Combine(Application.dataPath, fileName);

        // StringBuilder�� ���� �ؽ�Ʈ�� �ۼ�
        StringBuilder content = new StringBuilder();

        // ���̿� �Բ� �̸� �ۼ�
        WriteChildNames(targetObject.transform, 0, content);

        // ���� ����
        File.WriteAllText(path, content.ToString());

        Debug.Log($"Names saved to {path}");
    }

    private void WriteChildNames(Transform obj, int depth, StringBuilder content)
    {
        // Depth�� �´� �� �߰� �� �̸� �ۼ�
        content.AppendLine(new string('\t', depth) + obj.name);

        // �ڽĵ鿡 ���� ��������� �̸� �ۼ�
        foreach (Transform child in obj)
        {
            WriteChildNames(child, depth + 1, content);
        }
    }
}
