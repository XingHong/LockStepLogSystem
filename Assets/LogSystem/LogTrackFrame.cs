using System.Collections;
using System.Collections.Generic;

public class LogTrackFrame
{
    private List<ushort> m_items;
    private List<long> m_args;
    private int m_frameIndex;

    public LogTrackFrame(int frame)
    {
        m_frameIndex = frame;
    }

    public void Save(List<ushort> items, List<long> args)
    {
        m_items = items;
        m_args = args;
    }
}
