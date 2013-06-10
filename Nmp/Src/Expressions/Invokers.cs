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
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;


#pragma warning disable 414, 219

using NmpBase;
using Nmp;


namespace NmpExpressions {


	/////////////////////////////////////////////////////////////////////////////

	static class Invokers {

		/////////////////////////////////////////////////////////////////////////////
		
		//public static TypeHelperDictionary	TypeHelpers	{ get { return ThreadContext.TypeHelpers; } }


		/////////////////////////////////////////////////////////////////////////////

		public static Invoker GetMethodInvoker( object parent, string methodName, TypeHelperDictionary typeHelpers, ExtensionTypeDictionary methodExtensions )
		{
			// ******
			var objInfo = new ObjectInfo( parent, methodName );
			if( objInfo.IsMethod ) {
				return new MethodInvoker( objInfo );
			}
			else if( methodExtensions.IsPossibleExtension( parent.GetType(), methodName ) ) {
				//
				// there is at least one possible method extension; MethodInvoker.MethodInvoke() will
				// fail when looking up method "memberName", however it will search for an extension
				// method and use it if found
				//
				return new MethodExtensionInvoker( parent, methodName, methodExtensions );
			}

			// ******
			INmpDynamic dyn = parent as INmpDynamic;
			if( null != dyn ) {
				if( dyn.HasMethod(methodName) ) {
					return new DynamicMethodInvoker( parent, methodName );
				}
			}

			// ******
			object standin = typeHelpers.GetHelper( parent );
			if( null != standin ) {
				objInfo = new ObjectInfo( standin, methodName );
				if( objInfo.IsMethod ) {
					return new MethodInvoker( objInfo );
				}
			}

			// ******
			return null;
		}
		
		
		/////////////////////////////////////////////////////////////////////////////

		public static Invoker GetIndexerInvoker( object parent, string indexerName, TypeHelperDictionary typeHelpers )
		{
			// ******
			var objInfo = new ObjectInfo( parent, indexerName );
			if( objInfo.IsIndexer ) {
				return new IndexerInvoker( objInfo );
			}

			// ******
			INmpDynamic dyn = parent as INmpDynamic;
			if( null != dyn && dyn.HasIndexer(indexerName) ) {
				return new DynamicIndexerInvoker( parent, indexerName);
			}

			// ******
			object standin = typeHelpers.GetHelper( parent );
			if( null != standin ) {
				objInfo = new ObjectInfo( standin, indexerName );
				if( objInfo.IsIndexer ) {
					return new IndexerInvoker( objInfo );
				}
			}

			// ******
			return null;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static Invoker GetDefaultIndexer( object parent, TypeHelperDictionary typeHelpers )
		{
			//
			// looks up default indexer name and then calls GetIndexerInvoker() - above
			//

			// ******
			Type objectType = ObjectInfo.GetObjectType( parent );
			object [] customAttrs = objectType.GetCustomAttributes( typeof(DefaultMemberAttribute), true );

			// ******
			string memberName = string.Empty;

			if( 0 == customAttrs.Length ) {
				//var methods = objectType.GetMethods();
				//if( 0 == methods.Length ) {
				//	return null;
				//}
				//
				//
				//is array use Get ??
				//
				//bool found = false;
				//foreach( MethodInfo method in methods ) {
				//	if( method.Name == "Item" ) {
				//		found = true;
				//		break;
				//	}
				//}
				//
				//if( ! found ) {
				//	return null;
				//}
				//
				//memberName = "Item";
				if( objectType.IsArray ) {
					memberName = "GetValue";
				}
				else {
					return null;
				}
			}
			else {
				memberName = ((DefaultMemberAttribute) customAttrs [ 0 ]).MemberName;
			}
			
			// ******
			return GetIndexerInvoker( parent, memberName, typeHelpers );
		}


		/////////////////////////////////////////////////////////////////////////////

		public static object EvalMemberHelper( object parent, string memberName, TypeHelperDictionary typeHelpers, ExtensionTypeDictionary methodExtensions )
		{
			// ******
			object value;

			// ******
			var oo = new ObjectInfo( parent, memberName );
			if( null != (value = oo.GetValue()) ) {
				return value;
			}
			else if( methodExtensions.IsPossibleExtension( parent.GetType(), memberName ) ) {
				return new MethodExtensionInvoker( parent, memberName, methodExtensions );
			}

			// ******
			INmpDynamic dyn = parent as INmpDynamic;
			if( null != dyn ) {
				NmpDynamicType dynType = dyn.HasMember( memberName );

				switch( dynType ) {
					case NmpDynamicType.None:
						break;

					case NmpDynamicType.Property:
						return DynamicPropertyInvoker.Invoke( parent, memberName );

					case NmpDynamicType.Indexer:
						return new DynamicIndexerInvoker( parent, memberName );

					case NmpDynamicType.Method:
						return new DynamicMethodInvoker( parent, memberName );
				}
			}

			// ******
			object standin = typeHelpers.GetHelper( parent );
			if( null != standin ) {
				oo = new ObjectInfo( standin, memberName );
				if( null != (value = oo.GetValue()) ) {
					return value;
				}
			}

			// ******
			return null;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static object EvalMethodCallHelper( object objIn, object [] args )
		{
			// ******
			//
			// expression:  macro( ... )
			//
			// expression:  macro.something( x )( y ), or macro.something[ z ]( zz )
			//
			object objResult = null;

			// ******
			//
			// could be a MethodInvoker or DelegateInvoker
			//
			Invoker invoker = objIn as Invoker;
			if( null != invoker ) {
				//
				// only need to supply args
				//
				objResult = invoker.Invoke( args );
			}
			else {
				//
				// could be a "raw" delegate retreived by a simple field or
				// property reference, #define( `delegate', `@someObj.delegate')
				//
				invoker = DelegateInvoker.TryCreate( objIn );
				if( null != invoker ) {
					objResult = invoker.Invoke( args );
				}
				else {
					//
					// it's not a MethodInvoker or delegate then we don't know how to handle it
					//
					ThreadContext.MacroError( "expecting method reference or Delegate, found: {0}", ObjectInfo.GetTypeName(objIn) );
				}
			}

			// ******
			return objResult;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static object EvalIndexerHelper( object objIn, object [] args, TypeHelperDictionary typeHelpers )
		{
			// ******
			//
			// expression:  macro[ ... ]
			//
			// expression:  macro.something[ x ][ y ], or macro.something( z )[ zz ]
			//
			object objResult = null;

			// ******
			Invoker invoker = objIn as Invoker;
			if( null != invoker ) {
				objResult = invoker.Invoke( args );
			}
			else {
				invoker = Invokers.GetDefaultIndexer( objIn, typeHelpers );
				if( null == invoker ) {
					ThreadContext.MacroError( "there is no default indexer for the object type \"{0}\"",  ObjectInfo.GetTypeName(objIn) );
				}
				objResult = invoker.Invoke( args );
			}

			// ******
			return objResult;
		}


	}


}


/*

signature of delegate if you have its type

MethodInfo method = delegateType.GetMethod("Invoke");     
Console.WriteLine(method.ReturnType.Name + " (ret)");     
foreach (ParameterInfo param in method.GetParameters()) {          
	Console.WriteLine("{0} {1}", param.ParameterType.Name, param.Name);
} 



*/

