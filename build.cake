///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

var temp = Directory("./.temp");

Task("JMeter")
   .Does(() =>
{
   //var resource = DownloadFile("http://www.pirbot.com/mirrors/apache//jmeter/binaries/apache-jmeter-5.0.zip");
   //Unzip(resource, temp + Directory("JMeter"));

   var jMeterPath = temp + Directory("JMeter");
   var innerPath = jMeterPath + Directory("apache-jmeter-5.0");

   var extensionsPath = innerPath + Directory("lib") + Directory("ext");

   // Download the plugin manager
   var outputPath = extensionsPath + File("jmeter-plugins-manager-1.3.jar");
   DownloadFile("http://search.maven.org/remotecontent?filepath=kg/apc/jmeter-plugins-manager/1.3/jmeter-plugins-manager-1.3.jar", outputPath);
   // Download most common plugins
   // See: http://flauschig.ch/imgshare/img/1819966995c5db20d43b58.png

   return;


   var nuGetPackSettings   = new NuGetPackSettings {
      Id                       = "TestNuget",
      Version                  = "0.0.0.1",
      Title                    = "The tile of the package",
      Authors                  = new[] {"John Doe"},
      Owners                   = new[] {"Contoso"},
      Description              = "The description of the package",
      Summary                  = "Excellent summary of what the package does",
      ProjectUrl               = new Uri("https://github.com/SomeUser/TestNuget/"),
      IconUrl                  = new Uri("http://cdn.rawgit.com/SomeUser/TestNuget/master/icons/testnuget.png"),
      LicenseUrl               = new Uri("https://github.com/SomeUser/TestNuget/blob/master/LICENSE.md"),
      Copyright                = "Some company 2015",
      ReleaseNotes             = new [] {"Bug fixes", "Issue fixes", "Typos"},
      Tags                     = new [] {"Cake", "Script", "Build"},
      RequireLicenseAcceptance = false,
      Symbols                  = false,
      NoPackageAnalysis        = true,
      Files                    = new [] {
                                    new NuSpecContent {Source = ".temp/JMeter/apache-jmeter-5.0/README.md", Target = "tools"},
                                 },
      BasePath                 = "./",
      OutputDirectory          = "./.nuget"
   };

   NuGetPack("./JMeter/JMeter.nuspec", nuGetPackSettings);
});

Task("Default")
   .Does(() => {
   Information("Hello Cake!");
});

RunTarget(target);

