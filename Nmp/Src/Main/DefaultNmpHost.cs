using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NmpBase;
//using Nmp;

namespace Nmp {

	/////////////////////////////////////////////////////////////////////////////

	class DefaultNmpHost : INmpHost {

	// ******
		public IRecognizer					Recognizer	{ get { return null; } }
		public IMacroTracer					Tracer			{ get { return null; } }
		public IMacroProcessorBase MacroHandler	{ get { return null; } }
		public string HostName { get { return "default"; } }


		///////////////////////////////////////////////////////////////////////////////

		public bool ErrorReturns
		{
			get {
				return false;
			}
		}


		///////////////////////////////////////////////////////////////////////////////

		public void Error( Notifier notifier, string fmt, params object [] args )
		{
			// ******
			var ei = null == notifier.ExecutionInfo ? new ExecutionInfo() : notifier.ExecutionInfo;
			string fileName = ei.FileName;

			Console.WriteLine( ei.FullMessage );
			Console.WriteLine();
			Console.WriteLine( "{0} ({1},0): error: {2}", fileName, ei.Line, Helpers.SafeStringFormat( fmt, args ) );

			// ******
			throw new ExitException(1);
		}


		///////////////////////////////////////////////////////////////////////////////

		public void Warning( Notifier notifier, string fmt, params object [] args )
		{
			// ******
			var ei = null == notifier.ExecutionInfo ? new ExecutionInfo() : notifier.ExecutionInfo;
			string fileName = ei.FileName;
			Console.WriteLine( "{0} ({1},0): warning: {2}", fileName, ei.Line, string.Format(fmt, args) );
		}


		///////////////////////////////////////////////////////////////////////////////

		public void WriteMessage( ExecutionInfo ei, string fmt, params object [] args )
		{
			Console.WriteLine( Helpers.SafeStringFormat(fmt, args) );
		}



		///////////////////////////////////////////////////////////////////////////////

		public void Trace( ExecutionInfo ei, string fmt, params object [] args )
		{
			System.Diagnostics.Trace.WriteLine( string.Format("at Line {0}, in {1}:", ei.Line, ei.FileName) );
			System.Diagnostics.Trace.WriteLine( Helpers.SafeStringFormat(fmt, args) );
		}


		/////////////////////////////////////////////////////////////////////////////
		
		public void Die( string fmt, params object [] args )
		{
			Console.Error.WriteLine( Helpers.SafeStringFormat(fmt, args) );
			Environment.Exit( 1 );
		}


	}
	

}
