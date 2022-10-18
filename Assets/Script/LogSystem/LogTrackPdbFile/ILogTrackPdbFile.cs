public interface ILogTrackPdbFile
{
    LogTrackInfoItem GetInfoItem(int id);
    void SavePdb();
    int AddItem(int hash, int argCnt, string subPath, int line, string dbgStr);
}