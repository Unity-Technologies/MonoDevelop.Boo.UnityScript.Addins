using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using MonoDevelop.Core;
using MonoDevelop.Projects;

namespace Boo.MonoDevelop.ProjectModel
{
	public class BooCompiler
	{
		DotNetProjectConfiguration config;
		ConfigurationSelector selector;
		ProjectItemCollection projectItems;
		IProgressMonitor monitor;
		BooCompilationParameters compilationParameters;

		public BooCompiler (DotNetProjectConfiguration config,
							ConfigurationSelector selector,
							ProjectItemCollection projectItems,
							IProgressMonitor monitor)
		{
			this.config = config;
			this.selector = selector;
			this.projectItems = projectItems;
			this.monitor = monitor;

			compilationParameters = (BooCompilationParameters)config.CompilationParameters ?? new BooCompilationParameters ();
		}

		public BuildResult Run()
		{
			var responseFileName = Path.GetTempFileName ();

			try
			{
				WriteOptionsToResponseFile (responseFileName);
				var compilerOutput = ExecuteProcess(BoocPath(), "\"@"+responseFileName+"\"");

				var buildResult = ParseBuildResult(compilerOutput);

				if(!buildResult.Failed)
					CopyRequiredReferences ();

				return buildResult;
			}
			finally 
			{
				FileService.DeleteFile (responseFileName);
			}
		}

		void CopyRequiredReferences()
		{
			var outputDir = Path.GetDirectoryName (config.CompiledOutputName);

			foreach (var reference in ProjectReferences())
				foreach (var file in reference.GetReferencedFileNames(selector))
					CopyReferencesdFileTo (file, outputDir);
		}

		void CopyReferencesdFileTo(string file, string outputDir)
		{
			var targetFile = Path.Combine (outputDir, Path.GetFileName (file));

			if(!File.Exists(targetFile) || (File.GetLastWriteTime(targetFile) < File.GetLastWriteTime(file)))
				File.Copy(file, targetFile, true);
		}

		private string BoocPath()
		{
			return BooAssemblyPath ("booc.exe");
		}

		private string BooAssemblyPath(string fileName)
		{
			return Path.Combine(AssemblyPath(), Path.Combine("boo", fileName));		
		}

		private string AssemblyPath()
		{
			return Path.GetDirectoryName (GetType ().Assembly.ManifestModule.FullyQualifiedName);
		}

		private void WriteOptionsToResponseFile(string responseFile)
		{
			var options = new StringWriter ();

			options.WriteLine ("-t:"+OutputType());
			options.WriteLine("-out:"+config.CompiledOutputName);

			if (compilationParameters.Ducky)
				options.WriteLine ("-ducky");
			
			if (compilationParameters.NoStdLib)
				options.WriteLine("-nostdlib");

			foreach (var define in compilationParameters.DefineSymbols)
				options.WriteLine ("-define:"+define);

			var projectFiles = projectItems.Where (item => item is ProjectFile);

			foreach (ProjectFile file in projectFiles) 
			{
				if (file.Subtype == Subtype.Directory)
					continue;

				if (file.BuildAction == BuildAction.Compile)
					options.WriteLine ("\"" + file.Name + "\"");
				else if (file.BuildAction == BuildAction.EmbeddedResource)
					options.WriteLine ("-embedres:"+file.FilePath+","+file.ResourceId);
				else
					Console.WriteLine("Unrecognized build action for file " + file + " - " + file.BuildAction);
			}

			foreach (var reference in ProjectReferences())
				foreach (var fileName in reference.GetReferencedFileNames(selector))
					options.WriteLine ("-reference:" + fileName);

			var optionsString = options.ToString ();

			if (monitor != null)
				monitor.Log.WriteLine (optionsString);

			Console.WriteLine (optionsString);

			File.WriteAllText (responseFile, optionsString);
		}

		private IEnumerable<ProjectReference> ProjectReferences()
		{
			foreach (var item in projectItems) {
				var reference = item as ProjectReference;
				if(reference != null)
					yield return reference;
			}
		}

		private String OutputType()
		{
			return config.CompileTarget.ToString ().ToLower ();;
		}

		private string ExecuteProcess(string executable, string commandLine)
		{
			var startInfo = new System.Diagnostics.ProcessStartInfo (executable, commandLine);

			startInfo.UseShellExecute = false;
			startInfo.RedirectStandardOutput = true;
			startInfo.RedirectStandardError = true;
			startInfo.CreateNoWindow = true;

			using (var process = Runtime.SystemAssemblyService.CurrentRuntime.ExecuteAssembly (startInfo, config.TargetFramework))
				return process.StandardError.ReadToEnd ();
		}

		private bool IsWarningCode(string code)
		{
			return !string.IsNullOrEmpty (code) && code.StartsWith ("BCW");
		}

		private BuildResult ParseBuildResult(string stdout)
		{
			var result = new BuildResult ();

			using (StringReader reader = new StringReader(stdout))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					var match = Regex.Match (line, @"^(.+)\((\d+),(\d+)\):\s+(.+?):\s+(.+)$");

					if (match.Success) {
						result.Append (new BuildError {
							FileName = match.Groups [1].Value,
							Line = int.Parse (match.Groups [2].Value),
							Column = int.Parse (match.Groups [3].Value),
							IsWarning = IsWarningCode (match.Groups [4].Value),
							ErrorNumber = match.Groups [4].Value,
							ErrorText = match.Groups [5].Value									
						});
					} 
					else 
					{
						match = Regex.Match (line, @"^(.+):\s+(.+)$");

						if (match.Success) {
							result.Append (new BuildError {
								IsWarning = IsWarningCode (match.Groups [1].Value),
								ErrorNumber = match.Groups [1].Value,
								ErrorText = match.Groups [2].Value									
							});
						}
					}
				}
			}

			return result;
		}

	}
}

