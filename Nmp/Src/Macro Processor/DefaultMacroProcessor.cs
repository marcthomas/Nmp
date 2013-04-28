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
using System.Text.RegularExpressions;

using Microsoft.Win32;

using NmpBase;
using NmpExpressions;
using Nmp.Expressions;
using Nmp.Builtin.Macros;
using Nmp.Output;
//using Nmp.Powershell;


namespace Nmp {


	/////////////////////////////////////////////////////////////////////////////

	partial class DefaultMacroProcessor : IMacroProcessor, IHub {

		protected Hub						hub;
		protected IMacroTracer	tracer;
		protected int						tracerId;

		private IPowerShellInterface	powershell;

		protected NmpMacroList	macroList = new NmpMacroList();
		protected CoreMacros		builtins;

		protected GrandCentral	gc;

		//
		// we use this as the macro handler when someone just wants
		// to register a MacroCall style macro - a free standing
		// macro
		//
		protected BuiltinMacroHandler		registeredMethodMacroHandler;
		protected NetObjectMacroHandler	registeredObjectMacroHandler;
		protected TextMacroHandler			registeredTextMacroHandler;

		/////////////////////////////////////////////////////////////////////////////

		private int	macroDataNameIndex = 1001;
	
	
		/////////////////////////////////////////////////////////////////////////////
	
		public bool	DumpExpressionOnly	= false;


		//////////////////////////////////////////////////////////////////////////////

		public object OutputInstance
		{
			get;
			set;
		}


		//////////////////////////////////////////////////////////////////////////////

		public IPowerShellInterface	Powershell
		{
			get {
				return powershell;
			}
		}


		//////////////////////////////////////////////////////////////////////////////

		public IMacroArguments CurrentMacroArgs
		{
			get;
			private set;
		}


		/////////////////////////////////////////////////////////////////////////////

		public IGrandCentral GrandCentral
		{
			get
			{
				return gc;
			}
		}




		//////////////////////////////////////////////////////////////////////////////

		public NmpMacroList _MacroList
		{
			get {
				return macroList;
			}
		}


		//////////////////////////////////////////////////////////////////////////////
		
