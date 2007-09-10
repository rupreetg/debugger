using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections;
using System.Threading;
using C = Mono.CompilerServices.SymbolWriter;

using Mono.Debugger.Backends;

namespace Mono.Debugger.Languages.Mono
{
	internal delegate void BreakpointHandler (Inferior inferior, TargetAddress address,
						  object user_data);

	// <summary>
	//   This class is the managed representation of the
	//   MonoDefaults struct (at least the types we're interested
	//   in) as defined in mono/metadata/class-internals.h.
	// </summary>
	internal class MonoBuiltinTypeInfo
	{
		public readonly MonoSymbolFile Corlib;
		public readonly MonoObjectType ObjectType;
		public readonly TargetFundamentalType ByteType;
		public readonly MonoOpaqueType VoidType;
		public readonly TargetFundamentalType BooleanType;
		public readonly TargetFundamentalType SByteType;
		public readonly TargetFundamentalType Int16Type;
		public readonly TargetFundamentalType UInt16Type;
		public readonly TargetFundamentalType Int32Type;
		public readonly TargetFundamentalType UInt32Type;
		public readonly TargetFundamentalType IntType;
		public readonly TargetFundamentalType UIntType;
		public readonly TargetFundamentalType Int64Type;
		public readonly TargetFundamentalType UInt64Type;
		public readonly TargetFundamentalType SingleType;
		public readonly TargetFundamentalType DoubleType;
		public readonly TargetFundamentalType CharType;
		public readonly MonoStringType StringType;
		public readonly MonoClassType ExceptionType;
		public readonly MonoClassType DelegateType;
		public readonly MonoClassType ArrayType;

