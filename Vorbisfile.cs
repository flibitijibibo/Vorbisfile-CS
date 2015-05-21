#region License
/* Vorbisfile# - C# Wrapper for Vorbisfile
 *
 * Copyright (c) 2013-2015 Ethan Lee.
 *
 * This software is provided 'as-is', without any express or implied warranty.
 * In no event will the authors be held liable for any damages arising from
 * the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 * claim that you wrote the original software. If you use this software in a
 * product, an acknowledgment in the product documentation would be
 * appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not be
 * misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source distribution.
 *
 * Ethan "flibitijibibo" Lee <flibitijibibo@flibitijibibo.com>
 *
 */
#endregion

#region Using Statements
using System;
using System.Runtime.InteropServices;
#endregion

public static class Vorbisfile
{
	#region Native Library Name

	private const string nativeLibName = "libvorbisfile.dll";

	#endregion

	#region malloc/free Entry Points

	// Yes, we're seriously using these. -flibit

	[DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
	private static extern IntPtr malloc(IntPtr size);

	[DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
	private static extern void free(IntPtr memblock);

	#endregion

	#region Vorbis Structures

	[StructLayout(LayoutKind.Sequential)]
	public struct vorbis_info
	{
		public int version;
		public int channels;
		public long rate;
		public long bitrate_upper;
		public long bitrate_nominal;
		public long bitrate_lower;
		public long bitrate_window;
		public IntPtr codec_setup; // Refers to a void*
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct vorbis_comment
	{
		public IntPtr user_comments;	// Refers to a char**
		public IntPtr comment_lengths;	// Refers to an int*
		public int comments;
		public IntPtr vendor;		// Refers to a char*
	}

	#endregion

	#region Vorbisfile Implementation

	/* Notice that we did not implement an OggVorbis_File struct, but are
	 * instead using a pointer natively malloc'd.
	 *
	 * C# Interop for Vorbisfile structs is basically impossible to do, so
	 * we just alloc what _should_ be the full size of the structure for
	 * the OS and architecture, then pass that around as if that's a real
	 * struct. The size is just what you get from sizeof(OggVorbis_File).
	 *
	 * Don't get mad at me, get mad at C#.
	 *
	 * -flibit
	 */

	[DllImport(nativeLibName, EntryPoint = "ov_fopen", CallingConvention = CallingConvention.Cdecl)]
	private static extern int INTERNAL_ov_fopen(
		[In()] [MarshalAs(UnmanagedType.LPStr)]
			string path,
		IntPtr vf
	);
	public static int ov_fopen(string path, out IntPtr vf)
	{
		// Do not attempt to understand these numbers at all costs!
		const int win32Size = 720;
		const int unix32Size = 704;
		const int unix64Size = 944;

		PlatformID platform = Environment.OSVersion.Platform;
		if (platform == PlatformID.Win32NT)
		{
			// Assuming 32-bit, because why would Windows want to move to 64?!
			vf = malloc((IntPtr) win32Size);
		}
		else if (platform == PlatformID.Unix)
		{
			if (IntPtr.Size == 8)
			{
				vf = malloc((IntPtr) unix64Size);
			}
			else if (IntPtr.Size == 4)
			{
				vf = malloc((IntPtr) unix32Size);
			}
			else
			{
				throw new NotSupportedException("Unhandled architecture!");
			}
		}
		else
		{
			throw new NotSupportedException("Unhandled platform!");
		}
		return INTERNAL_ov_fopen(path, vf);
	}

	[DllImport(nativeLibName, EntryPoint = "ov_info", CallingConvention = CallingConvention.Cdecl)]
	private static extern IntPtr INTERNAL_ov_info(
		IntPtr vf,
		int link
	);
	public static vorbis_info ov_info(
		IntPtr vf,
		int link
	) {
		IntPtr result = INTERNAL_ov_info(vf, link);
		vorbis_info info = (vorbis_info) Marshal.PtrToStructure(
			result,
			typeof(vorbis_info)
		);
		return info;
	}

	[DllImport(nativeLibName, EntryPoint = "ov_comment", CallingConvention = CallingConvention.Cdecl)]
	private static extern IntPtr INTERNAL_ov_comment(
		IntPtr vf,
		int link
	);
	public static vorbis_comment ov_comment(
		IntPtr vf,
		int link
	) {
		IntPtr result = INTERNAL_ov_comment(vf, link);
		vorbis_comment comment = (vorbis_comment) Marshal.PtrToStructure(
			result,
			typeof(vorbis_comment)
		);
		return comment;
	}

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern double ov_time_total(IntPtr vf, int i);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern long ov_read(
		IntPtr vf,
		byte[] buffer,
		int length,
		int bigendianp,
		int word,
		int sgned,
		out int current_section
	);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern long ov_read(
		IntPtr vf,
		IntPtr buffer,
		int length,
		int bigendianp,
		int word,
		int sgned,
		out int current_section
	);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern int ov_time_seek(IntPtr vf, double s);

	[DllImport(nativeLibName, EntryPoint = "ov_clear", CallingConvention = CallingConvention.Cdecl)]
	private static extern int INTERNAL_ov_clear(IntPtr vf);
	public static int ov_clear(ref IntPtr vf)
	{
		int result = INTERNAL_ov_clear(vf);
		free(vf);
		vf = IntPtr.Zero;
		return result;
	}

	#endregion
}
