using System;
using System.IO;
using System.Text;

using Mono.Debugger.Backend;

namespace Mono.Debugger.Backend
{
	[Serializable]
	internal class TargetReader
	{
		byte[] data;
		TargetBinaryReader reader;
		TargetMemoryInfo info;

		internal TargetReader (byte[] data, TargetMemoryInfo info)
		{
			if ((info == null) || (data == null))
				throw new ArgumentNullException ();
			this.reader = new TargetBinaryReader (data, info);
			this.info = info;
			this.data = data;
		}

		internal TargetReader (TargetBlob data)
			: this (data.Contents, data.TargetMemoryInfo)
		{ }

		public long Offset {
			get {
				return reader.Position;
			}

			set {
				reader.Position = value;
			}
		}

		public long Size {
			get {
				return data.Length;
			}
		}

		public byte[] Contents {
			get {
				return data;
			}
		}

		public TargetBinaryReader BinaryReader {
			get {
				return reader;
			}
		}

		public int TargetIntegerSize {
			get {
				return info.TargetIntegerSize;
			}
		}

		public int TargetLongIntegerSize {
			get {
				return info.TargetLongIntegerSize;
			}
		}

		public int TargetAddressSize {
			get {
				return info.TargetAddressSize;
			}
		}

		public bool IsBigEndian {
			get {
				return info.IsBigEndian;
			}
		}

		public AddressDomain AddressDomain {
			get {
				return info.AddressDomain;
			}
		}

		public byte ReadByte ()
		{
			return reader.ReadByte ();
		}

		public byte PeekByte ()
		{
			return reader.PeekByte ();
		}

		public byte PeekByte (long offset)
		{
			return reader.PeekByte (offset);
		}

		public int ReadInteger ()
		{
			return reader.ReadInt32 ();
		}

		public int PeekInteger ()
		{
			return reader.PeekInt32 ();
		}

		public int PeekInteger (long offset)
		{
			return reader.PeekInt32 (offset);
		}

		public long ReadLongInteger ()
		{
			if (TargetLongIntegerSize == 4)
				return reader.ReadInt32 ();
			else if (TargetLongIntegerSize == 8)
				return reader.ReadInt64 ();
			else
				throw new TargetMemoryException (
					"Unknown target long integer size " + TargetLongIntegerSize);
		}

		public long PeekLongInteger ()
		{
			if (TargetLongIntegerSize == 4)
				return reader.PeekInt32 ();
			else if (TargetLongIntegerSize == 8)
				return reader.PeekInt64 ();
			else
				throw new TargetMemoryException (
					"Unknown target long integer size " + TargetLongIntegerSize);
		}

		public long PeekLongInteger (long offset)
		{
			if (TargetLongIntegerSize == 4)
				return reader.PeekInt32 (offset);
			else if (TargetLongIntegerSize == 8)
				return reader.PeekInt64 (offset);
			else
				throw new TargetMemoryException (
					"Unknown target long integer size " + TargetLongIntegerSize);
		}

		long do_read_address ()
		{
			if (TargetAddressSize == 4)
				return (uint) reader.ReadInt32 ();
			else if (TargetAddressSize == 8)
				return reader.ReadInt64 ();
			else
				throw new TargetMemoryException (
					"Unknown target address size " + TargetAddressSize);
		}

		public TargetAddress ReadAddress ()
		{
			long address = do_read_address ();

			if (address == 0)
				return TargetAddress.Null;
			else
				return new TargetAddress (info.AddressDomain, address);
		}

		long do_peek_address ()
		{
			if (TargetAddressSize == 4)
				return (uint) reader.PeekInt32 ();
			else if (TargetAddressSize == 8)
				return reader.PeekInt64 ();
			else
				throw new TargetMemoryException (
					"Unknown target address size " + TargetAddressSize);
		}

		public TargetAddress PeekAddress ()
		{
			long address = do_peek_address ();

			if (address == 0)
				return TargetAddress.Null;
			else
				return new TargetAddress (info.AddressDomain, address);
		}

		long do_peek_address (long offset)
		{
			if (TargetAddressSize == 4)
				return (uint) reader.PeekInt32 (offset);
			else if (TargetAddressSize == 8)
				return reader.PeekInt64 (offset);
			else
				throw new TargetMemoryException (
					"Unknown target address size " + TargetAddressSize);
		}

		public TargetAddress PeekAddress (long offset)
		{
			long address = do_peek_address (offset);

			if (address == 0)
				return TargetAddress.Null;
			else
				return new TargetAddress (info.AddressDomain, address);
		}

		public override string ToString ()
		{
			return String.Format ("MemoryReader ([{0}])", TargetBinaryReader.HexDump (data));
		}
	}
}
