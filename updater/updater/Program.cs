using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace updater
{
    class Program
    {
        static private string foldername = "temp_myprogram_folder";
        static private string zipname = "temp_myprogram";
        static void Main(string[] args)
        {
            Thread.Sleep(500);
            //try
            //{
            string process = args[0].Replace(".exe", "");
            Console.WriteLine("Terminate process!");
            while (Process.GetProcessesByName(process).Length > 0)
            {
                Process[] myProcesses2 = Process.GetProcessesByName(process);
                for (int i = 1; i < myProcesses2.Length; i++) { myProcesses2[i].Kill(); }

                Thread.Sleep(300);
            }

            string[] files = Directory.GetFiles(foldername).Select(file => file.Replace(foldername + "\\", "")).ToArray();
            for (int i = 0; i < files.Length; i++)
            {
                if (File.Exists(files[i])) { File.Delete(files[i]); }
            }
            foreach (string file in files)
            {
                File.Move(foldername + "\\" + file, file);
            }

            string[] directories = Directory.GetDirectories(foldername).Select(file => file.Replace(foldername + "\\", "")).ToArray();
            for (int i = 0; i < directories.Length; i++)
            {
                if (Directory.Exists(directories[i])) { Directory.Delete(directories[i], true); }
            }
            foreach (string directory in directories)
            {
                Directory.Move(foldername + "\\" + directory, directory);
            }

            if (Directory.Exists(foldername)) Directory.Delete(foldername, true);
                if (File.Exists(zipname)) { File.Delete(zipname); }

                Console.WriteLine("Starting " + args[0]);
                Process.Start(args[0]);
            //}
            //catch (Exception) { }
        }
        private static List<string> DirSearch(string sDir)
        {
            List<string> files = new List<string>();
            try
            {
                foreach (string f in Directory.GetFiles(sDir))
                {
                    files.Add(f);
                }
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    files.AddRange(DirSearch(d));
                }
            }
            catch (Exception) { }

            return files;
        }
    }
}