		public MonoBuiltinTypeInfo (MonoSymbolFile corlib, TargetMemoryAccess memory,
					    MonoMetadataInfo info)
		{
			this.Corlib = corlib;

			TargetReader mono_defaults = new TargetReader (
				memory.ReadMemory (info.MonoDefaultsAddress, info.MonoDefaultsSize));

			MonoLanguageBackend language = corlib.MonoLanguage;
			mono_defaults.Offset = info.MonoDefaultsObjectOffset;
			TargetAddress klass = mono_defaults.ReadAddress ();
			int object_size = 2 * corlib.TargetInfo.TargetAddressSize;
			Cecil.TypeDefinition object_type = corlib.ModuleDefinition.Types ["System.Object"];
			ObjectType = new MonoObjectType (corlib, object_type, object_size);
			language.AddCoreType (ObjectType.ClassType, object_type, klass);

			mono_defaults.Offset = info.MonoDefaultsByteOffset;
			klass = mono_defaults.ReadAddress ();
			Cecil.TypeDefinition byte_type = corlib.ModuleDefinition.Types ["System.Byte"];
			ByteType = new MonoFundamentalType (corlib, byte_type, FundamentalKind.Byte, 1);
			language.AddCoreType (ByteType, byte_type, klass);

			mono_defaults.Offset = info.MonoDefaultsVoidOffset;
			klass = mono_defaults.ReadAddress ();
			Cecil.TypeDefinition void_type = corlib.ModuleDefinition.Types ["System.Void"];
			VoidType = new MonoOpaqueType (corlib, void_type);
			language.AddCoreType (VoidType, void_type, klass);

			mono_defaults.Offset = info.MonoDefaultsBooleanOffset;
			klass = mono_defaults.ReadAddress ();
			Cecil.TypeDefinition bool_type = corlib.ModuleDefinition.Types ["System.Boolean"];
			BooleanType = new MonoFundamentalType (corlib, bool_type, FundamentalKind.Boolean, 1);
			language.AddCoreType (BooleanType, bool_type, klass);

			mono_defaults.Offset = info.MonoDefaultsSByteOffset;
			klass = mono_defaults.ReadAddress ();
			Cecil.TypeDefinition sbyte_type = corlib.ModuleDefinition.Types ["System.SByte"];
			SByteType = new MonoFundamentalType (corlib, sbyte_type, FundamentalKind.SByte, 1);

			language.AddCoreType (SByteType, sbyte_type, klass);

			mono_defaults.Offset = info.MonoDefaultsInt16Offset;
			klass = mono_defaults.ReadAddress ();
			Cecil.TypeDefinition short_type = corlib.ModuleDefinition.Types ["System.Int16"];
			Int16Type = new MonoFundamentalType (corlib, short_type, FundamentalKind.Int16, 2);
			language.AddCoreType (Int16Type, short_type, klass);

			mono_defaults.Offset = info.MonoDefaultsUInt16Offset;
			klass = mono_defaults.ReadAddress ();
			Cecil.TypeDefinition ushort_type = corlib.ModuleDefinition.Types ["System.UInt16"];
			UInt16Type = new MonoFundamentalType (corlib, ushort_type, FundamentalKind.UInt16, 2);
			language.AddCoreType (UInt16Type, ushort_type, klass);

			mono_defaults.Offset = info.MonoDefaultsInt32Offset;
			klass = mono_defaults.ReadAddress ();
			Cecil.TypeDefinition int_type = corlib.ModuleDefinition.Types ["System.Int32"];
			Int32Type = new MonoFundamentalType (corlib, int_type, FundamentalKind.Int32, 4);

			language.AddCoreType (Int32Type, int_type, klass);

			mono_defaults.Offset = info.MonoDefaultsUInt32Offset;
			klass = mono_defaults.ReadAddress ();
			Cecil.TypeDefinition uint_type = corlib.ModuleDefinition.Types ["System.UInt32"];
			UInt32Type = new MonoFundamentalType (corlib, uint_type, FundamentalKind.UInt32, 4);
			language.AddCoreType (UInt32Type, uint_type, klass);

			mono_defaults.Offset = info.MonoDefaultsIntOffset;
			klass = mono_defaults.ReadAddress ();
			Cecil.TypeDefinition intptr_type = corlib.ModuleDefinition.Types ["System.IntPtr"];
			IntType = new MonoFundamentalType (corlib, intptr_type, FundamentalKind.IntPtr, 4);
			language.AddCoreType (IntType, intptr_type, klass);

			mono_defaults.Offset = info.MonoDefaultsUIntOffset;
			klass = mono_defaults.ReadAddress ();
			Cecil.TypeDefinition uintptr_type = corlib.ModuleDefinition.Types ["System.UIntPtr"];
			UIntType = new MonoFundamentalType (corlib, uintptr_type, FundamentalKind.Object, 4);
			language.AddCoreType (UIntType, uintptr_type, klass);

			mono_defaults.Offset = info.MonoDefaultsInt64Offset;
			klass = mono_defaults.ReadAddress ();
			Cecil.TypeDefinition long_type = corlib.ModuleDefinition.Types ["System.Int64"];
			Int64Type = new MonoFundamentalType (corlib, long_type, FundamentalKind.Int64, 8);
			language.AddCoreType (Int64Type, long_type, klass);

			mono_defaults.Offset = info.MonoDefaultsUInt64Offset;
			klass = mono_defaults.ReadAddress ();
			Cecil.TypeDefinition ulong_type = corlib.ModuleDefinition.Types ["System.UInt64"];
			UInt64Type = new MonoFundamentalType (corlib, ulong_type, FundamentalKind.UInt64, 8);
			language.AddCoreType (UInt64Type, ulong_type, klass);

			mono_defaults.Offset = info.MonoDefaultsSingleOffset;
			klass = mono_defaults.ReadAddress ();
			Cecil.TypeDefinition float_type = corlib.ModuleDefinition.Types ["System.Single"];
			SingleType = new MonoFundamentalType (corlib, float_type, FundamentalKind.Single, 4);
			language.AddCoreType (SingleType, float_type, klass);

			mono_defaults.Offset = info.MonoDefaultsDoubleOffset;
			klass = mono_defaults.ReadAddress ();
			Cecil.TypeDefinition double_type = corlib.ModuleDefinition.Types ["System.Double"];
			DoubleType = new MonoFundamentalType (corlib, double_type, FundamentalKind.Double, 8);
			language.AddCoreType (DoubleType, double_type, klass);

			mono_defaults.Offset = info.MonoDefaultsCharOffset;
			klass = mono_defaults.ReadAddress ();
			Cecil.TypeDefinition char_type = corlib.ModuleDefinition.Types ["System.Char"];
			CharType = new MonoFundamentalType (corlib, char_type, FundamentalKind.Char, 2);
			language.AddCoreType (CharType, char_type, klass);

			mono_defaults.Offset = info.MonoDefaultsStringOffset;
			klass = mono_defaults.ReadAddress ();
			Cecil.TypeDefinition string_type = corlib.ModuleDefinition.Types ["System.String"];
			StringType = new MonoStringType (corlib, string_type, object_size, object_size + 4);
			language.AddCoreType (StringType, string_type, klass);

			mono_defaults.Offset = info.MonoDefaultsArrayOffset;
			klass = mono_defaults.ReadAddress ();
			Cecil.TypeDefinition array_type = corlib.ModuleDefinition.Types ["System.Array"];
			ArrayType = new MonoClassType (memory, corlib, array_type, klass);
			language.AddCoreType (ArrayType, array_type, klass);

			mono_defaults.Offset = info.MonoDefaultsDelegateOffset;
			klass = mono_defaults.ReadAddress ();
			Cecil.TypeDefinition delegate_type = corlib.ModuleDefinition.Types ["System.Delegate"];
			DelegateType = new MonoClassType (corlib, delegate_type);
			language.AddCoreType (DelegateType, delegate_type, klass);

			mono_defaults.Offset = info.MonoDefaultsExceptionOffset;
			klass = mono_defaults.ReadAddress ();
			Cecil.TypeDefinition exception_type = corlib.ModuleDefinition.Types ["System.Exception"];
			ExceptionType = new MonoClassType (memory, corlib, exception_type, klass);
			language.AddCoreType (ExceptionType, exception_type, klass);
		}
	}

