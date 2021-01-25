using System.Security.Cryptography;
using UnityEngine.Events;

namespace SWAC
{


    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using UnityEngine;

    public class Injections : MonoBehaviour
    {
        private bool isBusy;
        public ProtInt delay = 1;
        public bool StartScene;
        private Process process;
        private string file;
        
        public class DLLInjectionEvent : UnityEvent {}
        private DLLInjectionEvent m_onInjection = new DLLInjectionEvent();
        
        public DLLInjectionEvent onInjection
        {
            get { return m_onInjection; }
            set { m_onInjection = value; }
        }
        private void Awake()
        {
            file = Directory.GetCurrentDirectory() + "\\signs.txt";
            if (StartScene)
            {
                
                if (File.Exists(file))
                {
                    File.Delete(file);
                }

                File.Create(file).Dispose();
                process = Process.GetCurrentProcess();
                List<string> modules = getDlls(process );
                using (StreamWriter sw = File.AppendText(file))
                {
                    sw.WriteLine(arrayToString(modules.ToArray()));

                }	
                
            }
        }
        private string arrayToString(string[] array)
        {
            string ggg = "";
            foreach (string l in array)
            {
                ggg = ggg + l+"\n";
            }

            return ggg;
        }

        private void Update()
        {
            if (!isBusy)
            {
                StartCoroutine(TestCoroutine());
            }

        }

        private IEnumerator TestCoroutine()
        {
            isBusy = true;

            string modules = arrayToString(getDlls(process).ToArray());
            string fmodule;
            using (StreamReader sw = File.OpenText(file))
            {
                fmodule = sw.ReadToEnd();
                
            }
            int riznutha = modules.Length - fmodule.Length;
            if (riznutha >= 4 || riznutha <= -4)
            {
                m_onInjection.Invoke();
            }
            yield return new WaitForSeconds(delay);
            isBusy = false;
        }

        private List<string> getDlls(Process p)
        {
            List<String> modulesi = new List<string>();
            foreach (Module pm in CollectModules(p))
            {
                modulesi.Add(pm.ModuleName);
            }

            return modulesi;
        }
        private List<Module> CollectModules(Process process)
        {
            List<Module> collectedModules = new List<Module>();

            IntPtr[] modulePointers = new IntPtr[0];
            int bytesNeeded;

            // Determine number of modules
            if (!Native.EnumProcessModulesEx(process.Handle, modulePointers, 0, out bytesNeeded,
                (uint) Native.ModuleFilter.ListModulesAll))
            {
                return collectedModules;
            }

            int totalNumberofModules = bytesNeeded / IntPtr.Size;
            modulePointers = new IntPtr[totalNumberofModules];

            // Collect modules from the process
            if (Native.EnumProcessModulesEx(process.Handle, modulePointers, bytesNeeded, out bytesNeeded,
                (uint) Native.ModuleFilter.ListModulesAll))
            {
                for (int index = 0; index < totalNumberofModules; index++)
                {
                    StringBuilder moduleFilePath = new StringBuilder(1024);
                    Native.GetModuleFileNameEx(process.Handle, modulePointers[index], moduleFilePath,
                        (uint) (moduleFilePath.Capacity));

                    string moduleName = Path.GetFileName(moduleFilePath.ToString());
                    Native.ModuleInformation moduleInformation;
                    Native.GetModuleInformation(process.Handle, modulePointers[index], out moduleInformation,
                        (uint) (IntPtr.Size * (modulePointers.Length)));

                    // Convert to a normalized module and add it to our list
                    Module module = new Module(moduleName, moduleInformation.lpBaseOfDll,
                        moduleInformation.SizeOfImage);
                    collectedModules.Add(module);
                }
            }

            return collectedModules;
        }
    }

    public class Native
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct ModuleInformation
        {
            public IntPtr lpBaseOfDll;
            public uint SizeOfImage;
            public IntPtr EntryPoint;
        }

        internal enum ModuleFilter
        {
            ListModulesDefault = 0x0,
            ListModules32Bit = 0x01,
            ListModules64Bit = 0x02,
            ListModulesAll = 0x03,
        }

        [DllImport("psapi.dll")]
        public static extern bool EnumProcessModulesEx(IntPtr hProcess,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U4)] [In] [Out]
            IntPtr[] lphModule, int cb, [MarshalAs(UnmanagedType.U4)] out int lpcbNeeded, uint dwFilterFlag);

        [DllImport("psapi.dll")]
        public static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName,
            [In] [MarshalAs(UnmanagedType.U4)] uint nSize);

        [DllImport("psapi.dll", SetLastError = true)]
        public static extern bool GetModuleInformation(IntPtr hProcess, IntPtr hModule, out ModuleInformation lpmodinfo,
            uint cb);
    }

    public class Module
    {
        public Module(string moduleName, IntPtr baseAddress, uint size)
        {
            ModuleName = moduleName;
            BaseAddress = baseAddress;
            Size = size;
        }

        public string ModuleName { get; }
        public IntPtr BaseAddress { get; }
        public uint Size { get;}
    }

}