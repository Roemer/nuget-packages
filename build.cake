///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

var temp = Directory("./.temp");
var nugetDir = Directory("./.nuget");

TaskSetup(setupContext =>
{
    CleanDirectory(temp);
});

Task("Clean-Output")
    .Does(() =>
{
    CleanDirectory(nugetDir);
});

Task("All")
    .IsDependentOn("Clean-Output")
    .IsDependentOn("JMeter")
    .IsDependentOn("dotnet-framework-sonarscanner")
    .IsDependentOn("7-Zip.StandaloneConsole")
    .IsDependentOn("Flyway.CommandLine")
    .Does(() =>
{
});

Task("JMeter")
    .IsDependentOn("Clean-Output")
    .Does(() =>
{
    var jMeterVersion = "5.6.3";
    var jMeterPath = temp + Directory($"apache-jmeter-{jMeterVersion}");
    var libPath = jMeterPath + Directory("lib");
    var extPath = libPath + Directory("ext");

    // Download and Extract JMeter
    var resource = DownloadFile($"https://dlcdn.apache.org//jmeter/binaries/apache-jmeter-{jMeterVersion}.zip");
    Unzip(resource, temp);

    // Install the plugin manager
    var pluginManagerVersion = "1.10";
    DownloadFile($"http://search.maven.org/remotecontent?filepath=kg/apc/jmeter-plugins-manager/{pluginManagerVersion}/jmeter-plugins-manager-{pluginManagerVersion}.jar", extPath + File($"jmeter-plugins-manager-{pluginManagerVersion}.jar"));
    // Install the command runner
    var cmdRunnerVersion = "2.3";
    DownloadFile($"https://search.maven.org/remotecontent?filepath=kg/apc/cmdrunner/{cmdRunnerVersion}/cmdrunner-{cmdRunnerVersion}.jar", libPath + File($"cmdrunner-{cmdRunnerVersion}.jar"));
    // Generate the cmd wrappers
    var exitCodeWithArgument = StartProcess("java", new ProcessSettings{ Arguments = $"-cp {jMeterPath}/lib/ext/jmeter-plugins-manager-{pluginManagerVersion}.jar org.jmeterplugins.repository.PluginManagerCMDInstaller" });

    // Create the NuGet package
    var nuGetPackSettings = new NuGetPackSettings {
        Id                          = "JMeter",
        Version                     = jMeterVersion,
        Authors                     = new[] {"phmarques", "Roemer"},
        Description                 = "The Apache JMeter™ application is open source software, a 100% pure Java application designed to load test functional behavior and measure performance. It was originally designed for testing Web Applications but has since expanded to other test functions.",
        ProjectUrl                  = new Uri("https://github.com/Roemer/nuget-packages"),
        LicenseUrl                  = new Uri("https://www.apache.org/licenses/"),
        RequireLicenseAcceptance    = false,
        Symbols                     = false,
        NoPackageAnalysis           = true,
        Files                       = new [] {
                                        new NuSpecContent { Source = $@".temp\apache-jmeter-{jMeterVersion}\**", Exclude = $@".temp\apache-jmeter-{jMeterVersion}\docs\**;.temp\apache-jmeter-{jMeterVersion}\printable_docs\**", Target = "tools" }
                                    },
        BasePath                    = "./",
        OutputDirectory             = nugetDir,
        ArgumentCustomization       = args => args.Append("-NoDefaultExcludes"),
    };
    NuGetPack(nuGetPackSettings);
});

