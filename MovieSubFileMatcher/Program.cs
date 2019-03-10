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
                Backup(o, subs);

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
                    // generate new name for subtitle

                    if (o.MatchSubtitle)
                    {
                        string newSubName = Path.Combine(o.Target, Path.GetFileNameWithoutExtension(movieSorted[i]) + ".srt");
                        if (!ShouldRename(newSubName))
                        {
                            continue;
                        }

                        // do rename and handle any possible error that may happen
                        SafeRename(subsSorted[i], newSubName);
                    }
                    else // rename movie file to match subtitle
                    {
                        // combine subtitle file name with movie extension to make new name for movie file
                        string movieName = Path.Combine(o.Target, Path.GetFileNameWithoutExtension(subsSorted[i]) + Path.GetExtension(movieSorted[i]));

                        // check if movie with same name already exists
                        if (!ShouldRename(movieName))
                        {
                            continue;
                        }

                        // do rename and handle any possible error that may happen
                        SafeRename(movieSorted[i], movieName);
                    }

                }

            }).
            WithNotParsed(e =>
            {
                // parsing fail
                Console.WriteLine($"Parsing process failed!");
            });

        }

        private static void SafeRename(string oldName, string newName)
        {
            string oldFileName = Path.GetFileName(oldName);
            string newNFileName = Path.GetFileName(newName);
            try
            {
                File.Move(oldName, newName);
                // rename movie file name to match subtitle name
                Console.WriteLine($"{oldFileName} => {newNFileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine($"Fail renaming: {oldFileName} => {newNFileName}");
            }
        }

        private static bool ShouldRename(string newSubName)
        {
            // subtitle file with same name already exists or trying to rename already renamed subtitle files
            if (File.Exists(newSubName))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"! {Path.GetFileName(newSubName)}");
                Console.ResetColor();
                return false;
            }

            return true;
        }

        private static void Backup(Options o, string[] subtitleFiles)
        {
            if (!o.DeleteOriginal)
            {
                return;
            }

            string backupDir = Path.Combine(o.Target, "backup-subs");
            Directory.CreateDirectory(backupDir);

            // copy all subtitle file into a backup directory
            foreach (string subfile in subtitleFiles)
            {
                string newFile = Path.Combine(backupDir, Path.GetFileName(subfile));
                if (File.Exists(newFile))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Backup already exists, ignoreing <<{Path.GetFileName(newFile)}>>");
                    Console.ResetColor();
                    continue; // ignore or override?
                }
                File.Copy(subfile, newFile);
            }
        } 

    }
}

// todo: 
// allow specifying movie file extension in option
// code match subtitle which indicates that the movie file should get it's name from subtitle file (inverse of the current logic)
// add uni-test