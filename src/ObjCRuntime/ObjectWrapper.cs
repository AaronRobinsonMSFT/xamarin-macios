using System;
using System.Runtime.InteropServices;

namespace ObjCRuntime {
	[StructLayout(LayoutKind.Explicit)]
	internal struct ObjectWrapper {
		//[FieldOffset(0)] object obj;
		//[FieldOffset(0)] IntPtr handle;
		[DllImport("xmdnc", EntryPoint = "mono_gchandle_get_target")]
		static extern IntPtr mono_gchandle_get_target (IntPtr gchandle);

		[DllImport("xmdnc", EntryPoint = "mono_gchandle_new")]
		static extern IntPtr mono_gchandle_new (IntPtr objHandle, int pinned);

		internal static IntPtr Convert (object obj) {
			if (obj == null)
				return IntPtr.Zero;

			//ObjectWrapper wrapper = new ObjectWrapper ();

			//wrapper.obj = obj;

			//return wrapper.handle;
			GCHandle gchandleTemp = GCHandle.Alloc (obj);
			IntPtr objHandle = mono_gchandle_get_target(GCHandle.ToIntPtr(gchandleTemp));
			gchandleTemp.Free();

			return objHandle;
		}
		
		internal static object Convert (IntPtr ptr) 
		{
			//ObjectWrapper wrapper = new ObjectWrapper ();
			
			//wrapper.handle = ptr;
				
			//return wrapper.obj;
			GCHandle gch = GCHandle.FromIntPtr(mono_gchandle_new(ptr, 0));
			object obj = gch.Target;
			gch.Free();
			return obj;
		}
	}
}
