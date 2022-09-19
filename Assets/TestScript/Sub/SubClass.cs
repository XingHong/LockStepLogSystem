using System.Collections;
using System.Collections.Generic;

public class SubClass
{
    protected int t1;
    public virtual void Test1()
    {
    }

    public virtual void Test2(int a)
    {
        t1 = a;
    }
}
