using System;

namespace Mono.Debugger.Languages.Mono
{
	internal abstract class MonoObject : MarshalByRefObject, ITargetObject
	{
		protected IMonoTypeInfo type_info;
		protected TargetLocation location;
		protected bool is_valid;

		public MonoObject (IMonoTypeInfo type_info, TargetLocation location)
		{
			this.type_info = type_info;
			this.location = location;
			is_valid = true;
		}

		public IMonoTypeInfo TypeInfo {
			get {
				return type_info;
			}
		}

		ITargetType ITargetObject.Type {
			get {
				return type_info.Type;
			}
		}

		public string TypeName {
			get {
				return type_info.Type.Name;
			}
		}

		public TargetObjectKind Kind {
			get {
				return type_info.Type.Kind;
			}
		}

		public bool IsValid {
			get {
				return is_valid;
			}
		}

		public bool IsNull {
			get {
				if (!location.HasAddress)
					return false;
				else
					return location.Address.IsNull;
			}
		}

		public virtual byte[] RawContents {
			get {
				try {
					return location.ReadBuffer (type_info.Size);
				} catch (TargetException ex) {
					is_valid = false;
					throw new LocationInvalidException (ex);
				}
			}
			set {
				try {
					if (!type_info.HasFixedSize || (value.Length != type_info.Size))
						throw new ArgumentException ();
					location.WriteBuffer (value);
				} catch (TargetException ex) {
					is_valid = false;
					throw new LocationInvalidException (ex);
				}
			}
		}

		protected virtual int MaximumDynamicSize {
			get {
				return -1;
			}
		}

		public virtual long DynamicSize {
			get {
				if (type_info.HasFixedSize)
					throw new InvalidOperationException ();

				try {
					TargetLocation dynamic_location;
					TargetBlob blob = location.ReadMemory (type_info.Size);
					return GetDynamicSize (blob, location, out dynamic_location);
				} catch (TargetException ex) {
					is_valid = false;
					throw new LocationInvalidException (ex);
				}
			}
		}

		public virtual byte[] GetRawDynamicContents (int max_size)
		{
			if (type_info.HasFixedSize)
				throw new InvalidOperationException ();

			try {
				return GetDynamicContents (location, max_size).Contents;
			} catch (TargetException ex) {
				is_valid = false;
				throw new LocationInvalidException (ex);
			}
		}

		protected virtual TargetBlob GetDynamicContents (TargetLocation location, int max_size)
		{
			try {
				TargetLocation dynamic_location;
				TargetBlob blob = location.ReadMemory (type_info.Size);
				long size = GetDynamicSize (blob, location, out dynamic_location);

				if ((max_size > 0) && (size > (long) max_size))
					size = max_size;

				return dynamic_location.ReadMemory ((int) size);
			} catch (TargetException ex) {
				is_valid = false;
				throw new LocationInvalidException (ex);
			}
		}

		protected abstract long GetDynamicSize (TargetBlob blob, TargetLocation location,
							out TargetLocation dynamic_location);

		public TargetLocation Location {
			get {
				if (!IsValid)
					throw new LocationInvalidException ();

				return location;
			}
		}

		public virtual string Print ()
		{
			return ToString ();
		}

		public override string ToString ()
		{
			return String.Format ("{0} [{1}]", GetType (), TypeInfo);
		}
	}
}
