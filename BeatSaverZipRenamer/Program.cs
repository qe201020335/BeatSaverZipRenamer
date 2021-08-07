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
        private static char[] invalid = Path.GetInvalidFileNameChars();
        private static Mutex mut = new Mutex();
        private static async Task<string> GetNewFileName(string hash)
        {
            Beatmap beatmap = await bs.BeatmapByHash(hash);
            if (beatmap == null)
            {
                PrintColor(hash, ConsoleColor.Yellow);
                PrintLineColor(" NOT A HASH or NOTFOUND or TIMEOUT.", ConsoleColor.Magenta);
                return String.Empty;
            }
            string result = $"{beatmap.ID} ({beatmap.Metadata.SongName} - {beatmap.Metadata.LevelAuthorName})";

            result = RemoveIllegalChar(result);
            
            PrintColor(hash, ConsoleColor.Blue);
            Console.Write(" is ");
            PrintLineColor(result, ConsoleColor.Green);
            
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
                try
                {
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
                catch (IOException)
                {
                    PrintLineColor($"{newpath} already exists", ConsoleColor.Yellow);
                }
                catch (ArgumentException)
                {
                    PrintLineColor($"{newname} not valid or contains invalid characters", ConsoleColor.Yellow);
                }
                catch (Exception e)
                {
                    PrintLineColor(e.ToString(), ConsoleColor.Red);
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
                MessageBox.Show("Drag hash named files or folders on this tool.", prgname);
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
                        PrintLineColor(arg + " DOES NOT EXIST", ConsoleColor.Red);
                        continue;  // not exist
                    }

                    string name = Path.GetFileName(arg);
                    if (name.Contains(" "))
                    {
                        PrintLineColor(name + " IS NOT A HASH NAMED FILE", ConsoleColor.Yellow);
                        continue;
                    }
                    
                    string fullpath = Path.GetFullPath(arg);
                    tasks.Add(Rename(fullpath));
                    Thread.Sleep(250);
                }
                catch (Exception e)
                {
                    PrintLineColor(e.ToString(), ConsoleColor.Red);
                }
            }

            Task.WaitAll(tasks.ToArray());
            
            bs.Dispose();
            
            Console.Write("\a");
            MessageBox.Show("Done.", prgname);
        }

        public static void PrintColor(string content, ConsoleColor color)
        {
            mut.WaitOne();
            Console.ForegroundColor = color;
            Console.Write(content);
            Console.ResetColor();
            mut.ReleaseMutex();
        }

        public static void PrintLineColor(string content, ConsoleColor color) => PrintColor(content + "\n", color);

        public static string RemoveIllegalChar(string name)
        {
            
            foreach (char c in invalid)
            {
                name = name.Replace(c.ToString(), ""); 
            }

            return name;
        }
    }
}