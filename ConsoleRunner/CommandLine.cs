﻿using System;
using System.Collections.Generic;
using System.Globalization;

namespace Chutzpah
{
    public class CommandLine
    {
        private const string TeamcityProjectName = "TEAMCITY_PROJECT_NAME";

        private readonly Stack<string> arguments = new Stack<string>();

        protected CommandLine(string[] args)
        {
            for (var i = args.Length - 1; i >= 0; i--)
                arguments.Push(args[i]);

            Files = new List<string>();
            TeamCity = Environment.GetEnvironmentVariable(TeamcityProjectName) != null;
            Parse();
        }


        public bool Debug { get; protected set; }

        public bool Silent { get; protected set; }
        
        public bool OpenInBrowser { get; protected set; }

        public int? TimeOutMilliseconds { get; protected set; }

        public bool TeamCity { get; protected set; }

        public bool Wait { get; protected set; }

        public IList<string> Files { get; set; }

        private static void GuardNoOptionValue(KeyValuePair<string, string> option)
        {
            if (option.Value != null)
                throw new ArgumentException(String.Format("unknown command line option: {0}", option.Value));
        }

        public static CommandLine Parse(string[] args)
        {
            return new CommandLine(args);
        }

        protected void Parse()
        {
            if (!arguments.Peek().StartsWith("/"))
            {
                var fileName = arguments.Pop();
                AddFileOption(fileName);
            }

            while (arguments.Count > 0)
            {
                KeyValuePair<string, string> option = PopOption(arguments);
                string optionName = option.Key.ToLowerInvariant();

                if (!optionName.StartsWith("/"))
                    throw new ArgumentException(String.Format("unknown command line option: {0}", option.Key));

                if (optionName == "/wait")
                {
                    GuardNoOptionValue(option);
                    Wait = true;
                }
                else if (optionName == "/debug")
                {
                    GuardNoOptionValue(option);
                    Debug = true;
                }
                else if (optionName == "/openinbrowser")
                {
                    GuardNoOptionValue(option);
                    OpenInBrowser = true;
                }                
                else if (optionName == "/silent")
                {
                    GuardNoOptionValue(option);
                    Silent = true;
                }
                else if (optionName == "/teamcity")
                {
                    GuardNoOptionValue(option);
                    TeamCity = true;
                }                        
                else if (optionName == "/timeoutmilliseconds")
                {
                    AddTimeoutOption(option.Value);
                }                
                else if (optionName == "/file" || optionName == "/path")
                {
                    AddFileOption(option.Value);
                }
                else
                {
                    if (!optionName.StartsWith("/"))
                        throw new ArgumentException(String.Format("unknown command line option: {0}", option.Key));
                }
            }
        }

        private void AddTimeoutOption(string value)
        {
            int timeout;
            if(string.IsNullOrEmpty(value) || !int.TryParse(value,out timeout) || timeout < 0)
            {
                throw new ArgumentException(
                    "invalid or missing argument for /timeoutmilliseconds.  Expecting a postivie integer");
            }

            TimeOutMilliseconds = timeout;
        }

        private void AddFileOption(string file)
        {
            if (string.IsNullOrEmpty(file))
            {
                throw new ArgumentException(
                    "missing argument for /file.  Expecting a file path to a test file (e.g. test.html)");
            }

            Files.Add(file);
        }

        private static KeyValuePair<string, string> PopOption(Stack<string> arguments)
        {
            string option = arguments.Pop();
            string value = null;

            if (arguments.Count > 0 && !arguments.Peek().StartsWith("/"))
                value = arguments.Pop();

            return new KeyValuePair<string, string>(option, value);
        }
    }
}