﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Phantasma.Blockchain;
using Phantasma.Core.Utils;
using Phantasma.Core.Log;
using Phantasma.Cryptography;
using Phantasma.VM.Utils;
using Phantasma.Blockchain.Contracts;
using Phantasma.Blockchain.Storage;
using Phantasma.Core;
using Phantasma.CodeGen;
using Phantasma.CodeGen.Assembler;

namespace Phantasma.AssemblerConsole
{
    [TestClass]
    public class Program
    {
        /*
        public static void Main(string[] args)
        {
            var arguments = new Arguments(args);

            string sourceFilePath = null;

            string filepath = $@"D:\Repos\PhantasmaAssembler\PhantasmaAssembler\Tests\hello.asm";

            try
            {
                if (filepath != "")
                    sourceFilePath = filepath;
                else
                    sourceFilePath = arguments.GetDefaultValue();
            }
            catch
            {
                Trace.WriteLine($"{System.AppDomain.CurrentDomain.FriendlyName}.exe <filename.asm>");
                System.Environment.Exit(-1);
            }

            string[] lines = null;
            try
            {
                lines = File.ReadAllLines(sourceFilePath);
            }
            catch
            {
                Console.WriteLine("Error reading " + sourceFilePath);
                Environment.Exit(-1);
            }

            IEnumerable<Semanteme> semantemes = null;
            try
            {
                semantemes = Semanteme.ProcessLines(lines);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error parsing " + sourceFilePath + " :" + e.Message);
                Environment.Exit(-1);
            }

            var sb = new ScriptBuilder();
            byte[] script = null;

            try
            {               
                foreach (var entry in semantemes)
                {
                    Console.WriteLine($"{entry}");
                    entry.Process(sb);
                }
                script = sb.ToScript();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error assembling " + sourceFilePath + " :" + e.Message);
                Environment.Exit(-1);
            }

            var extension = Path.GetExtension(sourceFilePath);
            var outputName = sourceFilePath.Replace(extension, ScriptFormat.Extension);

            try
            {
                File.WriteAllBytes(outputName, script);
            }
            catch
            {
                Console.WriteLine("Error generating " + outputName);
                Environment.Exit(-1);
            }


            Console.WriteLine("Executing script...");
            var keys = KeyPair.Generate();
            var nexus = new Nexus("vmnet", keys.Address, new ConsoleLogger());
            var tx = new Transaction(nexus.Name, nexus.RootChain.Name, script, 0, 0);

            var changeSet = new StorageChangeSetContext(new MemoryStorageContext());

            var vm = new RuntimeVM(tx.Script, nexus.RootChain, null, tx, changeSet, true);
            
            var state = vm.Execute();
            Console.WriteLine("State = " + state);


            /*var vm = new TestVM(script);
            vm.Execute();*/
        //}

        [TestMethod]
        public void Move()
        {

        }

        [TestMethod]
        public void Copy()
        {

        }

        [TestMethod]
        public void Load()
        {
            var scriptString = new string[]
            {
                $@"load r1, """""
            };
        }

        [TestMethod]
        public void Push()
        {

        }

        [TestMethod]
        public void Pop()
        {

        }

        [TestMethod]
        public void Swap()
        {

        }

#region LogicalOps
        [TestMethod]
        public void Not()
        {
            var scriptString = new string[]
            {
                $@"load r1, true",
                @"not r1, r2",
                @"push r2",
                @"ret"
            };

            var vm = ExecuteScript(scriptString);

            Assert.IsTrue(vm.Stack.Count == 1);

            var result = vm.Stack.Pop().AsString();
            Assert.IsTrue(result == "false");

            scriptString = new string[]
            {
                $@"load r1, ""abc""",
                @"not r1, r2",
                @"push r2",
                @"ret"
            };

            try
            {
                vm = ExecuteScript(scriptString);
            }
            catch (InternalTestFailureException ie)
            {
                throw ie;
            }
            catch (Exception e)
            {
                return;
            }

            throw new Exception("Didn't throw an exception after trying to NOT a non-bool variable.");
        }

