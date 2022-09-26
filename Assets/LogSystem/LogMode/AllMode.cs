using UnityEngine;
using System.IO;

public class AllMode : ILockStepLog
{
    private LogTrackPdbFile ms_pdb;
    private long m_checksum;
    private string m_outputPath;
    public AllMode()
    {
        ms_pdb = new LogTrackPdbFile();
    }
    
    public void BeginTrack()
    {
        m_checksum = 0;
    }

    public void EndTrack()
    {

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
        Debug.Log($"EnterTrackFrame:{frameIndex}, hash:{m_checksum}");
    }

    private void Print(ushort hashId)
    {
        var item = ms_pdb.GetInfoItem(hashId);
        Debug.Log($"[hash:{m_checksum}]{item.DbgStr}(argCnt:{item.ArgCnt})(at {item.SubPath}:{item.Line})");
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