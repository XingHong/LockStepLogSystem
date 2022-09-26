using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSPDebugerTest : MonoBehaviour
{
    int frame;
    // Start is called before the first frame update
    void Start()
    {
        frame = 0;
        FSPDebuger.BeginTrack();
    }

    // Update is called once per frame
    void Update()
    {
        if (frame > 100)
            return;
        if (frame == 100)
        {
            FSPDebuger.EndTrack();
            ++frame;
            return;
        }
        FSPDebuger.EnterTrackFrame(frame);
        test1();
        FSPDebuger.SaveTrack();
        ++frame;
    }

    private void test1()
    {
        var t = new Test1();
        t.SetA1(999);
        Test1.TestStatic();
    }
}
