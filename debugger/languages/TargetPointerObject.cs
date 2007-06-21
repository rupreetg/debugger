namespace Mono.Debugger.Languages
{
	public abstract class TargetPointerObject : TargetObject
	{
		public new readonly TargetPointerType Type;

		internal TargetPointerObject (TargetPointerType type, TargetLocation location)
			: base (type, location)
		{
			this.Type = type;
		}

		// <summary>
		//   The current type of the object pointed to by this pointer.
		//   May only be used if ITargetPointerType.HasStaticType is false.
		// </summary>
		public abstract TargetType GetCurrentType (Thread target);

		// <summary>
		//   If HasDereferencedObject is true, return the dereferenced object.
		// </summary>
		public abstract TargetObject GetDereferencedObject (Thread target);

		public abstract TargetObject GetArrayElement (Thread target, int index);
	}
}