using UnityEngine;
using System.IO;
using System;
using System.Text;

public class CheckMode : ILockStepLog
{
    private LogTrackPdbFile ms_pdb;
    private long m_checksum;
    private string m_outputPath;
    private StringBuilder m_sb;

    public CheckMode()
    {
        ms_pdb = new LogTrackPdbFile();
        m_sb = new StringBuilder();
    }
    
    public void BeginTrack()
    {
        m_checksum = 0;
    }

    public void EndTrack()
    {
        string str = $"EndTrack hash:{m_checksum}";
        DoLog(str);
        var nowTime = DateTime.Now.ToString("yy_MM_dd_HH_mm_ss_ff");
        m_outputPath = Application.dataPath + $"/TrackLog_{nowTime}.txt";
        File.WriteAllText(m_outputPath, m_sb.ToString());
        m_sb.Clear();
    }

    public void SaveTrack()
    {

    }

    public long GetCheckSum()
    {
        return m_checksum;
    }

    public void EnterTrackFrame(int frameIndex)
    {
        string str = $"EnterTrackFrame:{frameIndex}, hash:{m_checksum}";
        DoLog(str);
    }

    private void Print(ushort hashId)
    {
        var item = ms_pdb.GetInfoItem(hashId);
        string str = $"[hash:{m_checksum}]{item.DbgStr}(argCnt:{item.ArgCnt})(at {item.SubPath}:{item.Line})";
        DoLog(str);
    }

    private void DoLog(string str)
    {
        m_sb.Append(str);
        m_sb.AppendLine();
        Debug.Log(str);
    }

    public void LogTrack(ushort hashId)
    {
        m_checksum += hashId;
        Print(hashId);
    }


    public void LogTrack(ushort hashId, long arg1)
    {
        m_checksum += hashId;
        m_checksum += arg1;
        Print(hashId);
    }

    public void LogTrack(ushort hashId, long arg1, long arg2)
    {
        m_checksum += hashId;
        m_checksum += arg1;
        m_checksum += arg2;
        Print(hashId);
    }

    public void LogTrack(ushort hashId, long arg1, long arg2, long arg3)
    {
        m_checksum += hashId;
        m_checksum += arg1;
        m_checksum += arg2;
        m_checksum += arg3;
        Print(hashId);
    }

    public void LogTrack(ushort hashId, long arg1, long arg2, long arg3, long arg4)
    {
        m_checksum += hashId;
        m_checksum += arg1;
        m_checksum += arg2;
        m_checksum += arg3;
        m_checksum += arg4;
        Print(hashId);
    }

    public void LogTrack(ushort hashId, long arg1, long arg2, long arg3, long arg4, long arg5)
    {
        m_checksum += hashId;
        m_checksum += arg1;
        m_checksum += arg2;
        m_checksum += arg3;
        m_checksum += arg4;
        m_checksum += arg5;
        Print(hashId);
    }

    public void LogTrack(ushort hashId, long arg1, long arg2, long arg3, long arg4, long arg5, long arg6)
    {
        m_checksum += hashId;
        m_checksum += arg1;
        m_checksum += arg2;
        m_checksum += arg3;
        m_checksum += arg4;
        m_checksum += arg5;
        m_checksum += arg6;
        Print(hashId);
    }

}