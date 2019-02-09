///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

var temp = Directory("./.temp");
CleanDirectory(temp);

Task("JMeter")
   .Does(() =>
{
   var jMeterVersion = "5.0";
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
   CleanDirectory("./.nuget");
   var nuGetPackSettings   = new NuGetPackSettings {
      Id                       = "JMeter",
      Version                  = "5.0.0",
      Authors                  = new[] {"phmarques, Roemer"},
      Description              = "The Apache JMeter™ application is open source software, a 100% pure Java application designed to load test functional behavior and measure performance. It was originally designed for testing Web Applications but has since expanded to other test functions.",
      ProjectUrl               = new Uri("https://github.com/Roemer/nuget-packages"),
      LicenseUrl               = new Uri("https://www.apache.org/licenses/"),
      RequireLicenseAcceptance = false,
      Symbols                  = false,
      NoPackageAnalysis        = true,
      Files                    = new [] {
                                 new NuSpecContent { Source = @".temp\apache-jmeter-5.0\**", Exclude = @".temp\apache-jmeter-5.0\docs\**;.temp\apache-jmeter-5.0\printable_docs\**", Target = "tools" }
                              },
      BasePath                 = "./",
      OutputDirectory          = "./.nuget"
   };
   NuGetPack(nuGetPackSettings);
});

Task("Default")
   .Does(() => {
   Information("Hello Cake!");
});

RunTarget(target);

