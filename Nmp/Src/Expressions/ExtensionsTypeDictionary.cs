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

			//
			// by doing this it may see derived type before it sees it's own type
			//
			// or am i nuts ??


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
						if( parameters.Length > 0 && types.Contains( parameters [ 0 ].ParameterType ) ) {
							return true;
						}
					}

				}
			}


			// ******
			return false;
		}


		/////////////////////////////////////////////////////////////////////////////

		public IEnumerable<MethodInfo> SortByFirstArg( IEnumerable<MethodInfo> methods )
		{
			// ******
			//
			// ?? order methods by first argument where the most derived appears first
			//
			List<KeyValuePair<MethodInfo, Type>> list = new List<KeyValuePair<MethodInfo, Type>> { };
			foreach( var method in methods ) {
				var parameters = method.GetParameters();
				if( 0 == parameters.Length ) {
					continue;
				}
				list.Add( new KeyValuePair<MethodInfo, Type>( method, parameters [ 0 ].ParameterType ) );
			}

			// ******
			list.Sort( ( l, r ) => l.Value.IsSubclassOf( r.Value ) ? -1 : 1 );

			// ******
			List<MethodInfo> returnList = new List<MethodInfo> { };
			foreach( var item in list ) {
				returnList.Add( item.Key );
			}

			// ******
			return returnList;
		}


		/////////////////////////////////////////////////////////////////////////////

		//public Tuple<MethodBase, object [], List<MethodBase>> FindExtensionMethod( Type objType, string methodName, object [] argsIn, Func<IList<MethodBase>, object [], Tuple<MethodBase, object []>> matchArgs )
		//{
		//	return null;
		//}
	
		public Tuple<MethodBase, object [], List<MethodBase>> FindExtensionMethod( Type objType, string methodName, object [] argsIn )
		{
			// ******
			MethodBase methodBase = null;
			object [] argsOut = null;
			var mbList = new List<MethodBase> { };

			// ******
			var extItem = FindTypes( objType );
			if( null != extItem ) {
				Type type = extItem.Type;

				foreach( var extType in extItem.List ) {
					//
					// get methods in extType (class that implements extension types) that
					// match 'methodName'
					//
					var methods = extType.GetMethods().Where( mi => methodName == mi.Name );

					// ******
					//
					// order methods by first argument where the most derived appears first, this
					// allows us to select the first method whose first argument can be assigned
					// 'objType' without fear that we'll select a method whose first arg is less
					// derived - in other words, if we're passed RuntimeConstructorInfo we'll get
					// ConstructorInfo before we'll get MethodBase - assuming all other args are
					// equal (MatchArgs does the actual selection)
					//
					methods = SortByFirstArg( methods );
					
					// ******
					//
					// don't have to do this pruning, MatchArgs may do it for us
					//
					foreach( var method in methods ) {
						//if( method.IsStatic ) {

						var parameters = method.GetParameters();
						if( parameters.Length > 0 ) {
							if( parameters [ 0 ].ParameterType.IsAssignableFrom( objType ) ) {
								mbList.Add( method );
							}
						}

						//}
					}
				}

				// ******
				if( mbList.Count > 0 ) {
					argsOut = ArgumentMatcher.MatchArgs( mbList, argsIn, out methodBase );
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
