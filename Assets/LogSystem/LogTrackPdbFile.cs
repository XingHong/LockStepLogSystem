using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class LogTrackPdbFile
{
    private string m_pdbPath;
    private int m_hashId;
    private HashSet<int> m_oldTableId;
    private HashSet<int> m_updateTableId;
    private Dictionary<int, LogTrackInfoItem> m_LogDict;
    public LogTrackPdbFile()
    {
        m_pdbPath = Application.dataPath + "/LogTrackPdb.pdb";
        m_hashId = 0;
        m_oldTableId = new HashSet<int>();
        m_updateTableId = new HashSet<int>();
        m_LogDict = new Dictionary<int, LogTrackInfoItem>();

        LoadPdb();
    }

    private void LoadPdb()
    {
        if (!File.Exists(m_pdbPath))
            return;
        var lines = File.ReadAllLines(m_pdbPath);
        for (int i = 0; i < lines.Length; ++i)
        {
            var line = lines[i];
            List<string> info = Split(line);
            int hashId = int.Parse(info[0]);
            int argCnt = int.Parse(info[1]);
            string subPath = info[2];
            int lineNum = int.Parse(info[3]);
            string dbgStr = info[4];
            LogTrackInfoItem item = new LogTrackInfoItem(hashId, argCnt, subPath, lineNum, dbgStr);
            m_LogDict[hashId] = item;
            m_oldTableId.Add(hashId);
        }
    }

    private List<string> Split(string line)
    {
        int markCount = 0;
        List<string> res = new List<string>();
        string str = string.Empty;
        for (int i = 0; i < line.Length; ++i)
        {
            char ch = line[i];
            if (ch == ',' && markCount == 0)
            {
                res.Add(str);
                str = string.Empty;
            }
            else
            {
                if (ch == '<')
                    ++markCount;
                if (ch == '>')
                    --markCount;
                str += ch;
            }
        }
        res.Add(str);
        return res;
    }

    public void SavePdb()
    { 
    }

    private int GetNextHashId()
    {
        while (m_oldTableId.Contains(m_hashId))
        {
            ++m_hashId;
        }
        return m_hashId;
    }

    public int AddItem(int hash, int argCnt, string subPath, int line, string dbgStr)
    {
        int cur = hash;
        LogTrackInfoItem item;
        if (m_oldTableId.Contains(hash))
        {
            m_oldTableId.Remove(hash);
            item = m_LogDict[hash];
            item.UpdateItem(hash, argCnt, subPath, line, dbgStr);
        }
        else
        {
            int newId = GetNextHashId();
            cur = newId;
            item = new LogTrackInfoItem(newId, argCnt, subPath, line, dbgStr);
            m_LogDict[newId] = item;
        }
        m_updateTableId.Add(cur);
        return 0;
    }


}