	internal class MonoLanguageBackend : Language, ILanguageBackend
	{
		Hashtable symbol_files;
		int last_num_symbol_files;
		Hashtable image_hash;
		Hashtable symfile_hash;
		Hashtable assembly_hash;
		Hashtable assembly_by_name;
		Hashtable class_hash;
		Hashtable class_info_by_addr;
		Hashtable class_info_by_type;
		MonoSymbolFile corlib;
		MonoBuiltinTypeInfo builtin_types;
		MonoFunctionType main_method;

		Hashtable data_tables;

		ProcessServant process;
		MonoDebuggerInfo info;
		TargetAddress[] trampolines;
		bool initialized;
		DebuggerMutex mutex;

		public MonoLanguageBackend (ProcessServant process, MonoDebuggerInfo info)
		{
			this.process = process;
			this.info = info;
			mutex = new DebuggerMutex ("mono_mutex");
			data_tables = new Hashtable ();
		}

		public override string Name {
			get { return "Mono"; }
		}

		public override bool IsManaged {
			get { return true; }
		}

		internal MonoDebuggerInfo MonoDebuggerInfo {
			get { return info; }
		}

		internal MonoMetadataInfo MonoMetadataInfo {
			get { return info.MonoMetadataInfo; }
		}

		Language ILanguageBackend.Language {
			get { return this; }
		}

		internal MonoBuiltinTypeInfo BuiltinTypes {
			get { return builtin_types; }
		}

		internal override ProcessServant Process {
			get { return process; }
		}

		public override TargetInfo TargetInfo {
			get { return corlib.TargetInfo; }
		}

		internal bool TryFindImage (Thread thread, string filename)
		{
			Cecil.AssemblyDefinition ass = Cecil.AssemblyFactory.GetAssembly (filename);
			if (ass == null)
				return false;

			MonoSymbolFile file = (MonoSymbolFile) assembly_hash [ass];
			if (file != null)
				return true;

			return true;
		}

		public TargetType LookupMonoType (Cecil.TypeReference type)
		{
			MonoSymbolFile file;

			Cecil.TypeDefinition typedef = type as Cecil.TypeDefinition;
			if (typedef != null) {
				file = (MonoSymbolFile) assembly_hash [type.Module.Assembly];
				if (file == null) {
					Console.WriteLine ("Type `{0}' from unknown assembly `{1}'",
							   type, type.Module.Assembly);
					return null;
				}

				return file.LookupMonoType (typedef);
			}

			Cecil.ArrayType array = type as Cecil.ArrayType;
			if (array != null) {
				TargetType element_type = LookupMonoType (array.ElementType);
				if (element_type == null)
					return null;

				return new MonoArrayType (element_type, array.Rank);
			}

			Cecil.ReferenceType reftype = type as Cecil.ReferenceType;
			if (reftype != null) {
				TargetType element_type = LookupMonoType (reftype.ElementType);
				if (element_type == null)
					return null;

				return new MonoPointerType (element_type);
			}

			Cecil.GenericParameter gen_param = type as Cecil.GenericParameter;
			if (gen_param != null)
				return new MonoGenericParameterType (this, gen_param);

			int rank = 0;

			string full_name = type.FullName;
			int pos = full_name.IndexOf ('[');
			if (pos > 0) {
				string dim = full_name.Substring (pos);
				full_name = full_name.Substring (0, pos);

				if ((dim.Length < 2) || (dim [dim.Length - 1] != ']'))
					throw new ArgumentException ();
				for (int i = 1; i < dim.Length - 1; i++)
					if (dim [i] != ',')
						throw new ArgumentException ();

				rank = dim.Length - 1;
			}

			TargetType mono_type = LookupType (full_name);
			if (mono_type == null)
				return null;

			if (rank > 0)
				return new MonoArrayType (mono_type, rank);
			else
				return mono_type;
		}

		internal void AddCoreType (TargetType type, Cecil.TypeDefinition typedef,
					   TargetAddress klass)
		{
			corlib.AddType (type, typedef);
			if (!class_hash.Contains (klass))
				class_hash.Add (klass, type);
		}

		public TargetType GetClass (TargetMemoryAccess target, TargetAddress klass_address)
		{
			TargetType type = (TargetType) class_hash [klass_address];
			if (type != null)
				return type;

			try {
				type = MonoClassType.ReadMonoClass (this, target, klass_address);
			} catch {
				return null;
			}

			if (type == null)
				return null;

			class_hash.Add (klass_address, type);
			return type;
		}

		public MonoSymbolFile GetImage (TargetAddress address)
		{
			return (MonoSymbolFile) image_hash [address];
		}

		protected MonoSymbolFile GetSymbolFile (int index)
		{
			return (MonoSymbolFile) symbol_files [index];
		}

