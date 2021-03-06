using System;
using System.Runtime.Serialization;

namespace Mono.Debugger
{
	[Serializable]
	public class TargetMemoryException : TargetException
	{
		public TargetMemoryException ()
			: this ("Memory access")
		{ }

		public TargetMemoryException (string message)
			: base (TargetError.MemoryAccess, message)
		{ }

		public TargetMemoryException (TargetAddress address)
			: this (String.Format ("Cannot read target memory at address {0:x}", address))
		{ }

		public TargetMemoryException (TargetAddress address, int size)
			: this (String.Format ("Cannot read {1} bytes from target memory at address {0:x}",
					       address, size))
		{ }

		protected TargetMemoryException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

                public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
		}
	}

	[Serializable]
	public class TargetMemoryReadOnlyException : TargetMemoryException
	{
		public TargetMemoryReadOnlyException ()
			: base ("The current target's memory is read-only")
		{ }

		public TargetMemoryReadOnlyException (TargetAddress address)
			: base (String.Format ("Can't write to target memory at address 0x{0:x}: {1}",
					       address, "the current target's memory is read-only"))
		{ }

		protected TargetMemoryReadOnlyException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

                public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
		}
	}
}