		public CoreMacros _Builtins
		{
			get {
				return builtins;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public T Get<T>() where T : class
		{
			return hub.Get<T>();
		}

		
		/////////////////////////////////////////////////////////////////////////////

		public string DefineMacro( IMacroArguments macroArgs, string macroName, object macroObject, List<string> argNames, bool isPushMacro )
		{
			return null;
		}


		//////////////////////////////////////////////////////////////////////////////

		protected object DumpExpression( IMacro macro, Expression exp )
		{
			ExpressionDumper dumper = new ExpressionDumper( this );
			return dumper.Evaluate( macro, exp );
		}


		/////////////////////////////////////////////////////////////////////////////

		private object PostProcessMacroResult( string invokedMacroName, IMacroOptions options, object objResult )
		{
			// ******
			if( null == objResult ) {
				return string.Empty;
			}

			// ******
			object optionsObj = objResult;

			//for( int i = 0; i < options.AdditionalOptions.Count; i++ ) {
			//	string optionName = options.AdditionalOptions[ i ];
			
			foreach( string optionName in options.AdditionalOptions ) {

				if( "/break" == optionName.ToLower() ) {
					return optionsObj;
				}

				// ******
				IMacro macro;
				if( FindMacro(optionName, false, out macro) ) {
					
					if( MacroType.Object == macro.MacroType ) {
						object macroObj = macro.MacroObject;

						var list = macroObj as NmpStringList;
						if( null != list ) {
							list.Add( optionsObj.ToString() );
							return string.Empty;
						}

						var array = macroObj as NmpArray;
						if( null != array ) {
							array.Add( invokedMacroName, optionsObj );
							return string.Empty;
						}
					}

					optionsObj = InvokeMacro( macro, new object [] { optionsObj }, false );
				}
				else {
					ThreadContext.MacroWarning( "macro instruction \"{0}\" not found", optionName );
				}
			}


			// ******
			string value = optionsObj.ToString();	//objResult.ToString();

			// ******
			if( options.Trim ) {
				value = value.Trim();
			}

			// ******
			if( options.CompressAllWhiteSpace ) {
				value = Helpers.CompressAllWhiteSpace( value );
			}

			// ******
			if( options.ILCompressWhiteSpace ) {
				value = Helpers.IntraLineCompressWhiteSpace( value );
			}

			// ******
			if( options.NLStrip ) {
				value = Helpers.StripNewlines( value );
			}

			// ******
			if( options.Quote ) {
				value = gc.QuoteWrapString( value );
			}

			// ******
			//
			// must be last because the result of wrapping a block
			// is a short string that references the block (which is
			// taken out of line)
			//
			if( options.TextBlockWrap ) {
				NamedTextBlocks blocks = gc.GetTextBlocks();
				value = blocks.AddTextBlock( value );
			}

			if( options.Empty ) {
				value = string.Empty;
			}

			// ******
			return value;
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// This overload of ProcessMacro() is called when processing expressions or
		/// other code that needs to run a macro, since it is not being executed from
		/// the scanner level (i.e. freshly parsed text) it does not have a macro invocation
		/// record
		/// </summary>
		/// <param name="macro"></param>
		/// <param name="exp"></param>
		/// <returns>object result of processing the macro</returns>

		public object _ProcessMacro( IMacro macro, IMacroArguments macroArgs, bool postProcess )
		{
			// ******
			int	id = tracerId++;
			if( null != tracer ) {
				tracer.ProcessMacroBegin( id, macro, macroArgs, postProcess );
			}

			// ******
			//
			// macro.IsBuiltinMacro
			// macro.IsTextMacro
			// macro.IsObjectMacro
			//

			//
			// must save and restore CurrentMacroArgs
			//
			var argsSave = CurrentMacroArgs;
			CurrentMacroArgs = macroArgs;
				object macroResult = macro.MacroHandler.Evaluate( macro, macroArgs );
			CurrentMacroArgs = argsSave;

			// ******
			object finalResult = null;

			if( postProcess || macroArgs.Options.CallerInstructions ) {
				//
				// when postProcess is true OR the macro was invoked with
				// instructions "@[...]"
				//
				// this is important if this macro was invoked as an argument 
				// to another macro - the object WILL be converted to a string
				// so care must be taken NOT to use macro instructions when
				// you want to preserve the resut of the call as an object !
				//
				finalResult = PostProcessMacroResult( macro.Name, macroArgs.Options, macroResult );
			}
			else {
				finalResult = macroResult;
			}

			// ******
			if( null != tracer ) {
				tracer.ProcessMacroDone( id, macro, postProcess, macroArgs.Options.Divert, macroResult, finalResult );
			}

//			// ******
//#if !NET35
//			if( macroArgs.Options.Razor || macroArgs.Options.RazorObject ) {
//				var razor = new RazorRunner( this );
//				finalResult = razor.Run( finalResult.ToString() );
//
//				if( macroArgs.Options.RazorObject ) {
//					razor.MacroFromLastInstance( macro.Name );
//					return string.Empty;
//				}
//
//			}
//#endif

			// ******
			if( macroArgs.Options.Divert ) {
				MasterOutput output = OutputInstance as MasterOutput;
				output.AddToDivert( macro.Name, finalResult.ToString() );
				return string.Empty;
			}
			else {
				return finalResult;
			}
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// This overload is called when we have a freshly parsed macro and expression
		/// found at the scanner level, it has a macro invocation record and we return
		/// a string
		/// </summary>
		/// <param name="mir"></param>
		/// <returns>object converted to a string</returns>

		public virtual string ProcessMacro( IMacroInvocationRecord mir )
		{

			// ******
			MacroProcessingState prevMirState = mir.State;
			mir.State = MacroProcessingState.Executing;

			// ******
			try {
				//
				// return a string that represents the expression
				//
				if( DumpExpressionOnly ) {
					return DumpExpression( mir.Macro, mir.MacroArgs.Expression ).ToString();
				}

				// ******
				return _ProcessMacro( mir.Macro, mir.MacroArgs, true ).ToString();
			}

			finally {
				mir.State = prevMirState;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public string FixText( string [] textArray )
		{
			return gc.FixText( textArray );
		}


		/////////////////////////////////////////////////////////////////////////////

		public string FixText( string text )
		{
			return FixText( new string [] {text} );
		}


		/////////////////////////////////////////////////////////////////////////////

		public object InvokeMacro( IInput input, MIR mir, MacroExpression expression, bool postProcess )
		{
			// ******
			using( Get<InvocationContext>().Init( mir ) ) {
				var macroArgs = new MacroArguments(	mir.Macro, input, expression );
				return _ProcessMacro( mir.Macro, macroArgs, postProcess );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public object InvokeMacro( IMacro macro, MacroExpression expression, bool postProcess )
		{
			// ******
			IInput input = gc.GetParseReader();	//ScanHelper.NewEmptyIInput;
			return InvokeMacro( input, new MIR( macro, input, "InvokeMacro() " + macro.Name ), expression, postProcess );
		}


		/////////////////////////////////////////////////////////////////////////////

		public object InvokeMacro( IMacro macro, string methodName, object [] args, bool postProcess )
		{
			// ******
			if( null == macro ) {
				throw new ArgumentNullException( "macro" );
			}

			// ******
			if( string.IsNullOrEmpty( methodName ) ) {
				throw new ArgumentNullException( "methodName" );
			}

			// ******
			if( null == args ) {
				args = new object [0];
			}

			// ******
			MacroExpression expression = ETB.CreateMacroCallExpression( methodName, args );
			if( null == expression ) {
				ThreadContext.MacroError( "InvokeMacro(): could not create MacroExpression for macro {0}, method {1}", macro.Name, methodName );
			}

			// ******
			return InvokeMacro( macro, expression, postProcess );
		}


		/////////////////////////////////////////////////////////////////////////////

		public object InvokeMacro( IMacro macro, object [] args, bool postProcess )
		{
			// ******
			if( null == macro ) {
				throw new ArgumentNullException( "macro" );
			}

			// ******
			if( null == args ) {
				args = new object [ 0 ];
			}

			// ******
			MacroExpression expression = ETB.CreateMacroCallExpression( macro, args );
			if( null == expression ) {
				ThreadContext.MacroError( "InvokeMacro(): the macro \"{0}\" is not a built-in macro, text macro, method reference or delegate", macro.Name );
			}

			// ******
			return InvokeMacro( macro, expression, postProcess );
		}


		/////////////////////////////////////////////////////////////////////////////

		public object InvokeMacro( string macroName, object [] args, bool postProcess )
		{
			// ******
			IMacro macro;
			if( FindMacro(macroName, false, out macro) ) {
				return InvokeMacro( macro, args, postProcess );
			}

			// ******
			ThreadContext.MacroError( "InvokeMacro(): \"{0}\" is not a macro, are you missing a macro file?", macroName );
			return null;
		}


		/////////////////////////////////////////////////////////////////////////////

		public List<IMacro> RemoveMacros( NmpStringList macrosList )
		{
			// ******
			var list = new List<IMacro>();

			// ******
			foreach( string name in macrosList ) {
				IMacro macro;

				if( macroList.TryGetValue(name, out macro) ) {
					list.Add( macro );
					macroList.Remove( name );
				}
			}

			// ******
			return list;
		}


		/////////////////////////////////////////////////////////////////////////////

		public void UndefMacro( string macroName )
		{
			// ******
			IMacro existingMacro;
			if( FindMacro(macroName, out existingMacro) ) {
				//
				// get Pushed, remove existingMacro
				//
				IMacro pushedMacro = existingMacro.Pushed;
				DeleteMacro( existingMacro );

				if( null != pushedMacro ) {
					//
					// add the Pushed macro
					//
					AddMacro( pushedMacro );
				}
			}
			else {
				ThreadContext.MacroWarning( "attempt to undef a macro that does not exist in the macro dictionary: {0}", macroName );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public void UndefMacros( NmpStringList macrosList )
		{
			// ******
			foreach( var macroName in macrosList ) {
				UndefMacro( macroName );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public void AddMacros( List<IMacro> macros )
		{
			foreach( IMacro macro in macros ) {
				macroList.Add( macro.Name, macro );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public NmpStringList GetMacroNames( bool userOnly )
		{
			// ******
			var list = new NmpStringList();

			// ******
			foreach( KeyValuePair<string, IMacro> kvp in macroList ) {
				string macroName = kvp.Key;
				IMacro macro = kvp.Value;

				if( ! userOnly || MacroType.Text == macro.MacroType || MacroType.Object == macro.MacroType ) {
					list.Add( macroName );
				}
			}

			// ******
			return list;
		}


		/////////////////////////////////////////////////////////////////////////////

		public List<IMacro> GetMacros( bool userOnly )
		{
			// ******
			var list = new List<IMacro>();

			// ******
			foreach( KeyValuePair<string, IMacro> kvp in macroList ) {
				string macroName = kvp.Key;
				IMacro macro = kvp.Value;

				if( ! userOnly || MacroType.Text == macro.MacroType || MacroType.Object == macro.MacroType ) {
					list.Add( macro );
				}
			}

			// ******
			return list;
		}


//		/////////////////////////////////////////////////////////////////////////////
//		
//		public object UnWrapObject( object obj )
//		{
//			//
//			// foreach() and maybe other methods require this, specificaly foreach needs
//			// the actual object so that it can figure what it's dealing with
//			//
//			return null != psIntf && null != obj ? psIntf.UnWrapPSObject( obj ) : obj;
//		}
//		
		
		/////////////////////////////////////////////////////////////////////////////

		//public bool FindMacro( string name, bool altTokenStart, out IMacro macro )
		//{
		//	//
		//	// we don't use altTokenStart, it's only (currently) important to
		//	// IMacroProcessorBase instances that are parsing files for Visual Studio
		//	//
		//
		//	// ******
		//	macro = null;
		//	if( string.IsNullOrEmpty(name) ) {
		//		return false;
		//	}
		//
		//	// ******
		//	//if( null == mTemp && null != psIntf && (! string.IsNullOrEmpty(name)) && '$' == name[0] ) {
		//	//	name = name.Substring( 1 );
		//	//	Macro lastPSMacro;
		//	//	
		//	//	//
		//	//	// not found and were running with Powershell, see if there is a PS variable by
		//	//	// this name
		//	//	//
		//	//
		//	//	//
		//	//	// can NOT cache by name because the PS variable might be having its value or reference
		//	//	// change between our fetching of it - like in a foreach loop which is how we 
		//	//	// caught this issue
		//	//	//
		//	//	object psVar = psIntf.GetVariable( name );
		//	//	if( null != psVar ) {
		//	//		lastPSMacro = Macro.NewNetObjMacro( this, name, netObjectHelper, psVar );
		//	//	}
		//	//	else {
		//	//		lastPSMacro = null;
		//	//	}
		//	//
		//	//	// ******
		//	//	if( null  != lastPSMacro ) {
		//	//		mOut = lastPSMacro;
		//	//		return true;
		//	//	}
		//	//}
		//
		//	/*
		//
		//		$				refs nmp object
		//
		//		$name		refs a macro that starts with a '$' OR a powershell variable, script or
		//						function
		//
		//		name		is a text or object macro where the user has choosen NOT to a '$' as
		//						the first char
		//
		//						might do this to replace text in a file that is not (strictly) a macro
		//						file written by user
		//	*/
		//
		//
		//	try {
		//		if( '$' == name[0] ) {
		//			if( null == powershell || 1 == name.Length ) {
		//				return false;
		//			}
		//			
		//			// ******
		//			object psVar;
		//			bool psVarFound = powershell.GetVariable( name.Substring(1), out psVar );
		//
	////
	//// ?? replace $ with special char to remove any possible change of
	//// name collision if we ever allow $ in names, or one somehow slips
	//// in ??
	////
		//
		//			if( macroList.TryGetValue(name, out macro) ) {
		//				if( ! psVarFound ) {
		//					//
		//					// no longer valid powershell variable, remove macro
		//					//
		//					DeleteMacro( macro );
		//					return false;
		//				}
		//
		//				//
		//				// update macro
		//				//
		//				macro.MacroObject = psVar;
		//			}
		//			else {
		//				//
		//				// macro not found
		//				//
		//				if( ! psVarFound ) {
		//					return false;
		//				}
		//
		//				//
		//				// add macro
		//				//
		//				macro = AddObjectMacro( name, psVar );
		//			}
		//
		//			// ******
		//			return true;
		//		}
		//		else {
		//			//
		//			// NOT powershell macro/value
		//			//
		//			bool found = macroList.TryGetValue( name, out macro );
		//			//if( ! found ) {
		//			//	macro = null;
		//			//}
		//			return found;
		//		}
		//	}
		//
		//	finally { 
		//		if( null != tracer ) {
		//			tracer.FindMacroCall( name, macro );
		//		}
		//	}
		//}
		//

		public bool FindMethodOnObjectMacro( string name, out IMacro macro )
		{
			try {
				//
				// split at '.', if two (?? or more) string look up the first one in
				// macro list; if found then if it's an object macro check that object
				// for having a public member whose name is the second string split from
				// 'name' - if found then true
				//
				string [] parts = name.Split( '.' );
				if( 2 == parts.Length ) {
					bool found = macroList.TryGetValue( parts [ 0 ], out macro );
					if( found && MacroType.Object == macro.MacroType ) {

						var method = macro.MacroObject.GetType()
												.GetMethods()
												.FirstOrDefault( m => m.Name == parts [ 1 ] );
						if( null != method ) {
							return true;
						}
					}
				}
			}
			catch {
			}

			// ******
			macro = null;
			return false;
		}


		public bool FindMacro( string name, bool altTokenStart, out IMacro macro )
		{
			//
			// we don't use altTokenStart, it's only (currently) important to
			// IMacroProcessorBase instances that are parsing files for Visual Studio
			//

			// ******
			macro = null;
			if( string.IsNullOrEmpty(name) ) {
				return false;
			}

			try {
				
				/*

						"#"			is the 'global' object for a macro script and is registered
						"#nmp"	is also the global object and is registered

							in either of the above cases access macros by "dotting" in to them

								#.define( ... ) is the same as #nmp.define( ... )


				*/

				if( '$' != name[0] ) {
					bool found = macroList.TryGetValue( name, out macro );
					if( found ) {
						return true;
					}

					// ******
					return FindMethodOnObjectMacro( name, out macro );
				}

				// ******
				//
				// starts with '$'
				//
				if( 1 == name.Length || null == powershell ) {
					//
					// at least one char must follow '$,' and powershell must be initialized
					//
					return false;
				}



			// ******
			//if( null == mTemp && null != psIntf && (! string.IsNullOrEmpty(name)) && '$' == name[0] ) {
			//	name = name.Substring( 1 );
			//	Macro lastPSMacro;
			//	
			//	//
			//	// not found and were running with Powershell, see if there is a PS variable by
			//	// this name
			//	//
			//
			//	//
			//	// can NOT cache by name because the PS variable might be having its value or reference
			//	// change between our fetching of it - like in a foreach loop which is how we 
			//	// caught this issue
			//	//
			//	object psVar = psIntf.GetVariable( name );
			//	if( null != psVar ) {
			//		lastPSMacro = Macro.NewNetObjMacro( this, name, netObjectHelper, psVar );
			//	}
			//	else {
			//		lastPSMacro = null;
			//	}
			//
			//	// ******
			//	if( null  != lastPSMacro ) {
			//		mOut = lastPSMacro;
			//		return true;
			//	}
			//}

				var psVarName = name.Substring(1 );

				object psValue;
				if( ! powershell.GetVariable(psVarName, out psValue) ) {
					//
					// no powershell variable by this name
					//
					return false;
				}

				//
				// by passing back a new instance of Macro that references the powershell
				// variable we are (in essense) creating a temporary - one each time the
				// variable is requested - HOPEFULLY they will be properly dereferenced and
				// go away
//
// TODO need to test this to make sure WE aren't holding a reference somewhere
//
				//
				macro = CreateObjectMacro( name, psValue );
				return true;
			}

			finally { 
				if( null != tracer ) {
					tracer.FindMacroCall( name, macro );
				}
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public virtual bool FindMacro( string name, out IMacro macro )
		{
			// ******
			var recognizer = Get<IRecognizer>();
			bool altTokenStart = recognizer.StartsWithAltTokenChars( name );
			if( altTokenStart ) {
				name = name.Substring( recognizer.AltTokenStart.CountChars );
			}

			// ******
			return FindMacro( name, altTokenStart, out macro );
		}


		/////////////////////////////////////////////////////////////////////////////

		public virtual bool IsMacroName( string name )
		{
			IMacro macro;
			return FindMacro( name, out macro );
		}

			
		/////////////////////////////////////////////////////////////////////////////

		public virtual bool IsStrictMacroName( string name )
		{
			
			return Get<IRecognizer>().IsValidMacroIdentifier(name, false );
		}

			
		/////////////////////////////////////////////////////////////////////////////

		public bool DeleteMacro( string name )
		{
			return macroList.Remove( name );
		}


		/////////////////////////////////////////////////////////////////////////////

		public bool DeleteMacro( IMacro macro )
		{
			return DeleteMacro( macro.Name );
		}


		///////////////////////////////////////////////////////////////////////////////
		//
		//public void UndefMacro( string macroName )
		//{
		//	InvokeMacro( "#undef", new object [] {macroName}, false );
		//}
		//

		/////////////////////////////////////////////////////////////////////////////

		public string GenerateMacroName( string appendText )
		{
			return string.Format( "__macro{0}{1}", macroDataNameIndex++, appendText );
		}


		/////////////////////////////////////////////////////////////////////////////
		
		public string GenerateArgListName( string appendText )
		{
			return string.Format( "__macroArgs{0}{1}", macroDataNameIndex++, appendText );
		}
		

		/////////////////////////////////////////////////////////////////////////////
		
		public string GenerateLocalName( string appendText )
		{
			return string.Format( "__local{0}{1}", macroDataNameIndex++, appendText );
		}
		

		/////////////////////////////////////////////////////////////////////////////
		
		public string GenerateListName( string appendText )
		{
			return string.Format( "__list{0}{1}", macroDataNameIndex++, appendText );
		}
		

		/////////////////////////////////////////////////////////////////////////////
		
		public string GenerateArrayName( string appendText )
		{
			return string.Format( "__array{0}{1}", macroDataNameIndex++, appendText );
		}


		/////////////////////////////////////////////////////////////////////////////

		public bool IsGeneratedName( string name )
		{
			// ******
			if( name.StartsWith( "__macro" ) ) {
				//
				// catches both "__macro" and "__macroArgs"
				//
				return true;
			}
			else if( name.StartsWith( "__local" ) ) {
				return true;
			}
			else if( name.StartsWith( "__list" ) ) {
				return true;
			}
			else if( name.StartsWith( "__array" ) ) {
				return true;
			}

			// ******
			return false;
		}


		/////////////////////////////////////////////////////////////////////////////

		public IMacro CreateObjectMacro( string macroName, object netObj )
		{
			return Macro.NewObjectMacro( this, macroName, registeredObjectMacroHandler, netObj );
		}


		/////////////////////////////////////////////////////////////////////////////

		public IMacro CreateBlockMacro( string macroName, IMacroHandler mh )
		{
			return Macro.NewBlockMacro( this, macroName, mh );
		}


		/////////////////////////////////////////////////////////////////////////////

		public IMacro CreateBuiltinMacro( string macroName, IMacroHandler mh )
		{
			return Macro.NewBuiltinMacro( this, macroName, mh );
		}


		/////////////////////////////////////////////////////////////////////////////

		public IMacro CreateBuiltinMacro( string macroName, IMacroHandler mh, object handlerData )
		{
			return Macro.NewBuiltinMacro( this, macroName, mh, handlerData );
		}


		/////////////////////////////////////////////////////////////////////////////
		
		public IMacro CreateBuiltinMacro( string macroName, MacroCall macroProc )
		{
			return Macro.NewBuiltinMacro( this, macroName, registeredMethodMacroHandler, macroProc );
		}
		

		/////////////////////////////////////////////////////////////////////////////

		public IMacro CreateTextMacro( string macroName, string macroText, IList<string> argNames )
		{
			// ******
			if( null == argNames ) {
				argNames = new NmpStringList();
			}

			// ******
			return Macro.NewTextMacro( this, macroName, registeredTextMacroHandler, macroText, argNames );
		}


		/////////////////////////////////////////////////////////////////////////////

		public virtual IMacro AddMacro( IMacro macro )
		{
			macroList.Remove( macro.Name );
			macroList.Add( macro.Name, macro );
			return macro;
		}


		/////////////////////////////////////////////////////////////////////////////

		public IMacro AddObjectMacro( string macroName, object netObj )
		{
			return AddMacro( Macro.NewObjectMacro( this, macroName, registeredObjectMacroHandler, netObj ) );
		}


		/////////////////////////////////////////////////////////////////////////////

		public IMacro AddBlockMacro( string macroName, IMacroHandler mh )
		{
			//
			// use this to add macros that function as an instance of some class
			// that implements IMacroHandler - unlike the method below you would
			// not use this to implement multiple macros in the same class since
			// the IMacroHandler.Evaluate() method is called
			//
			return AddMacro( Macro.NewBlockMacro( this, macroName, mh) );
		}


		/////////////////////////////////////////////////////////////////////////////

		public IMacro AddBuiltinMacro( string macroName, IMacroHandler mh )
		{
			//
			// use this to add macros that function as an instance of some class
			// that implements IMacroHandler - unlike the method below you would
			// not use this to implement multiple macros in the same class since
			// the IMacroHandler.Evaluate() method is called
			//
			return AddMacro( Macro.NewBuiltinMacro( this, macroName, mh) );
		}


		/////////////////////////////////////////////////////////////////////////////

		public IMacro AddBuiltinMacro( string macroName, IMacroHandler mh, object handlerData )
		{
			//
			// use this to add macros that function as an instance of some class
			// that implements IMacroHandler - unlike the method below you would
			// not use this to implement multiple macros in the same class since
			// the IMacroHandler.Evaluate() method is called
			//
			return AddMacro( Macro.NewBuiltinMacro( this, macroName, mh, handlerData) );
		}


		/////////////////////////////////////////////////////////////////////////////
		
		public IMacro AddBuiltinMacro( string macroName, MacroCall macroProc )
		{
			//
			// creates a new instance of BuiltinMacroHandler that holds a delegate that
			// points to the method that handles the macro - the method can belong to any
			// class as long as it matches MacroProc's signature - useful where you want to
			// have multiple macro method handles in the same class
			//
			return AddMacro( Macro.NewBuiltinMacro(this, macroName, registeredMethodMacroHandler, macroProc) );
		}
		

		/////////////////////////////////////////////////////////////////////////////

		public IMacro AddTextMacro( string macroName, string macroText, IList<string> argNames )
		{
			// ******
			if( null == argNames ) {
				argNames = new NmpStringList();
			}

			// ******
			return AddMacro( Macro.NewTextMacro( this, macroName, registeredTextMacroHandler, macroText, argNames ) );
		}


		///////////////////////////////////////////////////////////////////////////////

		public void RegisterMacros( string pathIn, bool displayFound )
		{
			// ******
			string path = gc.FindFile( pathIn );
			if( null == path ) {
				ThreadContext.MacroError( "RegisterMacros: could not locate file: \"{0}\"", pathIn );
			}

			// ******
			AutoRegisterMacros.RegisterMacroContainers( this, path, displayFound );
		}


		///////////////////////////////////////////////////////////////////////////////

		public IPowerShellInterface RegisterPowershell( IPowerShellInterface psIntf )
		{
			// ******
			//
			// first caller wins
			//
			if( null == powershell ) {
				powershell = psIntf;
				AddObjectMacro( "#ps", powershell );
			}

			return powershell;
		}


		/////////////////////////////////////////////////////////////////////////////

		//
		// note: must be called AFTER any objects that are created here that have a depedency
		// on having injected members have had those members (that are injected) added to DI
		// object pool
		//

		public void AddDefaultMacros( Hub hub, INmpHost host )
		{
			// ******
			AddTextMacro( "#host", host.HostName, null );

			// ******
			//RegisterPowershell( new PowershellHost(this) );

			/*

				$.LoadPowershell()

				load ps assembly

				call initialize with mp which should register powershell on mp

				this load should be on


			*/




			// ******
	builtins = new CoreMacros( this );
			//
			// under two different names
			//
			//AddObjectMacro( "$", builtins );
			//AddObjectMacro( "$nmp", builtins );
			
			//
			// and a couple more until we decide what we like
			//
			AddObjectMacro( "#", builtins );
			AddObjectMacro( "#nmp", builtins );
			AddObjectMacro( "#mp", this );
			
			// ******
	var objectMacros = new ObjectMacros( this );
			AddObjectMacro( "#object", objectMacros );

			// ******
	var ifMacros = new IfMacros( this );
			AddObjectMacro( "#if", ifMacros );

			// ******
	var isMacros = new IsMacros( this );
			AddObjectMacro( "#is", isMacros );


			// ******
			AddMacro( Defmacro.Create(this, builtins) );
			AddMacro( Pushdef.Create(this, builtins) );

			// ******
			AddMacro( IfMacroHandler.Create(this) );
			AddMacro( Textblock.Create(this) );

			// ******
			AddMacro( Defarray.Create(this) );
			AddMacro( Deflist.Create( this ) );

			// ******
			AddMacro( ExpandoBlock.Create( this ) );
			AddMacro( GenericBlock.Create( this ) );

			// ******
			AddMacro( ForeachBlockMacro.Create( this ) );
			AddMacro( ForloopMacros.Create( this ) );

			// ******
			AddObjectMacro( "#String", new StaticStandin(typeof(string)) );
			AddObjectMacro( "#Path", new StaticStandin(typeof(Path)) );
			AddObjectMacro( "#DateTime", new StaticStandin(typeof(DateTime)) );
			AddObjectMacro( "#Directory", new StaticStandin(typeof(Directory)) );
			AddObjectMacro( "#File", new StaticStandin(typeof(File)) );

			AddObjectMacro( "#Registry", new StaticStandin(typeof(Registry)) );
			AddObjectMacro( "#Environment", new StaticStandin(typeof(Environment)) );
			AddObjectMacro( "#NmpEnvironment", new StaticStandin(typeof(NmpEnvironment)) );


			// ******
			//
			// builtin object helpers
			//
			var typeHelpers = hub.Get<TypeHelperDictionary>();
			typeHelpers.Add( typeof( string ), StringObjectHelper.Create );
			typeHelpers.Add( typeof( string [] ), StringArrayObjectHelper.Create );


			// ******
			AddTestObjects();

			// ******
			//
			// legacy macros
			//
			AddObjectMacro( "#nmpRegion", ETB.CreateMethodInvoker( builtins, "nmpRegion" ) );
			AddObjectMacro( "#endNmpRegion", ETB.CreateMethodInvoker( builtins, "endNmpRegion" ) );
			
			AddObjectMacro( "#define", ETB.CreateMethodInvoker( builtins, "define" ) );
			AddObjectMacro( "#pop", ETB.CreateMethodInvoker(builtins, "pop") );
			AddObjectMacro( "#undef", ETB.CreateMethodInvoker(builtins, "undef") );
			AddObjectMacro( "#pushDivert", ETB.CreateMethodInvoker(builtins, "pushDivert") );
			AddObjectMacro( "#popDivert", ETB.CreateMethodInvoker(builtins, "popDivert") );
			AddObjectMacro( "#eval", ETB.CreateMethodInvoker(builtins, "eval") );

			AddObjectMacro( "#isDefined", ETB.CreateMethodInvoker(isMacros, "Defined") );
			AddObjectMacro( "#isEmpty", ETB.CreateMethodInvoker(isMacros, "Empty") );
			AddObjectMacro( "#isEqual", ETB.CreateMethodInvoker(isMacros, "Equal") );
			AddObjectMacro( "#isFalse", ETB.CreateMethodInvoker(isMacros, "False") );
			AddObjectMacro( "#isNotDefined", ETB.CreateMethodInvoker(isMacros, "NotDefined") );
			AddObjectMacro( "#isNotEmpty", ETB.CreateMethodInvoker(isMacros, "NotEmpty") );
			AddObjectMacro( "#isNotEqual", ETB.CreateMethodInvoker(isMacros, "NotEqual") );
			AddObjectMacro( "#isTrue", ETB.CreateMethodInvoker(isMacros, "True") );

			AddObjectMacro( "#ifDefined", ETB.CreateMethodInvoker(ifMacros, "Defined") );
			AddObjectMacro( "#ifelse", ETB.CreateMethodInvoker(ifMacros, "else") );
			AddObjectMacro( "#ifElse", ETB.CreateMethodInvoker(ifMacros, "Else") );
			AddObjectMacro( "#ifEmpty", ETB.CreateMethodInvoker(ifMacros, "Empty") );
			AddObjectMacro( "#iffalse", ETB.CreateMethodInvoker(ifMacros, "false") );
			AddObjectMacro( "#ifFalse", ETB.CreateMethodInvoker(ifMacros, "False") );
			AddObjectMacro( "#ifNotDefined", ETB.CreateMethodInvoker(ifMacros, "NotDefined") );
			AddObjectMacro( "#ifNotEmpty", ETB.CreateMethodInvoker(ifMacros, "NotEmpty") );
			AddObjectMacro( "#iftrue", ETB.CreateMethodInvoker(ifMacros, "true") );
			AddObjectMacro( "#ifTrue", ETB.CreateMethodInvoker(ifMacros, "True") );

		}


		/////////////////////////////////////////////////////////////////////////////

		//
		// note: injected
		//

		public DefaultMacroProcessor(	Hub hub, INmpHost host )
		{
			// ******
			this.hub = hub;

			// ******
			registeredMethodMacroHandler = new BuiltinMacroHandler( this );
			registeredObjectMacroHandler = new NetObjectMacroHandler( this );
			registeredTextMacroHandler = new TextMacroHandler( this );

			// ******
			this.tracer = host.Tracer;

			// ******
			//
			// bad design: we're called durring GrandCentral.hub being initialized - gc 
			// is not completely ready until 
			//
			gc = Get<GrandCentral>();

			// ******
			AddDefaultMacros( hub, host );
		}

	}


}

