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
using System.Reflection;

using NmpBase;
using Nmp;


namespace Nmp.Expressions {


	/////////////////////////////////////////////////////////////////////////////

	class DelegateInvoker : Invoker {

		Delegate _delegate;


		/////////////////////////////////////////////////////////////////////////////

		private static string GetDeclaringTypeName( Delegate d )
		{
			// ******
			Type type = d.Method.DeclaringType;
			return null == type ? "unknown type" : type.Name;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static object Invoke( Delegate dg, object [] args )
		{
			// ******
			if( null == dg ) {
				throw new ArgumentNullException( "delegate" );
			}

			// ******
			MethodInfo mi = dg.Method;
			object [] newArgs = MatchArgs( mi, args );
			if( null == newArgs ) {
				//
				// could not match arguments
				//
				ThreadContext.MacroError( "could not match the parameters for delegate \"{0}\" with the argument types {1} on the object type \"{2}\"", dg.Method.Name, Arguments.ObjectsTypeNames( args ), GetDeclaringTypeName( dg ) );
			}

			// ******
			try {
				return dg.DynamicInvoke( newArgs );
			}
			catch( Exception ex ) {
				//
				// never returns
				//
				//ThreadContext.MacroError( "{0}.DynamicInvoke() failed: {1}", mi.Name, ex.Message );
				string msg = ExceptionHelpers.RecursiveMessage( ex, "{0}.DynamicInvoke() failed: {1}", mi.Name, ex.Message );
				ThreadContext.MacroError( msg );
				return null;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public static object Invoke( ObjectInfo info, object [] args )
		{
			// ******
			Delegate dg = info.GetFieldOrProperty() as Delegate;
			if( null == dg ) {
				//
				// need error
				//
				return null;
			}

			// ******
			return Invoke( dg, args );
		}


		/////////////////////////////////////////////////////////////////////////////

		public override object Invoke( object [] args )
		{
			return Invoke( _delegate, args );
		}


		/////////////////////////////////////////////////////////////////////////////

		public DelegateInvoker( Delegate _delegate )
		{
			this._delegate = _delegate;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static DelegateInvoker TryCreate( object possibleDelegate )
		{
			// ******
			Delegate dg = possibleDelegate as Delegate;
			return null == dg ? null : new DelegateInvoker( dg );
		}

	}
}
