#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.1

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

Task("Restore-NuGet-Packages")
	.Does(() => {
		NuGetRestore("./DynamoDbAutoscaler.sln");
	});

Task("Build")
	.IsDependentOn("Restore-NuGet-Packages")
	.Does(() => {
		MSBuild("./DynamoDbAutoscaler.sln", settings =>
			settings.SetConfiguration(configuration));
	});

Task("UnitTest")
	.IsDependentOn("Build")
	.Does(() => {
		NUnit3("./DynamoDbAutoscaler.Test/bin/Release/DynamoDbAutoscaler.Test.dll");
	});

Task("Default")
	.IsDependentOn("Build")
	.IsDependentOn("UnitTest");
  
RunTarget(target);