#region License
// 
// Author: Joe McLain <nmp.developer@outlook.com>
// Copyright (c) 2013, Joe McLain and Digital Writing
// 
// Licensed under Eclipse Public License, Version 1.0 (EPL-1.0)
// See the file LICENSE.txt for details.
// 
#endregion
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

using NmpBase;


#pragma warning disable 414


namespace Nmp {


	/////////////////////////////////////////////////////////////////////////////

	partial class NMP {

		// ******
		//
		// {10F126C4-BE00-40FE-A0D1-613420E1674B}
		//
		static Guid nmpEventProviderId = new Guid( 0x10f126c4, 0xbe00, 0x40fe, 0xa0, 0xd1, 0x61, 0x34, 0x20, 0xe1, 0x67, 0x4b );


		/////////////////////////////////////////////////////////////////////////////

		public class NmpMakeCurrent : IDisposable {

			NMP	nmp;
			object savedState;
			object eventWriterToken;

			/////////////////////////////////////////////////////////////////////////////
		
			public void Dispose()
			{
				ThreadContext.SetThreadStorage( savedState );
				nmp = null;
				savedState = null;
			}
	

			/////////////////////////////////////////////////////////////////////////////

			private void MakeCurrent()
			{
				savedState = ThreadContext.SetThreadStorage( nmp.ThreadContextState );
				eventWriterToken = EventWriter.SetCurrentWriter( nmp );
			}


			/////////////////////////////////////////////////////////////////////////////

			public NmpMakeCurrent( NMP nmp )
			{
				// ******
				this.nmp = nmp;	// as NMP;

				//System.Diagnostics.Trace.Assert( null != this.nmp, "NmpMakeCurrent was passed an invalid NMP instance" );

				// ******
				MakeCurrent();
			}

		}


	}
}
