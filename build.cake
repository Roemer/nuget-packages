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
CleanDirectory(temp);
CleanDirectory(nugetDir);

Task("JMeter")
    .Does(() =>
{
    var jMeterVersion = "5.1.1";
    var jMeterPath = temp + Directory($"apache-jmeter-{jMeterVersion}");
    var libPath = jMeterPath + Directory("lib");
    var extPath = libPath + Directory("ext");

    // Download and Extract JMeter
    var resource = DownloadFile($"http://www.pirbot.com/mirrors/apache//jmeter/binaries/apache-jmeter-{jMeterVersion}.zip");
    Unzip(resource, temp);

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
    .Does(() =>
{
    var version = "4.7.1";
    var versionSuffix = "2311";

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

Task("Default")
    .Does(() =>
{
    Information("Hello Cake!");
});

RunTarget(target);
