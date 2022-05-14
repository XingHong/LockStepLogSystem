using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

public class AutoInstertEditor
{
    private const string CODE_FILE_ROOT = "TestScript";
    [MenuItem("AutoInsert/自动插入日志代码")]
    public static void InsertAction()
    {
        string filePath = Application.dataPath + "/" + CODE_FILE_ROOT;
        DirectoryInfo root = new DirectoryInfo(filePath);
        List<FileInfo> fileInfoList = new List<FileInfo>();
        ForeachDir(fileInfoList, root);

        foreach (FileInfo file in fileInfoList)
        {
            string content = ReadFileContent(file.FullName);
            IntertLogCode(content, "");
            break;
        }
    }

    /// <summary>
    /// 遍历当前文件夹下的cs文件
    /// </summary>
    /// <param name="list"></param>
    /// <param name="root"></param>
    private static void ForeachDir(List<FileInfo> list, DirectoryInfo root)
    {
        list.AddRange(root.GetFiles("*.cs"));
        foreach (DirectoryInfo cur in root.GetDirectories())
        {
            ForeachDir(list, cur);
        }
    }

    private static string ReadFileContent(string path)
    {
        string content = string.Empty;
        using (StreamReader sr = new StreamReader(path))
        {
            content = sr.ReadToEnd();
        }
        return content;
    }

    private static void IntertLogCode(string content, string path)
    {
        Regex regex = new Regex(@"(public|private|protected)((\s+(static|override|virtual)*\s+)|\s+)\w+(<\w+>)*(\[\])*\s+\w+(<\w+>)*\s*\(([^\)]+\s*)?\)\s*\{[^\{\}]*(((?'Open'\{)[^\{\}]*)+((?'-Open'\})[^\{\}]*)+)*(?(Open)(?!))\}");
        foreach (Match match in regex.Matches(content))
        {
            Debug.Log(match.Value);
        }
    }
}