		void read_mono_debugger_info (TargetMemoryAccess memory)
		{
			trampolines = new TargetAddress [4];
			TargetAddress address = info.MonoTrampolineCode;
			trampolines [0] = memory.ReadAddress (address);
			address += memory.TargetInfo.TargetAddressSize;
			trampolines [1] = memory.ReadAddress (address);
			address += memory.TargetInfo.TargetAddressSize;
			trampolines [2] = memory.ReadAddress (address);
			address += 2 * memory.TargetInfo.TargetAddressSize;
			trampolines [3] = memory.ReadAddress (address);

			symbol_files = new Hashtable ();
			image_hash = new Hashtable ();
			symfile_hash = new Hashtable ();
			assembly_hash = new Hashtable ();
			assembly_by_name = new Hashtable ();
			class_hash = new Hashtable ();
			class_info_by_addr = new Hashtable ();
			class_info_by_type = new Hashtable ();
		}

		void reached_main (TargetMemoryAccess target, TargetAddress method)
		{
			int token = target.ReadInteger (method + 4);
			TargetAddress klass = target.ReadAddress (method + 8);
			TargetAddress image = target.ReadAddress (klass);

			MonoSymbolFile file = GetImage (image);
			if (file == null)
				return;

			main_method = file.GetFunctionByToken (token);
		}

		internal MonoFunctionType MainMethod {
			get { return main_method; }
		}

#region symbol table management
		internal void Update (TargetMemoryAccess target)
		{
			Report.Debug (DebugFlags.JitSymtab, "Update requested");
			DateTime start = DateTime.Now;
			++data_table_count;
			foreach (DataTable table in data_tables.Values)
				table.Read (target);
			data_table_time += DateTime.Now - start;
		}

		void read_symbol_table (TargetMemoryAccess memory)
		{
			if (initialized)
				throw new InternalError ();

			Report.Debug (DebugFlags.JitSymtab, "Starting to read symbol table");
			try {
				DateTime start = DateTime.Now;
				++full_update_count;
				do_read_symbol_table (memory);
				update_time += DateTime.Now - start;
			} catch (ThreadAbortException) {
				return;
			} catch (Exception e) {
				Console.WriteLine ("Can't read symbol table: {0} {1} {2}",
						   memory, e, Environment.StackTrace);
				return;
			}
			Report.Debug (DebugFlags.JitSymtab, "Done reading symbol table");
			initialized = true;
		}

		void read_builtin_types (TargetMemoryAccess memory)
		{
			builtin_types = new MonoBuiltinTypeInfo (corlib, memory, info.MonoMetadataInfo);
		}

		protected class DataTable
		{
			public readonly int Domain;
			public readonly MonoLanguageBackend Mono;
			public readonly TargetAddress TableAddress;

			public readonly TargetAddress FirstChunk;
			TargetAddress current_chunk;
			int last_offset;

			public DataTable (MonoLanguageBackend mono, int domain,
					  TargetAddress address, TargetAddress first_chunk)
			{
				this.Mono = mono;
				this.Domain = domain;
				this.TableAddress = address;
				this.FirstChunk = first_chunk;

				current_chunk = first_chunk;
			}

			public void Read (TargetMemoryAccess memory)
			{
				int header_size = 16 + memory.TargetInfo.TargetAddressSize;

			again:
				TargetReader reader = new TargetReader (
					memory.ReadMemory (current_chunk, header_size));

				int size = reader.ReadInteger ();
				int allocated_size = reader.ReadInteger ();
				int current_offset = reader.ReadInteger ();
				reader.ReadInteger (); /* dummy */
				TargetAddress next = reader.ReadAddress ();

#if FIXME
				Report.Debug (DebugFlags.JitSymtab,
					"READ DATA TABLE CHUNK: {0} - {1} {2} - {3} {4} {5} {6} - {7}",
					this, current_chunk, last_offset, size, allocated_size,
					current_offset, next, reader.BinaryReader.HexDump ());
#endif

				read_data_items (memory, current_chunk + header_size,
						 last_offset, current_offset);

				last_offset = current_offset;

				if (!next.IsNull && (current_offset == allocated_size)) {
					current_chunk = next;
					last_offset = 0;
					goto again;
				}
			}

			void read_data_items (TargetMemoryAccess memory, TargetAddress address,
					      int start, int end)
			{
				TargetReader reader = new TargetReader (
					memory.ReadMemory (address + start, end - start));

				Report.Debug (DebugFlags.JitSymtab,
					      "READ DATA ITEMS: {0} {1} {2} - {3} {4}", address,
					      start, end, reader.BinaryReader.Position, reader.Size);

				while (reader.BinaryReader.Position + 4 < reader.Size) {
					int item_size = reader.BinaryReader.ReadInt32 ();
					if (item_size == 0)
						break;
					DataItemType item_type = (DataItemType)
						reader.BinaryReader.ReadInt32 ();

					long pos = reader.BinaryReader.Position;

					switch (item_type) {
					case DataItemType.Method:
						read_range_entry (memory, reader);
						break;

					case DataItemType.Class:
						read_class_entry (memory, reader);
						break;

					default:
						throw new InternalError (
							"Got unknown data item: {0}", item_type);
					}

					reader.BinaryReader.Position = pos + item_size;
				}
			}

			private enum DataItemType {
				Unknown		= 0,
				Class,
				Method
			}

