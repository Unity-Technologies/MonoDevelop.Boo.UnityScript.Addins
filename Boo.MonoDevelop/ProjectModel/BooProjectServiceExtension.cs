using System;
using System.Reflection;

using MonoDevelop.Core;
using MonoDevelop.Projects;
using MonoDevelop.Projects.Formats.MSBuild;

namespace Boo.MonoDevelop.ProjectModel
{
	public class BooProjectServiceExtension : ProjectServiceExtension
	{
		// This is hack to force MonoDevelop to use BooLanguageBindings.Compile to build Boo projects instead of msbuild build engine (xbuild).
		// Starting from some 4.x release of MonoDevelop, disabling msbuild build engine can only be done in the solution/project settings.
		protected override BuildResult Build (IProgressMonitor monitor, IBuildTarget item, ConfigurationSelector configuration)
		{
			var project = item as DotNetAssemblyProject;

			if (project != null && project.LanguageBinding is ProjectModel.BooLanguageBinding)
			{
				var msBuildProjectHandler = project.ResourceHandler as MSBuildProjectHandler;

				if (msBuildProjectHandler != null) 
				{
					var useMSBuildEngineByDefault = msBuildProjectHandler.GetType ().GetProperty ("UseMSBuildEngineByDefault", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

					if (useMSBuildEngineByDefault == null)
						throw new NotSupportedException ("Could not find UseMSBuildEngineByDefault property in MSBuildProjectHandler. Building of Boo projects is not supported.");

					useMSBuildEngineByDefault.SetValue (msBuildProjectHandler, false);
				}
			}

			return base.Build (monitor, item, configuration);
		}
	}
}