        [TestMethod]
        public void And()
        {
            string[] scriptString;
            RuntimeVM vm;

            var args = new List<List<string>>()
            {
                new List<string>() {"true", "true", "true"},
                new List<string>() {"true", "false", "false"},
                new List<string>() {"false", "true", "false"},
                new List<string>() {"false", "false", "false"}
            };

            for (int i = 0; i < args.Count; i++)
            {
                var argsLine = args[i];
                string r1 = argsLine[0];
                string r2 = argsLine[1];
                string target = argsLine[2];
                
                scriptString = new string[]
                {
                    $@"load r1, {r1}",
                    $@"load r2, {r2}",
                    @"and r1, r2, r3",
                    @"push r3",
                    @"ret"
                };
                
                vm = ExecuteScript(scriptString);

                Assert.IsTrue(vm.Stack.Count == 1);

                var result = vm.Stack.Pop().AsString();
                Assert.IsTrue(result == target);
            }

            scriptString = new string[]
            {
                $@"load r1, ""abc""",
                $@"load r2, false",
                @"and r1, r2, r3",
                @"push r3",
                @"ret"
            };

            try
            {
                vm = ExecuteScript(scriptString);
            }
            catch (InternalTestFailureException ie)
            {
                throw ie;
            }
            catch (Exception e)
            {
                return;
            }

            throw new Exception("Didn't throw an exception after trying to AND a non-bool variable.");
        }

        [TestMethod]
        public void Or()
        {
            string[] scriptString;
            RuntimeVM vm;

            var args = new List<List<string>>()
            {
                new List<string>() {"true", "true", "true"},
                new List<string>() {"true", "false", "true"},
                new List<string>() {"false", "true", "true"},
                new List<string>() {"false", "false", "false"}
            };

            for (int i = 0; i < args.Count; i++)
            {
                var argsLine = args[i];
                string r1 = argsLine[0];
                string r2 = argsLine[1];
                string target = argsLine[2];

                scriptString = new string[]
                {
                    $@"load r1, {r1}",
                    $@"load r2, {r2}",
                    @"or r1, r2, r3",
                    @"push r3",
                    @"ret"
                };

                vm = ExecuteScript(scriptString);

                Assert.IsTrue(vm.Stack.Count == 1);

                var result = vm.Stack.Pop().AsString();
                Assert.IsTrue(result == target);
            }

            scriptString = new string[]
            {
                $@"load r1, ""abc""",
                $@"load r2, false",
                @"or r1, r2, r3",
                @"push r3",
                @"ret"
            };

            try
            {
                vm = ExecuteScript(scriptString);
            }
            catch (InternalTestFailureException ie)
            {
                throw ie;
            }
            catch (Exception e)
            {
                return;
            }

            throw new Exception("Didn't throw an exception after trying to OR a non-bool variable.");
        }

        [TestMethod]
        public void Xor()
        {
            string[] scriptString;
            RuntimeVM vm;

            var args = new List<List<string>>()
            {
                new List<string>() {"true", "true", "false"},
                new List<string>() {"true", "false", "true"},
                new List<string>() {"false", "true", "true"},
                new List<string>() {"false", "false", "false"}
            };

            for (int i = 0; i < args.Count; i++)
            {
                var argsLine = args[i];
                string r1 = argsLine[0];
                string r2 = argsLine[1];
                string target = argsLine[2];

                scriptString = new string[]
                {
                    $@"load r1, {r1}",
                    $@"load r2, {r2}",
                    @"xor r1, r2, r3",
                    @"push r3",
                    @"ret"
                };

                vm = ExecuteScript(scriptString);

                Assert.IsTrue(vm.Stack.Count == 1);

                var result = vm.Stack.Pop().AsString();
                Assert.IsTrue(result == target);
            }

            scriptString = new string[]
            {
                $@"load r1, ""abc""",
                $@"load r2, false",
                @"xor r1, r2, r3",
                @"push r3",
                @"ret"
            };

            try
            {
                vm = ExecuteScript(scriptString);
            }
            catch (InternalTestFailureException ie)
            {
                throw ie;
            }
            catch (Exception e)
            {
                return;
            }

            throw new Exception("Didn't throw an exception after trying to XOR a non-bool variable.");
        }

