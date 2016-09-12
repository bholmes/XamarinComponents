#tool nuget:?package=XamarinComponent&version=1.1.0.42

#addin nuget:?package=Cake.XCode&version=2.0.9
#addin nuget:?package=Cake.Xamarin.Build&version=1.0.16
#addin nuget:?package=Cake.Xamarin&version=1.3.0.3
#addin nuget:?package=Cake.FileHelpers&version=1.0.3.2

void BuildXCodeFatLibrary(FilePath xcodeProject, string target, string libraryTitle = null, FilePath fatLibrary = null, DirectoryPath workingDirectory = null, string targetFolderName = null)
{
	if (!IsRunningOnUnix())
	{
		Warning("{0} is not available on the current platform.", "xcodebuild");
		return;
	}

	libraryTitle = libraryTitle ?? target;
	fatLibrary = fatLibrary ?? string.Format("lib{0}.a", libraryTitle);
	workingDirectory = workingDirectory ?? Directory("./externals/");

	var output = string.Format("lib{0}.a", libraryTitle);
	var i386 = string.Format("lib{0}-i386.a", libraryTitle);
	var x86_64 = string.Format("lib{0}-x86_64.a", libraryTitle);
	var armv7 = string.Format("lib{0}-armv7.a", libraryTitle);
	var armv7s = string.Format("lib{0}-armv7s.a", libraryTitle);
	var arm64 = string.Format("lib{0}-arm64.a", libraryTitle);

	var buildArch = new Action<string, string, FilePath>((sdk, arch, dest) => {
		if (!FileExists(dest))
		{
			XCodeBuild(new XCodeBuildSettings
			{
				Project = workingDirectory.CombineWithFilePath(xcodeProject).ToString(),
				Target = target,
				Sdk = sdk,
				Arch = arch,
				Configuration = "Release",
			});
			var tmpOutputPath = workingDirectory.Combine("build").Combine("Release-" + sdk);
			if (!string.IsNullOrEmpty (targetFolderName))
				tmpOutputPath = tmpOutputPath.Combine (targetFolderName);
			var outputPath = tmpOutputPath.CombineWithFilePath(output);

			CopyFile(outputPath, dest);
		}
	});

	buildArch("iphonesimulator", "i386", workingDirectory.CombineWithFilePath(i386));
	buildArch("iphonesimulator", "x86_64", workingDirectory.CombineWithFilePath(x86_64));

	buildArch("iphoneos", "armv7", workingDirectory.CombineWithFilePath(armv7));
	buildArch("iphoneos", "armv7s", workingDirectory.CombineWithFilePath(armv7s));
	buildArch("iphoneos", "arm64", workingDirectory.CombineWithFilePath(arm64));

	RunLipoCreate(workingDirectory, fatLibrary, i386, x86_64, armv7, armv7s, arm64);
}