Task("dotnet-framework-sonarscanner")
    .IsDependentOn("Clean-Output")
    .Does(() =>
{
    var version = "9.2.0";
    var versionSuffix = "110275";

    var fullVersionString = $"{version}.{versionSuffix}";
    var resource = DownloadFile($"https://github.com/SonarSource/sonar-scanner-msbuild/releases/download/{fullVersionString}/sonar-scanner-{fullVersionString}-net-framework.zip");
    Unzip(resource, temp);

    var nuGetPackSettings = new NuGetPackSettings {
        Id                          = "dotnet-framework-sonarscanner",
        Title                       = "SonarScanner for .Net Framework",
        Version                     = version,
        Authors                     = new[] {"SonarSource", "Microsoft", "Roemer"},
        Description                 = "The SonarScanner for .Net Framework allows easy analysis of any .NET project with SonarCloud/SonarQube.",
        ProjectUrl                  = new Uri("https://github.com/Roemer/nuget-packages"),
        LicenseUrl                  = new Uri("https://cdn.rawgit.com/SonarSource/sonar-scanner-msbuild/master/LICENSE.txt"),
        IconUrl                     = new Uri("https://cdn.rawgit.com/SonarSource/sonar-scanner-msbuild/cdd1f588/icon.png"),
        ReleaseNotes                = new [] {"All release notes for SonarScanner .Net Framework can be found on the GitHub site - https://github.com/SonarSource/sonar-scanner-msbuild/releases"},
        Tags                        = new [] {"sonarqube", "sonarcloud", "msbuild", "scanner", "sonarsource", "sonar", "sonar-scanner", "sonarscanner"},
        RequireLicenseAcceptance    = false,
        Symbols                     = false,
        NoPackageAnalysis           = true,
        Files                       = new [] {
                                        new NuSpecContent { Source = $@".temp\**", Target = "tools" }
                                    },
        BasePath                    = "./",
        OutputDirectory             = nugetDir
    };
    NuGetPack(nuGetPackSettings);
});

Task("7-Zip.StandaloneConsole")
    .IsDependentOn("Clean-Output")
    .Does(() =>
{
    var version = "24.09";

    var resource = DownloadFile($"https://www.7-zip.org/a/7z{version.Replace(".", "")}-extra.7z");
    UnSevenZip(resource, temp);

    resource = DownloadFile($"https://www.7-zip.org/a/7z{version.Replace(".", "")}-x64.exe");
    UnSevenZip(resource, temp);

    var genericFiles = new [] {
        new NuSpecContent { Source = temp + File("7-zip.chm"), Target = "tools" },
        new NuSpecContent { Source = temp + File("readme.txt"), Target = "tools" },
        new NuSpecContent { Source = temp + File("License.txt"), Target = "tools" },
    };
    var nuGetPackSettings = new NuGetPackSettings {
        Id                          = "7-Zip.StandaloneConsole",
        Title                       = "7-Zip Standalone Console Version",
        Version                     = version,
        Authors                     = new[] {"Igor Pavlov", "Roemer"},
        Description                 = "Standalone Console Version of the 7-Zip packer/unpacker.",
        ProjectUrl                  = new Uri("https://github.com/Roemer/nuget-packages"),
        License                     = new NuSpecLicense {
                                        Type = "file",
                                        Value = @"tools\License.txt"
                                    },
        IconUrl                     = new Uri("http://www.7-zip.org/7ziplogo.png"),
        ReleaseNotes                = new [] {"https://www.7-zip.org/history.txt"},
        Tags                        = new [] {"7z", "7zip", "7-Zip", "ZIP", "xz", "GZIP", "BZIP2", "TAR", "Z", "lzma", "CAB", "7za"},
        RequireLicenseAcceptance    = false,
        Symbols                     = false,
        NoPackageAnalysis           = true,
        Files                       = new List<NuSpecContent>(genericFiles) {
                                        // x86
                                        new NuSpecContent { Source = temp + File("7za.exe"), Target = @"tools" },
                                        // x64
                                        new NuSpecContent { Source = temp + Directory("x64") + File("7za.exe"), Target = @"tools\x64" }
                                    },
        BasePath                    = "./",
        OutputDirectory             = nugetDir
    };

    // Base version
    NuGetPack(nuGetPackSettings);

    // x64 version
    nuGetPackSettings.Id = "7-Zip.StandaloneConsole.x64";
    nuGetPackSettings.Files = new List<NuSpecContent>(genericFiles) {
        new NuSpecContent { Source = temp + Directory("x64") + File("7za.exe"), Target = @"tools" }
    };
    NuGetPack(nuGetPackSettings);

    // x86 version
    nuGetPackSettings.Id = "7-Zip.StandaloneConsole.x86";
    nuGetPackSettings.Files = new List<NuSpecContent>(genericFiles) {
        new NuSpecContent { Source = temp + File("7za.exe"), Target = @"tools" }
    };
    NuGetPack(nuGetPackSettings);
});