        [TestMethod]
        public void Equals()
        {
            string[] scriptString;
            RuntimeVM vm;
            string result;

            var args = new List<List<string>>()
            {
                new List<string>() {"true", "true", "true"},
                new List<string>() {"true", "false", "false"},
                new List<string>() {"1", "1", "true"},
                new List<string>() {"1", "2", "false"},
                new List<string>() {@"""hello""", @"""hello""", "true"},
                new List<string>() {@"""hello""", @"""world""", "false"},
                
                //TODO: add lines for bytes, structs, enums and structs
            };

            for (int i = 0; i < args.Count; i++)
            {
                var argsLine = args[i];
                string r1 = argsLine[0];
                string r2 = argsLine[1];
                string target = argsLine[2];

                scriptString = new string[]
                {
                    $@"load r1, {r1}",
                    $@"load r2, {r2}",
                    @"equal r1, r2, r3",
                    @"push r3",
                    @"ret"
                };

                vm = ExecuteScript(scriptString);

                Assert.IsTrue(vm.Stack.Count == 1);

                
                result = vm.Stack.Pop().AsString();
                Assert.IsTrue(result == target);
            }
        }

        [TestMethod]
        public void LessThan()
        {
            string[] scriptString;
            RuntimeVM vm;

            var args = new List<List<string>>()
            {
                new List<string>() {"1", "0", "false"},
                new List<string>() {"1", "1", "false"},
                new List<string>() {"1", "2", "true"},
            };

            for (int i = 0; i < args.Count; i++)
            {
                var argsLine = args[i];
                string r1 = argsLine[0];
                string r2 = argsLine[1];
                string target = argsLine[2];

                scriptString = new string[]
                {
                    $@"load r1, {r1}",
                    $@"load r2, {r2}",
                    @"lt r1, r2, r3",
                    @"push r3",
                    @"ret"
                };

                vm = ExecuteScript(scriptString);

                Assert.IsTrue(vm.Stack.Count == 1);

                var result = vm.Stack.Pop().AsString();
                Assert.IsTrue(result == target);
            }

            scriptString = new string[]
            {
                $@"load r1, ""abc""",
                $@"load r2, 2",
                @"lt r1, r2, r3",
                @"push r3",
                @"ret"
            };

            try
            {
                vm = ExecuteScript(scriptString);
            }
            catch (InternalTestFailureException ie)
            {
                throw ie;
            }
            catch (Exception e)
            {
                return;
            }

            throw new Exception("Didn't throw an exception after trying to compare non-integer variables.");
        }

        [TestMethod]
        public void GreaterThan()
        {
            string[] scriptString;
            RuntimeVM vm;

            var args = new List<List<string>>()
            {
                new List<string>() {"1", "0", "true"},
                new List<string>() {"1", "1", "false"},
                new List<string>() {"1", "2", "false"},
            };

            for (int i = 0; i < args.Count; i++)
            {
                var argsLine = args[i];
                string r1 = argsLine[0];
                string r2 = argsLine[1];
                string target = argsLine[2];

                scriptString = new string[]
                {
                    $@"load r1, {r1}",
                    $@"load r2, {r2}",
                    @"gt r1, r2, r3",
                    @"push r3",
                    @"ret"
                };

                vm = ExecuteScript(scriptString);

                Assert.IsTrue(vm.Stack.Count == 1);

                var result = vm.Stack.Pop().AsString();
                Assert.IsTrue(result == target);
            }

            scriptString = new string[]
            {
                $@"load r1, ""abc""",
                $@"load r2, 2",
                @"gt r1, r2, r3",
                @"push r3",
                @"ret"
            };

            try
            {
                vm = ExecuteScript(scriptString);
            }
            catch (InternalTestFailureException ie)
            {
                throw ie;
            }
            catch (Exception e)
            {
                return;
            }

            throw new Exception("Didn't throw an exception after trying to compare non-integer variables.");
        }

