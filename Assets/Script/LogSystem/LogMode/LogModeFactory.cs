public enum LogMode
{
    PC,
    PVP,
    CHECK
}
public class LogModeFactory
{
    public static ILockStepLog GetLog(LogMode ltype)
    {
        ILockStepLog res = null;
        switch(ltype)
        {
            case LogMode.PC:
                res = new PCMode();
                break;
            case LogMode.PVP:
                res = new PVPMode();
                break;
            case LogMode.CHECK:
                res = new CheckMode();
                break;
        }
        return res;
    }
}