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
using System.Linq;

using NmpBase;
using Nmp.Expressions;

namespace Nmp {


	/////////////////////////////////////////////////////////////////////////////

	class MacroOptions : IMacroOptions {

		// ******
		IGrandCentral gc;	


		// ******	
		public bool CallerInstructions	{ get; set; }

		public bool	Pushback	{ get; set; }
		public bool FwdExpand	{ get; set; }

		public bool Quote	{ get; set; }
		public bool Trim	{ get; set; }
		public bool NLStrip	{ get; set; }
		public bool CompressAllWhiteSpace	{ get; set; }
		public bool ILCompressWhiteSpace	{ get; set; }
		public bool TextBlockWrap	{ get; set; }
		public bool Divert	{ get; set; }
		public bool Eval	{ get; set; }

		public bool Data	{ get; set; }
		public bool Format	{ get; set; }

		public bool NoSubst	{ get; set; }

		public bool Razor	{ get; set; }
		public bool RazorObject	{ get; set; }

		public bool Empty { get; set; }

		public bool TabsToSpaces { get; set; }

		public bool HtmlEncode { get; set; }
		
		public bool Echo { get; set; }

		//
		// other options
		//
		public bool NoExpression	{ get; set; }

		public NmpStringList	AdditionalOptions	{ get; set; }


		/////////////////////////////////////////////////////////////////////////////

		protected bool FlagIsSet( int flags, int flag )
		{
			return flag == (flags & flag);
		}


		///////////////////////////////////////////////////////////////////////////////
		//
		//protected bool DeterminFlagState( bool currentState, bool set, bool clear )
		//{
		//	//
		//	// if set and clear are the same then they conflict and we return
		//	// current value otherwise 'set' which if true will if it is true
		//	// and false otherwise - clear will be true (take my word for it) and
		//	// false will be the correct value
		//	//
		//	return set == clear ? currentState : set;
		//}
		//

		/////////////////////////////////////////////////////////////////////////////

		protected void Initialize( IMacroProcessor mp )
		{
			// ******
			gc = mp.GrandCentral;

			// ******
			CallerInstructions = false;

			Pushback = gc.PushbackResult;
			FwdExpand = gc.ExpandAndScan;

			Quote = false;
			Trim = false;
			NLStrip = false;
			CompressAllWhiteSpace = false;
			ILCompressWhiteSpace = false;
			TextBlockWrap = false;
			Divert = false;
			Eval = false;

			Data = false;
			Format = false;

			NoSubst = false;

			Razor = false;
			RazorObject = false;

			Empty = false;

			TabsToSpaces = false;

			HtmlEncode = false;

			Echo = false;

			//
			// other options
			//
			NoExpression = false;		// for defining macro

			AdditionalOptions = new NmpStringList();
		}


		/////////////////////////////////////////////////////////////////////////////

		public MacroOptions( IMacroProcessor mp, MacroFlags macroFlags, NmpStringList options )
		{
			// ******
			Initialize( mp );
			
			//Pushback = mp.GrandCentral>.PushbackResult;

			if( FlagIsSet( (int) macroFlags, (int) MacroFlags.NoPushback ) ) {
				Pushback = false;
			}
			
			if( FlagIsSet( (int) macroFlags, (int) MacroFlags.Pushback ) ) {
				Pushback = true;
			}
			
			if( FlagIsSet( (int) macroFlags, (int) MacroFlags.NoFwdExpand ) ) {
				FwdExpand = false;
			}

			if( FlagIsSet( (int) macroFlags, (int) MacroFlags.FwdExpand ) ) {
				FwdExpand = true;
			}

			// ******
			if( null != options && options.Count > 0 ) {
				//
				// flags that @[] instructions were added
				//
				CallerInstructions = true;

				// ******
				foreach( string option in options ) {
					switch( option.ToLower() ) {
						case "format":
							Format = true;
							break;

						case "data":
							Data = true;
							break;

						case "expand":
							FwdExpand = true;
							break;

						case "noexp":
							FwdExpand = false;
							break;

						case "pushback":
							Pushback = true;
							break;

						case "nopb":
							Pushback = false;
							break;

						case "quote":
							Quote = true;
							break;

						case "noquote":
							Quote = false;
							break;

						case "trim":
							Trim = true;
							break;

						case "nlstrip":
							NLStrip = true;
							break;

						case "normalize":
						case "wscompress":
							CompressAllWhiteSpace = true;
							break;

						case "ilcompressws":
							ILCompressWhiteSpace = true;
							break;

						case "tbwrap":
							TextBlockWrap = true;
							break;

						case "divert":
							Divert = true;
							break;

						case "eval":
							Eval = true;
							break;

						case "noexpression":
							NoExpression = true;
							break;

						case "nosubst":
							NoSubst = true;
							break;

						case "razorobject":
							RazorObject = true;
							break;

						case "razor":
							Razor = true;
							break;

						case "empty":
							Empty = true;
							break;

						case "htmlencode":
							HtmlEncode = true;
							break;

						case "echo":
							Echo = true;
							break;

						default:
							AdditionalOptions.Add( option );
							break;
					}
				}
			}
		}
	}


	/////////////////////////////////////////////////////////////////////////////

	class MacroArguments : IMacroArguments {

		public IInput					Input				{ get; private set; }
		public Expression			Expression	{ get; private set; }
		public IMacroOptions	Options			{ get; set; }
		public string					BlockText		{ get; set; }

		public NmpStringList	SpecialArgs	{ get; private set; }


		/////////////////////////////////////////////////////////////////////////////

		public MacroArguments(	IMacro macro,
														IInput input, 
														MacroExpression macroExp, 
														
														NmpStringList specialArgs = null,
														string blockText = ""
													)
		{
			// ******
			if( null == input ) {
				throw new ArgumentNullException( "input" );
			}
			Input = input;

			// ******
			Expression = macroExp;

			// ******
			Options = new MacroOptions( macro.MacroProcessor, macro.Flags, macroExp.MacroInstructions );
			
			// ******
			SpecialArgs = null == specialArgs ? new NmpStringList() : specialArgs;
			BlockText = blockText;
		}


	}
}