        [TestMethod]
        public void LesserThanOrEquals()
        {
            string[] scriptString;
            RuntimeVM vm;

            var args = new List<List<string>>()
            {
                new List<string>() {"1", "0", "false"},
                new List<string>() {"1", "1", "true"},
                new List<string>() {"1", "2", "true"},
            };

            for (int i = 0; i < args.Count; i++)
            {
                var argsLine = args[i];
                string r1 = argsLine[0];
                string r2 = argsLine[1];
                string target = argsLine[2];

                scriptString = new string[]
                {
                    $@"load r1, {r1}",
                    $@"load r2, {r2}",
                    @"lte r1, r2, r3",
                    @"push r3",
                    @"ret"
                };

                vm = ExecuteScript(scriptString);

                Assert.IsTrue(vm.Stack.Count == 1);

                var result = vm.Stack.Pop().AsString();
                Assert.IsTrue(result == target);
            }

            scriptString = new string[]
            {
                $@"load r1, ""abc""",
                $@"load r2, 2",
                @"lte r1, r2, r3",
                @"push r3",
                @"ret"
            };

            try
            {
                vm = ExecuteScript(scriptString);
            }
            catch (InternalTestFailureException ie)
            {
                throw ie;
            }
            catch (Exception e)
            {
                return;
            }

            throw new Exception("Didn't throw an exception after trying to compare non-integer variables.");
        }

        [TestMethod]
        public void GreaterThanOrEquals()
        {
            string[] scriptString;
            RuntimeVM vm;

            var args = new List<List<string>>()
            {
                new List<string>() {"1", "0", "true"},
                new List<string>() {"1", "1", "true"},
                new List<string>() {"1", "2", "false"},
            };

            for (int i = 0; i < args.Count; i++)
            {
                var argsLine = args[i];
                string r1 = argsLine[0];
                string r2 = argsLine[1];
                string target = argsLine[2];

                scriptString = new string[]
                {
                    $@"load r1, {r1}",
                    $@"load r2, {r2}",
                    @"gte r1, r2, r3",
                    @"push r3",
                    @"ret"
                };

                vm = ExecuteScript(scriptString);

                Assert.IsTrue(vm.Stack.Count == 1);

                var result = vm.Stack.Pop().AsString();
                Assert.IsTrue(result == target);
            }

            scriptString = new string[]
            {
                $@"load r1, ""abc""",
                $@"load r2, 2",
                @"gte r1, r2, r3",
                @"push r3",
                @"ret"
            };

            try
            {
                vm = ExecuteScript(scriptString);
            }
            catch (InternalTestFailureException ie)
            {
                throw ie;
            }
            catch (Exception e)
            {
                return;
            }

            throw new Exception("Didn't throw an exception after trying to compare non-integer variables.");
        }

        [TestMethod]
        public void Min()
        {
            string[] scriptString;
            RuntimeVM vm;

            var args = new List<List<string>>()
            {
                new List<string>() {"1", "0", "0"},
                new List<string>() {"1", "1", "1"},
                new List<string>() {"1", "2", "1"},
            };

            for (int i = 0; i < args.Count; i++)
            {
                var argsLine = args[i];
                string r1 = argsLine[0];
                string r2 = argsLine[1];
                string target = argsLine[2];

                scriptString = new string[]
                {
                    $@"load r1, {r1}",
                    $@"load r2, {r2}",
                    @"min r1, r2, r3",
                    @"push r3",
                    @"ret"
                };

                vm = ExecuteScript(scriptString);

                Assert.IsTrue(vm.Stack.Count == 1);

                var result = vm.Stack.Pop().AsString();
                Assert.IsTrue(result == target);
            }

            scriptString = new string[]
            {
                $@"load r1, ""abc""",
                $@"load r2, 2",
                @"min r1, r2, r3",
                @"push r3",
                @"ret"
            };

            try
            {
                vm = ExecuteScript(scriptString);
            }
            catch (InternalTestFailureException ie)
            {
                throw ie;
            }
            catch (Exception e)
            {
                return;
            }

            throw new Exception("Didn't throw an exception after trying to compare non-integer variables.");
        }

