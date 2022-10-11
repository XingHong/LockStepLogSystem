using System.Collections;
using System.Collections.Generic;

public class SubClass4 : SubClass
{
    public void Test4(Fix64 f)
    {
        FSPDebuger.LogTrack(0, f.RawValue);
    }
}