			void read_range_entry (TargetMemoryAccess memory, TargetReader reader)
			{
				int size = reader.BinaryReader.PeekInt32 ();
				byte[] contents = reader.BinaryReader.PeekBuffer (size);
				reader.BinaryReader.ReadInt32 ();
				int file_idx = reader.BinaryReader.ReadInt32 ();
				Report.Debug (DebugFlags.JitSymtab, "READ RANGE ITEM: {0} {1}",
					      size, file_idx);
				MonoSymbolFile file = Mono.GetSymbolFile (file_idx);
				if (file != null)
					file.AddRangeEntry (memory, reader, contents);
			}

			void read_class_entry (TargetMemoryAccess memory, TargetReader reader)
			{
				reader.BinaryReader.ReadInt32 ();
				int file_idx = reader.BinaryReader.ReadInt32 ();

				MonoSymbolFile file = Mono.GetSymbolFile (file_idx);
				if (file == null)
					return;

				reader.BinaryReader.ReadLeb128 ();
				reader.BinaryReader.ReadLeb128 ();
				TargetAddress klass_address = reader.ReadAddress ();

				Mono.GetClassInfo (memory, klass_address);
			}

			public override string ToString ()
			{
				return String.Format ("DataTable ({0}:{1}:{2}:{3})",
						      Domain, TableAddress, FirstChunk, current_chunk);
			}
		}

		MonoSymbolFile load_symfile (TargetMemoryAccess memory, TargetAddress address)
		{
			MonoSymbolFile symfile = null;

			if (symfile_hash.Contains (address))
				return (MonoSymbolFile) symfile_hash [address];

			try {
				symfile = new MonoSymbolFile (this, process, memory, address);
			} catch (C.MonoSymbolFileException ex) {
				Console.WriteLine (ex.Message);
			} catch (SymbolTableException ex) {
				Console.WriteLine (ex.Message);
			} catch (Exception ex) {
				Console.WriteLine (ex);
			}

			symfile_hash.Add (address, symfile);

			if (symfile == null)
				return null;

			if (!assembly_by_name.Contains (symfile.Assembly.Name.FullName)) {
				image_hash.Add (symfile.MonoImage, symfile);
				assembly_hash.Add (symfile.Assembly, symfile);
				assembly_by_name.Add (symfile.Assembly.Name.FullName, symfile);
				symbol_files.Add (symfile.Index, symfile);
			}

			return symfile;
		}

		void close_symfile (int index)
		{
			MonoSymbolFile symfile = (MonoSymbolFile) symbol_files [index];
			if (symfile == null)
				throw new InternalError ();

			image_hash.Remove (symfile.MonoImage);
			assembly_hash.Remove (symfile.Assembly);
			assembly_by_name.Remove (symfile.Assembly.Name.FullName);
			symbol_files.Remove (symfile.Index);
		}

		// This method reads the MonoDebuggerSymbolTable structure
		// (struct definition is in mono-debug-debugger.h)
		void do_read_symbol_table (TargetMemoryAccess memory)
		{
			TargetAddress symtab_address = memory.ReadAddress (info.SymbolTable);
			if (symtab_address.IsNull)
				throw new SymbolTableException ("Symbol table is null.");

			TargetReader header = new TargetReader (
				memory.ReadMemory (symtab_address, info.SymbolTableSize));

			long magic = header.BinaryReader.ReadInt64 ();
			if (magic != MonoDebuggerInfo.DynamicMagic)
				throw new SymbolTableException (
					"Debugger symbol table has unknown magic {0:x}.", magic);

			int version = header.ReadInteger ();
			if (version < MonoDebuggerInfo.MinDynamicVersion)
				throw new SymbolTableException (
					"Debugger symbol table has version {0}, but " +
					"expected at least {1}.", version,
					MonoDebuggerInfo.MinDynamicVersion);
			if (version > MonoDebuggerInfo.MaxDynamicVersion)
				throw new SymbolTableException (
					"Debugger symbol table has version {0}, but " +
					"expected at most {1}.", version,
					MonoDebuggerInfo.MaxDynamicVersion);

			int total_size = header.ReadInteger ();
			if (total_size != info.SymbolTableSize)
				throw new SymbolTableException (
					"Debugger symbol table has size {0}, but " +
					"expected {1}.", total_size, info.SymbolTableSize);

			TargetAddress corlib_address = header.ReadAddress ();

			TargetAddress data_table = header.ReadAddress ();
			TargetAddress type_table = header.ReadAddress ();

			TargetAddress symbol_files = header.ReadAddress ();

			if (corlib_address.IsNull)
				throw new SymbolTableException ("Corlib address is null.");
			corlib = load_symfile (memory, corlib_address);
			if (corlib == null)
				throw new SymbolTableException ("Cannot read corlib!");

			TargetAddress ptr = symbol_files;
			while (!ptr.IsNull) {
				TargetAddress next_ptr = memory.ReadAddress (ptr);
				TargetAddress address = memory.ReadAddress (
					ptr + memory.TargetInfo.TargetAddressSize);

				ptr = next_ptr;
				load_symfile (memory, address);
			}

			add_data_table (memory, type_table);

			ptr = data_table;
			while (!ptr.IsNull) {
				TargetAddress next_ptr = memory.ReadAddress (ptr);
				TargetAddress address = memory.ReadAddress (
					ptr + memory.TargetInfo.TargetAddressSize);

				ptr = next_ptr;
				add_data_table (memory, address);
			}
		}

