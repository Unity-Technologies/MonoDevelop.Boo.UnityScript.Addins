
// Adapted from PythonEditorIndentation.cs
// Copyright (c) 2008 Christian Hergert <chris@dronelabs.com>
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;

using Gdk;
using MonoDevelop.Ide.Gui.Content;

namespace Boo.MonoDevelop.Editing
{
	public class BooEditorIndentation : TextEditorExtension
	{
		public override bool KeyPress (Gdk.Key key, char keyChar, Gdk.ModifierType modifier)
		{
			if(key != Gdk.Key.Return)
				return base.KeyPress (key, keyChar, modifier);

			var lastLine = Editor.GetLineText (Editor.Caret.Line);

			if (ShouldIndentAfter (lastLine)) 
			{
				base.KeyPress (key, keyChar, modifier);
				Editor.InsertAtCaret ("\t");
				return false;
			}

			return base.KeyPress (key, keyChar, modifier);
		}

		private bool ShouldIndentAfter(string line)
		{
			var trimmed = line.Trim ();
			if (trimmed.EndsWith (":"))
				return true;

			if (trimmed.StartsWith ("if ") || trimmed.StartsWith ("def ")) 
			{
				var openCount = line.Split('(').Length;
				var closeCount = line.Split(')').Length;

				return openCount > closeCount;
			}

			return false;
		}
	}
}

