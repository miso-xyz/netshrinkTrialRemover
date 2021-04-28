using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using System.IO;

namespace netshrinkTrialExtender
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = ".netshrink Trial Remover";
            Console.WriteLine(".netshrink Trial Remover by misonothx");
            Console.WriteLine(" |- https://github.com/miso-xyz/netshrinkTrialRemover/");
            Console.WriteLine();
            ModuleDefMD asm = ModuleDefMD.Load(args[0]);
            bool isTrial = false;
            bool hasPassword = false;
            int trialVarIndex = -1;
            if (asm.EntryPoint.Body.HasVariables)
            {
                foreach (var vars in asm.EntryPoint.Body.Variables)
                {
                    if (vars.Type.ReflectionName.ToString().ToLower() == "datetime")
                    {
                        isTrial = true;
                        trialVarIndex = vars.Index;
                        break;
                    }
                }
                if (!isTrial)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Coudn't find trial variable, are you sure this assembly has been protected with .netshrink Demo version?");
                    Console.ResetColor();
                    Console.WriteLine("Press any keys to exit...");
                    Console.ReadKey();
                    System.Environment.Exit(0);
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No variables found, cannot patch");
                Console.ResetColor();
                Console.WriteLine("Press any keys to exit...");
                Console.ReadKey();
                System.Environment.Exit(0);
            }
            asm.EntryPoint.Body.KeepOldMaxStack = true;
            foreach (Instruction inst in asm.EntryPoint.Body.Instructions)
            {
                if (inst.OpCode.Equals(OpCodes.Newobj))
                {
                    if (inst.Operand.ToString().Contains("SHA256"))
                    {
                        hasPassword = true;
                    }
                }
            }
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Requires Password: ");
            if (hasPassword)
            {
                Console.WriteLine("Yes");
            }
            else
            {
                Console.WriteLine("No");
            }
            Console.WriteLine();
            foreach (Instruction inst in asm.EntryPoint.Body.Instructions)
            {
                if (inst.OpCode.Equals(OpCodes.Ldloca_S))
                {
                    if (inst.Operand.ToString().Replace("V_", null).Contains(trialVarIndex.ToString()))
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Removing Trial Check & Alert...");
                        // Prometheo's help | Code scrapped but thanks for the help, it'll be usefull for future projects
                        // - Importer imp_ = new Importer(asm);
                        // - asm.EntryPoint.Body.Instructions.Insert(asm.EntryPoint.Body.Instructions.IndexOf(inst) + 1, OpCodes.Ldsfld.ToInstruction(imp_.Import(typeof(DateTime).GetField("MinValue"))));
                        // end of Prometheo's help
                        int baseInt = asm.EntryPoint.Body.Instructions.IndexOf(inst);
                        for (int x = baseInt; x < asm.EntryPoint.Body.Instructions.Count; x++)
                        {
                            if (asm.EntryPoint.Body.Instructions[baseInt].OpCode.Equals(OpCodes.Ret) || asm.EntryPoint.Body.Instructions[baseInt].OpCode.Equals(OpCodes.Leave))
                            {
                                asm.EntryPoint.Body.Instructions.RemoveAt(baseInt);
                                break;
                            }
                            asm.EntryPoint.Body.Instructions.RemoveAt(baseInt);
                        }
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Trial Check & Alert removed!");
                        break;
                    }
                }
            }
            foreach (Instruction inst in asm.EntryPoint.Body.Instructions)
            {
                if (inst.OpCode.Equals(OpCodes.Ldnull) && asm.EntryPoint.Body.Instructions[asm.EntryPoint.Body.Instructions.IndexOf(inst)+1].Operand.ToString().Contains("Inequality"))
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Removing Starting Message Box...");
                    int baseInt = asm.EntryPoint.Body.Instructions.IndexOf(inst) - 1;
                    for (int x = baseInt; x < asm.EntryPoint.Body.Instructions.Count; x++)
                    {
                        asm.EntryPoint.Body.Instructions.RemoveAt(baseInt);
                        if (asm.EntryPoint.Body.Instructions[baseInt].OpCode.Equals(OpCodes.Pop))
                        {
                            asm.EntryPoint.Body.Instructions.RemoveAt(baseInt);
                            break;
                        }
                    }
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Removed Starting Message Box!");
                    break;
                }
            }
            ModuleWriterOptions moduleWriterOptions = new ModuleWriterOptions(asm);
            moduleWriterOptions.MetadataOptions.Flags |= MetadataFlags.PreserveAll;
            moduleWriterOptions.Logger = DummyLogger.NoThrowInstance;
            NativeModuleWriterOptions nativeModuleWriterOptions = new NativeModuleWriterOptions(asm, true);
            nativeModuleWriterOptions.MetadataOptions.Flags |= MetadataFlags.PreserveAll;
            nativeModuleWriterOptions.Logger = DummyLogger.NoThrowInstance;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine();
            Console.WriteLine("Saving '" + Path.GetFileNameWithoutExtension(args[0]) + "-netshrinkNoTrial" + Path.GetExtension(args[0]) + "'...");
            try
            {
                asm.Write(Path.GetFileNameWithoutExtension(args[0]) + "-netshrinkNoTrial" + Path.GetExtension(args[0]));
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("'" + Path.GetFileNameWithoutExtension(args[0]) + "-netshrinkNoTrial" + Path.GetExtension(args[0]) + "' successfully saved!");
                Console.ResetColor();
                Console.WriteLine("Press any keys to exit...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to save '" + Path.GetFileNameWithoutExtension(args[0]) + "-netshrinkNoTrial" + Path.GetExtension(args[0]) + "' (" + ex.Message + ")");
                Console.ResetColor();
                Console.WriteLine("Press any keys to exit...");
                Console.ReadKey();
            }
        }
    }
}