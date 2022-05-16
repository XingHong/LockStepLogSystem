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
    //函数的第一行
    private const string FUNCTION_HEAD_REGEX = @"(public|private|protected)((\s+(static|override|virtual)*\s+)|\s+)\w+(<\w+>)*(\[\])*\s+\w+(<\w+>)*\s*\(([^\)]+\s*)?\)";
    private const string LEFT_BRACE_REGEX = @"{";
    private const string FIRST_CODE_REGEX = @"\S+(.)*";
    private const string LOG_TRACK_CODE_REGEX = @"FSPDebuger.LogTrack((.)*)";
    private const string LOG_TRACK_CODE_IGNORE_REGEX = @"FSPDebuger.IgnoreTrack()";

    private static Regex FunctionRegex;
    private static Regex FunctionHeadRegex;
    private static Regex LeftBraceRegex;
    private static Regex FirstCodeRegex;
    private static Regex LogTrackCodeRegex;
    private static Regex LogTrackCodeIgnoreRegex;

    [MenuItem("AutoInsert/自动插入日志代码")]
    public static void InsertAction()
    {
        FunctionRegex = new Regex(FUNCTION_REGEX);
        FunctionHeadRegex = new Regex(FUNCTION_HEAD_REGEX);
        LeftBraceRegex = new Regex(LEFT_BRACE_REGEX);
        FirstCodeRegex = new Regex(FIRST_CODE_REGEX);
        LogTrackCodeRegex = new Regex(LOG_TRACK_CODE_REGEX);
        LogTrackCodeIgnoreRegex = new Regex(LOG_TRACK_CODE_IGNORE_REGEX);

        string filePath = Application.dataPath + "/" + CODE_FILE_ROOT;
        DirectoryInfo root = new DirectoryInfo(filePath);
        List<FileInfo> fileInfoList = new List<FileInfo>();
        ForeachDir(fileInfoList, root);

        foreach (FileInfo file in fileInfoList)
        {
            IntertLogTrackCode(file.FullName);
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

    private static void IntertLogTrackCode(string path)
    {
        if (!File.Exists(path))
            return;
        string content = File.ReadAllText(path);
        string res = content;
        var matchs = FunctionRegex.Matches(content);
        int cnt = matchs.Count;
        for(int i = cnt - 1; i >= 0; --i)
        {
            var match = matchs[i];
            res = InsertCodeToFirstLine(match, res);
        }
        Debug.Log(res);
    }

    private static string InsertCodeToFirstLine(Match matchFunc, string content)
    {
        bool hasChange = false;
        Match mathcLeftBrace = LeftBraceRegex.Match(content, matchFunc.Index, matchFunc.Length);
        if (mathcLeftBrace.Success)
        {
            int len = matchFunc.Index + matchFunc.Length - (mathcLeftBrace.Index + mathcLeftBrace.Length);
            Match matchFirstCode = FirstCodeRegex.Match(content, mathcLeftBrace.Index + mathcLeftBrace.Length, len);
            if (!LogTrackCodeRegex.IsMatch(matchFirstCode.Value))
            {
                if (!LogTrackCodeIgnoreRegex.IsMatch(matchFirstCode.Value))
                {
                    Match mathcFunHead = FunctionHeadRegex.Match(content, matchFunc.Index, matchFunc.Length);
                    string code = GetInsertCode(mathcFunHead.ToString());
                    content = content.Insert(mathcLeftBrace.Index + mathcLeftBrace.Length, code);
                    hasChange = true;
                }
            }
        }
        if (hasChange)
        {
            Debug.Log(content);
        }
        return content;
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
