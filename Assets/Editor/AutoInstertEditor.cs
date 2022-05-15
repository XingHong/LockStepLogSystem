using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using System;

public class AutoInstertEditor
{
    private const string CODE_FILE_ROOT = "TestScript";
    private const string FUNCTION_REGEX = @"(public|private|protected)((\s+(static|override|virtual)*\s+)|\s+)\w+(<\w+>)*(\[\])*\s+\w+(<\w+>)*\s*\(([^\)]+\s*)?\)\s*\{[^\{\}]*(((?'Open'\{)[^\{\}]*)+((?'-Open'\})[^\{\}]*)+)*(?(Open)(?!))\}";
    private const string FIRSTLINE_REGEX = @"(public|private|protected)((\s+(static|override|virtual)*\s+)|\s+)\w+(<\w+>)*(\[\])*\s+\w+(<\w+>)*\s*\(([^\)]+\s*)?\)";
    private static Regex FunctionRegex;
    private static Regex FirstLineRegex;
    [MenuItem("AutoInsert/自动插入日志代码")]
    public static void InsertAction()
    {
        FunctionRegex = new Regex(FUNCTION_REGEX);
        FirstLineRegex = new Regex(FIRSTLINE_REGEX);
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
        string res = content;
        foreach (Match match in FunctionRegex.Matches(content))
        {
            string funContent = match.ToString();
            Match funNameMatch = FirstLineRegex.Match(funContent);
            string code = GetInsertCode(funNameMatch.ToString());
            res = InsertCodeToFirstLine(funContent, code);
        }
        Debug.Log(res);
    }

    private static string InsertCodeToFirstLine(string content, string code)
    {
        string res = string.Empty;
        int idx = content.IndexOf('{');
        string suffix = string.Empty;
        ++idx;
        while (idx < content.Length && (content[idx] == '\t' || content[idx] == ' ' || content[idx] == '\n' || content[idx] == '\r'))
        {
            suffix += content[idx];
            ++idx;
        }
        res = content.Insert(idx, code + suffix);
        return res;
    }

    private static string GetInsertCode(string firstLine)
    {
        string res = string.Empty;
        string funName = GetFunName(firstLine);
        List<ParamClass> list = GetParams(firstLine);
        res = "FSPDebuger.LogTrack(";
        res += "0,";
        for (int i = 0; i < list.Count; ++i)
        {
            ParamClass pc = list[i];
            res += pc.PValName + ",";
        }
        res = res.Remove(res.Length-1);
        res += ");";
        return res;
    }

    private static string GetFunName(string firstLine)
    {
        string funName = string.Empty;
        int idx = firstLine.IndexOf('(') - 1;
        while (idx >= 0 && firstLine[idx] != ' ')
        {
            funName += firstLine[idx];
            --idx;
        }
        char[] arr = funName.ToCharArray();
        Array.Reverse(arr);
        funName = new string(arr);

        GetParams(firstLine);
        return funName;
    }

    private static List<ParamClass> GetParams(string firstLine)
    {
        List<ParamClass> list = new List<ParamClass>();
        int idx = firstLine.IndexOf('(');
        int len = firstLine.Length - idx - 2;
        string paramsStr = firstLine.Substring(idx + 1, len);
        string[] paramsArr = paramsStr.Split(',');
        foreach(string p in paramsArr)
        {
            int cur = 0;
            while (cur < p.Length && p[cur] == ' ') ++cur;
            int start = cur;
            while (cur < p.Length && p[cur] != ' ') ++cur;
            string pType = p.Substring(start, cur - start);
            while (cur < p.Length && p[cur] == ' ') ++cur;
            string pValName = p.Substring(cur);
            if (pType == "int" || pType == "long"|| pType == "Fix64")
            {
                ParamClass pc = new ParamClass(pType, pValName);
                list.Add(pc);
            }
        }
        return list;
    }
}
