#addin "nuget:https://api.nuget.org/v3/index.json?package=Cake.Npx&version=1.7.0"

var target = Argument("target", "Semantic");
var configuration = Argument("configuration", "Release");
var releaseVersion = "0.0.0";
var changesDetectedSinceLastRelease = false;

Action<NpxSettings> requiredSemanticVersionPackages = settings => settings
    .AddPackage("semantic-release@13.1.1")
    .AddPackage("@semantic-release/changelog@1.0.0")
    .AddPackage("@semantic-release/git@3.0.0")
    .AddPackage("@semantic-release/exec@2.0.0");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .WithCriteria(c => HasArgument("rebuild"))
    .Does(() =>
{
    CleanDirectory($"./SampleLibrary/bin/{configuration}");
});

Task("Build")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetBuild("./dotnet-core-semantic-release.sln", new DotNetBuildSettings
    {
        Configuration = configuration,
    });
});

Task("Semantic")
    .Does(() =>
{
    string[] semanticReleaseOutput;
    Npx("semantic-release", "--dry-run", requiredSemanticVersionPackages, out semanticReleaseOutput);

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