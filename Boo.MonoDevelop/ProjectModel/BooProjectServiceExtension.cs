using System;
using System.Reflection;

using MonoDevelop.Core;
using MonoDevelop.Projects;
using MonoDevelop.Projects.Formats.MSBuild;

namespace Boo.MonoDevelop.ProjectModel
{
	public class BooProjectServiceExtension : ProjectServiceExtension
	{
		// This is hack to force MonoDevelop to not use msbuild to build the Boo projects and instead use BooLanguageBindings.Compile
		protected override BuildResult Build (IProgressMonitor monitor, IBuildTarget item, ConfigurationSelector configuration)
		{
			var project = item as DotNetAssemblyProject;

			if (project != null && project.LanguageBinding is ProjectModel.BooLanguageBinding)
			{
				var msBuildProjectHandler = project.ResourceHandler as MSBuildProjectHandler;

				if (msBuildProjectHandler != null) 
				{
					var useMSBuildEngineByDefault = msBuildProjectHandler.GetType ().GetProperty ("UseMSBuildEngineByDefault", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					useMSBuildEngineByDefault.SetValue (msBuildProjectHandler, false);
				}
			}

			return base.Build (monitor, item, configuration);
		}
	}
}

