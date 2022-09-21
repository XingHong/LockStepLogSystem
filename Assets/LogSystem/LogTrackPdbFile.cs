using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;


public class LogTrackPdbFile
{
    private string m_pdbPath;
    private int m_hashId;
    private HashSet<int> m_oldTableId;
    private HashSet<int> m_updateTableId;
    private Dictionary<int, LogTrackInfoItem> m_logDict;
    private bool m_hasChange;
    public LogTrackPdbFile()
    {
        m_pdbPath = Application.dataPath + "/LogTrackPdb.pdb";
        m_hashId = 1;
        m_oldTableId = new HashSet<int>();
        m_updateTableId = new HashSet<int>();
        m_logDict = new Dictionary<int, LogTrackInfoItem>();
        m_hasChange = false;

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
            m_logDict[hashId] = item;
            m_oldTableId.Add(hashId);
        }
    }

    public LogTrackInfoItem GetInfoItem(int id)
    {
        if (!m_logDict.ContainsKey(id))
            throw new ArgumentOutOfRangeException($"pdb id {id} is null.");
        return m_logDict[id];
    }

    private List<string> Split(string line)
    {
        int markCount = 4;
        List<string> res = new List<string>();
        string str = string.Empty;
        for (int i = 0; i < line.Length; ++i)
        {
            char ch = line[i];
            if (ch == ',' && markCount > 0)
            {
                res.Add(str);
                str = string.Empty;
                --markCount;
            }
            else
            {
                str += ch;
            }
        }
        res.Add(str);
        return res;
    }

    public void SavePdb()
    {
        if (!m_hasChange)
            return;
        List<string> lines = new List<string>();
        foreach(var id in m_updateTableId)
        {
            var item = m_logDict[id];
            lines.Add(item.ToItemFormat());
        }
        File.WriteAllLines(m_pdbPath, lines.ToArray());
    }

    private int GetNextHashId()
    {
        int id = m_hashId;
        while (m_oldTableId.Contains(id))
        {
            ++id;
        }
        m_hashId = id + 1;
        return id;
    }

    public int AddItem(int hash, int argCnt, string subPath, int line, string dbgStr)
    {
        int cur = hash;
        LogTrackInfoItem item;
        if (hash != 0 && !m_updateTableId.Contains(hash))
        {
            if (m_logDict.ContainsKey(hash))
            {
                item = m_logDict[hash];
                item.UpdateItem(hash, argCnt, subPath, line, dbgStr);
                if (!m_hasChange && item.HasChange())
                {
                    m_hasChange = true;
                }
            }
            else
            {
                cur = CreateNewItem(argCnt, subPath, line, dbgStr);
                m_hasChange = true;
            }
        }
        else
        {
            cur = CreateNewItem(argCnt, subPath, line, dbgStr);
            m_hasChange = true;
        }
        m_updateTableId.Add(cur);
        return cur;
    }
    private int CreateNewItem(int argCnt, string subPath, int line, string dbgStr)
    {
        int newId = GetNextHashId();
        LogTrackInfoItem item = new LogTrackInfoItem(newId, argCnt, subPath, line, dbgStr);
        m_logDict[newId] = item;
        return newId;
    }


}
