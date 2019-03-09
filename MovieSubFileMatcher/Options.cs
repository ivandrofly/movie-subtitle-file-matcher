using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovieSubFileMatcher
{
    class Options
    {
        /// <summary>
        /// the default is that subtitle file must match movie file name
        /// </summary>
        [Option('m', "match")]
        public bool MatchSubtitle { get; set; }

        /// <summary>
        /// Delete rename without createing a copy of the original subtitle file.
        /// </summary>
        [Option('d', "do", HelpText = "Delete original subtitle")]
        public bool DeleteOriginal { get; set; }

        /// <summary>
        /// The path that should be targetted
        /// </summary>
        [Option('t', "Target", HelpText = "Target directory", Required = true)]
        public string Target { get; set; }
    }
}
