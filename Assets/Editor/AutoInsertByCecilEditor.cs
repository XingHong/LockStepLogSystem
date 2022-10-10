using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.Callbacks;
using System.Linq;
 
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
                writerParameters.WriteSymbols = true;
                writerParameters.SymbolWriterProvider = new Mono.Cecil.Pdb.PdbWriterProvider();
 
                AssemblyDefinition assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyPath, readerParameters);
 
                Debug.Log("Processing " + Path.GetFileName(assemblyPath));
                if (AutoInsertByCecilEditor.ProcessAssembly(assemblyDefinition))
                {
                    Debug.Log("Writing to " + assemblyPath);
                    assemblyDefinition.Write(assemblyPath, writerParameters);
                    Debug.Log("Done writing");
                }
                else
                {
                    Debug.Log(Path.GetFileName(assemblyPath) + " didn't need to be processed");
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
        bool wasProcessed = false;
 
        foreach (ModuleDefinition moduleDefinition in assemblyDefinition.Modules)
        {
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
                    //过滤抽象方法、虚函数、get set 方法
                    if (methodDefinition.IsAbstract) continue;
                    if (methodDefinition.IsVirtual) continue;
                    if (methodDefinition.IsGetter) continue;
                    if (methodDefinition.IsSetter) continue;
                    //如果注入代码失败，可以打开下面的输出看看卡在了那个方法上。
                    Debug.Log(methodDefinition.Name + "======= " + typeDefinition.Name + "======= " +typeDefinition.BaseType.GenericParameters +" ===== "+ moduleDefinition.Name);
                    MethodReference logMethodReference = moduleDefinition.ImportReference(typeof(FSPDebuger).GetMethod("IgnoreTrack", new Type[] { }));
 
                    ILProcessor ilProcessor = methodDefinition.Body.GetILProcessor();
 
                    Instruction first = methodDefinition.Body.Instructions[0];
                    ilProcessor.InsertBefore(first, Instruction.Create(OpCodes.Ldstr, typeDefinition.FullName + "." + methodDefinition.Name));
                    ilProcessor.InsertBefore(first, Instruction.Create(OpCodes.Call, logMethodReference));
 
                    wasProcessed = true;
                }
            }
        }
 
        return wasProcessed;
    }
}
 