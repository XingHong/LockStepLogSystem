using System.Collections;
using System.Collections.Generic;

public class LogTrackFrame
{
    public List<ushort> m_items;
    public List<long> m_args;
    private int m_frameIndex;
    private const int LEN = 1024;

    public LogTrackFrame(int frame)
    {
        m_frameIndex = frame;
        m_items = new List<ushort>(LEN);
        m_args = new List<long>(LEN);
    }

    public void SetFrameIndex(int index)
    {
        m_frameIndex = index;
    }

}
