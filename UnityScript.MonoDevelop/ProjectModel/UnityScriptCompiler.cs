using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using MonoDevelop.Core;
using MonoDevelop.Projects;

namespace UnityScript.MonoDevelop.ProjectModel
{
	public class UnityScriptCompiler
	{
		DotNetProjectConfiguration config;
		ConfigurationSelector selector;
		ProjectItemCollection projectItems;
		IProgressMonitor monitor;
		UnityScriptCompilationParameters compilationParameters;


		public UnityScriptCompiler (DotNetProjectConfiguration config,
			ConfigurationSelector selector,
			ProjectItemCollection projectItems,
			IProgressMonitor monitor)
		{
			this.config = config;
			this.selector = selector;
			this.projectItems = projectItems;
			this.monitor = monitor;

			compilationParameters = (UnityScriptCompilationParameters)config.CompilationParameters ?? new UnityScriptCompilationParameters ();
		}

		public BuildResult Run()
		{
			var responseFileName = Path.GetTempFileName ();

			try
			{
				WriteOptionsToResponseFile(responseFileName);
				var compiler = MapPath("UnityScript/us.exe");
				var compilerOutput =  ExecuteProcess(compiler, "\"@"+responseFileName+"\"");
				return ParseBuildResult(compilerOutput);
			}
			finally 
			{
				FileService.DeleteFile (responseFileName);
			}
		}

		private void WriteOptionsToResponseFile(string responseFile)
		{
			var commandLine = new StringWriter ();

			var referencedFiles = GetReferencedFileNames ();

			if(ContainsReference(referencedFiles, "UnityEngine.dll"))
			{
				commandLine.WriteLine ("-nowarn:BCW0016"); // Unused namespace
				commandLine.WriteLine ("-nowarn:BCW0003"); // Unused local variable
				commandLine.WriteLine ("-base:UnityEngine.MonoBehaviour");
				commandLine.WriteLine ("-method:Main");
				commandLine.WriteLine ("-i:System.Collections");
				commandLine.WriteLine ("-i:UnityEngine");

				if(ContainsReference(referencedFiles, "UnityEditor.dll"))
					commandLine.WriteLine ("-i:UnityEditor"); 
				commandLine.WriteLine ("-t:library");
				commandLine.WriteLine ("-x-type-inference-rule-attribute:UnityEngineInternal.TypeInferenceRuleAttribute");
			}
			else
			{
				commandLine.WriteLine ("-base:System.Object");
				commandLine.WriteLine ("-method:Awake");
				commandLine.WriteLine ("-t:exe");
			}

			commandLine.WriteLine ("-debug+");
			commandLine.WriteLine ("-out:" + config.CompiledOutputName);

			foreach (var define in compilationParameters.DefineSymbols)
				commandLine.WriteLine ("-define:"+define);

			foreach (var reference in referencedFiles)
				commandLine.WriteLine ("-reference:" + reference);

			var projectFiles = projectItems.Where (item => item is ProjectFile);

			foreach (ProjectFile file in projectFiles) 
			{
				if (file.Subtype == Subtype.Directory)
					continue;

				if (file.BuildAction == BuildAction.Compile)
					commandLine.WriteLine ("\"" + file.Name + "\"");
				else
					Console.WriteLine("Unrecognized build action for file " + file + " - " + file.BuildAction);
			}

			var commandLineString = commandLine.ToString ();

			if (monitor != null)
				monitor.Log.WriteLine (commandLineString);

			Console.WriteLine (commandLineString);

			File.WriteAllText (responseFile, commandLineString);		
		}

		private string[] GetReferencedFileNames()
		{
			return projectItems.OfType<ProjectReference> ().SelectMany (r => r.GetReferencedFileNames (selector)).ToArray ();
		}

		private bool ContainsReference(string[] files, string fileName)
		{
			return System.Array.Exists(files, f => Path.GetFileName(f) == fileName);
		}

		private string MapPath(string path)
		{
			return Path.Combine (Path.GetDirectoryName (GetType ().Assembly.Location), path);
		}

		private string ExecuteProcess(string executable, string commandLine)
		{
			var startInfo = new System.Diagnostics.ProcessStartInfo (executable, commandLine);

			startInfo.UseShellExecute = false;
			startInfo.RedirectStandardOutput = true;
			startInfo.RedirectStandardError = true;
			startInfo.CreateNoWindow = true;

			using (var process = Runtime.SystemAssemblyService.CurrentRuntime.ExecuteAssembly (startInfo, config.TargetFramework))
				return process.StandardOutput.ReadToEnd () + System.Environment.NewLine + process.StandardError.ReadToEnd ();
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
					var match = Regex.Match (line, @"(.+)\((\d+),(\d+)\):\s+(BC.+?):\s+(.+)$");

					if (match.Success) 
					{
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
						match = Regex.Match (line, @"(BC.+):\s+(.+)");

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

