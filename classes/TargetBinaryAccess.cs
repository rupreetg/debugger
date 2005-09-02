using System;
using System.Text;

namespace Mono.Debugger
{
	[Serializable]
	public sealed class TargetBlob
	{
		public readonly byte[] Contents;
		public readonly ITargetInfo TargetInfo;

		public TargetBlob (byte[] contents, ITargetInfo target_info)
		{
			this.Contents = contents;
			this.TargetInfo = target_info;
		}

		public TargetBlob (int size, ITargetInfo target_info)
		{
			this.Contents = new byte [size];
			this.TargetInfo = target_info;
		}

		public int Size {
			get { return Contents.Length; }
		}
	}

	[Serializable]
	public class TargetBinaryAccess
	{
		protected TargetBlob blob;
		protected int pos;
		protected bool swap;

		public TargetBinaryAccess (TargetBlob blob)
		{
			this.blob = blob;
			this.swap = blob.TargetInfo.IsBigEndian;
		}

		public int AddressSize {
			get {
				int address_size = blob.TargetInfo.TargetAddressSize;
				if ((address_size != 4) && (address_size != 8))
					throw new TargetMemoryException (
						"Unknown target address size " + address_size);

				return address_size;
			}
		}

		public ITargetInfo TargetInfo {
			get {
				return blob.TargetInfo;
			}
		}

		public long Size {
			get {
				return blob.Contents.Length;
			}
		}

		public long Position {
			get {
				return pos;
			}

			set {
				pos = (int) value;
			}
		}

		public bool IsEof {
			get {
				return pos == blob.Contents.Length;
			}
		}

		public byte[] Contents {
			get {
				return blob.Contents;
			}
		}

		public string HexDump ()
		{
			return HexDump (blob.Contents);
		}

		public static string HexDump (byte[] data)
		{
			StringBuilder sb = new StringBuilder ();

			sb.Append ("\n" + TargetAddress.FormatAddress (0) + "   ");

			for (int i = 0; i < data.Length; i++) {
				if (i > 0) {
					if ((i % 16) == 0)
						sb.Append ("\n" + TargetAddress.FormatAddress (i) + "   ");
					else if ((i % 8) == 0)
						sb.Append (" - ");
					else
						sb.Append (" ");
				}
				sb.Append (String.Format ("{1}{0:x}", data [i], data [i] >= 16 ? "" : "0"));
			}
			return sb.ToString ();
		}

		public static string HexDump (TargetAddress start, byte[] data)
		{
			StringBuilder sb = new StringBuilder ();

			sb.Append (String.Format ("{0}   ", start));

			for (int i = 0; i < data.Length; i++) {
				if (i > 0) {
					if ((i % 16) == 0) {
						start += 16;
						sb.Append (String.Format ("\n{0}   ", start));
					} else if ((i % 8) == 0)
						sb.Append (" - ");
					else
						sb.Append (" ");
				}
				sb.Append (String.Format ("{1}{0:x}", data [i], data [i] >= 16 ? "" : "0"));
			}
			return sb.ToString ();
		}
	}
}
