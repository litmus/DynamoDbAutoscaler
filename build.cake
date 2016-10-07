#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.1

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var version = AppVeyor.IsRunningOnAppVeyor ? AppVeyor.Environment.Build.Version : "0.0.1";
var releaseBinPath = "./DynamoDbAutoScaler/bin/Release";
var artifactsDirectory = "./artifacts";

Task("Restore-NuGet-Packages")
	.Does(() => {
		NuGetRestore("./DynamoDbAutoscaler.sln");
	});

Task("Setup")
	.Does(() => { 
		CreateDirectory(artifactsDirectory);
	});

Task("Build")
	.IsDependentOn("Restore-NuGet-Packages")
	.Does(() => {
		MSBuild("./DynamoDbAutoscaler.sln", settings =>
			settings.SetConfiguration(configuration));
	});

Task("UnitTest")
	.IsDependentOn("Build")
	.IsDependentOn("Setup")
	.Does(() => {
		var resultsFile = artifactsDirectory + "/NUnitResults.xml";
		NUnit3("./DynamoDbAutoscaler.Test/bin/Release/DynamoDbAutoscaler.Test.dll", new NUnit3Settings()
		{
			ResultFormat = "AppVeyor",
			OutputFile = resultsFile,
		});

		if(AppVeyor.IsRunningOnAppVeyor)
		{
			AppVeyor.UploadTestResults(resultsFile, AppVeyorTestResultsType.NUnit3);
		}
	});

Task("Pack")
	.IsDependentOn("Build")
	.IsDependentOn("Setup")
	.Does(() => {
		NuGetPack("./DynamoDbAutoScaler/DynamoDbAutoscaler.nuspec", new NuGetPackSettings()
		{
			Version = version,
			ArgumentCustomization = args => args.Append("-Prop Configuration=" + configuration),
			BasePath = releaseBinPath,
			OutputDirectory = artifactsDirectory,
		});
	});

Task("Default")
	.IsDependentOn("Build")
	.IsDependentOn("UnitTest")
	.IsDependentOn("Pack");
  
RunTarget(target);