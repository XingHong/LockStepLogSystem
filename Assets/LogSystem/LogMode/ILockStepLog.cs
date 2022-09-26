public interface ILockStepLog
{
    void BeginTrack();
    void EndTrack();
    void SaveTrack();
    long GetCheckSum();
    void EnterTrackFrame(int frameIndex);
    void LogTrack(ushort hashId);
    void LogTrack(ushort hashId, long arg1);
    void LogTrack(ushort hashId, long arg1, long arg2);
    void LogTrack(ushort hashId, long arg1, long arg2, long arg3);
    void LogTrack(ushort hashId, long arg1, long arg2, long arg3, long arg4);
    void LogTrack(ushort hashId, long arg1, long arg2, long arg3, long arg4, long arg5);
    void LogTrack(ushort hashId, long arg1, long arg2, long arg3, long arg4, long arg5, long arg6);
}