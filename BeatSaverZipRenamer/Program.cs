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
        private static async Task GetNewFileName(string hash)
        {
            Beatmap beatmap = await bs.BeatmapByHash(hash);
            if (beatmap == null)
            {
                Console.WriteLine($"{hash} NOTFOUND or TIMEOUT.");
                return;
            }
            string result = $"{beatmap.ID} ({beatmap.Metadata.SongName} - {beatmap.Metadata.LevelAuthorName}).zip";
            Console.WriteLine(result);
            // return result;
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
                string filename = Path.GetFileNameWithoutExtension(arg);
                tasks.Add(GetNewFileName(filename));
                Thread.Sleep(250);
            }

            Task.WaitAll(tasks.ToArray());
            
            bs.Dispose();
            
            MessageBox.Show("Done.", prgname);
        }
    }
}