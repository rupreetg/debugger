using System;

namespace Mono.Debugger.Languages
{
	public class TargetFundamentalObject : TargetObject
	{
		new public readonly TargetFundamentalType Type;

		internal TargetFundamentalObject (TargetFundamentalType type, TargetLocation location)
			: base (type, location)
		{
			this.Type = type;
		}

		internal override long GetDynamicSize (TargetAccess target, TargetBlob blob,
						       TargetLocation location,
						       out TargetLocation dynamic_location)
		{
			throw new InvalidOperationException ();
		}

		public virtual object GetObject (TargetAccess target)
		{
			TargetBlob blob = Location.ReadMemory (target, Type.Size);

			switch (Type.FundamentalKind) {
			case FundamentalKind.Boolean:
				return blob.Contents [0] != 0;

			case FundamentalKind.Char:
				return BitConverter.ToChar (blob.Contents, 0);

			case FundamentalKind.SByte:
				return (sbyte) blob.Contents [0];

			case FundamentalKind.Byte:
				return (byte) blob.Contents [0];

			case FundamentalKind.Int16:
				return BitConverter.ToInt16 (blob.Contents, 0);

			case FundamentalKind.UInt16:
				return BitConverter.ToUInt16 (blob.Contents, 0);

			case FundamentalKind.Int32:
				return BitConverter.ToInt32 (blob.Contents, 0);

			case FundamentalKind.UInt32:
				return BitConverter.ToUInt32 (blob.Contents, 0);

			case FundamentalKind.Int64:
				return BitConverter.ToInt64 (blob.Contents, 0);

			case FundamentalKind.UInt64:
				return BitConverter.ToUInt64 (blob.Contents, 0);

			case FundamentalKind.Single:
				return BitConverter.ToSingle (blob.Contents, 0);

			case FundamentalKind.Double:
				return BitConverter.ToDouble (blob.Contents, 0);

			case FundamentalKind.IntPtr:
				if (blob.Contents.Length == 4)
					return new IntPtr (BitConverter.ToInt32 (blob.Contents, 0));
				else
					return new IntPtr (BitConverter.ToInt64 (blob.Contents, 0));

			case FundamentalKind.UIntPtr:
				if (blob.Size == 4)
					return new UIntPtr (BitConverter.ToUInt32 (blob.Contents, 0));
				else
					return new UIntPtr (BitConverter.ToUInt64 (blob.Contents, 0));

			default:
				throw new InvalidOperationException ();
			}
		}

		public void SetObject (TargetAccess target, TargetObject obj)
		{
			Type.SetObject (target, Location, obj);
		}

		public override string Print (TargetAccess target)
		{
			object obj = GetObject (target);
			if (obj is IntPtr)
				return String.Format ("0x{0:x}", ((IntPtr) obj).ToInt64 ());
			else if (obj is UIntPtr)
				return String.Format ("0x{0:x}", ((UIntPtr) obj).ToUInt64 ());
			else
				return obj.ToString ();
		}
	}
}