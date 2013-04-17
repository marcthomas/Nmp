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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NmpBase.Reflection {


	/////////////////////////////////////////////////////////////////////////////
	
	[Serializable()]
	public class TypeList : List<Type> {
	}
	
	

	/////////////////////////////////////////////////////////////////////////////

	public static class TypeLoader {


		/////////////////////////////////////////////////////////////////////////////

		public static string GetTypeError( string caller, string assemblyPath, string typeName )
		{
			if( ! string.IsNullOrEmpty(assemblyPath) ) {
				return string.Format( "{0}: could not locate type \"{1}\" in \"{2}\"", caller, typeName, assemblyPath );
			}
			else {
				return string.Format( "{0}: could not locate type \"{1}\" ", caller, typeName );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public static bool FullTypeName( ref string typeName )
		{
			// ******
			int pos = typeName.IndexOf( '.' );
			bool fullTypeName = pos >= 0;

			// ******
			if( 0 == pos ) {
				//
				// if a dot is the first char the intent is that this
				// is a full type name search and the type we're looking
				// for does NOT live in a namespace
				//
				typeName = typeName.Substring( 1 );
			}

			// ******
			return fullTypeName;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static Type GetCommonType( string typeName )
		{
			// ******
			switch( typeName.ToLower() ) {
				case "int16":
					return typeof( Int16 );

				case "uint16":
					return typeof( UInt16 );

				case "int":
				case "int32":
					return typeof( Int32 );

				case "uint":
				case "uint32":
					return typeof( UInt32 );

				case "long":
				case "int64":
					return typeof( Int64 );

				case "ulong":
				case "uint64":
					return typeof( UInt64 );

				case "float":
				case "double":
					return typeof( double );

				case "datetime":
					return typeof( DateTime );

				case "decimal":
					return typeof( decimal );

				case "bool":
					return typeof( bool );

				case "string":
					return typeof( string );

				default:
					return null;
			}

		}


		/////////////////////////////////////////////////////////////////////////////

		public static Type GetType( string typeName, bool ignoreCase = true )
		{
			// ******
			Type type = null;
			bool fullTypeName = FullTypeName( ref typeName );

			// ******
			try {
				type = Type.GetType( typeName, false, ignoreCase );
				if( null != type ) {
					return type;
				}
			}
			catch {
			}

			// ******
			type = GetCommonType( typeName );
			if( null != type ) {
				return type;
			}

			// ******
			Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();

			//
			// first check assemblies NOT in the assembly cache
			//
			foreach( var asm in asms ) {
				if( ! asm.GlobalAssemblyCache ) {
					TypeList tl = GetType( asm, typeName, fullTypeName, ignoreCase );
					if( tl.Count > 0 ) {
						return tl[ 0 ];
					}
				}
			}

			//
			// then assemblies in the assembly cache
			//
			foreach( var asm in asms ) {
				if( asm.GlobalAssemblyCache ) {
					TypeList tl = GetType( asm, typeName, fullTypeName, ignoreCase );
					if( tl.Count > 0 ) {
						return tl[ 0 ];
					}
				}
			}

			// ******
			return null;
		}


		/////////////////////////////////////////////////////////////////////////////

		private static bool TypeNameCompare( string typeName, string compareToTypeName, bool ignoreCase )
		{
			// ******
			//
			// if ignoreCase is true then we assume 'typeName' has already been lowercased
			//
			//if( ignoreCase ) {
			//	return typeName == compareToTypeName.ToLower();
			//}
			//else {
			//	return typeName == compareToTypeName;
			//}

			return 0 == string.Compare( typeName, compareToTypeName, ignoreCase );
		}


		/////////////////////////////////////////////////////////////////////////////
		
		private static TypeList GetType( Assembly assembly, string typeName, bool fullTypeName, bool ignoreCase = true )
		{
			// ******
			TypeList list = new TypeList();
			if( assembly.IsDynamic ) {
				//
				// not supported for dynamic assemblies
				//
				return list;
			}
		
			if( ignoreCase ) {
				typeName = typeName.ToLower();
			}

			// ******
			//
			// only public types
			// 
			Type [] types = assembly.GetExportedTypes();
			
			foreach( Type type in types ) {
				if( fullTypeName ) {
					if( ! TypeNameCompare(typeName, type.FullName, ignoreCase) ) {
						continue;
					}
				}
				else {
					if( ! TypeNameCompare(typeName, type.Name, ignoreCase) ) {
						continue;
					}
				}
		
				// ******
				list.Add( type );
			}
			
			// ******
			return list;
		}
		

		/////////////////////////////////////////////////////////////////////////////

		public static TypeList GetType( Assembly assembly, string typeName, bool ignoreCase )
		{
			bool fullTypeName = FullTypeName( ref typeName );
			return GetType( assembly, typeName, fullTypeName, ignoreCase );
		}


		/////////////////////////////////////////////////////////////////////////////
		
		public static Type GetType( string assemblyPathIn, string typeNameIn, bool ignoreCase = true )
		{
			// ******
			if( string.IsNullOrEmpty(typeNameIn) ) {
				throw new ArgumentNullException( "typeNameIn" );
			}
		
			// ******
			Type type = null;

			if( string.IsNullOrEmpty(assemblyPathIn) ) {
				type = GetType( typeNameIn, ignoreCase );
			}
			else {
				//
				// we have an assembly name
				//
				var assembly = LoadAssembly( assemblyPathIn );
				TypeList list = GetType( assembly, typeNameIn, ignoreCase );
				type = list.Count > 0 ? list[ 0 ] : null;
			}

			// ******
			return type;
		}
		

		/////////////////////////////////////////////////////////////////////////////

		public static Type GetInterface( string typeName, bool ignoreCase )
		{
			// ******
			Type type = GetType( typeName, ignoreCase );
			return null == type ? null : (type.IsInterface ? type : null);
		}


		/////////////////////////////////////////////////////////////////////////////

		public static TypeList GetInterface( Assembly assembly, string TypeName, bool ignoreCase = true )
		{
			// ******
			TypeList list = new TypeList();
		
			// ******
			//
			// only public types
			// 
			Type [] types = assembly.GetExportedTypes();
			
			foreach( Type type in types ) {
				if( null == type.GetInterface(TypeName, ignoreCase) ) {
					continue;
				}

				// ******
				list.Add( type );
			}
			
			// ******
			return list;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static TypeList GetInterface( string path, string TypeName, bool ignoreCase = true )
		{
			// ******
			var assembly = LoadAssembly( path );
			return GetInterface( assembly, TypeName, ignoreCase );
		}


		/////////////////////////////////////////////////////////////////////////////

		private static Assembly LoadAssembly( string path )
		{
			// ******
			if( ! File.Exists(path) ) {
				throw new Exception( string.Format("could not locate assembly \"{0}\"", path) );
			}

			// ******
			try {
				return Assembly.LoadFrom( path );
			}
			catch ( Exception ex ) {
				throw new Exception( string.Format("TypeLoader could not load assembly \"{0}\"", path), ex );
			}
		}


		///////////////////////////////////////////////////////////////////////////////
		//
		//public InterfaceFinder()
		//{
		//	
		//}
		//

	}


}
