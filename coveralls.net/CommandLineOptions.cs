﻿using CommandLine;
using Coveralls.Lib;

namespace coveralls.net
{
    public class CommandLineOptions : ICommandOptions
    {
        [Value(0)]
        public string InputFile { get; set; }

        [Option('p', "parser", HelpText = "Parser to use (Currently only supports OpenCover)")]
        public ParserTypes Parser { get; set; }

        private bool _useOpenCover;
        [Option("opencover")]
        public bool UseOpenCover
        {
            get { return _useOpenCover; }
            set
            {
                _useOpenCover = value;
                if (_useOpenCover) Parser = ParserTypes.OpenCover;
            }
        }
    }
}