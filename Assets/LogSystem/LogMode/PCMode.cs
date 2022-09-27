public class PCMode : ILockStepLog
{
    private long m_checksum;
    public PCMode()
    {

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

    }

    public void LogTrack(ushort hashId)
    {
        m_checksum += hashId;
    }

    public void LogTrack(ushort hashId, long arg1)
    {
        m_checksum += hashId;
        m_checksum += arg1;
    }

    public void LogTrack(ushort hashId, long arg1, long arg2)
    {
        m_checksum += hashId;
        m_checksum += arg1;
        m_checksum += arg2;
    }

    public void LogTrack(ushort hashId, long arg1, long arg2, long arg3)
    {
        m_checksum += hashId;
        m_checksum += arg1;
        m_checksum += arg2;
        m_checksum += arg3;
    }

    public void LogTrack(ushort hashId, long arg1, long arg2, long arg3, long arg4)
    {
        m_checksum += hashId;
        m_checksum += arg1;
        m_checksum += arg2;
        m_checksum += arg3;
        m_checksum += arg4;
    }

    public void LogTrack(ushort hashId, long arg1, long arg2, long arg3, long arg4, long arg5)
    {
        m_checksum += hashId;
        m_checksum += arg1;
        m_checksum += arg2;
        m_checksum += arg3;
        m_checksum += arg4;
        m_checksum += arg5;
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
    }

}