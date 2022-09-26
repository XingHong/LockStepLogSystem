using System.Collections.Generic;
using UnityEngine;

public class FSPDebuger
{
    private static ILockStepLog ms_log;

    public static void BeginTrack()
    {
        ms_log = LogModeFactory.GetLog(LogMode.All);
        ms_log.BeginTrack();
    }

    public static void EndTrack()
    { 
        ms_log.EndTrack();
        ms_log = null;
    }

    public static void SaveTrack()
    {
        ms_log.SaveTrack();
    }

    public static void EnterTrackFrame(int frameIndex)
    {
        ms_log.EnterTrackFrame(frameIndex);
    }

    public static void LogTrack(ushort hashId)
    {
        ms_log.LogTrack(hashId);
    }

    public static void LogTrack(ushort hashId, long arg1)
    {
        ms_log.LogTrack(hashId, arg1);
    }

    public static void LogTrack(ushort hashId, long arg1, long arg2)
    {
        ms_log.LogTrack(hashId, arg1, arg2);
    }

    public static void LogTrack(ushort hashId, long arg1, long arg2, long arg3)
    {
        ms_log.LogTrack(hashId, arg1, arg2, arg3);
    }

    public static void LogTrack(ushort hashId, long arg1, long arg2, long arg3, long arg4)
    {
        ms_log.LogTrack(hashId, arg1, arg2, arg3, arg4);
    }

    public static void LogTrack(ushort hashId, long arg1, long arg2, long arg3, long arg4, long arg5)
    {
        ms_log.LogTrack(hashId, arg1, arg2, arg3, arg4, arg5);
    }

    public static void LogTrack(ushort hashId, long arg1, long arg2, long arg3, long arg4, long arg5, long arg6)
    {
        ms_log.LogTrack(hashId, arg1, arg2, arg3, arg4, arg5, arg6);
    }

    public static void IgnoreTrack()
    { 
    }
}
