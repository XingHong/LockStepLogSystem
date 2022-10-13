using System.Collections;
using System.Collections.Generic;

public class SubClass4 : SubClass
{
    public void TestParam1(Fix64 f)
    {FSPDebuger.LogTrack(0, f.RawValue);
    }

    public void TestParam2(int a, int b)
    {
        FSPDebuger.LogTrack(0, b);
    }

    public void TestParam3(int a, long b)
    {
        FSPDebuger.LogTrack(0, a, b);
    }

    public void TestParam4(int a, long b, int c, int d, int e, int f)
    {
        FSPDebuger.LogTrack(0, c, d);
    }
}