        [TestMethod]
        public void Max()
        {
            string[] scriptString;
            RuntimeVM vm;

            var args = new List<List<string>>()
            {
                new List<string>() {"1", "0", "1"},
                new List<string>() {"1", "1", "1"},
                new List<string>() {"1", "2", "2"},
            };

            for (int i = 0; i < args.Count; i++)
            {
                var argsLine = args[i];
                string r1 = argsLine[0];
                string r2 = argsLine[1];
                string target = argsLine[2];

                scriptString = new string[]
                {
                    $@"load r1, {r1}",
                    $@"load r2, {r2}",
                    @"max r1, r2, r3",
                    @"push r3",
                    @"ret"
                };

                vm = ExecuteScript(scriptString);

                Assert.IsTrue(vm.Stack.Count == 1);

                var result = vm.Stack.Pop().AsString();
                Assert.IsTrue(result == target);
            }

            scriptString = new string[]
            {
                $@"load r1, ""abc""",
                $@"load r2, 2",
                @"max r1, r2, r3",
                @"push r3",
                @"ret"
            };

            try
            {
                vm = ExecuteScript(scriptString);
            }
            catch (InternalTestFailureException ie)
            {
                throw ie;
            }
            catch (Exception e)
            {
                return;
            }

            throw new Exception("Didn't throw an exception after trying to compare non-integer variables.");
        }
        #endregion

#region NumericOps
        [TestMethod]
        public void Increment()
        {
            string[] scriptString;
            RuntimeVM vm;

            var args = new List<List<string>>()
            {
                new List<string>() {"1", "2"},
            };

            for (int i = 0; i < args.Count; i++)
            {
                var argsLine = args[i];
                string r1 = argsLine[0];
                string target = argsLine[1];

                scriptString = new string[]
                {
                    $@"load r1, {r1}",
                    @"inc r1",
                    @"push r1",
                    @"ret"
                };

                vm = ExecuteScript(scriptString);

                Assert.IsTrue(vm.Stack.Count == 1);

                var result = vm.Stack.Pop().AsString();
                Assert.IsTrue(result == target);
            }

            scriptString = new string[]
            {
                $@"load r1, ""hello""",
                @"inc r1",
                @"push r1",
                @"ret"
            };

            try
            {
                vm = ExecuteScript(scriptString);
            }
            catch (InternalTestFailureException ie)
            {
                throw ie;
            }
            catch (Exception e)
            {
                return;
            }

            throw new Exception("Didn't throw an exception after trying to compare non-integer variables.");
        }

        [TestMethod]
        public void Decrement()
        {
            string[] scriptString;
            RuntimeVM vm;

            var args = new List<List<string>>()
            {
                new List<string>() {"2", "1"},
            };

            for (int i = 0; i < args.Count; i++)
            {
                var argsLine = args[i];
                string r1 = argsLine[0];
                string target = argsLine[1];

                scriptString = new string[]
                {
                    $@"load r1, {r1}",
                    @"dec r1",
                    @"push r1",
                    @"ret"
                };

                vm = ExecuteScript(scriptString);

                Assert.IsTrue(vm.Stack.Count == 1);

                var result = vm.Stack.Pop().AsString();
                Assert.IsTrue(result == target);
            }

            scriptString = new string[]
            {
                $@"load r1, ""hello""",
                @"dec r1",
                @"push r1",
                @"ret"
            };

            try
            {
                vm = ExecuteScript(scriptString);
            }
            catch (InternalTestFailureException ie)
            {
                throw ie;
            }
            catch (Exception e)
            {
                return;
            }

            throw new Exception("Didn't throw an exception after trying to compare non-integer variables.");
        }

