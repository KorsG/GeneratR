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
};

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup((ctx) =>
{
    // Executed BEFORE the first task.
    Information("Running tasks...");
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
    var buildSettings = new DotNetCoreBuildSettings() {
        Configuration = configuration,
        NoRestore = true, // Already done by restore task.
        Verbosity = Cake.Common.Tools.DotNetCore.DotNetCoreVerbosity.Minimal    };

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
    var settings = new DotNetCorePackSettings
    {
        NoBuild = true, // Already done by build task.
        NoRestore = true, // Already done by restore task.
        Configuration = configuration,
        OutputDirectory = $"{artifactsDir}"
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