using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSPDebuger
{
    private const int MAX_FRAME = 50;
    private static List<ushort> ms_items;
    private static List<long> ms_args;

    private static long ms_checksum;

    private static CircularQueue<LogTrackFrame> ms_logTrackLoopQueue;
    private static LogTrackFrame ms_currFrame;

    public static void BeginTrack()
    {
        ms_checksum = 0;
        ms_logTrackLoopQueue = new CircularQueue<LogTrackFrame>(MAX_FRAME);
    }

    public static void EndTrack()
    { 
        ms_logTrackLoopQueue = null;
        ms_items = null;
        ms_args = null;
    }

    public static void SaveTrack()
    {
        ms_logTrackLoopQueue.Enqueue(ms_currFrame);
    }

    public static void EnterTrackFrame(int frameIndex)
    {
        if (ms_logTrackLoopQueue.IsFull())
            ms_logTrackLoopQueue.Dequeue();
        ms_currFrame = ms_logTrackLoopQueue.GetNextItem();
        ms_items = ms_currFrame.m_items;
        ms_items.Clear();
        ms_args = ms_currFrame.m_args;
        ms_args.Clear();
    }

    public static void LogTrack(ushort hashId, params object[] ps)
    {
        ms_items.Add(hashId);
        ms_checksum += hashId;
        for (int i = 0; i < ps.Length; ++i)
        {
            long val = (long)ps[i];
            ms_args.Add(val);
            ms_checksum += val;
        }
    }

    public static void LogTrack(ushort hashId, long arg1)
    {

    }

    public static void LogTrack(ushort hashId, long arg1, long arg2)
    {

    }

    public static void LogTrack(ushort hashId, long arg1, long arg2, long arg3)
    {

    }

    public static void LogTrack(ushort hashId, long arg1, long arg2, long arg3, long arg4)
    {

    }

    public static void LogTrack(ushort hashId, long arg1, long arg2, long arg3, long arg4, long arg5)
    {

    }

    public static void LogTrack(ushort hashId, long arg1, long arg2, long arg3, long arg4, long arg5, long arg6)
    {

    }

    public static void IgnoreTrack()
    { 
    }
}