        [TestMethod]
        public void Sign()
        {
            string[] scriptString;
            RuntimeVM vm;

            var args = new List<List<string>>()
            {
                new List<string>() {"-1", "-1"},
                new List<string>() {"0", "0"},
                new List<string>() {"1", "1"}
            };

            for (int i = 0; i < args.Count; i++)
            {
                var argsLine = args[i];
                string r1 = argsLine[0];
                string target = argsLine[1];

                scriptString = new string[]
                {
                    $@"load r1, {r1}",
                    @"sign r1, r2",
                    @"push r2",
                    @"ret"
                };

                vm = ExecuteScript(scriptString);

                Assert.IsTrue(vm.Stack.Count == 1);

                var result = vm.Stack.Pop().AsString();
                Assert.IsTrue(result == target);
            }

            scriptString = new string[]
            {
                $@"load r1, ""abc""",
                $@"load r2, false",
                @"and r1, r2, r3",
                @"push r3",
                @"ret"
            };

            try
            {
                vm = ExecuteScript(scriptString);
            }
            catch (InternalTestFailureException ie)
            {
                throw ie;
            }
            catch (Exception e)
            {
                return;
            }

            throw new Exception("Didn't throw an exception after trying to AND a non-bool variable.");
        }

        #endregion
        [TestMethod]
        public void Cat()
        {
            var args = new List<List<string>>()
            {
                new List<string>() {"Hello", null},
                new List<string>() {null, " world"},
                new List<string>() {"", ""},
                new List<string>() {"Hello ", "world"}
            };

            for(int i = 0; i < args.Count; i++)
            {
                var argsLine = args[i];
                string r1 = argsLine[0] == null ? null : $@"""{argsLine[0]}""";
                string r2 = argsLine[1] == null ? null : $@"""{argsLine[1]}""";

                var scriptString = new string[1];

                switch (i)
                {
                    case 0:
                        scriptString = new string[]
                        {
                            $@"load r1, {r1}",
                            @"cat r1, r2, r3",
                            @"push r3",
                            @"ret"
                        };
                        break;
                    case 1:
                        scriptString = new string[]
                        {
                            $@"load r2, {r2}",
                            @"cat r1, r2, r3",
                            @"push r3",
                            @"ret"
                        };
                        break;
                    case 2:
                        scriptString = new string[]
                        {
                            @"cat r1, r2, r3",
                            @"push r3",
                            @"ret"
                        };
                        break;
                    case 3:
                        scriptString = new string[]
                        {
                            $@"load r1, {r1}",
                            $@"load r2, {r2}",
                            @"cat r1, r2, r3",
                            @"push r3",
                            @"ret"
                        };
                        break;
                }

                var vm = ExecuteScript(scriptString);

                Assert.IsTrue(vm.Stack.Count == 1);

                var result = vm.Stack.Pop().AsString();
                Assert.IsTrue(result == String.Concat(argsLine[0], argsLine[1]));
            }

            var scriptString2 = new string[]
            {
                $@"load r1, ""Hello""",
                $@"load r2, 1",
                @"cat r1, r2, r3",
                @"push r3",
                @"ret"
            };

            try
            {
                var vm2 = ExecuteScript(scriptString2);
            }
            catch (InternalTestFailureException ie)
            {
                throw ie;
            }
            catch (Exception e)
            {
                return;
            }

            throw new Exception("VM did not throw exception when trying to cat a string and a non-string object, and it should");
        }

        private RuntimeVM ExecuteScript(string[] scriptString)
        {
            var script = BuildScript(scriptString);

            var keys = KeyPair.Generate();
            var nexus = new Nexus("vmnet", keys.Address, new ConsoleLogger());
            var tx = new Transaction(nexus.Name, nexus.RootChain.Name, script, 0, 0);

            var changeSet = new StorageChangeSetContext(new MemoryStorageContext());

            var vm = new RuntimeVM(tx.Script, nexus.RootChain, null, tx, changeSet, true);
            vm.Execute();

            return vm;
        }


        private byte[] BuildScript(string[] lines)
        {
            IEnumerable<Semanteme> semantemes = null;
            try
            {
                semantemes = Semanteme.ProcessLines(lines);
            }
            catch (Exception e)
            {
                throw new InternalTestFailureException("Error parsing the script");
            }

            var sb = new ScriptBuilder();
            byte[] script = null;

            try
            {
                foreach (var entry in semantemes)
                {
                    Trace.WriteLine($"{entry}");
                    entry.Process(sb);
                }
                script = sb.ToScript();
            }
            catch (Exception e)
            {
                throw new InternalTestFailureException("Error assembling the script");
            }

            return script;
        }
        
    }
}