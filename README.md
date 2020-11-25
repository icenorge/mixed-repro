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

- ❗️ Fails: building the master branch using `msbuild.exe`
- ✅ Works: builds the master branch using the IDE, VS2019 16.8.X.
- ✅ Works: modify the `TestProject` adding a direct reference to `TransientProjectB`, then build using `msbuild.exe`. As in https://github.com/icenorge/mixed-repro/pull/1


### Microsoft recommendations:

- It has earlier been _recommended_ from MS to run `nuget.exe restore` for the legacy projects (`packages.config`).  [Source](https://docs.microsoft.com/en-us/nuget/reference/msbuild-targets#restore-target).
- ℹ️ Update: MS recommends to use `msbuild -restore /p:RestorePackagesConfig=true` for 16.8 + 5.8 to avoid version conflicts between nuget.exe and msbuild versions. [Source](https://twitter.com/dsplaisted/status/1331683532570181633).



- It's recommended to match `nuget.exe` version with the `msbuild.exe`/ `Visual Studio` / `dotnet.exe`:

> It is advised that your toolchain includes VS 16.8+, MSBuild 16.8+ and .NET 5.0+ when using NuGet 5.8 or later.
> If you restored using an earlier version of NuGet than 5.8 or 16.8, then please delete your obj/ folders and restore & compile with the tools listed above or higher.

Sources: [[1]](https://developercommunity.visualstudio.com/comments/1266427/view.html), [[2]](https://devblogs.microsoft.com/nuget/getting-started-with-nuget-5-8/#known-issues)


### Known bugs:

_.NET 5 SDK,  v.5.0.100_
- "This will fix issues where CI builds fail because they download an older version of NuGet.exe and use this for restore." https://github.com/dotnet/sdk/pull/14517 (❗️ merged, but not released)

## Errors

### 'project.assets.json' doesn't have a target for {TFM}'

If mismatching `nuget.exe` version with msbuild version (i.e using nuget 5.7 with 16.8, instead of nuget 5.8):

>C:\Program%20Files\dotnet\sdk\5.0.100\Sdks\Microsoft.NET.Sdk\targets\Microsoft.PackageDependencyResolution.targets(241,5): error NETSDK1005: 
Assets file 'D:\a\mixed-repro\mixed-repro\TransientProjectB\obj\project.assets.json' doesn't have a target for 'netstandard2.0'. 
Ensure that restore has run and that you have included 'netstandard2.0' in the TargetFrameworks for your project. 
[D:\a\mixed-repro\mixed-repro\TransientProjectB\TransientProjectB.csproj]

Solution: _Downgrade to 16.7 tooling (uninstall .NET 5 SDK, and downgrade msbuild 16.8 to 16.7)_

### Unable to resolve transient references 

Hypothesis: Resolving the transient dependency worked in 16.7, but no longer works in 16.8

>D:\a\mixed-repro\mixed-repro\TestProject\UnitTest1.cs(3,7): error CS0246: The type or namespace name 'TransientProjectB' could not be found (are you missing a using directive or an assembly reference?) [D:\a\mixed-repro\mixed-repro\TestProject\TestProject.csproj]

Solution: _Make the transient reference a direct reference in the test project._

## Gotchas

### Cake
Building via Cake and their boostrapper `build.ps1` will use the installed `nuget.exe` version, if it already exists. If not, it will download and install the latest. You might end up using a nuget.exe version not compatible with the msbuild / sdk tooling you are using.