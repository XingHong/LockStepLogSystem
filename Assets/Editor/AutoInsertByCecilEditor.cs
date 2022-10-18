using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Mono.Collections.Generic;
using System.Reflection;
 
public class AutoInsertByCecilEditor 
{
    static List<string> assemblyPathss = new List<string>()
    {
        Application.dataPath+"/../Library/ScriptAssemblies/Test.dll",
        // Application.dataPath+"/../TestLib/TestInDotNetCore.exe",
        // Application.dataPath+"/../TestLib/battle.exe",
        // Application.dataPath+"/../TestLib/TestInDotNetCore.dll",
    };
 
    [MenuItem("AutoInsert/IL注入日志代码")]
    static void Inject()
    {
        AssemblyPostProcessorRun();
    }
 
    static void AssemblyPostProcessorRun()
    {
        try
        {
            Debug.Log("AssemblyPostProcessor running");
            EditorApplication.LockReloadAssemblies();
            DefaultAssemblyResolver assemblyResolver = new DefaultAssemblyResolver();
 
            foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                assemblyResolver.AddSearchDirectory(Path.GetDirectoryName(assembly.Location));
            }
 
            assemblyResolver.AddSearchDirectory(Path.GetDirectoryName(EditorApplication.applicationPath) + "/Data/Managed");
 
            ReaderParameters readerParameters = new ReaderParameters();
            readerParameters.AssemblyResolver = assemblyResolver;
 
            WriterParameters writerParameters = new WriterParameters();
 
            foreach (String assemblyPath in assemblyPathss)
            {
                readerParameters.ReadSymbols = true;
                readerParameters.SymbolReaderProvider = new Mono.Cecil.Pdb.PdbReaderProvider();
                readerParameters.ReadWrite = true;
                writerParameters.WriteSymbols = true;
                writerParameters.SymbolWriterProvider = new Mono.Cecil.Pdb.PdbWriterProvider();
 
                using(AssemblyDefinition assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyPath, readerParameters))
                {
                    Debug.Log("Processing " + Path.GetFileName(assemblyPath));
                    if (AutoInsertByCecilEditor.ProcessAssembly(assemblyDefinition))
                    {
                        Debug.Log("Writing to " + assemblyPath);
                        // assemblyDefinition.Write(assemblyPath, writerParameters);
                        assemblyDefinition.Write(writerParameters);
                        // assemblyDefinition.Write();
                        Debug.Log("Done writing");  
                    }
                    else
                    {
                        Debug.Log(Path.GetFileName(assemblyPath) + " didn't need to be processed");
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
        EditorApplication.UnlockReloadAssemblies();
    }
 
    private static bool ProcessAssembly(AssemblyDefinition assemblyDefinition)
    {
        bool wasProcessed = true;
        
        foreach (ModuleDefinition moduleDefinition in assemblyDefinition.Modules)
        {
            foreach (TypeDefinition typeDefinition in moduleDefinition.Types)
            {
                if (FilterTypeDef(typeDefinition)) continue;
                foreach (MethodDefinition methodDefinition in typeDefinition.Methods)
                {
                    if (FilterMethodDef(methodDefinition)) continue;

                    //过滤掉已经有FSPDebuger操作的函数，没必要自动注入
                    if (HasFSPDebuger(methodDefinition)) continue;
                    //如果注入代码失败，可以打开下面的输出看看卡在了那个方法上。
                    //Debug.Log(methodDefinition.Name + "======= " + typeDefinition.Name + "======= " +GetDebugParams(methodDefinition.Parameters) +" ===== "+ moduleDefinition.Name);

                    IntertLogTrackCode(moduleDefinition, methodDefinition);
                }
            }

            LogTrackPdbFile pdb = new LogTrackPdbFile();
            foreach (TypeDefinition typeDefinition in moduleDefinition.Types)
            {
                if (FilterTypeDef(typeDefinition)) continue;
                foreach (MethodDefinition methodDefinition in typeDefinition.Methods)
                {
                    if (FilterMethodDef(methodDefinition)) continue;

                    HashLogTrackCode(moduleDefinition, methodDefinition);
                }
            }
            pdb.SavePdb();
        }

        return wasProcessed;
    }

    private static bool FilterTypeDef(TypeDefinition typeDefinition)
    {
        //过滤特定类，抽象类，接口
        return typeDefinition.Name == typeof(PCMode).Name || typeDefinition.IsAbstract || typeDefinition.IsInterface;
    }

    private static bool FilterMethodDef(MethodDefinition methodDefinition)
    {
        return methodDefinition.Name == ".ctor" || methodDefinition.Name == ".cctor"        //过滤构造函数
            || methodDefinition.IsAbstract || methodDefinition.IsGetter || methodDefinition.IsSetter;   //过滤抽象方法,get set 方法
    }

    private static string GetDebugParams(Collection<ParameterDefinition> param)
    {
        var items = param.items;
        var size = param.size;
        string res = "";
        for (int i = 0; i < size; i++) {
            var item = items[i];
            res += $"{item.ParameterType}_{item.Name} ,";
        }
        if (size > 0)
            res = res.Remove(res.Length - 1, 1);
        return res;
    }

    private static bool HasFSPDebuger(MethodDefinition methodDefinition)
    {
        var processor = methodDefinition.Body.GetILProcessor();
        foreach (var instruction in methodDefinition.Body.Instructions)
        {
            if (instruction.OpCode == OpCodes.Call)
            {
                MethodReference methodCall = instruction.Operand as MethodReference;
                if(methodCall != null)
                {
                    string mfName = methodCall.MemberFullName();
                    if (mfName == "FSPDebuger::LogTrack" || mfName == "FSPDebuger::IgnoreTrack")
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private static void IntertLogTrackCode(ModuleDefinition moduleDefinition, MethodDefinition methodDefinition)
    {
        var processor = methodDefinition.Body.GetILProcessor();
        var firstInstruction = methodDefinition.Body.Instructions[0];
        processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Ldc_I4_0));
        var list = GetParamList(methodDefinition);
        for(int i = 0; i < list.Count; ++i)
        {
            var item = list[i];
            var pType = item.Pdef.ParameterType;
            if (pType.Name == "Fix64")
            {
                processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Ldarga_S, item.Pdef));
                var method = typeof(Fix64).GetMethod("get_RawValue", new Type[] {});
                var rawValueMethod = moduleDefinition.ImportReference(method);
                processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Call, rawValueMethod));
            }
            else
            {
                var op = GetIndexOpcode(item.Index);
                if (op == OpCodes.Ldarg_S)
                {
                    processor.InsertBefore(firstInstruction, processor.Create(op, item.Pdef));
                }
                else
                {
                    processor.InsertBefore(firstInstruction, processor.Create(op));
                }
                if (pType.Name != "Int64")
                {
                    processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Conv_I8));
                }
            }
        }
        MethodBase logTrackMethod = GetLogTrackMethod(list.Count);
        MethodReference logMethodReference = moduleDefinition.ImportReference(logTrackMethod);
        var newInstruction = processor.Create(OpCodes.Call, logMethodReference);
        processor.InsertBefore(firstInstruction, newInstruction);
    }

    private static OpCode GetIndexOpcode(int idx)
    {
        if (idx > 3)
            return OpCodes.Ldarg_S;
        switch(idx)
        {
            case 1:
                return OpCodes.Ldarg_1;
            case 2:
                return OpCodes.Ldarg_2;
            case 3:
                return OpCodes.Ldarg_3;
        }
        return OpCodes.Nop;
    }

    private static MethodBase GetLogTrackMethod(int paramCount)
    {
        MethodBase method = null;
        switch(paramCount)
        {
            case 0:
                method =  typeof(FSPDebuger).GetMethod("LogTrack", new Type[] {typeof(UInt16)});
                break;
            case 1:
                method =  typeof(FSPDebuger).GetMethod("LogTrack", new Type[] {typeof(UInt16), typeof(Int64)});
                break;
            case 2:
                method =  typeof(FSPDebuger).GetMethod("LogTrack", new Type[] {typeof(UInt16), typeof(Int64), typeof(Int64)});
                break;
            case 3:
                method =  typeof(FSPDebuger).GetMethod("LogTrack", new Type[] {typeof(UInt16), typeof(Int64), typeof(Int64), typeof(Int64)});
                break;
            case 4:
                method =  typeof(FSPDebuger).GetMethod("LogTrack", new Type[] {typeof(UInt16), typeof(Int64), typeof(Int64), typeof(Int64), typeof(Int64)});
                break;
            case 5:
                method =  typeof(FSPDebuger).GetMethod("LogTrack", new Type[] {typeof(UInt16), typeof(Int64), typeof(Int64), typeof(Int64), typeof(Int64), typeof(Int64)});
                break;
            case 6:
                method =  typeof(FSPDebuger).GetMethod("LogTrack", new Type[] {typeof(UInt16), typeof(Int64), typeof(Int64), typeof(Int64), typeof(Int64), typeof(Int64), typeof(Int64)});
                break;
            default:
                throw new ArgumentException("paramCount > 6");
        }
        return method;
    }

    private static List<ParamClassForIL> GetParamList(MethodDefinition methodDefinition)
    {
        var parameters = methodDefinition.Parameters;
        var items = parameters.items;
        var size = parameters.size;
        List<ParamClassForIL> res = new List<ParamClassForIL>();
        for(int i = 0; i < size; ++i)
        {
            var item = items[i];
            var pType = item.ParameterType;
            if (pType.IsValueType && (pType.Name == "Int64" || pType.Name == "Int32" || pType.Name == "UInt16" || pType.Name == "Fix64"))
            {
                ParamClassForIL element = new ParamClassForIL(item, i + 1);
                res.Add(element);
            }
        }
        return res;
    }

    private static void HashLogTrackCode(ModuleDefinition moduleDefinition, MethodDefinition methodDefinition)
    {

    }
}
 