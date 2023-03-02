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
        SourcePath = "deps/LiteBroker",
        Platform = "x64"
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
        if (IsRunningOnWindows())
        {
            DotNetPack("./eng/LiteBroker.Native.Windows.x64.csproj");
        } 
        else if(IsRunningOnLinux())
        {
            DotNetPack("./eng/LiteBroker.Native.Linux.x64.csproj");
        }
 
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
    DotNetBuild("./src/LiteBrokerNet/LiteBrokerNet.csproj", new DotNetBuildSettings
    {
        Configuration = configuration,
    });
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    DotNetTest("./src/LiteBrokerNet/LiteBrokerNet.csproj", new DotNetTestSettings
    {
        Configuration = configuration,
        NoBuild = true,
    });
});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);