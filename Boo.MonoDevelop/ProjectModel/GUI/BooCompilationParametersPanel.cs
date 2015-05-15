using Gtk;
using System;
using MonoDevelop.Projects;
using MonoDevelop.Ide.Gui.Dialogs;

namespace Boo.MonoDevelop.ProjectModel.GUI
{
	public class BooCompilationParametersPanel : MultiConfigItemOptionsPanel
	{
		Gtk.Entry definesEntry;
		Gtk.CheckButton noStdLibCheckButton;

		BooCompilationParameters CompilationParatemers
		{
			get 
			{
				return (BooCompilationParameters)((DotNetProjectConfiguration)CurrentConfiguration).CompilationParameters;
			}
		}

		public override Gtk.Widget CreatePanelWidget ()
		{
			var vbox = new VBox ();
			var definesHbox = new HBox ();
			noStdLibCheckButton = CheckButton.NewWithLabel ("No standard libraries");
			vbox.PackStart (noStdLibCheckButton, false, true, 5);
			definesEntry = new Entry ();
			var definesLabel = new Label ("Define Symbols: ");
			definesHbox.PackStart (definesLabel, false, false, 5);
			definesHbox.PackStart (definesEntry, true, true, 5);
			vbox.PackStart (definesHbox, true, true, 5);
			vbox.ShowAll ();
			return vbox;
		}

		public override void ApplyChanges ()
		{
			CompilationParatemers.NoStdLib = noStdLibCheckButton.Active;
			CompilationParatemers.DefineConstants = definesEntry.Text;
		}

		public override void LoadConfigData ()
		{
			noStdLibCheckButton.Active = CompilationParatemers.NoStdLib;
			definesEntry.Text = CompilationParatemers.DefineConstants;
		}
	}
}

