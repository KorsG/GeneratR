///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument<string>("target", "Default");
var configuration = Argument<string>("configuration", "Release");

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////

var isLocal = BuildSystem.IsLocalBuild;

var sourceDir = Directory("./src");
var artifactsDir = Directory("./artifacts");

var solutions = new List<FilePath> {
    File("./src/GeneratR.sln"),
};

var projectsToPack = new List<FilePath> {
    Directory(sourceDir) + File("GeneratR/GeneratR.csproj"),
    Directory(sourceDir) + File("GeneratR.Database.Templates/GeneratR.Database.Templates.csproj"),
    Directory(sourceDir) + File("GeneratR.T4/GeneratR.T4.csproj"),
};

GitVersion gitVersionResult;

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup((ctx) =>
{
    // Executed BEFORE the first task.
    Information("Running tasks...");

    Information("Retrieving gitversion...");
    gitVersionResult = GitVersion(new GitVersionSettings {
        OutputType = GitVersionOutput.Json,
    });

    Information("GitVersion.InformationalVersion {0}", gitVersionResult.InformationalVersion);
    Information("GitVersion.AssemblySemVer {0}", gitVersionResult.AssemblySemVer);
    Information("GitVersion.LegacySemVerPadded {0}", gitVersionResult.LegacySemVerPadded);
    Information("GitVersion.SemVer {0}", gitVersionResult.SemVer);
    Information("GitVersion.MajorMinorPatch {0}", gitVersionResult.MajorMinorPatch);
    Information("GitVersion.NuGetVersion {0}", gitVersionResult.NuGetVersion);
    Information("GitVersion.NuGetVersionV2 {0}", gitVersionResult.NuGetVersionV2);
    Information("GitVersion.BranchName {0}", gitVersionResult.BranchName);   
});

Teardown((ctx) =>
{
    // Executed AFTER the last task.
    Information("Finished running tasks...");
});

///////////////////////////////////////////////////////////////////////////////
// TASK DEFINITIONS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    var sourceDirCleanBinPattern = sourceDir.ToString() + "/**/bin/*";
    Information($"Cleaning source bin directory '{sourceDirCleanBinPattern}'...");
    CleanDirectories(sourceDirCleanBinPattern);

    var sourceDirCleanObjPattern = sourceDir.ToString() + "/**/obj/*";
    Information($"Cleaning source obj directory '{sourceDirCleanObjPattern}'...");
    CleanDirectories(sourceDirCleanObjPattern);
    
    Information($"Cleaning artifacts directory '{artifactsDir}'...");
    CleanDirectory(artifactsDir);
});

Task("Restore")
    .Does(() =>
{
    // Restore all packages.
    foreach(var solution in solutions)
    {
        Information($"Restoring {solution}...");
        NuGetRestore(solution);
    }
});

Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .Does(() =>
{
    // https://cakebuild.net/api/Cake.Common.Tools.DotNetCore.Build/DotNetCoreBuildSettings/
    var versionArg = "/p:Version=" + gitVersionResult.NuGetVersion;
    var buildSettings = new DotNetCoreBuildSettings() {
        Configuration = configuration,
        NoRestore = true, // Already done by restore task.
        Verbosity = Cake.Common.Tools.DotNetCore.DotNetCoreVerbosity.Minimal,
		ArgumentCustomization = args => args.Append(versionArg),
    };

    // Build all solutions.
    foreach(var solution in solutions)
    {
        Information("Building {0}", solution);
        DotNetCoreBuild(solution.ToString(), buildSettings);
    }
});

Task("Pack")
    .IsDependentOn("Build")
    .Does(() =>
{
    // https://cakebuild.net/api/Cake.Common.Tools.DotNetCore.Pack/DotNetCorePackSettings/
    var versionArg = "/p:PackageVersion=" + gitVersionResult.NuGetVersion;
    var settings = new DotNetCorePackSettings
    {
        NoBuild = true, // Already done by build task.
        NoRestore = true, // Already done by restore task.
        Configuration = configuration,
        OutputDirectory = $"{artifactsDir}",
		ArgumentCustomization = args => args.Append(versionArg)
    };

    // Pack projects.
    foreach(var project in projectsToPack)
    {
        Information("Packing {0}...", project);      
        DotNetCorePack(project.ToString(), settings);
    }
});

///////////////////////////////////////////////////////////////////////////////
// TARGETS
///////////////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Build")
    .IsDependentOn("Pack");

///////////////////////////////////////////////////////////////////////////////
// EXECUTION
///////////////////////////////////////////////////////////////////////////////

RunTarget(target);

///////////////////////////////////////////////////////////////////////////////
// HELPER METHODS
///////////////////////////////////////////////////////////////////////////////