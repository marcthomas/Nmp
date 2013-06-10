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

	public static class TypeCreator {


		/////////////////////////////////////////////////////////////////////////////

		public static object Create( Type type, object [] constructorArgs )
		{
			// ******													 
			ConstructorInfo [] ctors = type.GetConstructors();

			if( 0 == ctors.Length ) {
				throw ExceptionHelpers.CreateException( "could not locate any public constructors for the type \"{0}\"", type.FullName );
			}

			if( null == constructorArgs ) {
				constructorArgs = new object [0];
			}

			// ******
			MethodBase mi;
			object [] ctorArgs = null;

			try {
				ctorArgs = ArgumentMatcher.MatchArgs( ctors, constructorArgs, out mi );
			}
			catch {
				mi = null;
			}

			if( null == mi ) {
				if( 0 == constructorArgs.Length ) {
					throw ExceptionHelpers.CreateException( "unable to locate a default constructor \"{0}\"", type.FullName );
				}
				else {
					throw ExceptionHelpers.CreateException( "unable to locate a constructor for \"{0}\" whose parameters match (or can be converted from): {1}\nTried matching:\n{2}", type.FullName, Arguments.ObjectsTypeNames(constructorArgs), Arguments.GetMethodSignatures(ctors) );
				}
			}

			// ******
			try {
				object newObject = null;
				newObject = Activator.CreateInstance( type, ctorArgs );
				return newObject;
			}
			catch ( Exception ex ) {
				throw ExceptionHelpers.CreateException( "unable to create instance of \"{0}\", Activator.CreateInstance() threw an exception: {1}", type.FullName, ex.Message );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public static object Create( string assemblyPath, string typeName, object [] constructorArgs )
		{
			// ******
			//int numConstructorArgs = constructorArgs.Length;

			// ******
			if( string.IsNullOrEmpty(typeName) ) {
				throw new ArgumentNullException( "typeName");
			}

			// ******
			Type type = TypeLoader.GetType( assemblyPath, typeName );
			if( null == type ) {
				throw ExceptionHelpers.CreateException( "could not locate the type \"{0}\"", typeName );
			}
			
			// ******
			return Create( type, constructorArgs );
		}
		
		
		/////////////////////////////////////////////////////////////////////////////

		public static object Create( string typeName, object [] constructorArgs )
		{
			return Create( null, typeName, constructorArgs );
		}


	}


}
