using System.Collections.Generic;

public class NormalMode : ILockStepLog
{
    private const int MAX_FRAME = 50;
    private List<ushort> m_items;
    private List<long> m_args;
    private long m_checksum;
    private CircularQueue<LogTrackFrame> m_logTrackLoopQueue;
    private LogTrackFrame m_currFrame;

    public NormalMode()
    {

    }
    
    public void BeginTrack()
    {
        m_checksum = 0;
        m_logTrackLoopQueue = new CircularQueue<LogTrackFrame>(MAX_FRAME);
    }

    public void EndTrack()
    {
        m_logTrackLoopQueue = null;
        m_items = null;
        m_args = null;
    }

    public void SaveTrack()
    {
        m_logTrackLoopQueue.Enqueue(m_currFrame);
    }

    public void EnterTrackFrame(int frameIndex)
    {
        if (m_logTrackLoopQueue.IsFull())
            m_logTrackLoopQueue.Dequeue();
        m_currFrame = m_logTrackLoopQueue.GetNextItem();
        m_items = m_currFrame.m_items;
        m_items.Clear();
        m_args = m_currFrame.m_args;
        m_args.Clear();
    }

    public void LogTrack(ushort hashId)
    {
        m_checksum += hashId;
        m_items.Add(hashId);

    }

    public void LogTrack(ushort hashId, long arg1)
    {
        m_checksum += hashId;
        m_checksum += arg1;
        m_items.Add(hashId);
        m_args.Add(arg1);
    }

    public void LogTrack(ushort hashId, long arg1, long arg2)
    {
        m_checksum += hashId;
        m_checksum += arg1;
        m_checksum += arg2;
        m_items.Add(hashId);
        m_args.Add(arg1);
        m_args.Add(arg2);
    }

    public void LogTrack(ushort hashId, long arg1, long arg2, long arg3)
    {
        m_checksum += hashId;
        m_checksum += arg1;
        m_checksum += arg2;
        m_checksum += arg3;
        m_items.Add(hashId);
        m_args.Add(arg1);
        m_args.Add(arg2);
        m_args.Add(arg3);
    }

    public void LogTrack(ushort hashId, long arg1, long arg2, long arg3, long arg4)
    {
        m_checksum += hashId;
        m_checksum += arg1;
        m_checksum += arg2;
        m_checksum += arg3;
        m_checksum += arg4;
        m_items.Add(hashId);
        m_args.Add(arg1);
        m_args.Add(arg2);
        m_args.Add(arg3);
        m_args.Add(arg4);
    }

    public void LogTrack(ushort hashId, long arg1, long arg2, long arg3, long arg4, long arg5)
    {
        m_checksum += hashId;
        m_checksum += arg1;
        m_checksum += arg2;
        m_checksum += arg3;
        m_checksum += arg4;
        m_checksum += arg5;
        m_items.Add(hashId);
        m_args.Add(arg1);
        m_args.Add(arg2);
        m_args.Add(arg3);
        m_args.Add(arg4);
        m_args.Add(arg5);
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
        m_items.Add(hashId);
        m_args.Add(arg1);
        m_args.Add(arg2);
        m_args.Add(arg3);
        m_args.Add(arg4);
        m_args.Add(arg5);
        m_args.Add(arg6);
    }

}