		void add_data_table (TargetMemoryAccess memory, TargetAddress ptr)
		{
			int table_size = 8 + 2 * memory.TargetInfo.TargetAddressSize;

			TargetReader reader = new TargetReader (memory.ReadMemory (ptr, table_size));

			int domain = reader.ReadInteger ();
			reader.Offset += 4;

			DataTable table = (DataTable) data_tables [domain];
			if (table == null) {
				TargetAddress first_chunk = reader.ReadAddress ();
				table = new DataTable (this, domain, ptr, first_chunk);
				data_tables.Add (domain, table);
			}
		}

		void destroy_data_table (int domain, TargetAddress table)
		{
			data_tables.Remove (domain);
		}

		static int manual_update_count;
		static int full_update_count;
		static int update_count;
		static int data_table_count;
		static TimeSpan data_table_time;
		static TimeSpan update_time;
		static int range_entry_count;
		static TimeSpan range_entry_time;
		static TimeSpan range_entry_method_time;

		public static void RangeEntryCreated (TimeSpan time)
		{
			range_entry_count++;
			range_entry_time += time;
		}

		public static void RangeEntryGetMethod (TimeSpan time)
		{
			range_entry_method_time += time;
		}

		public static void PrintStatistics ()
		{
#if FIXME
			Console.WriteLine ("MONO LANGUAGE: {0} {1} {2} {3} - {4} {5} - {6} {7} {8} - {9}",
					   manual_update_count, full_update_count,
					   update_count, data_table_count, data_table_time, update_time,
					   range_entry_count, range_entry_time,
					   range_entry_method_time,

					   data_table_time + update_time + range_entry_time +
					   range_entry_method_time);
#endif
		}

		internal MonoClassInfo GetClassInfo (TargetMemoryAccess memory, TargetAddress klass_address)
		{
			MonoClassInfo info = (MonoClassInfo) class_info_by_addr [klass_address];
			if (info != null)
				return info;

			int token = memory.ReadInteger (
				klass_address + MonoMetadataInfo.KlassTokenOffset);

			TargetAddress image = memory.ReadAddress (klass_address);
			MonoSymbolFile file = GetImage (image);
			if (file == null)
				return null;

			if ((token & 0xff000000) != 0x02000000)
				throw new InternalError ();
			token &= 0x00ffffff;

			Cecil.TypeDefinition tdef = file.ModuleDefinition.LookupByToken (
				Cecil.Metadata.TokenType.TypeDef, (int) token) as Cecil.TypeDefinition;
			if (tdef == null)
				throw new InternalError ();

			info = new MonoClassInfo (memory, this, token, klass_address);
			class_info_by_addr.Add (klass_address, info);
			if (!info.IsGenericClass)
				class_info_by_type.Add (tdef, info);
			return info;
		}

		internal MonoClassInfo GetClassInfo (Cecil.TypeDefinition type)
		{
			return (MonoClassInfo) class_info_by_type [type];
		}
#endregion

#region Class Init Handlers

		static int next_unique_id;

		internal static int GetUniqueID ()
		{
			return ++next_unique_id;
		}

#endregion

#region Method Load Handlers

		Hashtable method_load_handlers = new Hashtable ();

		void method_from_jit_info (TargetMemoryAccess target, TargetAddress data,
					   MethodLoadedHandler handler)
		{
			int size = target.ReadInteger (data);
			TargetReader reader = new TargetReader (target.ReadMemory (data, size));

			reader.BinaryReader.ReadInt32 ();
			int count = reader.BinaryReader.ReadInt32 ();

			for (int i = 0; i < count; i++) {
				TargetAddress address = reader.ReadAddress ();
				Method method = read_range_entry (target, address);

				handler (target, method);
			}
		}

		Method read_range_entry (TargetMemoryAccess target, TargetAddress address)
		{
			int size = target.ReadInteger (address);
			TargetReader reader = new TargetReader (target.ReadMemory (address, size));

			byte[] contents = reader.BinaryReader.PeekBuffer (size);

			reader.BinaryReader.ReadInt32 ();
			int file_idx = reader.BinaryReader.ReadInt32 ();
			MonoSymbolFile file = (MonoSymbolFile) symbol_files [file_idx];

			return file.ReadRangeEntry (target, reader, contents);
		}

		internal int RegisterMethodLoadHandler (Thread target, TargetAddress method_address,
							MethodLoadedHandler handler)
		{
			int index = GetUniqueID ();

			TargetAddress retval = target.CallMethod (
				info.InsertBreakpoint, method_address, index);

			if (!retval.IsNull)
				method_from_jit_info (target, retval, handler);

			method_load_handlers.Add (index, handler);
			return index;
		}

