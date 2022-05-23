using System.Collections;
using System.Collections.Generic;

public class LogTrackInfoItem
{
    private int m_hashId;
    private int m_argCnt;
    private string m_subPath;
    private int m_line;
    private string m_dbgStr = string.Empty;
    private bool m_hasChange;
    public LogTrackInfoItem(int hashId, int argCnt, string subPath, int line, string dbgStr)
    {
        UpdateItem(hashId, argCnt, subPath, line, dbgStr);
        m_hasChange = false;
    }

    public void UpdateItem(int hashId, int argCnt, string subPath, int line, string dbgStr)
    {
        if (hashId != m_hashId || argCnt != m_argCnt || subPath != m_subPath || line != m_line || dbgStr != m_dbgStr)
            m_hasChange = true;
        m_hashId = hashId;
        m_argCnt = argCnt;
        m_subPath = subPath;
        m_line = line;
        if (!string.IsNullOrEmpty(dbgStr))
            m_dbgStr = dbgStr;
    }

    public bool HasChange()
    {
        return m_hasChange;
    }

    public string ToItemFormat()
    {
        return $"{m_hashId},{m_argCnt},{m_subPath},{m_line},{m_dbgStr}";
    }
}
