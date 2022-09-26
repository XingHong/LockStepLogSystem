using System.Collections;
using System.Collections.Generic;

public class Test1
{
    private int a1;
    private long a2;

    public Test1()
    {
        a1 = 1;
        a2 = 2;
    }

    /// <summary>
    /// test
    /// </summary>
    /// <param name="val"></param>
    public void SetA1(int val)
    {
        a1 = val;
    }

    protected virtual void SetA2(int val)
    {
        a2 = val;
    }

    public static int TestStatic()
    {
        int res = 399;
        if (res > 0)
        {
            res = 333;
        }
        return res;
    }

    private void SetAll(int v1 = 2, int v2 = 1)
    {
        a1 = v1;
        a2 = v2;
    }

    private void TestOutRef(out int v1, ref int v2)
    {
        v1 = 45;
    }

    public void TestFix64(Fix64 f)
    {

    }
}
