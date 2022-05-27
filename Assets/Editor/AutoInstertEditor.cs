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
    private const string LOG_TRACK_CODE_REGEX = @"FSPDebuger.LogTrack((.)*);";
    private const string LOG_TRACK_CODE_IGNORE_REGEX = @"FSPDebuger.IgnoreTrack();";
    private const string NUMBER_REGEX = @"\d+";
    private const string DEBUG_STR_REGEX = @"(/#(.)*#/)|(/\*(.)*\*/)";
    private static Regex ms_regexFunction;
    private static Regex ms_regexFunctionHead;
    private static Regex ms_regexLeftBrace;
    private static Regex ms_regexFirstCode;
    private static Regex ms_regexLogTrackCode;
    private static Regex ms_regexLogTrackCodeIgnore;
    private static Regex ms_regexNumber;
    private static Regex ms_regexDebugStr;

    private static string ms_basePath;

    [MenuItem("AutoInsert/自动插入日志代码")]
    public static void InsertAction()
    {
        ms_regexFunction = new Regex(FUNCTION_REGEX);
        ms_regexFunctionHead = new Regex(FUNCTION_HEAD_REGEX);
        ms_regexLeftBrace = new Regex(LEFT_BRACE_REGEX);
        ms_regexFirstCode = new Regex(FIRST_CODE_REGEX);
        ms_regexLogTrackCode = new Regex(LOG_TRACK_CODE_REGEX);
        ms_regexLogTrackCodeIgnore = new Regex(LOG_TRACK_CODE_IGNORE_REGEX);
        ms_regexNumber = new Regex(NUMBER_REGEX);
        ms_regexDebugStr = new Regex(DEBUG_STR_REGEX);

        ms_basePath = Path.Combine(Application.dataPath, CODE_FILE_ROOT);
        ms_basePath = ms_basePath.Replace("/", "\\");

        DirectoryInfo root = new DirectoryInfo(ms_basePath);
        List<FileInfo> fileInfoList = new List<FileInfo>();
        ForeachDir(fileInfoList, root);

        foreach (FileInfo file in fileInfoList)
        {
            IntertLogTrackCode(file.FullName);
        }

        foreach (FileInfo file in fileInfoList)
        {
            HandleLogTrack(file.FullName);
        }

        LogTrackPdbFile pdb = new LogTrackPdbFile();
        foreach (FileInfo file in fileInfoList)
        {
            HashLogTrackCode(file.FullName, pdb);
        }
        pdb.SavePdb();
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
        var matchs = ms_regexFunction.Matches(content);
        int cnt = matchs.Count;
        bool hasChange = false;
        for (int i = cnt - 1; i >= 0; --i)
        {
            var match = matchs[i];
            res = InsertCodeToFirstLine(match, res, ref hasChange);
        }
        if (hasChange)
        { 
            File.WriteAllText(path, res);
        }
    }

    private static string InsertCodeToFirstLine(Match matchFunc, string content, ref bool hasChange)
    {
        Match mathcLeftBrace = ms_regexLeftBrace.Match(content, matchFunc.Index, matchFunc.Length);
        if (mathcLeftBrace.Success)
        {
            int len = matchFunc.Index + matchFunc.Length - (mathcLeftBrace.Index + mathcLeftBrace.Length);
            Match matchFirstCode = ms_regexFirstCode.Match(content, mathcLeftBrace.Index + mathcLeftBrace.Length, len);
            if (!ms_regexLogTrackCode.IsMatch(matchFirstCode.Value))
            {
                if (!ms_regexLogTrackCodeIgnore.IsMatch(matchFirstCode.Value))
                {
                    Match mathcFunHead = ms_regexFunctionHead.Match(content, matchFunc.Index, matchFunc.Length);
                    string code = GetInsertCode(mathcFunHead.ToString());
                    content = content.Insert(mathcLeftBrace.Index + mathcLeftBrace.Length, code);
                    hasChange = true;
                }
            }
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
        res += $"/#{funName}#/";
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
        return funName;
    }

    private static List<ParamClass> GetParams(string firstLine)
    {
        List<ParamClass> list = new List<ParamClass>();
        int idx = firstLine.IndexOf('(');
        int len = firstLine.Length - idx - 2;
        string paramsStr = firstLine.Substring(idx + 1, len);
        string[] paramsArr = paramsStr.Split(',');
        foreach(string str in paramsArr)
        {
            int pos = 0;
            string pType = GetFuncType(str, ref pos);
            if (pType == "int" || pType == "long"|| pType == "Fix64")
            {
                string pValName = GetParamName(str, ref pos);
                ParamClass pc = new ParamClass(pType, pValName);
                list.Add(pc);
            }
        }
        return list;
    }

    private static string GetFuncType(string str, ref int pos)
    {
        while (pos < str.Length && str[pos] == ' ') ++pos;
        int start = pos;
        while (pos < str.Length && str[pos] != ' ') ++pos;
        string res = str.Substring(start, pos - start);
        if(res == "ref" || res == "out")
            res = "invaild";
        return res;
    }

    private static string GetParamName(string str, ref int pos)
    {
        while (pos < str.Length && str[pos] == ' ') ++pos;
        int start = pos;
        while (pos < str.Length && str[pos] != ' ' && str[pos] != ')' && str[pos] != ',' && str[pos] != '=') ++pos;
        string res = str.Substring(start, pos - start);
        return res;
    }

    private static void HandleLogTrack(string path)
    {

    }

    private static void HashLogTrackCode(string path, LogTrackPdbFile pdb)
    {
        bool hasChanged = false;
        if (!File.Exists(path))
            return;
        var lines = File.ReadAllLines(path);
        var subPath = Path.GetFileNameWithoutExtension(path);
        for (int i = 0; i < lines.Length; ++i)
        {
            var line = lines[i];
            var matchLogCode = ms_regexLogTrackCode.Match(line);
            if (matchLogCode.Success)
            {
                var matchLogHash = ms_regexNumber.Match(line, matchLogCode.Index, matchLogCode.Length);
                if (matchLogHash.Success)
                {
                    int hash = 0;
                    int.TryParse(matchLogHash.Value, out hash);
                    int argCnt = GetLogTrackArgCnt(matchLogCode.Value);
                    //寻找可能的注释
                    var dbgStr = GetLogTrackDebguString(ref line, matchLogCode.Index + matchLogCode.Length);
                    int vaildHash = pdb.AddItem(hash, argCnt, subPath, i + 1, dbgStr);
                    if (hash != vaildHash)
                    {
                        line = line.Remove(matchLogHash.Index, matchLogHash.Length);
                        line = line.Insert(matchLogHash.Index, vaildHash.ToString());

                        lines[i] = line;
                        hasChanged = true;
                    }
                }
            }
        }
        if (hasChanged)
        {
            File.WriteAllLines(path, lines);
        }
    }

    private static int GetLogTrackArgCnt(string str)
    {
        var tmp = str.Split(',');
        return tmp.Length - 1;
    }

    private static string GetLogTrackDebguString(ref string line, int index)
    {
        var res = ms_regexDebugStr.Match(line, index);
        if (!line.Contains("/*"))
        { 
            line = line.Substring(0, index);
        }
        int len = res.Value.Length;
        if (res.Success)
        { 
            return res.Value.Substring(2, len - 4);
        }
        return string.Empty;
    }
}
