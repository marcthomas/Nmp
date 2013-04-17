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
using System.Diagnostics;
using System.Linq;
using System.Text;
using NmpBase;

namespace Nmp {
	//
	// using 'parser' to gather tokens and text from 'input'
	//
	// using 'expressionEvaluator' to generate a tree to feed 'macroHandler'
	// or the DLR
	//

	/*
							
			we should be able to generate a complete tree for our expression

			that tree might then be modified/extended or whatever as we evaluate
			it's pieces

			actually, we evaluate as we build the tree - each time we run across
			another macro that needs to be evaluated

			macros only to begin with

				define
				pushdef

				blockdefine
				blockpush
								
				undef
				popdef

				isdefined
				isnotdevined

			
				faux processor should recognize above and we should output them
				surrounded by [[]]

				blocks should be processed and ???

	*/


	/////////////////////////////////////////////////////////////////////////////

	[DebuggerDisplay("Name: {Name}, Counter = {counter}")]
	class Macro : IMacro {

		public static int	_counter = 0;

		public int	counter;

		// ******
		public IMacro					Pushed						{ get; set; }

		public string					Name							{ get; private set; }
		public MacroType			MacroType					{ get; private set; }
		public MacroFlags			Flags							{ get; set; }

		public IMacroHandler	MacroHandler			{ get; set; }
		public object					MacroHandlerData	{ get; set; }

		public object					MacroObject				{ get; set; }
		public string					MacroText 				{ get; set; }

		public bool						IsBlockMacro 			{ get; set; }

		public IList<string>	ArgumentNames			{ get; private set; }

		public IMacroProcessor	MacroProcessor	{ get; set; }

		// ******
		public string					SourceFile					{ get; set; }
		public int						MacroStartLine			{ get; set; }

		// ******
		public object					PrivateData				{ get; set; }


		/////////////////////////////////////////////////////////////////////////////

		public override string ToString()
		{
			return Name;
		}


		/////////////////////////////////////////////////////////////////////////////

		public Macro( string name )
		{
			Pushed = null;
			Name = name;
			counter = ++_counter;
		}


		///////////////////////////////////////////////////////////////////////////////
		//
		//public Macro( string name, MacroType macroType, CreateHandlerInstance createHandler, object netObj, string text, NmpStringList argNames )
		//	: this( name )
		//{
		//	MacroType = macroType;
		//
		//	//MacroHandler
		//	CreateHandler = createHandler;
		//
		//	MacroObject = netObj;
		//	MacroText = string.IsNullOrEmpty(text) ? string.Empty : text;
		//	ArgumentNames = null == argNames ? new NmpStringList() : argNames;
		//}
		//

		/////////////////////////////////////////////////////////////////////////////

		public Macro(	string name, 
									MacroType macroType, 
									IMacroHandler mh, 
									object netObj, 
									string text, 
									IList<string> argNames,
									IMacroProcessor mp
								)
			: this( name )
		{
			// ******
			MacroType = macroType;
			MacroHandler = mh;

			// ******
			if( null != MacroHandler ) {
				IsBlockMacro = MacroHandler.HandlesBlocks;
			}

			// ******
			MacroObject = netObj;
			MacroText = string.IsNullOrEmpty(text) ? string.Empty : text;
			
			// ******
			ArgumentNames = null == argNames ? new NmpStringList() : argNames;

			for( int i = 0; i < ArgumentNames.Count; i++ ) {
				ArgumentNames[ i ] = ArgumentNames[ i ].Trim();
			}

			// ******
			MacroProcessor = mp;

			// ******
			//
			// only do this if MacroProcessor is initialized - if it's not
			// we may be generating a macro for tooling, not when an actual
			// instance of NMP is running
			//
			if( null != MacroProcessor ) {
				//if( ThreadContext.PushbackResult ) {
				if( mp.GrandCentral.PushbackResult ) {
					Flags = MacroFlags.Pushback;
				}

				if( MacroType.Text == macroType ) {

					if( mp.GrandCentral.ExpandAndScan ) {
						Flags |= MacroFlags.FwdExpand;
					}
				}
			}

			// ******
			PrivateData = null;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static Macro NewObjectMacro( IMacroProcessor mp, string name, IMacroHandler mh, object netObj )
		{
			return new Macro( name, MacroType.Object, mh, netObj, null, null, mp );
		}


		/////////////////////////////////////////////////////////////////////////////

		public static Macro NewBlockMacro( IMacroProcessor mp, string name, IMacroHandler mh )
		{
			return new Macro( name, MacroType.Builtin, mh, null, null, null, mp );
		}


		/////////////////////////////////////////////////////////////////////////////

		public static Macro NewBuiltinMacro( IMacroProcessor mp, string name, IMacroHandler mh )
		{
			return new Macro( name, MacroType.Builtin, mh, null, null, null, mp );
		}


		/////////////////////////////////////////////////////////////////////////////

		public static Macro NewBuiltinMacro( IMacroProcessor mp, string name, IMacroHandler mh, MacroCall method )
		{
			// ******
			Macro macro = new Macro( name, MacroType.Builtin, mh, null, null, null, mp );
			macro.MacroHandlerData = method;

			// ******
			return macro;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static Macro NewBuiltinMacro( IMacroProcessor mp, string name, IMacroHandler mh, object handlerData )
		{
			// ******
			Macro macro = new Macro( name, MacroType.Builtin, mh, null, null, null, mp );
			macro.MacroHandlerData = handlerData;

			// ******
			return macro;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static Macro NewTextMacro( IMacroProcessor mp, string name, IMacroHandler mh, string macroText, IList<string> argNames )
		{
			// ******
			Macro macro = new Macro( name, MacroType.Text, mh, null, macroText, argNames, mp );
			
			// ******
			return macro;
		}


	}
}
