using System;
using System.Xml;
using MonoDevelop.Core;
using MonoDevelop.Projects;

namespace Boo.MonoDevelop.ProjectModel
{
	public class BooLanguageBinding : IDotNetLanguageBinding
	{
		#region IDotNetLanguageBinding implementation

		public ConfigurationParameters CreateCompilationParameters (XmlElement projectOptions)
		{
			return new BooCompilationParameters ();
		}

		public ProjectParameters CreateProjectParameters (XmlElement projectOptions)
		{
			return new BooProjectParameters ();
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
			return new Boo.Lang.CodeDom.BooCodeProvider ();
		}

		public string ProjectStockIcon { get { return "md-project"; } }

		#endregion

		#region ILanguageBinding implementation

		private static bool IsBooFile(string fileName)
		{
			return fileName.ToLower ().EndsWith (".boo");
		}

		public bool IsSourceCodeFile (FilePath fileName)
		{
			return IsBooFile (fileName);
		}

		public FilePath GetFileName (FilePath fileNameWithoutExtension)
		{
			return fileNameWithoutExtension + ".boo";
		}

		public string Language { get { return "Boo"; }	}

		public string SingleLineCommentTag { get { return "#"; } }

		public string BlockCommentStartTag { get { return "/*"; } }

		public string BlockCommentEndTag { get { return "*/"; } }

		#endregion
	}
}

