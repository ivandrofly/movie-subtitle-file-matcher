using System;
using System.IO;
using System.Linq;
using CommandLine;


namespace MovieSubFileMatcher
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(o =>
            {
                Console.WriteLine($"target: {o.Target}");
                if (!Path.IsPathRooted(o.Target))
                {
                    throw new InvalidOperationException("Target path");
                }

                // get subtitle files
                var subs = Directory.GetFiles(o.Target, "*.srt");

                // create a copy of the original 
                if (!o.DeleteOriginal)
                {
                    string backupDir = Path.Combine(o.Target, "backup-subs");
                    Directory.CreateDirectory(backupDir);

                    // copy all subtitle file into a backup directory
                    foreach (string sub in subs)
                    {
                        string newFile = Path.Combine(backupDir, Path.GetFileName(sub));
                        if (File.Exists(newFile))
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Backup already exists, ignoreing <<{Path.GetFileName(newFile)}>>");
                            Console.ResetColor();
                            continue; // ignore or override?
                        }
                        File.Copy(sub, newFile);
                    }
                }

                // sort subtitle file
                var subsSorted = subs.OrderBy(s => Path.GetFileName(s)).ToList();

                // sort movie file
                var movieSorted = Directory.GetFiles(o.Target, "*.mp4").OrderBy(m => Path.GetFileName(m)).ToList();

                // only to renaming if the amount of subtitle file in current directory match the
                // amount of movie/tv-show files in order to avoid unnessesary renamings
                if (subsSorted.Count != movieSorted.Count)
                {
                    Console.WriteLine("The amount of files in target directory missmatched!");
                    return;
                }

                // do move/rename subtitle file to match movie file name in current directory
                for (int i = 0; i < movieSorted.Count; i++)
                {
                    string newSubName = Path.Combine(o.Target, Path.GetFileNameWithoutExtension(movieSorted[i]) + ".srt");

                    // subtitle file with same name already exists or trying to rename already renamed subtitle files
                    if (File.Exists(newSubName))
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"! {Path.GetFileName(newSubName)}");
                        Console.ResetColor();
                        continue;
                    }

                    // display info old-file-name => new-file-name
                    Console.WriteLine($"{Path.GetFileName(subsSorted[i])} => {Path.GetFileName(newSubName)}");
                    File.Move(subsSorted[i], newSubName);
                }

            }).
            WithNotParsed(e =>
            {
                // parsing fail
                Console.WriteLine($"Parsing process failed!");
            });

        }
    }
}

// todo: 
// allow specifying movie file extension in option
// code match subtitle which indicates that the movie file should get it's name from subtitle file (inverse of the current logic)