Task("Flyway.CommandLine")
    .IsDependentOn("Clean-Output")
    .Does(() =>
{
    var version = "11.3.3";

    var licenseFile = @"licenses\LICENSE.md";
    //licenseFile = @"licenses\flyway-community.txt"; // For pre-10 versions
    //licenseFile = @"LICENSE.txt"; // For pre-6 versions

    var nuGetPackSettings = new NuGetPackSettings {
        Version                     = version,
        Authors                     = new[] {"Boxfuse", "Roemer"},
        Description                 = "The Flyway command-line tool is a standalone Flyway distribution. It is primarily meant for users who wish to migrate their database from the command-line without having to integrate Flyway into their applications nor having to install a build tool.",
        ProjectUrl                  = new Uri("https://github.com/Roemer/nuget-packages"),
        License                     = new NuSpecLicense {
                                        Type = "file",
                                        Value = $@"tools\{licenseFile}"
                                    },
        IconUrl                     = new Uri("https://flywaydb.org/assets/logo/flyway-logo.png"),
        ReleaseNotes                = new [] {"All release notes can be found on - https://flywaydb.org/documentation/releaseNotes"},
        Tags                        = new [] {"flyway", "migration", "db"},
        RequireLicenseAcceptance    = false,
        Symbols                     = false,
        NoPackageAnalysis           = true,
        Files                       = new [] {
                                        new NuSpecContent { Source = $@".temp\flyway-{version}\**", Target = "tools" }
                                    },
        BasePath                    = "./",
        OutputDirectory             = nugetDir
    };

    // Handle the file without jre
    {
        var resource = DownloadFile($"https://repo1.maven.org/maven2/org/flywaydb/flyway-commandline/{version}/flyway-commandline-{version}.zip");
        Unzip(resource, temp);

        nuGetPackSettings.Id = "Flyway.CommandLine";
        nuGetPackSettings.Title = "Flyway command-line tool";

        NuGetPack(nuGetPackSettings);
    }

    CleanDirectory(temp);

    // Handle the file with JRE
    {
        var resource = DownloadFile($"https://repo1.maven.org/maven2/org/flywaydb/flyway-commandline/{version}/flyway-commandline-{version}-windows-x64.zip");
        Unzip(resource, temp);

        nuGetPackSettings.Id = "Flyway.CommandLine.Jre";
        nuGetPackSettings.Title = "Flyway command-line tool with JRE";

        NuGetPack(nuGetPackSettings);
    }
});

Task("Docker-CLI")
    .IsDependentOn("Clean-Output")
    .Does(() =>
{
    var version = "28.0.0";    
    var resource = DownloadFile($"https://download.docker.com/win/static/stable/x86_64/docker-{version}.zip");
    Unzip(resource, temp);

    var nuGetPackSettings = new NuGetPackSettings {
        Id                          = "docker-cli",
        Title                       = "Docker CLI for Windows",
        Version                     = version,
        Authors                     = new[] {"Docker", "StefanScherer", "Roemer"},
        Description                 = "This package contains the docker-cli executable for Windows.",
        ProjectUrl                  = new Uri("https://github.com/Roemer/nuget-packages"),
        LicenseUrl                  = new Uri("https://github.com/StefanScherer/docker-cli-builder/blob/master/LICENSE"),
        Tags                        = new [] {"docker", "cli", "windows"},
        RequireLicenseAcceptance    = false,
        Symbols                     = false,
        NoPackageAnalysis           = true,
        Files                       = new [] {
                                        new NuSpecContent { Source = $@".temp\docker\**", Target = "tools" }
                                    },
        BasePath                    = "./",
        OutputDirectory             = nugetDir
    };
    NuGetPack(nuGetPackSettings);
});

Task("Push-Packages")
    .Does(() =>
{
    var apiKey = System.IO.File.ReadAllText(".nugetapikey");

    var files = GetFiles($"{nugetDir}/*.nupkg");
    foreach (var package in files) {
        Information($"Pushing {package}");
        NuGetPush(package, new NuGetPushSettings {
            Source = "https://nuget.org/api/v2/package",
            ApiKey = apiKey
        });
    }
});

Task("Default")
    .Does(() =>
{
    Information("Hello Cake!");
});

RunTarget(target);

private void UnSevenZip(FilePath fileToExtract, DirectoryPath targetDirectory)
{
    var settings = new ProcessSettings
    {
        WorkingDirectory = MakeAbsolute(targetDirectory),
        Arguments = $"x -aoa {MakeAbsolute(fileToExtract).ToString().Quote()} *"
    };
    var exePath = Context.Environment.GetSpecialPath(SpecialPath.ProgramFiles).CombineWithFilePath(@"7-Zip\7z.exe");
    StartProcess(exePath, settings);
}
