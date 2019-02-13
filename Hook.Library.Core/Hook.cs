using System;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using Mod.Properties;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Hook.Library.Core
{
    public static class Hook
    {
        public static void UnMod(string[] args)
        {
            try
            {
                var dll_name = args[0];
                var arg = args.ToList();
                arg.Remove(args[0]);

                arg.Remove("--unmod");

                Console.WriteLine("Loading " + args[0]);

                var dll_temp = "." + Path.GetFileName(dll_name) + "_temp";

                File.Copy(Path.GetFullPath(dll_name), Path.GetFullPath(dll_temp), true);

                var exe = AssemblyDefinition.ReadAssembly(Path.GetFullPath(dll_temp)).MainModule;

                foreach (var mod in arg)
                {
                    Console.WriteLine("Loading " + mod);
                    try
                    {
                        var mod_dll_attr = Assembly.LoadFile(Path.GetFullPath(mod));
                        var mod_dll = AssemblyDefinition.ReadAssembly(Path.GetFullPath(mod)).MainModule;


                        foreach (var mod_t in from t in mod_dll.Types
                                              where IsDefinedAsMod(t)
                                              select t)
                        {
                            Console.WriteLine("Found type " + mod_t.Name);

                            var exe_t = exe.GetTypes().Single(t => t.Name == GetHook(mod_dll_attr.GetType(mod_t.FullName)));

                            Console.WriteLine("Using type " + exe_t.Name);

                            foreach (var mod_m in mod_t.Methods)
                            {
                                try
                                {
                                    Console.WriteLine("Found method " + mod_m.Name);

                                    var exe_m = exe_t.Methods.Single(m => m.Name == mod_m.Name);
                                    var mod_m_ref = exe.ImportReference(mod_m);

                                    if (IsInstalled(mod_m_ref, exe_m))
                                    {
                                        Console.WriteLine("The mod is installed, starting injection...");
                                        foreach (var bodyInstruction in from t in exe_m.Body.Instructions.ToArray()
                                                                        where t.OpCode == OpCodes.Call &&
                                                                              ((MethodReference)t.Operand).FullName == mod_m_ref.FullName
                                                                        select t)
                                        {
                                            exe_m.Body.Instructions.Remove(bodyInstruction);
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("The mod wasn't installed, skipping it.");
                                    }
                                }
                                catch
                                {

                                }
                            }
                            Console.WriteLine("Writing into the assembly...");

                            exe.Write(Path.GetFullPath(dll_name));
                            File.Delete(dll_temp);

                            Console.WriteLine("Finished !");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }

            }
            catch
            {
                Console.WriteLine("Usage: \nmod <assembly to mod> <mod assembly 1> <mod assembly 2> ...");
            }

        }

        public static void InjectMod(string[] args)
        {
            try
            {
                var dll_name = args[0];
                var arg = args.ToList();
                arg.Remove(args[0]);

                Console.WriteLine("Loading " + args[0]);

                var dll_temp = "." + Path.GetFileName(dll_name) + "_temp";

                File.Copy(Path.GetFullPath(dll_name), Path.GetFullPath(dll_temp), true);

                var exe = AssemblyDefinition.ReadAssembly(Path.GetFullPath(dll_temp)).MainModule;

                foreach (var mod in arg)
                {
                    Console.WriteLine("Loading " + mod);
                    try
                    {
                        var mod_dll_attr = Assembly.LoadFile(Path.GetFullPath(mod));
                        var mod_dll = AssemblyDefinition.ReadAssembly(Path.GetFullPath(mod)).MainModule;


                        foreach (var mod_t in from t in mod_dll.Types
                                              where IsDefinedAsMod(t)
                                              select t)
                        {
                            Console.WriteLine("Found type " + mod_t.Name);

                            var exe_t = exe.GetTypes().Single(t => t.Name == GetHook(mod_dll_attr.GetType(mod_t.FullName)));

                            Console.WriteLine("Using type " + exe_t.Name);

                            foreach (var mod_m in mod_t.Methods)
                            {
                                try
                                {
                                    Console.WriteLine("Found method " + mod_m.Name);

                                    var exe_m = exe_t.Methods.Single(m => m.Name == mod_m.Name);
                                    var mod_m_ref = exe.ImportReference(mod_m);

                                    if (!IsInstalled(mod_m_ref, exe_m))
                                    {
                                        Console.WriteLine("The mod wasn't installed, starting injection...");
                                        exe_m.Body.GetILProcessor().InsertBefore(exe_m.Body.Instructions[0], Instruction.Create(OpCodes.Call, mod_m_ref));
                                    }
                                    else
                                    {
                                        Console.WriteLine("The mod was already installed; skipping it");
                                    }
                                }
                                catch
                                {

                                }
                            }
                            Console.WriteLine("Writing into the assembly...");

                            exe.Write(Path.GetFullPath(dll_name));
                            File.Delete(dll_temp);

                            Console.WriteLine("Finished !");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
            catch
            {
                Console.WriteLine("Usage: \nmod <assembly to mod> <mod assembly 1> <mod assembly 2> ...");
            }
        }


        public static bool IsDefinedAsMod(Type t) => t.IsDefined(typeof(ModAttribute), false);

        public static bool IsDefinedAsMod(TypeDefinition t) => Assembly.LoadFile(t.Module.FileName).GetType(t.FullName ?? throw new Exception("Failed to get the class")).IsDefined(typeof(ModAttribute), false);


        public static string GetHook(Type t) => ((ModAttribute)t.GetCustomAttributes(false).Single(a => a is ModAttribute)).GetClass();

        [Obsolete("Use a type instead")]
        public static string GetHook(TypeDefinition t, Assembly mod) => ((ModAttribute)Attribute.GetCustomAttributes(Assembly.LoadFile(Path.GetFullPath(t.Module.FileName)).GetType(t.FullName), false)
                .Single(a => a is ModAttribute))
            .GetClass();

        static bool IsInstalled(MethodReference mod, MethodDefinition original)
        {
            Console.WriteLine("Checking if the mod was already installed.");

            foreach (var bodyInstruction in from t in original.Body.Instructions.ToArray() where t.OpCode == OpCodes.Call && ((MethodReference)t.Operand).FullName == mod.FullName select t)
                return true;

            return false;
        }

    }
}
