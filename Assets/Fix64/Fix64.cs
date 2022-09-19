using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Fix64
{
    long m_rawValue;
    public long RawValue{set {m_rawValue = value;} get{return m_rawValue;}}
}
