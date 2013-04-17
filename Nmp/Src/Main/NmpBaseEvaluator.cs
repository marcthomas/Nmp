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
using Nmp.Output;

namespace Nmp {


	/////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// NmpEvaluator is the class seen by the outside world, create and invoke
	/// methods on this class to run the macro processor
	/// </summary>

	// ?? TransitoryEvaluator

	public class NmpBaseEvaluator : IDisposable {

		// ******
		NMP	nmp;

		public string FileExt			{ get { return nmp.OutputFileExt; }}
		public bool NoOutputFile { get { return nmp.NoOutputFile; } }
		public string OutputEncoding { get { return nmp.OutputEncoding; } }


		/////////////////////////////////////////////////////////////////////////////
		
		protected object GetNmpAsObject()
		{
			return nmp;
		}
		

		///////////////////////////////////////////////////////////////////////////////
		//
		//private IMacroProcessor _MacroProcessorInterface
		//{
		//	get {
		//		return nmp.MacroProcessor;
		//	}
		//}
		//

		/////////////////////////////////////////////////////////////////////////////
		
		//public IMacroProcessor GetMacroProcessorInterface()
		//{
		//	return nmp.MacroProcessor;
		//}
		

		/////////////////////////////////////////////////////////////////////////////

		public virtual void Dispose()
		{
			nmp.Dispose();
		}


		/////////////////////////////////////////////////////////////////////////////

		public void ChangeRootDirectory( string path )
		{
			path = Path.GetFullPath( path ).ToLower();
			path = Path.GetDirectoryName( path );
			nmp.GrandCentral.GetDirectoryStack()[ 0 ] = path;
		}


		/////////////////////////////////////////////////////////////////////////////

