using System;
using System.Reflection;

using MonoDevelop.Core;
using MonoDevelop.Projects;
using MonoDevelop.Projects.Formats.MSBuild;

namespace UnityScript.MonoDevelop.ProjectModel
{
	public class UnityScriptProjectServiceExtension : ProjectServiceExtension
	{
		// This is hack to force MonoDevelop to use UnityScriptLanguageBindings.Compile to build UnityScript projects instead of msbuild build engine (xbuild).
		// Starting from some 4.x release of MonoDevelop, disabling msbuild build engine can only be done in the solution/project settings.
		protected override BuildResult Build (IProgressMonitor monitor, IBuildTarget item, ConfigurationSelector configuration)
		{
			var project = item as DotNetAssemblyProject;

			if (project != null && project.LanguageBinding is ProjectModel.UnityScriptLanguageBinding)
			{
				var msBuildProjectHandler = project.ResourceHandler as MSBuildProjectHandler;

				if (msBuildProjectHandler != null) 
				{
					var useMSBuildEngineByDefault = msBuildProjectHandler.GetType ().GetProperty ("UseMSBuildEngineByDefault", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

					if (useMSBuildEngineByDefault == null) 
					{
						BuildResult result = new BuildResult ();
						result.AddError("Could not find UseMSBuildEngineByDefault property in MSBuildProjectHandler. Building of UnityScript projects is not supported.");
						return result;
					}

					useMSBuildEngineByDefault.SetValue (msBuildProjectHandler, false);
				}
			}

			return base.Build (monitor, item, configuration);
		}
	}
}

