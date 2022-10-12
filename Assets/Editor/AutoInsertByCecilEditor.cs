using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Mono.Collections.Generic;
 
public class AutoInsertByCecilEditor 
{
    static List<string> assemblyPathss = new List<string>()
    {
        Application.dataPath+"/../Library/ScriptAssemblies/Assembly-CSharp.dll",
        // Application.dataPath+"/../Library/ScriptAssemblies/Assembly-CSharp-firstpass.dll",         
 
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
 
                AssemblyDefinition assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyPath, readerParameters);
                Debug.Log("Processing " + Path.GetFileName(assemblyPath));
                if (AutoInsertByCecilEditor.ProcessAssembly(assemblyDefinition))
                {
                    Debug.Log("Writing to " + assemblyPath);
                    // assemblyDefinition.Write(assemblyPath, writerParameters);
                    assemblyDefinition.Write(assemblyPath + ".backup", writerParameters);
                    Debug.Log("Done writing");  
                }
                else
                {
                    Debug.Log(Path.GetFileName(assemblyPath) + " didn't need to be processed");
                }
                assemblyDefinition.Dispose();
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
        bool wasProcessed = false;
 
        foreach (ModuleDefinition moduleDefinition in assemblyDefinition.Modules)
        {
            Debug.Log($"model:{moduleDefinition.Name}");
            foreach (TypeDefinition typeDefinition in moduleDefinition.Types)
            {
                if (typeDefinition.Name == typeof(AutoInsertByCecilEditor).Name) continue;
                //过滤抽象类
                if (typeDefinition.IsAbstract) continue;
                //过滤抽象方法
                if (typeDefinition.IsInterface) continue;
                foreach (MethodDefinition methodDefinition in typeDefinition.Methods)
                {
                    //过滤构造函数
                    if(methodDefinition.Name == ".ctor")continue;
                    if (methodDefinition.Name == ".cctor") continue;
                    //过滤抽象方法,get set 方法
                    if (methodDefinition.IsAbstract) continue;
                    if (methodDefinition.IsGetter) continue;
                    if (methodDefinition.IsSetter) continue;
                    //过滤掉已经有FSPDebuger操作的函数，没必要自动注入
                    if (HasFSPDebuger(methodDefinition)) continue;
                    //如果注入代码失败，可以打开下面的输出看看卡在了那个方法上。
                    Debug.Log(methodDefinition.Name + "======= " + typeDefinition.Name + "======= " +GetDebugParams(methodDefinition.Parameters) +" ===== "+ moduleDefinition.Name);
                    
                    IntertLogTrackCode(moduleDefinition, methodDefinition);
                    
                    // processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Ldc_I4_0));
                    // processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Ldarg_1));
                    // processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Conv_I8));
                    // processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Call, logMethodReference));
                    // SequencePoint seqPoint = methodDefinition.DebugInformation.GetSequencePoint(firstInstruction);   //指令行数

                    wasProcessed = true;
                }
            }
            
        }
        return wasProcessed;
    }

    private static string GetDebugParams(Collection<ParameterDefinition> param)
    {
        var items = param.items;
        var size = param.size;
        string res = "";
        for (int i = 0; i < size; i++) {
            var item = items[i];
            res += $"{item.ParameterType} {item.Name} ,";
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
        MethodReference logMethodReference = moduleDefinition.ImportReference(typeof(FSPDebuger).GetMethod("LogTrack", new Type[] {typeof(UInt16), typeof(Int64)}));
    }
}
 