using System;
using System.IO;
using System.Collections.Generic;
using BCLTestImporter;

namespace xharness.BCLTestImporter {
	// Class that is use as the connection between xharness and the BCLImporter
	// This class knows about both worlds, will get the required information, 
	// from xharness, that will then be passed to the importer to generate
	// the different projects, once those projects are generater, the factory
	// will return the iOS targets to be executed.
	public class BCLTestImportTargetFactory {
		public Harness Harness;
		BCLTestProjectGenerator projectGenerator;
		
		public BCLTestImportTargetFactory (Harness harness)
		{
			Harness = harness;
			var outputDir = Path.GetFullPath (Path.Combine (Harness.RootDirectory, "bcl-test", "BCLTests"));
			var projectTemplatePath = outputDir;
			var registerTypesTemplatePath = Path.Combine (outputDir, "RegisterType.cs.in");
			var plistTemplatePath = outputDir;

			projectGenerator = new BCLTestProjectGenerator (outputDir, Harness.MONO_PATH, projectTemplatePath, registerTypesTemplatePath, plistTemplatePath) {
				iOSMonoSDKPath = Harness.MONO_IOS_SDK_DESTDIR,
				MacMonoSDKPath = Harness.MONO_MAC_SDK_DESTDIR,
				Override = true,
				GuidGenerator = Harness.NewStableGuid,
				GroupTests = Harness.InJenkins || Harness.UseGroupedApps,
			};
		}
		
		// generate all the different test targets.
		public List<iOSTestProject> GetiOSBclTargets ()
		{
			var result = new List<iOSTestProject> ();
			// generate all projects, then create a new iOSTarget per project
			foreach (var (name, path, xunit, extraArgs, platforms, failure) in projectGenerator.GenerateAlliOSTestProjects ()) {
				var prefix = xunit ? "xUnit" : "NUnit";
				result.Add (new iOSTestProject (path) {
					Name = $"[{prefix}] Mono {name}",
					SkipiOSVariation = !platforms.Contains (Platform.iOS),
					SkiptvOSVariation = !platforms.Contains (Platform.TvOS),
					SkipwatchOSVariation = !platforms.Contains (Platform.WatchOS),
					FailureMessage = failure,
					RestoreNugetsInProject = true,
					MTouchExtraArgs = extraArgs,
				});
			}
			return result;
		}
		
		public List<MacTestProject> GetMacBclTargets (MacFlavors flavor)
		{
			Platform platform;
			if (flavor == MacFlavors.Full)
				platform = Platform.MacOSFull;
			else
				platform = Platform.MacOSModern;
			var result = new List<MacTestProject> ();
			foreach (var (name, path, xunit, extraArgs, failure) in projectGenerator.GenerateAllMacTestProjects (platform)) {
				var prefix = xunit ? "xUnit" : "NUnit";
				result.Add (new MacTestProject (path, targetFrameworkFlavor: flavor, generateVariations: false) {
					Name = $"[{prefix}] Mono {name}",
					Platform = "AnyCPU",
					IsExecutableProject = true,
					FailureMessage = failure,
					RestoreNugetsInProject = true,
					MTouchExtraArgs = extraArgs,
				});
			}
			return result;
		}

		public List<MacTestProject> GetMacBclTargets ()
		{
			var result = new List<MacTestProject> ();
			foreach (var flavor in new [] { MacFlavors.Full, MacFlavors.Modern}) {
				result.AddRange (GetMacBclTargets (flavor));
			}
			return result;
		}
	}
}

