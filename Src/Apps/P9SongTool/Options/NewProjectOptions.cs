﻿using CommandLine;
using CommandLine.Text;

namespace P9SongTool.Options;

[Verb("newproj", HelpText = "Create new song project from scratch")]
public class NewProjectOptions : BaseOptions
{
    [Value(0, Required = true, MetaName = "dirPath", HelpText = "Path to output project directory")]
    public string OutputPath { get; set; }

    [Option('n', "name", HelpText = "Shortname of song (ex. \"temporarysec\")", Required = true)]
    public string ProjectName { get; set; }

    [Usage(ApplicationAlias = Constants.APPLICATION_EXE_NAME)]
    public static IEnumerable<Example> Examples
        => new[]
        {
            new Example("Convert milo archive to song project", new NewProjectOptions
            {
                OutputPath = "project_temporarysec",
                ProjectName = "temporarysec"
            })
        };
}
