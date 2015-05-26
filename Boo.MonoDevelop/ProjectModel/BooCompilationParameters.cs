using System;
using System.Collections.Generic;
using System.Linq;

using MonoDevelop.Projects;
using MonoDevelop.Core.Serialization;

namespace Boo.MonoDevelop.ProjectModel
{
	public class BooCompilationParameters : ConfigurationParameters
	{
		static string DefineSeparator = ";";
		private List<string> defines = new List<string>();

		[ItemProperty]
		public bool GenWarnings { get; set; }

		[ItemProperty]
		public bool Ducky { get; set; }

		[ItemProperty]
		public string Culture { get; set; }

		[ItemProperty]
		public bool NoStdLib { get; set; }

		[ItemProperty]
		public string DefineConstants { 
			get { 
				return string.Join (DefineSeparator, defines);
			}

			set {
				if (string.IsNullOrEmpty (value))
					defines = new List<string> ();
				else
					defines = new List<string> (value.Split (new char[1]{ ';' }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray());
			}
		}

		public List<string> DefineSymbols { get { return defines; } }

		[Obsolete]
		public override void AddDefineSymbol (string symbol)
		{
			if (!defines.Contains (symbol))
				defines.Add (symbol);
		}


		[Obsolete]
		public override void RemoveDefineSymbol (string symbol)
		{
			if (defines.Contains (symbol))
				defines.Remove (symbol);
		}
	}
}

