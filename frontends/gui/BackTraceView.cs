using GLib;
using Gtk;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Mono.Debugger.GUI
{
	public class BackTraceView : DebuggerWidget
	{
		Gtk.TreeView tree;
		Gtk.ListStore store;

		public BackTraceView (Gtk.Container window, Gtk.Container container)
			: base (window, container)
		{
			store = new ListStore ((int)TypeFundamentals.TypeInt,
					       (int)TypeFundamentals.TypeString,
					       (int)TypeFundamentals.TypeString,
					       (int)TypeFundamentals.TypeString);

			tree = new TreeView (store);

			tree.HeadersVisible = true;
			tree.RulesHint = true;
			TreeViewColumn IdCol = new TreeViewColumn ();
			CellRenderer IdRenderer = new CellRendererText ();
			IdCol.Title = "#ID";
			IdCol.PackStart (IdRenderer, true);
			IdCol.AddAttribute (IdRenderer, "text", 0);
			tree.AppendColumn (IdCol);

			TreeViewColumn AddressCol = new TreeViewColumn ();
			CellRenderer AddressRenderer = new CellRendererText ();
			AddressCol.Title = "Address";
			AddressCol.PackStart (AddressRenderer, false);
			AddressCol.AddAttribute (AddressRenderer, "text", 1);
			tree.AppendColumn (AddressCol);

			TreeViewColumn MethodCol = new TreeViewColumn ();
			CellRenderer MethodRenderer = new CellRendererText ();
			MethodCol.Title = "Method";
			MethodCol.PackStart (MethodRenderer, false);
			MethodCol.AddAttribute (MethodRenderer, "text", 2);
			tree.AppendColumn (MethodCol);

			TreeViewColumn LocationCol = new TreeViewColumn ();
			CellRenderer LocationRenderer = new CellRendererText ();
			LocationCol.Title = "Location";
			LocationCol.PackStart (LocationRenderer, false);
			LocationCol.AddAttribute (LocationRenderer, "text", 3);
			tree.AppendColumn (LocationCol);

			container.Add (tree);
			container.ShowAll ();
		}

		public override void SetBackend (DebuggerBackend backend)
		{
			base.SetBackend (backend);
			
 backend.FrameChangedEvent += new StackFrameHandler (FrameChangedEvent);
			backend.FramesInvalidEvent += new StackFrameInvalidHandler (FramesInvalidEvent);
		}
		
		void FramesInvalidEvent ()
		{
			if (!IsVisible)
				return;

			store.Clear ();
		}

		void add_frame (int id, StackFrame frame)
		{
			TreeIter iter = new TreeIter ();

			store.Append (out iter);
			store.SetValue (iter, 0, new GLib.Value (id));
			store.SetValue (iter, 1, new GLib.Value (frame.TargetAddress.ToString ()));
			if (frame.Method != null)
				store.SetValue (iter, 2, new GLib.Value (frame.Method.Name));
			if (frame.SourceLocation != null) {
				string filename = Utils.GetBasename (frame.SourceLocation.Name);
				store.SetValue (iter, 3, new GLib.Value (filename));
			}
		}

		void FrameChangedEvent (StackFrame frame)
		{
			if (!IsVisible)
				return;

			store.Clear ();

			if (!backend.HasTarget)
				return;

			try {
				StackFrame[] frames = backend.GetBacktrace ();
				for (int i = 0; i < frames.Length; i++)
					add_frame (i, frames [i]);
			} catch {
				store.Clear ();
			}
		}
	}
}
