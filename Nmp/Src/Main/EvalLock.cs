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

//	[Serializable()]
	partial class NMP {

		private bool inEval = false;


		/////////////////////////////////////////////////////////////////////////////

		class EvalLock : UsingHelper {

			NMP nmp;

			/////////////////////////////////////////////////////////////////////////////

			public override void DoDispose()
			{
				//nmp.SaveEnvironment();
				nmp.inEval = false;
			}


			/////////////////////////////////////////////////////////////////////////////

			public EvalLock( NMP nmp )
			{
				// ******
				this.nmp = nmp;

				// ******
				object threadContext = ThreadContext.GetThreadStorage();
				System.Diagnostics.Trace.Assert( null != threadContext, "EvalLock: thread context is null" );
				System.Diagnostics.Trace.Assert( threadContext == nmp.threadContext, "EvalLock: the current thread context is not equal to the required thread context" );

				// ******
				if( nmp.inEval ) {
					throw new Exception( "Nmp: evaluation already in progress, Nmp is not reentrant" );
				}
				
				// ******
				nmp.inEval = true;
				//nmp.RestoreEnvironment();
			}
		}

	}


}