		public IMacro AddMacro( IMacro macro )
		{
			// ******
			using( new NMP.NmpMakeCurrent(nmp) ) {
				if( macro.MacroText.Length > 0 ) {
					macro.MacroText = nmp.GrandCentral.FixText( macro.MacroText );
				}

				// ******
				return nmp.MacroProcessor.AddMacro( macro );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public IMacro AddObjectMacro( string macroName, object netObj )
		{
			using( new NMP.NmpMakeCurrent(nmp) ) {
				return nmp.MacroProcessor.AddObjectMacro( macroName, netObj );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public IMacro AddBuiltinMacro( string macroName, IMacroHandler mh )
		{
			using( new NMP.NmpMakeCurrent(nmp) ) {
				return nmp.MacroProcessor.AddBuiltinMacro( macroName, mh );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public IMacro AddBuiltinMacro( string macroName, IMacroHandler mh, object handlerData )
		{
			using( new NMP.NmpMakeCurrent(nmp) ) {
				return nmp.MacroProcessor.AddBuiltinMacro( macroName, mh, handlerData );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

//		public IMacro AddBuiltinMacro( string macroName, MacroCall macroProc )
//		{
//			return nmp.MacroProcessor.AddBuiltinMacro( macroName, macroProc );
//		}


		/////////////////////////////////////////////////////////////////////////////

		public void AddObjectMacros( object obj )
		{
			// ******
			using( new NMP.NmpMakeCurrent(nmp) ) {
				PropertyInfo [] pInfos = obj.GetType().GetProperties();

				foreach( var pi in pInfos ) {
					try {
						ParameterInfo [] pii = pi.GetIndexParameters();
						if( pii.Length > 0 ) {
							continue;
						}

						string name = pi.Name;
						if( name.StartsWith("___") ) {
							name = "#" + name.Substring( 3 );
						}

						AddObjectMacro( name, pi.GetValue(obj, null) );
					}
					catch {
					}
				}
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public IMacro AddTextMacro( string macroName, string macroText, NmpStringList argNames )
		{
			using( new NMP.NmpMakeCurrent(nmp) ) {
				string text = nmp.GrandCentral.FixText( macroText );
				return nmp.MacroProcessor.AddTextMacro( macroName, text, argNames );
			}
		}


		//////////////////////////////////////////////////////////////////////////////

		public void RegisterPowershell( IPowerShellInterface psIntf )
		{
			using( new NMP.NmpMakeCurrent(nmp) ) {
				nmp.MacroProcessor.RegisterPowershell( psIntf );
			}
		}


		//////////////////////////////////////////////////////////////////////////////

		public string GetMacroText( string name )
		{
			// ******
			using( new NMP.NmpMakeCurrent(nmp) ) {
				IMacro macro;
				if( ! FindMacro(name, out macro) ) {
					return null;
				}

				// ******
				return macro.MacroText;
			}
		}


		//////////////////////////////////////////////////////////////////////////////

		public object GetMacroObject( string name )
		{
			// ******
			using( new NMP.NmpMakeCurrent(nmp) ) {
				IMacro macro;
				if( ! FindMacro(name, out macro) ) {
					return null;
				}

				// ******
				return macro.MacroObject;
			}
		}


		//////////////////////////////////////////////////////////////////////////////

		public string GetDiversionText( string name )
		{
			// ******
			using( new NMP.NmpMakeCurrent(nmp) ) {
				var output = nmp.MacroProcessor.OutputInstance as MasterOutput;
				if( null == output ) {
					//
					// should never happend
					//
					return null;
				}

				// ******
				//
				// if does not exist will issue a warning and return empty string
				//
				return output.FetchDivert( name, false );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public bool FindMacro( string name, out IMacro macro )
		{
			using( new NMP.NmpMakeCurrent(nmp) ) {
				return nmp.MacroProcessor.FindMacro( name, out macro );
			}
		}


//		/////////////////////////////////////////////////////////////////////////////
//
////		object	InvokeMacro( IMacro macro, MacroExpression exp, bool postProcess );
//
//		public object _InvokeMacro( IMacro macro, object [] args, bool postProcess )
//		{
//			return nmp.MacroProcessor.InvokeMacro( macro, args, postProcess );
//		}
//
//
//
//		/////////////////////////////////////////////////////////////////////////////
//
//		public object InvokeMacro( string macroName, object [] args, bool postProcess )
//		{
//			return nmp.InvokeMacro( macroName, args, postProcess );
//		}
//
//

		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Invokes a macro by name
		/// </summary>
		/// <param name="macroName"></param>
		/// <param name="args"></param>
		/// <param name="newOutput"></param>
		/// <returns></returns>
		
		public string InvokeMacro( string macroName, object [] args, bool newOutput )
		{
			using( new NMP.NmpMakeCurrent(nmp) ) {
				return nmp.InvokeMacro( macroName, args, newOutput );
			}
		}
		

		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Invokes a macro with an instance of IEvaluationContext which provides
		/// the text to be processed along with some information on where it came
		/// from
		/// </summary>
		/// <param name="evx"></param>
		/// <param name="newOutput"></param>
		/// <returns></returns>
		
		public string Evaluate( IEvaluationContext evx, bool newOutput )
		{
			using( new NMP.NmpMakeCurrent(nmp) ) {
				return nmp.Evaluate( evx, newOutput );
			}
		}


		/////////////////////////////////////////////////////////////////////////////
		
		public string GetMultileEvalOutput()
		{
			return nmp.GetMultileEvalOutput();
		}
		
		
		/////////////////////////////////////////////////////////////////////////////
		
		public void ClearMultileEvalOutput()
		{
			//
			// NOT a reset, just clears output buffer
			//
			nmp.ClearMultileEvalOutput();
		}
		
		
		/////////////////////////////////////////////////////////////////////////////
		
		public void MultipleEvaluate( IEvaluationContext evx )
		{
			nmp.MultileEvaluate( evx );
		}
		

		/////////////////////////////////////////////////////////////////////////////

		public IEvaluationContext GetFileContext( string fileName )
		{
			return new EvaluateFromFileContext(nmp, fileName);
		}


		/////////////////////////////////////////////////////////////////////////////

		public IEvaluationContext GetStringContext( string text )
		{
			return new EvaluateStringContext(nmp, text);
		}


		/////////////////////////////////////////////////////////////////////////////

		public string	FixText( string [] text )
		{
			return nmp.MacroProcessor.FixText( text );
		}


		/////////////////////////////////////////////////////////////////////////////

		public string	FixText( string text )
		{
			return nmp.MacroProcessor.FixText( text );
		}


		/////////////////////////////////////////////////////////////////////////////

		public bool Initialize( INmpHost host )
		{
			//
			// if null == host then create a reporting host
			//

			nmp = new NMP( host );
			return null != nmp;
		}


		/////////////////////////////////////////////////////////////////////////////

		public NmpBaseEvaluator( INmpHost host )
		{
			Initialize( host );
		}

	
		/////////////////////////////////////////////////////////////////////////////


	}





}
