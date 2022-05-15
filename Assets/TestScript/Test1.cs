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

    private void SetAll(int v1, int v2)
    {
        a1 = v1;
        a2 = v2;
    }
}
