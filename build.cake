#addin nuget:?package=Cake.CMake&version=1.3.1

var target = Argument("target", "Test");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("EnsureNugetSource")
    .Does(()=>
{
    EnsureDirectoryExists("_Result");
    EnsureDirectoryExists("_Result/NuGet");
});



Task("LiteBrokerNativeGenerate")
    .Does(() =>
{
    var settings = new CMakeSettings
    {
        OutputPath = "deps/LiteBroker/build",
        SourcePath = "deps/LiteBroker"
    };

    CMake(settings);
});

Task("LiteBrokerNativeBuild")
    .IsDependentOn("LiteBrokerNativeGenerate")
    .Does(()=>
{
    var settings = new CMakeBuildSettings
    {
        BinaryPath = "deps/LiteBroker/build",
        Configuration = configuration
    };

    CMakeBuild(settings);
});

Task("LiteBrokerNativeNugetPack")
    .IsDependentOn("EnsureNugetSource")
    .IsDependentOn("LiteBrokerNativeBuild")
    .Does(()=>
    {
        DotNetPack("./eng/LiteBroker.Native.Windows.csproj");
    });


Task("Clean")
    .WithCriteria(c => HasArgument("rebuild"))
    .Does(() =>
{
    CleanDirectory($"./src/LiteBrokerNet/bin/{configuration}");
});

Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("LiteBrokerNativeNugetPack")
    .Does(() =>
{
    DotNetBuild("./src/LiteBrokerNet.sln", new DotNetBuildSettings
    {
        Configuration = configuration,
    });
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    DotNetTest("./src/LiteBrokerNet.sln", new DotNetTestSettings
    {
        Configuration = configuration,
        NoBuild = true,
    });
});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);