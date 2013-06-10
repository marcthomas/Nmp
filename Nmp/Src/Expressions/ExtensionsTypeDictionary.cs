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
using NmpBase.Reflection;


namespace NmpExpressions {


	/////////////////////////////////////////////////////////////////////////////

	public class ExtensionTypeDictionary {

		/// <summary>
		/// 
		/// Type hold the original type that was registered
		/// </summary>
		class ExtensionItem {
			public Type Type { get; set; }
			public List<Type> List { get; set; }
		}


		// ******
		Dictionary<Type, ExtensionItem> dict = new Dictionary<Type, ExtensionItem> { };

		/////////////////////////////////////////////////////////////////////////////

		public void AddMethodExtension( Type extType, Type implementsExtensionMethodsClassType )
		{
			// ******
			ExtensionItem item;

			if( dict.TryGetValue( extType, out item ) ) {
				if( !item.List.Contains( implementsExtensionMethodsClassType ) ) {
					item.List.Add( implementsExtensionMethodsClassType );
				}
			}
			else {
				dict [ extType ] = new ExtensionItem { Type = extType, List = new List<Type> { implementsExtensionMethodsClassType } };
			}
		}

		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// 
		/// 
		/// At runtime we're going to get "Runtime" types, not the types that were
		/// passed to us (or that we looked up via name) so we need walk the BaseType
		/// list until we find (or don't find) the types that were interested in
		/// 
		/// When we find a runtime type we put it in the dictionary with its value
		/// being the same as the type were interested in - both the type and its
		/// runtime type will now have dictionary entries
		/// 
		/// </summary>
		/// <param name="typeIn"></param>
		/// <returns></returns>

		ExtensionItem FindTypes( Type typeIn )
		{
			// ******
			ExtensionItem item;

			// ******
			if( dict.TryGetValue( typeIn, out item ) ) {
				return item;
			}

			for( var type = typeIn.BaseType; null != type; type = type.BaseType ) {
				if( dict.TryGetValue( type, out item ) ) {
					dict.Add( typeIn, item );
					return item;
				}
			}

			// ******
			return null;
		}


		/////////////////////////////////////////////////////////////////////////////

		public bool IsPossibleExtension( Type typeIn, string methodName )
		{
			// ******
			var extItem = FindTypes( typeIn );

			if( null != extItem ) {
				Type type = extItem.Type;
				
				//
				// list of type and types its derived from
				//
				List<Type> types = new List<Type> { type };
				for( var t = type.BaseType; null != t; t = t.BaseType ) {
					if( typeof( object ) != t ) {
						types.Add( t );
					}
				}

				foreach( var extType in extItem.List ) {
					var methods = extType.GetMethods().Where( mi => methodName == mi.Name );

					foreach( var method in methods ) {
						var parameters = method.GetParameters();
						//
						// accept parameter that is 'type' or anything it derives from
						//
						if( parameters.Length > 0 && types.Contains( parameters [ 0 ].ParameterType) ) {
							return true;
						}
					}

				}
			}


			// ******
			return false;
		}


		/////////////////////////////////////////////////////////////////////////////

		public Tuple<MethodBase, object [], List<MethodBase>> FindExtensionMethod( Type objType, string methodName, object [] argsIn, Func<IList<MethodBase>, object [], Tuple<MethodBase, object []>> matchArgs )
		{
			// ******
			MethodBase methodBase = null;
			object [] argsOut = null;
			var mbList = new List<MethodBase> { };

			// ******
			//
			// optimize check 
			//

			// ******
			var extItem = FindTypes( objType );
			if( null != extItem ) {
				Type type = extItem.Type;

				foreach( var extType in extItem.List ) {
					var methods = extType.GetMethods().Where( mi => methodName == mi.Name );
					mbList.AddRange( methods );
				}

				var result = matchArgs( mbList, argsIn );
				if( null != result && null != result.Item1 ) {
					methodBase = result.Item1;
					argsOut = result.Item2;

					//
					// build optimization
					//
				}
			}

			// ******
			return new Tuple<MethodBase, object [], List<MethodBase>>( methodBase, argsOut, mbList );
		}


		/////////////////////////////////////////////////////////////////////////////

		//public List<MethodBase> FindPossibleExtensions( Type typeIn, string methodName )
		//{
		//	// ******
		//	var mbList = new List<MethodBase> { };
		//	var extItem = FindTypes( typeIn );
		//	if( null == extItem ) {
		//		return mbList;
		//	}

		//	// ******
		//	Type type = extItem.Type;

		//	//
		//	// if we could find a way to "know" what method was successufully used we could
		//	// hash methodName typeIn and method info for a quick lookup the next time
		//	//

		//	foreach( var extType in extItem.List ) {
		//		var methods = extType.GetMethods().Where( mi => methodName == mi.Name );
		//		foreach( var method in methods ) {
		//			var parameters = method.GetParameters();
		//			if( parameters.Length > 0 && type == parameters [ 0 ].ParameterType ) {
		//				mbList.Add( method );
		//			}
		//		}

		//	}

		//	// ******
		//	return mbList;
		//}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Check 'handlingClassType' for extensions to 'extTypes'
		/// </summary>
		/// <param name="implementsExtensionMethodsType"> Type of a class that contains extensions</param>
		/// <param name="extTypes"> Extension types that are implemented in 'handlingClassType'</param>

		public void AddMethodExtensions( Type implementsExtensionMethodsType, params Type [] extTypes )
		{
			// ******
			foreach( var type in extTypes ) {
				AddMethodExtension( type, implementsExtensionMethodsType );
			}
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Type by the name 'handlingClassName' implemnts extensions of 'extTypeName'
		/// and 'moreExtTypeNames'
		/// </summary>
		/// <param name="implementsExtensionMethodsClassName"></param>
		/// <param name="extTypeName"></param>
		/// <param name="moreExtTypeNames"></param>

		public void AddMethodExtensions( string implementsExtensionMethodsClassName, string extTypeName, params string [] moreExtTypeNames )
		{
			// ******
			Type implementsExtensionMethodsType = TypeLoader.GetType( implementsExtensionMethodsClassName );
			if( null == implementsExtensionMethodsType ) {
				ThreadContext.MacroError( "extensions error: could not locate the type \"{0}\"", implementsExtensionMethodsClassName );
			}

			// ******
			Type extHandlerType = TypeLoader.GetType( extTypeName );
			if( null == extHandlerType ) {
				ThreadContext.MacroError( "extensions error: could not locate the type \"{0}\"", implementsExtensionMethodsClassName );
			}

			// ******
			AddMethodExtension( extHandlerType, implementsExtensionMethodsType );

			foreach( var name in moreExtTypeNames ) {
				extHandlerType = TypeLoader.GetType( name );

				if( null == extHandlerType ) {
					ThreadContext.MacroError( "extensions error: could not locate the type \"{0}\"", implementsExtensionMethodsClassName );
				}

				// ******
				AddMethodExtension( extHandlerType, implementsExtensionMethodsType );
			}
		}


	}




}
