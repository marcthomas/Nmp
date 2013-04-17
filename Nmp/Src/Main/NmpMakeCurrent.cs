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


		/////////////////////////////////////////////////////////////////////////////

		public class NmpMakeCurrent : IDisposable {

			NMP	nmp;
			object savedState;

			/////////////////////////////////////////////////////////////////////////////
		
			public void Dispose()
			{
				ThreadContext.SetThreadStorage( savedState );
				nmp = null;
				savedState = null;
			}
	

			/////////////////////////////////////////////////////////////////////////////

			//private void MakeCurrent()
			//{
			//	savedState = nmp.MakeCurrent();
			//}
			//

			private void MakeCurrent()
			{
				savedState = ThreadContext.SetThreadStorage( nmp.ThreadContextState );
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
