using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubClass2 : SubClass
{
    public override void Test1()
    {FSPDebuger.IgnoreTrack();
        Debug.Log("sb2, Test!!!");
    }

    public override void Test2(int a)
    {
        FSPDebuger.LogTrack(14, a);/*test2 in subclass2*/
    }
}
