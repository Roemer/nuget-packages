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
    var jMeterVersion = "5.2.1";
    var jMeterPath = temp + Directory($"apache-jmeter-{jMeterVersion}");
    var libPath = jMeterPath + Directory("lib");
    var extPath = libPath + Directory("ext");

    // Download and Extract JMeter (with two mirrors)
    try {
        var resource = DownloadFile($"http://www.pirbot.com/mirrors/apache//jmeter/binaries/apache-jmeter-{jMeterVersion}.zip");
        Unzip(resource, temp);
    } catch {
        var resource = DownloadFile($"http://mirror.easyname.ch/apache//jmeter/binaries/apache-jmeter-{jMeterVersion}.zip");
        Unzip(resource, temp);
    }

    // Install the plugin manager
    DownloadFile("http://search.maven.org/remotecontent?filepath=kg/apc/jmeter-plugins-manager/1.3/jmeter-plugins-manager-1.3.jar", extPath + File("jmeter-plugins-manager-1.3.jar"));

    // Install the command runner
    DownloadFile("https://search.maven.org/remotecontent?filepath=kg/apc/cmdrunner/2.2/cmdrunner-2.2.jar", libPath + File("cmdrunner-2.2.jar"));
    // Generate the cmd wrappers
    var exitCodeWithArgument = StartProcess("java", new ProcessSettings{ Arguments = $"-cp {jMeterPath}/lib/ext/jmeter-plugins-manager-1.3.jar org.jmeterplugins.repository.PluginManagerCMDInstaller" });

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
        OutputDirectory             = nugetDir
    };
    NuGetPack(nuGetPackSettings);
});

Task("dotnet-framework-sonarscanner")
    .IsDependentOn("Clean-Output")
    .Does(() =>
{
    var version = "4.9.0";
    var versionSuffix = "17385";

    var fullVersionString = $"{version}.{versionSuffix}";
    var resource = DownloadFile($"https://github.com/SonarSource/sonar-scanner-msbuild/releases/download/{fullVersionString}/sonar-scanner-msbuild-{fullVersionString}-net46.zip");
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
    var version = "19.00";

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
    var version = "6.4.1";

    var licenseFile = @"licenses\flyway-community.txt";
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