		internal void RegisterMethodLoadHandler (int index, MethodLoadedHandler handler)
		{
			method_load_handlers.Add (index, handler);
		}

		internal void RemoveMethodLoadHandler (Thread target, int index)
		{
			target.CallMethod (info.RemoveBreakpoint, TargetAddress.Null, 0);
			method_load_handlers.Remove (index);
		}
#endregion

#region Language implementation
		public override string SourceLanguage (StackFrame frame)
		{
			return "";
		}

		public override TargetType LookupType (string name)
		{
			switch (name) {
			case "short":   name = "System.Int16";   break;
			case "ushort":  name = "System.UInt16";  break;
			case "int":     name = "System.Int32";   break;
			case "uint":    name = "System.UInt32";  break;
			case "long":    name = "System.Int64";   break;
			case "ulong":   name = "System.UInt64";  break;
			case "float":   name = "System.Single";  break;
			case "double":  name = "System.Double";  break;
			case "char":    name = "System.Char";    break;
			case "byte":    name = "System.Byte";    break;
			case "sbyte":   name = "System.SByte";   break;
			case "object":  name = "System.Object";  break;
			case "string":  name = "System.String";  break;
			case "bool":    name = "System.Boolean"; break;
			case "void":    name = "System.Void";    break;
			case "decimal": name = "System.Decimal"; break;
			}

			if (name.IndexOf ('[') >= 0)
				return null;

			foreach (MonoSymbolFile symfile in symbol_files.Values) {
				try {
					Cecil.TypeDefinitionCollection types = symfile.Assembly.MainModule.Types;
					// FIXME: Work around an API problem in Cecil.
					foreach (Cecil.TypeDefinition type in types) {
						if (type.FullName != name)
							continue;

						return symfile.LookupMonoType (type);
					}
				} catch {
				}
			}

			return null;
		}

		TargetFundamentalType GetFundamentalType (Type type)
		{
			switch (Type.GetTypeCode (type)) {
			case TypeCode.Boolean:
				return builtin_types.BooleanType;
			case TypeCode.Char:
				return builtin_types.CharType;
			case TypeCode.SByte:
				return builtin_types.SByteType;
			case TypeCode.Byte:
				return builtin_types.ByteType;
			case TypeCode.Int16:
				return builtin_types.Int16Type;
			case TypeCode.UInt16:
				return builtin_types.UInt16Type;
			case TypeCode.Int32:
				return builtin_types.Int32Type;
			case TypeCode.UInt32:
				return builtin_types.UInt32Type;
			case TypeCode.Int64:
				return builtin_types.Int64Type;
			case TypeCode.UInt64:
				return builtin_types.UInt64Type;
			case TypeCode.Single:
				return builtin_types.SingleType;
			case TypeCode.Double:
				return builtin_types.DoubleType;
			case TypeCode.String:
				return builtin_types.StringType;
			case TypeCode.Object:
				if (type == typeof (IntPtr))
					return builtin_types.IntType;
				else if (type == typeof (UIntPtr))
					return builtin_types.UIntType;
				return null;

			default:
				return null;
			}
		}

		public override bool CanCreateInstance (Type type)
		{
			return GetFundamentalType (type) != null;
		}

		public override TargetFundamentalObject CreateInstance (Thread target, object obj)
		{
			TargetFundamentalType type = GetFundamentalType (obj.GetType ());
			if (type == null)
				return null;

			return type.CreateInstance (target, obj);
		}

		public override TargetPointerObject CreatePointer (StackFrame frame, TargetAddress address)
		{
			return process.BfdContainer.NativeLanguage.CreatePointer (frame, address);
		}

		public override TargetObject CreateObject (Thread target, TargetAddress address)
		{
			TargetLocation location = new AbsoluteTargetLocation (address);
			MonoObjectObject obj = (MonoObjectObject)builtin_types.ObjectType.GetObject (location);
			if (obj == null)
				return null;

			TargetObject result;
			try {
				result = obj.GetDereferencedObject (target);
				if (result == null)
					result = obj;
			} catch {
				result = obj;
			}

			return result;
		}

		public override TargetObject CreateNullObject (Thread target, TargetType type)
		{
			TargetLocation location = new AbsoluteTargetLocation (TargetAddress.Null);

			return new MonoNullObject ((TargetType) type, location);
		}

		public override TargetPointerType CreatePointerType (TargetType type)
		{
			return null;
		}

		public override TargetFundamentalType IntegerType {
			get { return builtin_types.Int32Type; }
		}

		public override TargetFundamentalType LongIntegerType {
			get { return builtin_types.Int64Type; }
		}

		public override TargetFundamentalType StringType {
			get { return builtin_types.StringType; }
		}

		public override TargetType PointerType {
			get { return builtin_types.IntType; }
		}

		public override TargetType VoidType {
			get { return builtin_types.VoidType; }
		}

		public override TargetClassType ExceptionType {
			get { return builtin_types.ExceptionType; }
		}

		public override TargetClassType DelegateType {
			get { return builtin_types.DelegateType; }
		}

