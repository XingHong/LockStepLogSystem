using System.Collections;
using System.Collections.Generic;
using Mono.Cecil;

public class ParamClassForIL
{
    public ParameterDefinition Pdef;
    public int Index;

    public ParamClassForIL(ParameterDefinition pdef, int index)
    {
        Pdef = pdef;
        Index = index;
    }
}
