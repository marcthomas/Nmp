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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;

using NmpBase;
using NmpBase.Reflection;
using Nmp.Expressions;

#pragma warning disable 618

namespace Nmp.Builtin.Macros {


	
	/////////////////////////////////////////////////////////////////////////////

	class ObjectMacros : MacroContainer {

		// ******
		static string [] macroNames = { "$", "$nmp" };

		// ******
		//IMacroProcessor mp;


		/////////////////////////////////////////////////////////////////////////////
		//
		// #gettype
		//
		/////////////////////////////////////////////////////////////////////////////

		public object getType( string typeName )
		{
			// ******
			Type type = TypeLoader.GetType( typeName );
			if( null == type ) {
				ThreadContext.MacroError( "the #newobject macro could not locate the type \"{0}\"", typeName );
			}
			
			// ******
			return type;
		}


		/////////////////////////////////////////////////////////////////////////////
		//
		// #typeof
		//
		/////////////////////////////////////////////////////////////////////////////

		public object @typeof( object obj )
		{
			// ******
			if( null == obj ) {
				return string.Empty;
			}

			// ******
			return obj.GetType().ToString().Replace( '`', '^' );
		}


		/////////////////////////////////////////////////////////////////////////////
		//
		// #newobject
		//
		/////////////////////////////////////////////////////////////////////////////

		public static void LoadAssembly( string assemblyPathIn )
		{
			// ******
			if( string.IsNullOrEmpty( assemblyPathIn ) ) {
				ThreadContext.MacroError( "the assembly path passed to LoadAssembly is empty" );
			}
			else {
				//
				// we have an assembly name
				//
				try {
					var assembly = Assembly.LoadFrom( assemblyPathIn );
				}
				catch( Exception ex ) {
					ThreadContext.MacroError( "could not load assembly {0}\nException: {1}", assemblyPathIn, ex.AllExMessages() );
				}

			}

			// ******
			//return string.Empty;
		}


		/////////////////////////////////////////////////////////////////////////////

		private object CreateNewObject( string assemblyPath, string typeName, object [] constructorArgs )
		{
			// ******
			try {
				object newObject = TypeCreator.Create( assemblyPath, typeName, constructorArgs );
				return newObject;
			}
			catch ( Exception ex ) {
				ThreadContext.MacroError( "an object creation macro could not create  the type \"{0}\": {1}", typeName, ex.Message );
				return null;
			}
		}
		
		
		/////////////////////////////////////////////////////////////////////////////

		public object newObject( string typeName, params object [] constructorArgs )
		{
			// ******
			object newObject = CreateNewObject( null, typeName, constructorArgs );
			return newObject;
		}
		
		
		/////////////////////////////////////////////////////////////////////////////

		public object newObjectMacro( string macroName, string typeName, params object [] constructorArgs )
		{
			// ******
			object obj = newObject( typeName, constructorArgs );
			mp.AddObjectMacro( macroName, obj );

			// ******
			return string.Empty;
		}


		/////////////////////////////////////////////////////////////////////////////

		// with assembly path

		public object newObject( string assemblyPath, string typeName, params object [] constructorArgs )
		{
			// ******
			object newObject = CreateNewObject( assemblyPath, typeName, constructorArgs );
			return newObject;
		}


		/////////////////////////////////////////////////////////////////////////////

		// with assembly path

		public object newObjectMacro( string macroName, string assemblyPath, string typeName, params object [] constructorArgs )
		{
			// ******
			object obj = newObject( assemblyPath, typeName, constructorArgs );
			mp.AddObjectMacro( macroName, obj );

			// ******
			return string.Empty;
		}


		/////////////////////////////////////////////////////////////////////////////
		//
		// #newstatic
		//
		/////////////////////////////////////////////////////////////////////////////

		private StaticStandin CreateNewStandin( string typeName )
		{
			// ******
			if( string.IsNullOrEmpty(typeName) ) {
				ThreadContext.MacroError( "the type name argument for the #newstatic macro is empty" );
			}

			// ******
			//Type type = Helpers.FindType( null, typeName, true );
			Type type = TypeLoader.GetType( typeName );
			if( null == type ) {
				ThreadContext.MacroError( "the #newstatic macro could not locate the type \"{0}\"", typeName );
			}
			
			// ******
			var standin = new StaticStandin( type );
			return standin;
		}


		/////////////////////////////////////////////////////////////////////////////

		public object newStatic( string typeName )
		{
			object standin = CreateNewStandin( typeName );
			return standin;
		}


		/////////////////////////////////////////////////////////////////////////////

		public object newStaticMacro( string macroName, string typeName )
		{
			// ******
			object standin = newStatic( typeName );
			mp.AddObjectMacro( macroName, standin );

			// ******
			return string.Empty;
		}


		/////////////////////////////////////////////////////////////////////////////

		public ObjectMacros( IMacroProcessor mp )
			:	base(mp)
		{
			//this.mp = mp;
		}


	}

}
