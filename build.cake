#addin "nuget:https://api.nuget.org/v3/index.json?package=Cake.Npx&version=1.7.0"

var target = Argument("target", "Semantic");
var configuration = Argument("configuration", "Release");
var releaseVersion = "0.0.0";
var artifactsDir =  Directory("./artifacts");
var changesDetectedSinceLastRelease = false;

Action<NpxSettings> requiredSemanticVersionPackages = settings => settings
    .AddPackage("@semantic-release/changelog")
    .AddPackage("@semantic-release/git");
    

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .WithCriteria(c => HasArgument("rebuild"))
    .Does(() =>
{
    CleanDirectories($"./**/bin/{configuration}");
    CleanDirectory(artifactsDir);
});

Task("Build")
    .Does(() =>
{
    var solutions = GetFiles("./**/*.sln");
    foreach(var solution in solutions)
    {
        Information("Building solution {0} v{1}", solution.GetFilenameWithoutExtension(), releaseVersion);

        var assemblyVersion = $"{releaseVersion}.0";

        DotNetCoreBuild(solution.FullPath, new DotNetCoreBuildSettings()
        {
            Configuration = configuration,
            MSBuildSettings = new DotNetCoreMSBuildSettings()
                .WithProperty("Version", assemblyVersion)
                .WithProperty("AssemblyVersion", assemblyVersion)
                .WithProperty("FileVersion", assemblyVersion)
                // 0 = use as many processes as there are available CPUs to build the project
                // see: https://develop.cakebuild.net/api/Cake.Common.Tools.MSBuild/MSBuildSettings/60E763EA
                .SetMaxCpuCount(0)
        });
    }
});

Task("Test")
    .Does(() =>
{
    var xunitArgs = "--no-build --configuration " + configuration;

    var testProjects = GetFiles("./**/*.Tests.csproj");
    foreach(var testProject in testProjects)
    {
        Information("Testing project {0} with args {1}", testProject.GetFilenameWithoutExtension(), xunitArgs);

        DotNetTool(testProject.FullPath, "test", xunitArgs);
    }
});

Task("Semantic")
    .Does(() =>
{
    string[] semanticReleaseOutput;
    Npx("semantic-release", args => args.Append(""), requiredSemanticVersionPackages, out semanticReleaseOutput);

    Information(string.Join(Environment.NewLine, semanticReleaseOutput));

    var nextSemanticVersionNumber = ExtractNextSemanticVersionNumber(semanticReleaseOutput);

    if (nextSemanticVersionNumber == null) {
        Warning("There are no relevant changes, skipping release");
    } else {
        Information("Next semantic version number is {0}", nextSemanticVersionNumber);
        releaseVersion = nextSemanticVersionNumber;
        changesDetectedSinceLastRelease = true;
    }
});

Task("Package")
    .Does(() =>
{
    var projects = GetFiles("./**/*.csproj");
    foreach(var project in projects)
    {
        var projectDirectory = project.GetDirectory().FullPath;
        if(projectDirectory.EndsWith("Tests")) continue;

        Information("Packaging project {0} v{1}", project.GetFilenameWithoutExtension(), releaseVersion);

        var assemblyVersion = $"{releaseVersion}.0";

        DotNetCorePack(project.FullPath, new DotNetCorePackSettings {
            Configuration = configuration,
            OutputDirectory = artifactsDir,
            NoBuild = true,
            MSBuildSettings = new DotNetCoreMSBuildSettings()
                .WithProperty("Version", assemblyVersion)
                .WithProperty("AssemblyVersion", assemblyVersion)
                .WithProperty("FileVersion", assemblyVersion)
        });
    }
});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);

///////////////////////////////////////////////////////////////////////////////
// Helpers
///////////////////////////////////////////////////////////////////////////////

string ExtractNextSemanticVersionNumber(string[] semanticReleaseOutput)
{
    var extractRegEx = new System.Text.RegularExpressions.Regex("^.+next release version is (?<SemanticVersionNumber>.*)$");

    return semanticReleaseOutput
        .Select(line => extractRegEx.Match(line).Groups["SemanticVersionNumber"].Value)
        .Where(line => !string.IsNullOrWhiteSpace(line))
        .SingleOrDefault();
}