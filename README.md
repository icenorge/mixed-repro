# Nuget restore issues

This is a repro for an issue in solutions that contain a mix of `packages.config` (old) and `PackageReference` (new) based projects when using the 16.8 tooling (msbuild, visual studio)

### Scenario

Dependency chain::

```
TestProject (netcoreapp3.1) 
╚ MvcWebApplication (net461, packages.config) 
  ╚ DependencyA (netstandard2) 
    ╚ TransientProjectB (netstandard2)
```

The `TestProject` use classes from `TransientProjectB` in a test.


### Microsoft recommendations:

- It's _recommended_ to run `nuget.exe restore` for the old projects (`packages.config`)

Source: https://docs.microsoft.com/en-us/nuget/reference/msbuild-targets#restore-target

- It's recommended to match `nuget.exe` version with the `msbuild.exe`/ `Visual Studio` / `dotnet.exe`:

> It is advised that your toolchain includes VS 16.8+, MSBuild 16.8+ and .NET 5.0+ when using NuGet 5.8 or later.
> If you restored using an earlier version of NuGet than 5.8 or 16.8, then please delete your obj/ folders and restore & compile with the tools listed above or higher.

Source: https://developercommunity.visualstudio.com/comments/1266427/view.html


## Errors


### 'project.assets.json' doesn't have a target for {TFM}'

If mismatching `nuget.exe` version with msbuild version (i.e using nuget 5.7 with 16.8, instead of nuget 5.8):

>C:\Program%20Files\dotnet\sdk\5.0.100\Sdks\Microsoft.NET.Sdk\targets\Microsoft.PackageDependencyResolution.targets(241,5): error NETSDK1005: 
Assets file 'D:\a\mixed-repro\mixed-repro\TransientProjectB\obj\project.assets.json' doesn't have a target for 'netstandard2.0'. 
Ensure that restore has run and that you have included 'netstandard2.0' in the TargetFrameworks for your project. 
[D:\a\mixed-repro\mixed-repro\TransientProjectB\TransientProjectB.csproj]

Solution: _Downgrade to 16.7 tooling (uninstall .NET 5 SDK, and downgrade msbuild 16.8 to 16.7)_

### Unable to resolve transient references 

Hypothesis: Resolving the transient dependency worked in 16.7.

>D:\a\mixed-repro\mixed-repro\TestProject\UnitTest1.cs(3,7): error CS0246: The type or namespace name 'TransientProjectB' could not be found (are you missing a using directive or an assembly reference?) [D:\a\mixed-repro\mixed-repro\TestProject\TestProject.csproj]

Solution: _Make the transient reference a direct reference in the test project._

## Gotchas

### Cake
Building via Cake and their boostrapper `build.ps1` will use the installed `nuget.exe` version, if it already exists. If not, it will download and install the latest. You might end up using a nuget.exe version not compatible with the msbuild / sdk tooling you are using.