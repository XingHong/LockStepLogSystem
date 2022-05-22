using System.Collections;
using System.Collections.Generic;

public class LogTrackInfoItem
{
    private int m_hashId;
    private int m_argCnt;
    private string m_subPath;
    private int m_line;
    private string m_dbgStr = string.Empty;
    public LogTrackInfoItem(int hashId, int argCnt, string subPath, int line, string dbgStr)
    {
        UpdateItem(hashId, argCnt, subPath, line, dbgStr);
    }

    public void UpdateItem(int hashId, int argCnt, string subPath, int line, string dbgStr)
    {
        m_hashId = hashId;
        m_argCnt = argCnt;
        m_subPath = subPath;
        m_line = line;
        if (!string.IsNullOrEmpty(dbgStr))
            m_dbgStr = dbgStr;
    }
}
