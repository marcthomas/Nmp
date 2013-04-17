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


namespace Nmp {


	/////////////////////////////////////////////////////////////////////////////

	// ?? TransitoryEvaluator

	public class NmpEvaluator : NmpBaseEvaluator {

		NMP.NmpMakeCurrent makeCurrent;

		
		/////////////////////////////////////////////////////////////////////////////

		//
		// if NmpEvaluator is NOT being used in a using statement then wrap any
		// access to NmpEvaluator by calling this method first inside of a using
		// statment
		//
		// using( nmpEval.MakeCurrent() {
		// }
		//
		
		public IDisposable MakeCurrent()
		{
			return new NMP.NmpMakeCurrent( GetNmpAsObject() as NMP );
		}
			

		/////////////////////////////////////////////////////////////////////////////
	
		public override void Dispose()
		{
			//
			// disposes whatever NmpBaseEvaluator (and it's wrapped NMP instance)
			// need to dispose of then calls Dispose() on our NmpMakeCurrent instance
			// which will restore the previous NMP instance (or null) on the current
			// thread
			//

			//
			// restores previous instance of NMP, want to do this before we dispose
			// of NmpBaseEvaluator and NMP
			//
			makeCurrent.Dispose();

			//
			// Dispose NmpBaseEvaluator - which Disposes of NMP
			//
			base.Dispose();
		}
	

		/////////////////////////////////////////////////////////////////////////////

		public NmpEvaluator( INmpHost host )
			:	base(host)
		{
			//
			// saves the threads current NMP instance (might be null) and then
			// sets the NNP instance wrapped by NmpBaseEvaluator as the threads
			// current NMP instance
			//
			makeCurrent = new NMP.NmpMakeCurrent( GetNmpAsObject() as NMP );
		}

	}


}
