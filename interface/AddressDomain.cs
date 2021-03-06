using System;

namespace Mono.Debugger
{
	// <summary>
	//   This is the "domain" of a TargetAddress - you cannot compare addresses from
	//   different domains.  See TargetAddress.cs for details.
	//
	//   The debugger has the following address domains:
	//
	//   * the global address domain
	//
	//     This is used to do symbol lookups and to store addresses which are independent
	//     from the execution of a particular thread.
	//
	//   * the local address domain
	//
	//     Each thread has its own local address domain.  Technically, all threads share
	//     the same memory space, but this is to enforce the policy that you may not share
	//     any information about variables across thread boundaries.
	//
	//   * the frame address domain
	//
	//     Each stack frame has its own address domain (which is only constructed when it's
	//     actually used) which has the same lifetime than its corresponding frame.
	//
	//     This is normally used when reading an address from the stack or from a register,
	//     for instance the address of a variable which is stored on the stack.  Since the
	//     object such an address is pointing to becomes invalid when the target leaves the
	//     stack frame, the address will also become invalid.
	//
	// </summary>
	[Serializable]
	public struct AddressDomain : IComparable
	{
		// <summary>
		//   `name' is just used in the error messages.
		// </summary>
		public AddressDomain (string name, int id)
		{
			this.id = id;
			this.name = name;
		}

		public static readonly AddressDomain Global = new AddressDomain ("global", 0);

		int id;
		string name;

		public int ID {
			get { return id; }
		}

		public string Name {
			get { return name; }
		}

		public bool IsGlobal {
			get { return (id == 0) && (name == "global"); }
		}

		public int CompareTo (object obj)
		{
			AddressDomain domain = (AddressDomain) obj;

			if (id < domain.id)
				return -1;
			else if (id > domain.id)
				return 1;
			else
				return 0;
		}

		public override bool Equals (object o)
		{
			if (o == null || !(o is AddressDomain))
				return false;

			AddressDomain b = (AddressDomain) o;
			return (id == b.id) && (name == b.name);
		}

		public override int GetHashCode ()
		{
			return id;
		}

		public override string ToString ()
		{
			return String.Format ("AddressDomain ({0}:{1})", ID, Name);
		}
	}
}
