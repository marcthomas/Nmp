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
using System.IO;
using System.Reflection;
using System.Text;


using NmpBase;

#pragma warning disable 414


namespace Nmp {



	/////////////////////////////////////////////////////////////////////////////

	partial class AutoRegisterMacros {


		/////////////////////////////////////////////////////////////////////////////
		
		public static bool RegisterMacroContainers( IMacroProcessor mp, string fullPath, bool displayFound )
		{
			// ******
			Assembly asm = null;

			// ******
			try {
				asm = Assembly.LoadFrom( fullPath );
			}
			catch ( Exception ex ) {
				throw ExceptionHelpers.CreateException( "unable to load library \"{0}\": {1}", fullPath, ex.Message );
			}
			
			// ******
			//
			// register macros
			//
			return RegisterMacroContainers( mp, asm, displayFound );
		}


		///////////////////////////////////////////////////////////////////////////
		
		public static bool RegisterMacroContainers( IMacroProcessor mp, Assembly asm, bool displayFound )
		{
		int	countFound = 0;
		
			// ******
			//
			// only public types
			// 
			Type [] types = asm.GetExportedTypes();

			string interfaceName = typeof( IMacroContainer ).Name;

			foreach( Type type in types ) {
				if( null != type.GetInterface(interfaceName) ) {
					object typeInstance = null;

					// ******
					try {																																 
						//
						// first try creating with a ctor that takes an IMacroProcessor 
						// parameter
						//
						typeInstance = Activator.CreateInstance( type, new object [] { mp } );
					}
					catch {
					}

					if( null == typeInstance ) {
						try {																																 
							//
							// default constructor
							//
							typeInstance = Activator.CreateInstance( type );
						}
						catch {
						}
					}

					if( null == typeInstance ) {
						throw ExceptionHelpers.CreateException( "unable to create instance of \"{0}\" while trying to load object macros from library: \"{1}\"", type.FullName, asm.CodeBase );
					}

					// ******
					IMacroContainer container = typeInstance as IMacroContainer;
					if( null == container ) {
						throw ExceptionHelpers.CreateException( "unable to cast instance of \"{0}\" to IMacroCreate from library: \"{1}\"", type.FullName, asm.CodeBase );
					}

					// ******
					container.Initialize( mp );

					// ******
					var macroNames = container.ObjectMacroNames;
					if( null == macroNames || 0 == macroNames.Length ) {
						if( displayFound ) {
							ThreadContext.MacroWarning( "{0} did not define an object macro name (IMacroContainer.ObjectMacroNames)", type.FullName );
						}
					}
					else {
						foreach( var macroName in macroNames ) {
							
							if( displayFound ) {
								ThreadContext.WriteMessage( "adding object macro {0} with instance of \"{0}\"", macroName, type.FullName );
							}
							
							mp.AddObjectMacro( macroName, container );
						}
					}

					// ******
					++countFound;
				}
			}
			
			//// ******
			//if( displayFound ) {
			//	ThreadContext.WriteMessage( string.Empty );
			//}
			
			// ******
			return countFound > 0;
		}
	
	

//		/////////////////////////////////////////////////////////////////////////////
//		
//		public static void RegisterMacros( IMacroProcessor mp, string fullPath, bool displayFound )
//		{
//			// ******
//			Assembly asm = null;
//
//			// ******
//			try {
//				asm = Assembly.LoadFrom( fullPath );
//			}
//			catch ( Exception ex ) {
//				throw new Exception( string.Format("unable to load library \"{0}\": {1}", fullPath, ex.Message) );
//			}
//			
//			// ******
//			//
//			// register macros
//			//
//			RegisterMacroContainers( mp, asm, displayFound );
//
//			if( ! RegisterMacros(mp, asm, displayFound) ) {
//				throw new Exception( string.Format("unable to locate any macros in plugin library \"{0}\"", asm.FullName) );
//			}
//		}
//
//
//		///////////////////////////////////////////////////////////////////////////
//		
//		public static bool RegisterMacros( IMacroProcessor mp, Assembly asm, bool displayFound )
//		{
//		int	countRegistered = 0;
//		
//			// ******
//			//
//			// only public types
//			// 
//			Type [] types = asm.GetExportedTypes();
//			
//			foreach( Type type in types ) {
//				if( null != type.GetInterface("IMacroLoader") ) {
//					object typeInstance = null;
//
//					// ******
//					try {																																 
//						//
//						// first try creating with a ctor that takes an IMacroProcessor 
//						// parameter
//						//
//						typeInstance = Activator.CreateInstance( type, new object [] { mp } );
//					}
//					catch {
//					}
//
//					if( null == typeInstance ) {
//						try {																																 
//							//
//							// default constructor
//							//
//							typeInstance = Activator.CreateInstance( type );
//						}
//						catch {
//						}
//					}
//
//					if( null == typeInstance ) {
//						throw new Exception( string.Format("unable to create instance of \"{0}\" while trying to load macros from library: \"{1}\"", type.FullName, asm.CodeBase) );
//					}
//
//					// ******
//					IMacroLoader loader = typeInstance as IMacroLoader;
//					if( null == loader ) {
//						throw new Exception( string.Format("unable to cast instance of \"{0}\" to IMacroLoader while trying to load macros from library: \"{1}\"", type.FullName, asm.CodeBase) );
//					}
//
//					// ******
//					if( displayFound ) {
//						ThreadContext.WriteMessage( "calling LoadMacros() on \"{0}\"", type.FullName );
//					}
//
//					// ******
//					string objectMacroName = loader.ObjectMacroName;
//					if( ! string.IsNullOrEmpty(objectMacroName) ) {
//						mp.AddObjectMacro( objectMacroName, typeInstance );
//					}
//
//					// ******
//					countRegistered += loader.LoadMacros( mp, displayFound );
//				}
//			}
//			
//			// ******
//			if( displayFound ) {
//				ThreadContext.WriteMessage( string.Empty );
//			}
//			
//			// ******
//			return countRegistered > 0;
//		}
	
	
		/////////////////////////////////////////////////////////////////////////////
		//
		//public static int _RegisterMacros( IMacroProcessor mp, Type type, bool displayFound )
		//{
		//int	countRegistered = 0;
		//
		//	// ******
		//	object typeInstance = null;
		//
		//	// ******
		//	MethodInfo [] methods = type.GetMethods();
		//	foreach( MethodInfo info in methods ) {
		//		if( info.IsStatic ) {
		//			continue;
		//		}
		//		
		//		// ******
		//		if( typeof(object) == info.ReturnType ) {
		//			ParameterInfo [] pi = info.GetParameters();
		//			
		//			if( 3 == pi.Length ) {
		//				if( typeof(IMacroProcessor) == pi[0].ParameterType && typeof(IMacro) == pi[1].ParameterType && typeof(ArgumentList) == pi[2].ParameterType ) {
		//					if( displayFound ) {
		//						ThreadContext.WriteMessage( "adding macro: {0}", info.Name );
		//					}
		//					
		//					// ******
		//					if( null == typeInstance ) {
		//						try {
		//							typeInstance = Activator.CreateInstance( type );
		//						}
		//						catch ( Exception e ) {
		//							throw new Exception( string.Format("while attempting create instance of \"{0}\"", type.Name), e );
		//						}
		//						
		//					}
		//
		//					// ******

		////Delegate macroDelegate= Delegate.CreateDelegate( typeof(MacroInvoker_Old), typeInstance, info );
		////					if( null != macroDelegate ) {
		////						macros.NewBuiltinMacro( info.Name, (MacroInvoker_Old) macroDelegate, 0 );
		////						++countRegistered;
		////					}
							
		//					Delegate macroDelegate= Delegate.CreateDelegate( typeof(MacroCall), typeInstance, info );
		//					if( null != macroDelegate ) {
		//						
		//						// NewBuiltinMacro( string name, IMacroHandler mh, MacroCall method )
		//						
		//						//mp.NewBuiltinMacro( info.Name, (MacroCall) macroDelegate, 0 );
		//						
		//							//mp.NewBuiltinMacro( info.Name, , (MacroCall) macroDelegate );
		//
		//
		//
		//						++countRegistered;
		//					}
		//					
		//				}
		//			}
		//		}
		//	}
		//		
		//	// ******
		//	return countRegistered;
		//}
		//
	
		///////////////////////////////////////////////////////////////////////////

		public void Register( NMP nmp, IMacroProcessor mp )
		{
		}


		///////////////////////////////////////////////////////////////////////////

		public AutoRegisterMacros()
		{
		}


	}


}
