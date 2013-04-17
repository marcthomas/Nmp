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
using System.Linq;
using System.Reflection;

using NmpBase;
using Nmp;


namespace Nmp.Expressions {

	/////////////////////////////////////////////////////////////////////////////

	class IndexerInvoker : Invoker {

		ObjectInfo info;


		/////////////////////////////////////////////////////////////////////////////

		protected static object [] FindMethod( ObjectInfo objInfo, object [] argsIn, out MethodBase miOut )
		{
			// ******
			IList<MethodBase> methods = objInfo.MembersAs<MethodBase>();

			// ******
			MethodBase mi;
			object [] args = MatchArgs( methods, argsIn, out mi );
			if( null != args ) {
				miOut = mi;
				return args;
			}

			//
			// never returns
			//
			ThreadContext.MacroError( "unable to locate an implementation of \"{0}\" whose parameters match (or can be converted from) {1}\nTried matching:\n{2}", objInfo.MemberName, Arguments.ObjectsTypeNames(argsIn), Arguments.GetMethodSignatures(methods) );

			// ******
			miOut = null;
			return null;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static object Invoke( ObjectInfo objInfo, object [] argsIn )
		{
			// ******
			MethodBase mi;
			object [] args = FindMethod( objInfo, argsIn, out mi );
			if( null == args ) {
				//
				// could not match arguments
				//
				ThreadContext.MacroError( "could not locate the indexer \"{0}\" that takes the argument types {1} on the object type \"{2}\"", objInfo.MemberName, Arguments.ObjectsTypeNames( argsIn ), objInfo.ObjectType.Name );
			}

			// ******
			try {
				return mi.Invoke( objInfo.Object, args );
			}
			catch ( Exception ex ) {
//				var ior = ExceptionHelpers.FindException( ex, typeof( IndexOutOfRangeException ) );
//				if( null != ior ) {
				if( null != ExceptionHelpers.FindException( ex, typeof( IndexOutOfRangeException ) ) || null != ExceptionHelpers.FindException( ex, typeof( ArgumentOutOfRangeException ) ) ) {
					//
					// for index out of range we warn and return null
					//
					//ThreadContext.MacroWarning( "error invoking indexer \"{0}\": {1}", objInfo.MemberName, ior.Message );
					ThreadContext.MacroWarning( "Indexer out of range: [{0}]", argsIn[0] );
				}
				else {
					//
					// never returns
					//
					//ThreadContext.MacroError( ExceptionHelpers.RecursiveMessage( ex, "error invoking indexer \"{0}\"", objInfo.MemberName ) );
					ThreadContext.MacroError( "Unable to index object \"{0}\", indexer [{1}]", objInfo.GetType().Name, argsIn[0] );
				}

				// ******
				return null;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public override object Invoke( object [] argsIn )
		{
			return Invoke( info, argsIn );
		}


		/////////////////////////////////////////////////////////////////////////////

		public IndexerInvoker( ObjectInfo objInfo )
		{
			info = objInfo;
		}

	}
}
