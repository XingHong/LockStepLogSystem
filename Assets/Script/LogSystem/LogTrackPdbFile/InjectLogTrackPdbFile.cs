using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;


public class InjectLogTrackPdbFile : ILogTrackPdbFile
{
    private string m_pdbPath;
    private int m_hashId;
    private SortedDictionary<int, LogTrackInfoItem> m_logDict;

    public InjectLogTrackPdbFile()
    {
        m_pdbPath = Application.dataPath + "/LogTrackPdb.pdb";
        m_hashId = 1;
        m_logDict = new SortedDictionary<int, LogTrackInfoItem>();
    }

    public LogTrackInfoItem GetInfoItem(int id)
    {
        if (!m_logDict.ContainsKey(id))
            throw new ArgumentOutOfRangeException($"pdb id {id} is null.");
        return m_logDict[id];
    }

    public void SavePdb()
    {
        List<string> lines = new List<string>();
        foreach(var item in m_logDict.Values)
        {
            lines.Add(item.ToItemFormat());
        }
        File.WriteAllLines(m_pdbPath, lines.ToArray());
    }

    public int AddItem(int hash, int argCnt, string subPath, int line, string dbgStr)
    {
        int newId = m_hashId;
        ++m_hashId;
        LogTrackInfoItem item = new LogTrackInfoItem(newId, argCnt, subPath, line, dbgStr);
        m_logDict[newId] = item;
        return newId;
    }
}