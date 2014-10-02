#region License
/* Vorbisfile# - C# Wrapper for Vorbisfile
 *
 * Copyright (c) 2013-2014 Ethan Lee.
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

	private const string nativeLibName = "vorbisfile.dll";

	#endregion

	#region Ogg Structures

	[StructLayout(LayoutKind.Sequential)]
	public struct oggpack_buffer
	{
		public long endbyte;
		public int endbit;

		public IntPtr buffer;	// Refers to an unsigned char*
		public IntPtr ptr;	// Refers to an unsigned char*
		public long storage;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct ogg_sync_state
	{
		public IntPtr data; // Refers to an unsigned char*
		public int storage;
		public int fill;
		public int returned;
		public int unsynced;
		public int headerbytes;
		public int bodybytes;
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct ogg_stream_state
	{
		public IntPtr body_data;	// Refers to an unsigned char*
		public long body_storage;
		public long body_fill;
		public long body_returned;

		public int lacing_vals;
		public Int64 granule_vals;	// Refers to an ogg_int64_t
		public long lacing_storage;
		public long lacing_fill;
		public long lacing_packet;
		public long lacing_returned;

		public fixed byte header[282];
		public int header_fill;

		public int e_o_s;
		public int b_o_s;
		public long serialno;
		public int pageno;
		public Int64 packetno;		// Refers to an ogg_int64_t
		public Int64 granulepos;	// Refers to an ogg_int64_t
	}

	#endregion

	#region Vorbis Structures

	[StructLayout(LayoutKind.Sequential)]
	public struct vorbis_dsp_state
	{
		public int analysisp;
		public IntPtr vi;		// Refers to a vorbis_info*

		public IntPtr pcm;		// Refers to a float**
		public IntPtr pcmret;		// Refers to a float**
		public int pcm_storage;
		public int pcm_current;
		public int pcm_returned;

		public int preextrapolate;
		public int eofflag;

		public long lW;
		public long W;
		public long nW;
		public long centerW;

		public Int64 granulepos;	// Refers to an ogg_int64_t
		public Int64 sequence;		// Refers to an ogg_int64_t

		public Int64 glue_bits;		// Refers to an ogg_int64_t
		public Int64 time_bits;		// Refers to an ogg_int64_t
		public Int64 floor_bits;	// Refers to an ogg_int64_t
		public Int64 res_bits;		// Refers to an ogg_int64_t

		public IntPtr backend_state;	// Refers to an IntPtr
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct vorbis_block
	{
		/* Necessary stream state for linking to the framing
		 * abstraction
		 */
		public IntPtr pcm;		// Refers to a float**
		public oggpack_buffer opb;

		public long lW;
		public long W;
		public long nW;
		public int pcmend;
		public int mode;

		public int eofflag;
		public Int64 granulepos;	// Refers to an ogg_int64_t
		public Int64 sequence;		// Refers to an ogg_int64_t
		public IntPtr vd;		// Refers to a vorbis_dsp_state*

		/* Local storage to avoid remallocing;
		 * it's up to the mapping to structure it
		 */
		public IntPtr localstore;	// Refers to a void*
		public long localtop;
		public long localalloc;
		public long totaluse;
		public IntPtr reap;		// Refers to an alloc_chain*

		/* Bitmetrics for the frame */
		public long glue_bits;
		public long time_bits;
		public long floor_bits;
		public long res_bits;

		// void *internal, lolC# -flibit
		public IntPtr blockinternal;	// Refers to a void*
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct vorbis_info
	{
		public int version;
		public int channels;
		public int rate;
		public long bitrate_upper;
		public long bitrate_nominal;
		public long bitrate_lower;
		public long bitrate_window;
		public IntPtr codec_setup; // Refers to a void*
	}

	#endregion

	#region Vorbisfile Implementation

	[StructLayout(LayoutKind.Sequential)]
	public struct ov_callbacks
	{
		public IntPtr read_func;
		public IntPtr seek_func;
		public IntPtr close_func;
		public IntPtr tell_func;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct OggVorbis_File
	{
		public IntPtr datasource;	// Refers to a void*
		public int seekable;
		public Int64 offset;		// Refers to an ogg_int64_t
		public Int64 end;		// Refers to an ogg_int64_t
		public ogg_sync_state oy;

		/* If the file handle isn't seekable (e.g. a pipe),
		 * only the current stream appears.
		 */
		public int links;
		public IntPtr offsets;		// Refers to an ogg_int64_t*
		public IntPtr dataoffsets;	// Refers to an ogg_int64_t*
		public IntPtr serialnos;	// Refers to a long*
		public IntPtr pcmlengths;	// Refers to an ogg_int64_t
		public IntPtr vi;		// Refers to a vorbis_info*
		public IntPtr vc;		// Refers to a vorbis_comment*

		// Decoding working state local storage
		public Int64 pcm_offset;	// Refers to an ogg_int64_t
		public int ready_state;
		public long current_serialno;
		public int current_link;

		public Int64 bittrack;		// Refers to an ogg_int64_t
		public Int64 samptrack;		// Refers to an ogg_int64_t

		// Take physical pages, weld into a logical stream of packets
		public ogg_stream_state os;

		// Central working state for the packet->PCM decoder
		public vorbis_dsp_state vd;

		// Local working space for packet->PCM decode
		public vorbis_block vb;

		public ov_callbacks callbacks;
	}

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern int ov_fopen(
		[In()] [MarshalAs(UnmanagedType.LPStr)]
			string path,
		ref OggVorbis_File vf
	);

	[DllImport(nativeLibName, EntryPoint = "ov_info", CallingConvention = CallingConvention.Cdecl)]
	private static extern IntPtr INTERNAL_ov_info(
		ref OggVorbis_File vf,
		int link
	);
	public static vorbis_info ov_info(
		ref OggVorbis_File vf,
		int link
	) {
		IntPtr result = INTERNAL_ov_info(ref vf, link);
		vorbis_info info = (vorbis_info) Marshal.PtrToStructure(
			result,
			typeof(vorbis_info)
		);
		return info;
	}

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern long ov_read(
		ref OggVorbis_File vf,
		byte[] buffer,
		int length,
		int bigendianp,
		int word,
		int sgned,
		ref int current_section
	);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	public static extern int ov_clear(
		ref OggVorbis_File vf
	);

	#endregion
}
