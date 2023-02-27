#addin nuget:?package=Cake.CMake&version=1.3.1
#tool nuget:?package=NuGet.CommandLine&version=5.9.1

var target = Argument("target", "Test");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////


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
    .IsDependentOn("LiteBrokerNativeBuild")
    .Does(()=>{
var nuGetPackSettings   = new NuGetPackSettings {
                                     BasePath                = "./deps/LiteBroker/build",
                                     OutputDirectory         = "./_Result/Nuget"
                                 };

     NuGetPack("./eng/LiteBrokerNative.nuspec", nuGetPackSettings);
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