		public override TargetClassType ObjectType {
			get { return builtin_types.ObjectType.ClassType; }
		}

		public override TargetClassType ArrayType {
			get { return builtin_types.ArrayType; }
		}
#endregion

#region ILanguageBackend implementation
		public TargetAddress RuntimeInvokeFunc {
			get { return info.RuntimeInvoke; }
		}

		public TargetAddress GetTrampolineAddress (TargetMemoryAccess memory,
							   TargetAddress address,
							   out bool is_start)
		{
			is_start = false;

			if (trampolines == null)
				return TargetAddress.Null;

			foreach (TargetAddress trampoline in trampolines) {
				TargetAddress result = memory.Architecture.GetTrampoline (
					memory, address, trampoline);
				if (!result.IsNull)
					return result;
			}

			return TargetAddress.Null;
		}

		public MethodSource GetTrampoline (TargetMemoryAccess memory,
						   TargetAddress address)
		{
			bool is_start;
			TargetAddress trampoline = GetTrampolineAddress (memory, address, out is_start);
			if (trampoline.IsNull)
				return null;

			int token = memory.ReadInteger (trampoline + 4);
			TargetAddress klass = memory.ReadAddress (trampoline + 8);
			TargetAddress image = memory.ReadAddress (klass);

			foreach (MonoSymbolFile file in symbol_files.Values) {
				if (file.MonoImage != image)
					continue;

				return file.GetMethodByToken (token);
			}

			return null;
		}

		void JitBreakpoint (Inferior inferior, int idx, TargetAddress data)
		{
			Method method = read_range_entry (inferior, data);
			if (method == null)
				return;

			MethodLoadedHandler handler = (MethodLoadedHandler) method_load_handlers [idx];
			if (handler != null)
				handler (inferior, method);
		}

		internal void Initialize (TargetMemoryAccess memory)
		{
			Report.Debug (DebugFlags.JitSymtab, "Initialize mono language");
		}

		internal void InitializeCoreFile (TargetMemoryAccess memory)
		{
			Report.Debug (DebugFlags.JitSymtab, "Initialize mono language");
			read_mono_debugger_info (memory);
			read_symbol_table (memory);
			read_builtin_types (memory);
		}

		internal void InitializeAttach (TargetMemoryAccess memory)
		{
			Report.Debug (DebugFlags.JitSymtab, "Initialize mono language");
			read_mono_debugger_info (memory);
			read_symbol_table (memory);
			read_builtin_types (memory);
		}

		public bool Notification (SingleSteppingEngine engine, Inferior inferior,
					  NotificationType type, TargetAddress data, long arg)
		{
			switch (type) {
			case NotificationType.InitializeCorlib:
				Report.Debug (DebugFlags.JitSymtab, "Initialize corlib");
				read_mono_debugger_info (inferior);
				read_symbol_table (inferior);
				break;

			case NotificationType.InitializeManagedCode:
				Report.Debug (DebugFlags.JitSymtab, "Initialize managed code");
				read_builtin_types (inferior);
				reached_main (inferior, data);
				break;

			case NotificationType.LoadModule: {
				MonoSymbolFile symfile = load_symfile (inferior, data);
				Report.Debug (DebugFlags.JitSymtab,
					      "Module load: {0} {1}", data, symfile);
				if ((builtin_types != null) && (symfile != null)) {
					if (engine.OnModuleLoaded ())
						return false;
				}
				break;
			}

			case NotificationType.ReachedMain:
				if (engine.OnModuleLoaded ())
					return false;
				break;

			case NotificationType.UnloadModule:
				Report.Debug (DebugFlags.JitSymtab,
					      "Module unload: {0} {1}", data, arg);
				close_symfile ((int) arg);
				break;

			case NotificationType.JitBreakpoint:
				JitBreakpoint (inferior, (int) arg, data);
				break;

			case NotificationType.DomainCreate:
				Report.Debug (DebugFlags.JitSymtab,
					      "Domain create: {0}", data);
				add_data_table (inferior, data);
				break;

			case NotificationType.DomainUnload:
				Report.Debug (DebugFlags.JitSymtab,
					      "Domain unload: {0} {1:x}", data, arg);
				destroy_data_table ((int) arg, data);
				engine.Process.BreakpointManager.DomainUnload (inferior, (int) arg);
				break;

			default:
				Console.WriteLine ("Received unknown notification {0:x} / {1} {2:x}",
						   (int) type, data, arg);
				break;
			}

			return true;
		}
#endregion

		private bool disposed = false;

		private void Dispose (bool disposing)
		{
			lock (this) {
				if (disposed)
					return;
			  
				disposed = true;
			}

			if (disposing) {
				if (symbol_files != null) {
					foreach (MonoSymbolFile symfile in symbol_files.Values)
						symfile.Dispose();

					symbol_files = null;
				}
			}
		}

		public void Dispose ()
		{
			Dispose (true);
			// Take yourself off the Finalization queue
			GC.SuppressFinalize (this);
		}

		~MonoLanguageBackend ()
		{
			Dispose (false);
		}

	}
}
