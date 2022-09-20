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

    public static void LogTrack(ushort hashId)
    {
        ms_checksum += hashId;
        ms_items.Add(hashId);
    }

    public static void LogTrack(ushort hashId, long arg1)
    {
        ms_checksum += hashId;
        ms_checksum += arg1;
        ms_items.Add(hashId);
        ms_args.Add(arg1);
    }

    public static void LogTrack(ushort hashId, long arg1, long arg2)
    {
        ms_checksum += hashId;
        ms_checksum += arg1;
        ms_checksum += arg2;
        ms_items.Add(hashId);
        ms_args.Add(arg1);
        ms_args.Add(arg2);
    }

    public static void LogTrack(ushort hashId, long arg1, long arg2, long arg3)
    {
        ms_checksum += hashId;
        ms_checksum += arg1;
        ms_checksum += arg2;
        ms_checksum += arg3;
        ms_items.Add(hashId);
        ms_args.Add(arg1);
        ms_args.Add(arg2);
        ms_args.Add(arg3);
    }

    public static void LogTrack(ushort hashId, long arg1, long arg2, long arg3, long arg4)
    {
        ms_checksum += hashId;
        ms_checksum += arg1;
        ms_checksum += arg2;
        ms_checksum += arg3;
        ms_checksum += arg4;
        ms_items.Add(hashId);
        ms_args.Add(arg1);
        ms_args.Add(arg2);
        ms_args.Add(arg3);
        ms_args.Add(arg4);
    }

    public static void LogTrack(ushort hashId, long arg1, long arg2, long arg3, long arg4, long arg5)
    {
        ms_checksum += hashId;
        ms_checksum += arg1;
        ms_checksum += arg2;
        ms_checksum += arg3;
        ms_checksum += arg4;
        ms_checksum += arg5;
        ms_items.Add(hashId);
        ms_args.Add(arg1);
        ms_args.Add(arg2);
        ms_args.Add(arg3);
        ms_args.Add(arg4);
        ms_args.Add(arg5);
    }

    public static void LogTrack(ushort hashId, long arg1, long arg2, long arg3, long arg4, long arg5, long arg6)
    {
        ms_checksum += hashId;
        ms_checksum += arg1;
        ms_checksum += arg2;
        ms_checksum += arg3;
        ms_checksum += arg4;
        ms_checksum += arg5;
        ms_checksum += arg6;
        ms_items.Add(hashId);
        ms_args.Add(arg1);
        ms_args.Add(arg2);
        ms_args.Add(arg3);
        ms_args.Add(arg4);
        ms_args.Add(arg5);
        ms_args.Add(arg6);
    }

    public static void IgnoreTrack()
    { 
    }
}
