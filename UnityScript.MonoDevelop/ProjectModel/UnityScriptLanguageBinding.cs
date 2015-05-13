using System;
using MonoDevelop.Core;
using MonoDevelop.Projects;

namespace UnityScript.MonoDevelop.ProjectModel
{
	public class UnityScriptLanguageBinding : IDotNetLanguageBinding
	{
		#region IDotNetLanguageBinding implementation
		public ConfigurationParameters CreateCompilationParameters (System.Xml.XmlElement projectOptions)
		{
			return new UnityScriptCompilationParameters ();
		}
		public ProjectParameters CreateProjectParameters (System.Xml.XmlElement projectOptions)
		{
			return new UnityScriptProjectParameters ();
		}
		public BuildResult Compile (ProjectItemCollection items, DotNetProjectConfiguration configuration, ConfigurationSelector configSelector, IProgressMonitor monitor)
		{
			throw new NotImplementedException ();
		}
		public ClrVersion[] GetSupportedClrVersions ()
		{
			return new ClrVersion[] { ClrVersion.Net_1_1, ClrVersion.Net_2_0, ClrVersion.Clr_2_1, ClrVersion.Net_4_0 };
		}
		public System.CodeDom.Compiler.CodeDomProvider GetCodeDomProvider ()
		{
			return null;
		}
		public string ProjectStockIcon { get { return "md-project"; } }

		#endregion
		#region ILanguageBinding implementation

		private static bool IsUnitScriptFile(string fileName)
		{
			return fileName.ToLower ().EndsWith (".js") || fileName.ToLower ().EndsWith (".us");
		}

		public bool IsSourceCodeFile (FilePath fileName)
		{
			return IsUnitScriptFile (fileName);
		}
		public FilePath GetFileName (FilePath fileNameWithoutExtension)
		{
			return fileNameWithoutExtension + ".js";
		}
		public string Language { get { return "UnityScript"; } }
		public string SingleLineCommentTag { get { return "//"; } }
		public string BlockCommentStartTag { get { return "/*"; } }
		public string BlockCommentEndTag { get { return "*/"; } }
		#endregion
	}
}

