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
using System.Linq;
using System.Text;

using System.Reflection;

namespace NmpBase {


	/////////////////////////////////////////////////////////////////////////////

	public class ShouldNeverHappenException : Exception {

		/////////////////////////////////////////////////////////////////////////////

		public ShouldNeverHappenException( string msg = "" )
			:	base(msg)
		{
		}

	}


	///////////////////////////////////////////////////////////////////////////////

	public class ArgumentCastException : Exception {


		///////////////////////////////////////////////////////////////////////////////

		public ArgumentCastException( string fmt, params object [] args )
			: base( string.Format(fmt, args) )
		{
		}


		///////////////////////////////////////////////////////////////////////////////

		public ArgumentCastException( string convertToType, string textToConvert, string fmt, params object [] args )
			: base( string.Format( fmt, args ) )
		{
			Data.Add( "type", convertToType );
			Data.Add( "text", textToConvert );
		}
			

	}


	///////////////////////////////////////////////////////////////////////////////

	public class RunawayMacroException : Exception {

		///////////////////////////////////////////////////////////////////////////////

		public RunawayMacroException( string fmt, params object [] args )
			: base( string.Format(fmt, args) )
		{
		}

	}


	///////////////////////////////////////////////////////////////////////////////

	public class ExecutionTimeoutException : Exception {

		///////////////////////////////////////////////////////////////////////////////

		public ExecutionTimeoutException( string fmt, params object [] args )
			: base( string.Format(fmt, args) )
		{
		}

	}


	///////////////////////////////////////////////////////////////////////////////

	public class MacroErrorException : Exception {

		///////////////////////////////////////////////////////////////////////////////

		public MacroErrorException( string fmt, params object [] args )
			: base( Helpers.SafeStringFormat( fmt, args ) )
		{
		}

	}


	///////////////////////////////////////////////////////////////////////////////

	public class ExitException : Exception {

		public int ExitCode;


		///////////////////////////////////////////////////////////////////////////////

		public ExitException( int exitCode )
		{
			ExitCode = exitCode;
		}


		///////////////////////////////////////////////////////////////////////////////

		public ExitException( int exitCode, string fmt, params object [] args )
			: base( string.Format(fmt, args) )
		{
			ExitCode = exitCode;
		}

	}


	/////////////////////////////////////////////////////////////////////////////

	public static class ExceptionHelpers {

		/////////////////////////////////////////////////////////////////////////////

		public static string AllExMessages( this Exception exParent )
		{
			// ******
			StringBuilder sb = new StringBuilder();
			Exception e = exParent;		//exParent is TargetInvocationException ? exParent.InnerException : exParent;
			while( null != e ) {
				sb.AppendFormat( " : {0}", e.Message );
				e = e.InnerException;
			}

			// ******
			return sb.ToString();
		}


		/////////////////////////////////////////////////////////////////////////////
		
		public static string RecursiveMessage( Exception exParent, string message, params object [] args )
		{
			// ******
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat( message, args );

			// ******
			Exception e = exParent is TargetInvocationException ? exParent.InnerException : exParent;
			while( null != e ) {
				sb.AppendFormat( " : {0}", e.Message );
				e = e.InnerException;
			}
			
			// ******
			return sb.ToString();
		}
		
		
		/////////////////////////////////////////////////////////////////////////////

		public static Exception FindException( Exception ex, Type exType )
		{
			// ******
			while( null != ex ) {
				if( exType.Equals(ex.GetType()) ) {
					return ex;
				}
				ex = ex.InnerException;
			}

			// ******
			return null;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static bool ContainsException( Exception ex, Type exType )
		{
			return null != FindException( ex, exType );
		}


		/////////////////////////////////////////////////////////////////////////////
		
		public static Exception CreateException( string message, params object [] args )
		{
			return new Exception( string.Format(message, args) );
		}


		/////////////////////////////////////////////////////////////////////////////
		
		public static Exception CreateException( Exception exParent, string message, params object [] args )
		{
			return new Exception( string.Format(message, args), exParent );
		}


		/////////////////////////////////////////////////////////////////////////////
		
		public static Exception CreateArgumentException( string message, params object [] args )
		{
			return new ArgumentException( string.Format(message, args) );
		}


		/////////////////////////////////////////////////////////////////////////////
		
		public static Exception CreateArgumentExceptionWithName( string memberName, string message, params object [] args )
		{
			return new ArgumentException( string.Format(message, args), memberName );
		}


		/////////////////////////////////////////////////////////////////////////////
		
		public static Exception CreateInvalidOperationException( string message, params object [] args )
		{
			return new InvalidOperationException( string.Format(message, args) );
		}


		/////////////////////////////////////////////////////////////////////////////
		
		public static Exception CreateInvalidOperationException( Exception exParent, string message, params object [] args )
		{
			return new InvalidOperationException( string.Format(message, args), exParent );
		}


	}
			
}
