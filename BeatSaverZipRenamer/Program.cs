using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BeatSaverSharp;
using BeatSaverSharp.Models;

namespace BeatSaverZipRenamer
{
    internal class Program
    {
        private static BeatSaver bs;
        private static string prgname;
        private static async Task<string> GetNewFileName(string hash)
        {
            Beatmap beatmap = await bs.BeatmapByHash(hash);
            if (beatmap == null)
            {
                Console.WriteLine($"{hash} NOT A HASH or NOTFOUND or TIMEOUT.");
                return String.Empty;
            }
            string result = $"{beatmap.ID} ({beatmap.Metadata.SongName} - {beatmap.Metadata.LevelAuthorName})";
            Console.WriteLine($"{hash} IS {result}");
            return result;
        }

        private static async Task Rename(string path)
        {
            string hash = Path.GetFileNameWithoutExtension(path);
            string parent = Path.GetDirectoryName(path);
            string extension = Path.GetExtension(path);
            string newname = await GetNewFileName(hash);

            if (newname != String.Empty)
            {
                string newpath = parent + "\\" + newname + extension;
                if (Directory.Exists(path))
                {
                    // a directory
                    Directory.Move(path, newpath);
                }
                else if (File.Exists(path))
                {
                    // a file
                    File.Move(path, newpath);
                }
            }
        }

        public static void Main(string[] args)
        {
            AssemblyName ver = typeof(Program).Assembly.GetName();
            prgname = ver.Name;
            
            int len = args.Length;
            if (len == 0)
            {
                MessageBox.Show("Drag hash named zips on this tool.", prgname);
                return;
            }
            
            bs = new BeatSaver(ver.Name, ver.Version);

            List<Task> tasks = new List<Task>();
            
            foreach (string arg in args)
            {
                try
                {
                    if (!(File.Exists(arg) || Directory.Exists(arg)))
                    {
                        Console.WriteLine(arg + " DOES NOT EXIST");
                        continue;  // not exist
                    }
                    string fullpath = Path.GetFullPath(arg);
                    tasks.Add(Rename(fullpath));
                    Thread.Sleep(250);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }

            Task.WaitAll(tasks.ToArray());
            
            bs.Dispose();
            
            Console.Write("\a");
            MessageBox.Show("Done.", prgname);
        }
    }
}