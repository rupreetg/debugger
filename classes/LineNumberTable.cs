using System;
using System.Text;
using System.Collections;

namespace Mono.Debugger
{
	public abstract class LineNumberTable : DebuggerMarshalByRefObject
	{
		string name;
		bool is_dynamic;

		TargetAddress start, end;
		TargetAddress method_start, method_end;

		protected LineNumberTable (Method method, bool is_dynamic)
		{
			this.start = method.StartAddress;
			this.end = method.EndAddress;
			this.name = method.Name;

			if (method.HasMethodBounds) {
				method_start = method.MethodStartAddress;
				method_end = method.MethodEndAddress;
			} else {
				method_start = start;
				method_end = end;
			}
		}

		protected LineNumberTable (TargetAddress start, TargetAddress end)
		{
			this.start = this.method_start = start;
			this.end = this.method_end = end;
			this.name = "<unknown>";
		}

		protected virtual void SetEndAddress (TargetAddress end)
		{
			this.end = this.method_end = end;
		}

		ObjectCache cache = null;
		object read_line_numbers (object user_data)
		{
			return ReadLineNumbers ();
		}

		protected LineNumberTableData Data {
			get {
				if (cache == null)
					cache = new ObjectCache
						(new ObjectCacheFunc (read_line_numbers), null, 5);

				return (LineNumberTableData) cache.Data;
			}
		}

		protected abstract LineNumberTableData ReadLineNumbers ();

		public virtual string Name {
			get {
				return name;
			}
		}

		public bool IsDynamic {
			get {
				return is_dynamic;
			}
		}

		public SourceBuffer SourceBuffer {
			get {
				if (!IsDynamic)
					throw new InvalidOperationException ();

				return Data.SourceBuffer;
			}
		}

		public Module Module {
			get {
				return Data.Module;
			}
		}

		public SourceMethod SourceMethod {
			get {
				if (IsDynamic)
					throw new InvalidOperationException ();

				return Data.SourceMethod;
			}
		}

		protected LineEntry[] Addresses {
			get {
				return Data.Addresses;
			}
		}

		public virtual int StartRow {
			get {
				return Data.StartRow;
			}
		}

		public virtual int EndRow {
			get {
				return Data.EndRow;
			}
		}

		public virtual string[] GetNamespaces ()
		{
			return null;
		}

		public TargetAddress Lookup (int line)
		{
			if ((Addresses == null) || (line < StartRow) || (line > EndRow))
				return TargetAddress.Null;

			for (int i = 0; i < Addresses.Length; i++) {
				LineEntry entry = (LineEntry) Addresses [i];

				if (line <= entry.Line)
					return entry.Address;
			}

			return TargetAddress.Null;
		}

		public SourceAddress Lookup (TargetAddress address)
		{
			if (address.IsNull || (address < start) || (address >= end))
				return null;

			if (address < method_start)
				return new SourceAddress (
					this, StartRow, (int) (address - start),
					(int) (method_start - address));

			TargetAddress next_address = end;

			for (int i = Addresses.Length-1; i >= 0; i--) {
				LineEntry entry = (LineEntry) Addresses [i];

				int range = (int) (next_address - address);
				next_address = entry.Address;

				if (next_address > address)
					continue;

				int offset = (int) (address - next_address);

				return new SourceAddress (this, entry.Line, offset, range);
			}

			if (Addresses.Length > 0)
				return new SourceAddress (
					this, Addresses [0].Line, (int) (address - start),
					(int) (end - address));

			return null;
		}

		public virtual void DumpLineNumbers ()
		{
			Console.WriteLine ("--------");
			Console.WriteLine ("DUMPING LINE NUMBER TABLE: {0}", name);
			Console.WriteLine ("BOUNDS: start = {0} / {1}, end = {2} / {3}", 
					   start, method_start, end, method_end);
			Console.WriteLine ("--------");
			for (int i = 0; i < Addresses.Length; i++) {
				LineEntry entry = (LineEntry) Addresses [i];
				Console.WriteLine ("{0,4} {1,4}  {2}", i, entry.Line, entry.Address);
			}
			Console.WriteLine ("--------");
		}

		protected class LineNumberTableData
		{
			public readonly int StartRow;
			public readonly int EndRow;
			public readonly LineEntry[] Addresses;
			public readonly SourceMethod SourceMethod;
			public readonly SourceBuffer SourceBuffer;
			public readonly Module Module;

			public LineNumberTableData (int start, int end, LineEntry[] addresses,
						    SourceBuffer buffer, Module module)
				: this (start, end, addresses, null, buffer, module)
			{ }

			public LineNumberTableData (int start, int end, LineEntry[] addresses,
						    SourceMethod method, SourceBuffer buffer,
						    Module module)
			{
				this.StartRow = start;
				this.EndRow = end;
				this.SourceMethod = method;
				this.SourceBuffer = buffer;
				this.Addresses = addresses;
				this.Module = module;
			}
		}
	}
}
