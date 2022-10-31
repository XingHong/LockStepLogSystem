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
    private static List<string> ms_assemblyPathss = new List<string>()
    {
        Application.dataPath+"/../Library/ScriptAssemblies/Test.dll",
        // Application.dataPath+"/../TestLib/TestInDotNetCore.exe",
        // Application.dataPath+"/../TestLib/battle.exe",
        // Application.dataPath+"/../TestLib/TestInDotNetCore.dll",
    };

    private static HashSet<string> ms_excludeTypeSet;
    private const string CODE_FILE_ROOT = "TestScript";
    private static string ms_basePath;
 
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
            ms_basePath = Path.Combine(Application.dataPath, CODE_FILE_ROOT);
            ms_basePath = ChangePathSeparatorChar(ms_basePath);
            ms_excludeTypeSet = ExcludeTypeSet(ms_basePath);
            
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
 
            foreach (String assemblyPath in ms_assemblyPathss)
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

                    //过滤掉已经有FSPDebuger操作的函数和有匿名委托的函数，没必要自动注入
                    if (FilterInvaild(methodDefinition)) continue;
                    //如果注入代码失败，可以打开下面的输出看看卡在了那个方法上。
                    //Debug.Log(methodDefinition.Name + "======= " + typeDefinition.Name + "======= " +GetDebugParams(methodDefinition.Parameters) +" ===== "+ moduleDefinition.Name);

                    IntertLogTrackCode(moduleDefinition, methodDefinition);
                }
            }

            ILogTrackPdbFile pdb = new InjectLogTrackPdbFile();
            foreach (TypeDefinition typeDefinition in moduleDefinition.Types)
            {
                foreach (MethodDefinition methodDefinition in typeDefinition.Methods)
                {
                    if (FilterMethodDef(methodDefinition)) continue;

                    HashLogTrackCode(moduleDefinition, methodDefinition, typeDefinition, pdb);
                }
            }
            pdb.SavePdb();
        }

        return wasProcessed;
    }

    private static string ChangePathSeparatorChar(string path)
    {
        if (Path.DirectorySeparatorChar != '/')
            return path.Replace('/', Path.DirectorySeparatorChar);
        return path;
    }

    private static HashSet<string> ExcludeTypeSet(string root)
    {
        var set = new HashSet<string>(GetExcludeTypeList());
        List<string> res = new List<string>();
        foreach(var dir in GetExcludeDirectoryList())
        {
            string excludeDir = Path.Combine(root, dir);
            excludeDir = ChangePathSeparatorChar(excludeDir);
            var files = Directory.GetFiles(excludeDir, "*.cs", SearchOption.AllDirectories);
            foreach(var file in files)
            {
                string cn = Path.GetFileNameWithoutExtension(file);
                set.Add(cn);
            }
        }
        return set;
    }

    private static List<string> GetExcludeDirectoryList()
    {
        return new List<String>
        {
            "Other",
        };
    }

    public static List<string> GetExcludeTypeList()
    {
        return new List<String>
        {
            "SubClass3",
        };
    }

    private static bool FilterTypeDef(TypeDefinition typeDefinition)
    {
        //过滤特定类，抽象类，接口
        return ms_excludeTypeSet.Contains(typeDefinition.Name) || typeDefinition.IsAbstract || typeDefinition.IsInterface;
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

    //过滤有FSPDebuger的函数，有匿名委托的函数（如果需要注入，要手动注入）
    private static bool FilterInvaild(MethodDefinition methodDefinition)
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
        
        foreach (var instruction in methodDefinition.Body.Instructions)
        {
            if (instruction.OpCode == OpCodes.Newobj)
            {
                MethodDefinition methodDef =  instruction.Operand as MethodDefinition;
                if (methodDef != null)
                {
                    if (methodDef.FullName.Contains("c__DisplayClass") || methodDef.FullName.Contains("c__AnonStorey"))
                    {
                        Debug.LogWarning($"instruction.Operand:{instruction.Operand}, 如果需要，请手动注入");
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
                OpCode op;
                if (methodDefinition.IsStatic)
                {
                    op = GetStaticIndexOpcode(item.Index);
                }
                else
                {
                    op = GetIndexOpcode(item.Index);
                }

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

    private static OpCode GetStaticIndexOpcode(int idx)
    {
        if (idx > 3)
            return OpCodes.Ldarg_S;
        switch(idx)
        {
            case 1:
                return OpCodes.Ldarg_0;
            case 2:
                return OpCodes.Ldarg_1;
            case 3:
                return OpCodes.Ldarg_2;
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

    private static void HashLogTrackCode(ModuleDefinition moduleDefinition, MethodDefinition methodDefinition, TypeDefinition typeDefinition, ILogTrackPdbFile pdb)
    {
        ReplaceHash(methodDefinition, typeDefinition, pdb);
    }

    private static void ReplaceHash(MethodDefinition methodDefinition, TypeDefinition typeDefinition, ILogTrackPdbFile pdb)
    {
        List<PdbInfoForILInject> replaceInsList = new List<PdbInfoForILInject>();

        var processor = methodDefinition.Body.GetILProcessor();
        foreach (var instruction in methodDefinition.Body.Instructions)
        {
            if (instruction.OpCode == OpCodes.Call)
            {
                MethodReference methodCall = instruction.Operand as MethodReference;
                if(methodCall != null)
                {
                    string mfName = methodCall.MemberFullName();
                    if (mfName == "FSPDebuger::LogTrack")
                    {
                        var curIns = instruction.Previous;
                        while(curIns != null)
                        {
                            if (curIns.opcode == OpCodes.Ldc_I4_0)
                            {
                                var item = new PdbInfoForILInject();
                                item.ins = curIns;
                                item.argCnt = methodCall.parameters.Count - 1;
                                item.debugStr = methodDefinition.Name;
                                item.typeName = typeDefinition.Name;
                                SequencePoint seqPoint = methodDefinition.DebugInformation.GetSequencePoint(curIns);
                                item.line = seqPoint.StartLine;
                                replaceInsList.Add(item);
                                break;
                            }
                            curIns = curIns.Previous;
                        }
                    }
                }
            }
        }

        for(int i = 0; i < replaceInsList.Count; ++i)
        {
            var item = replaceInsList[i];
            int id = pdb.AddItem(0, item.argCnt, item.typeName, item.line, item.debugStr);
            var newInstrcution = processor.Create(OpCodes.Ldc_I4, id);
            processor.Replace(item.ins, newInstrcution);
        }
    }
}
 