public enum LogMode
{
    None = 0,
    Normal,
    All
}
public class LogModeFactory
{
    public static ILockStepLog GetLog(LogMode ltype)
    {
        ILockStepLog res = null;
        switch(ltype)
        {
            case LogMode.None:
                res = new NoneMode();
                break;
            case LogMode.Normal:
                res = new NormalMode();
                break;
            case LogMode.All:
                res = new AllMode();
                break;
        }
        return res;
    }
}