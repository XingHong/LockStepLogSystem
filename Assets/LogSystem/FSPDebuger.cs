using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSPDebuger
{
    private const int MAX_FRAME = 50;
    private static List<ushort> ms_items;
    private static List<long> ms_args;

    private static long ms_hash;

    private static Queue<LogTrackFrame> ms_logTrackLoopQueue;
    private static LogTrackFrame ms_currFrame;

    public static void BeginTrack()
    {
        ms_hash = 0;
        ms_logTrackLoopQueue = new Queue<LogTrackFrame>();
    }

    public static void EndTrack()
    { 
    }

    public static void SaveTrack()
    {
        ms_currFrame.Save(ms_items, ms_args);
        if (ms_logTrackLoopQueue.Count > MAX_FRAME)
        {
            ms_logTrackLoopQueue.Dequeue();
        }
        ms_logTrackLoopQueue.Enqueue(ms_currFrame);
    }

    public static void EnterTrackFrame(int frameIndex)
    {
        ms_items = new List<ushort>();
        ms_args = new List<long>();
        ms_currFrame = new LogTrackFrame(frameIndex);
    }

    public static void LogTrack(ushort hashId, params object[] ps)
    {
        ms_items.Add(hashId);
        ms_hash += hashId;
        for (int i = 0; i < ps.Length; ++i)
        {
            long val = (long)ps[i];
            ms_args.Add(val);
            ms_hash += val;
        }
    }

    public static void IgnoreTrack()
    { 
    }


}
