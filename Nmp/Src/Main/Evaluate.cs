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
using NmpEvaluators;
using Nmp.Output;

namespace Nmp {


	/////////////////////////////////////////////////////////////////////////////

	partial class NMP {

		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Evaluate the text in the IEvaluationContext, newOutput is currently
		/// ignored - each evaluation gets a new Output object
		/// </summary>
		/// <param name="evx"></param>
		/// <param name="newOutput"></param>
		/// <returns></returns>

		public string Evaluate( IEvaluationContext evx, bool newOutput )
		{
			// ******
			//
			// EvalLock() will throw an exception if this instance of NMP is already
			// evaluating something, it also calls Restore()/Save() for our thread
			// data
			//
			using( new EvalLock( this ) ) {
				try {
					using( var input = gc.GetMasterParseReader( new ParseStringReader( evx.Text, evx.FileName ) ) ) {
						MasterOutput output = GetMasterOutput( newOutput );
						string result = string.Empty;

						// ******
						var mir = new MIR( null, input, "Root" );

						using ( Get<InvocationContext>().Init( mir ) ) {
							SetMacroProcessorOutputInstance( output );
							
							Scanner( input, output );
							StringBuilder sb = output.AllText;
							result = sb.ToString();

							SetMacroProcessorOutputInstance( null );
						}

						// ******
						return result;
					}
				}
				finally {
				}
			}
		}


		/////////////////////////////////////////////////////////////////////////////
		
		MasterOutput multileEvalOutput = null;
		
		
		public string GetMultileEvalOutput()
		{
			// ******
			if( null == multileEvalOutput ) {
				return string.Empty;
			}
		
			// ******
			StringBuilder sb = multileEvalOutput.AllText;
			string result = sb.ToString();
			return result;
		}
		
		
		/////////////////////////////////////////////////////////////////////////////
		
		public void ClearMultileEvalOutput()
		{
			multileEvalOutput = null;
		}
		
		
		/////////////////////////////////////////////////////////////////////////////
		
		public void MultileEvaluate( IEvaluationContext evx )
		{
			// ******
			//
			// EvalLock() will throw an exception if this instance of NMP is already
			// evaluating something, it also calls Restore()/Save() for our thread
			// data
			//
			using( new EvalLock( this ) ) {
				try {
					using( var input = gc.GetMasterParseReader( new ParseStringReader( evx.Text, evx.FileName ) ) ) {
						
						if( null == multileEvalOutput ) {
							multileEvalOutput = new MasterOutput( gc );
						}
		
						// ******
						var mir = new MIR( null, input, "Root" );
						
						using( Get<InvocationContext>().Init( mir ) ) {
							SetMacroProcessorOutputInstance( multileEvalOutput );
							Scanner( input, multileEvalOutput );
							SetMacroProcessorOutputInstance( null );
						}
		
					}
				}
				finally {
				}
			}
		}
		
		
	}


}
