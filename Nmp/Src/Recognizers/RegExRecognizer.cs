#region License
// 
// Author: Joe McLain <nmp.developer@outlook.com>
// Copyright (c) 2013, Joe McLain and Digital Writing
// 
// Licensed under Eclipse Public License, Version 1.0 (EPL-1.0)
// See the file LICENSE.txt for details.
// 
#endregion
// RegExRecognizer.cs
//
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using NmpBase;


#pragma warning disable 414



namespace Nmp {


	/////////////////////////////////////////////////////////////////////////////

	partial class RegExRecognizer : BaseRecognizer {

		/////////////////////////////////////////////////////////////////////////////
		
		public class RxInfo {

			public string	Name = string.Empty;
			public Regex	RegEx = null;
			public string	MacroToCall = string.Empty;

		}

	}

	
	/////////////////////////////////////////////////////////////////////////////

	partial class RegExRecognizer : BaseRecognizer {

		/*

			removed multi char conversions from input, still have others that
			regex must avoid - including comments - which remain in the input
			string

		*/


		// ******
		List<RxInfo>	regexes = new List<RxInfo>();

		bool active = false;

		bool checkBase = true;
		bool checkWhiteSpace = false;


		/////////////////////////////////////////////////////////////////////////////

		public bool CheckBase
		{
			get {
				return checkBase;
			}

			set {
				checkBase = value;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public bool CheckWhiteSpace
		{
			get {
				return checkWhiteSpace;
			}

			set {
				checkWhiteSpace = value;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		private void SetActive()
		{
			active = regexes.Count > 0;
		}


		/////////////////////////////////////////////////////////////////////////////

		public override bool StartsWithAltTokenChars( string str )
		{
			// ******
			if( ! active || checkBase ) {
				//
				// no regexes or we call base if we don't find one
				//
				return base.StartsWithAltTokenChars( str );
			}

			//
			// we don't support alt token format
			//

			return false;
		}


		/////////////////////////////////////////////////////////////////////////////

		public override bool IsMacroIdentifierStartChar( char ch )
		{
			// ******
			if( ! active ) {
				return base.IsMacroIdentifierStartChar( ch );
			}

			// ******
			//
			// if we check white space then we check every char
			//
			return checkWhiteSpace ? true : ! char.IsWhiteSpace( ch );
		}


		/////////////////////////////////////////////////////////////////////////////

		public override bool IsValidMacroIdentifier( string name, bool allowDots = false )
		{
			// ******
			if( ! active ) {
				return base.IsValidMacroIdentifier( name, allowDots );
			}

			// ******
			foreach( RxInfo ri in regexes ) {
				if( ri.RegEx.IsMatch(name) ) {
					return true;
				}
			}

			// ******
			if( checkBase ) {
				return base.IsValidMacroIdentifier( name, allowDots );
			}

			// ******
			return false;
		}


		/////////////////////////////////////////////////////////////////////////////
		
		private string GetToken( RxInfo ri, Match match )
		{
			// ******
			if( ! string.IsNullOrEmpty(ri.MacroToCall) ) {
				//
				// the regex that matched has a specific macro that
				// needs to be called when the match happens
				//
				return ri.MacroToCall;
			}

			// ******
			//
			// look for a capture named "macro"
			//
			GroupCollection gc = match.Groups;

			Group group = gc[ "macro" ];
			if( group.Success ) {
				return group.Value;
			}

			// ******
			//
			// use the entire capture as the name
			//
			return gc[ 0 ].Value;
		}
		

		/////////////////////////////////////////////////////////////////////////////

		private TokenMap CreateTokenMap( IReader reader, RxInfo ri, Match match )
		{
			// ******
			var tm = new TokenMap( true );

			// ******
			tm.Token = GetToken( ri, match );
			tm.IsAltTokenFormat = false;

			// ******
			tm.MatchLength = match.Length;

			// ******
			foreach( Group group in match.Groups ) {
				tm.RegExCaptures.Add( group.Value );
			}

			// ******
			return tm;
		}


		/////////////////////////////////////////////////////////////////////////////

		public override RecognizedCharType Next( IInput input, out TokenMap tm )
		{
			// ******
			if( ! active ) {
				return base.Next( input, out tm );
			}

			// ******
			char ch = input.Peek();

			if( IsMacroIdentifierStartChar(ch) ) {
				IReader reader = input.Current;
				string buffer = reader.Buffer;
				int index = reader.Index;

				// ******
				foreach( RxInfo ri in regexes ) {
					Match match = ri.RegEx.Match( buffer, index );
					
					if( match.Success ) {
						tm = CreateTokenMap( reader, ri, match );
						return RecognizedCharType.TokenStart;
					}
				}
			}

			// ******
			if( checkBase ) {
				//
				// we did not find an identifer, maybe base can
				//
				return base.Next( input, out tm );
			}
				
			// ******
			tm = null;
			return base.OtherCharTypes( ch );
		}






		/////////////////////////////////////////////////////////////////////////////
		//
		//
		//
		/////////////////////////////////////////////////////////////////////////////

		public void AddRegEx( string regExName, string regExStr, string macroToCall )
		{
			// ******
			//ThreadContext.WriteMessage( "AddRegEx()" );

			// ******
			if( string.IsNullOrEmpty(regExStr) ) {
				//
				// remove named regex
				//
				if( string.IsNullOrEmpty(regExName) ) {
					ThreadContext.MacroError( "atempt to remove RegExRecognizer regular expression that does not have a name" );
				}

				var item = regexes.Find( i => regExName == i.Name );
				if( null != item ) {
					regexes.Remove( item );
				}
			}
			else {
				Regex rx = null;

				try {
					rx = new Regex( regExStr, RegexOptions.Compiled );
				}
				catch ( Exception ex ) {
					ThreadContext.MacroError( "while creating a Regex instance of RegExRecognizer: {0}", ex.Message );
				}

				// ******
				regexes.Add( new RxInfo { Name = regExName, RegEx = rx, MacroToCall = macroToCall } );
			}

			// ******
			SetActive();
		}


		/////////////////////////////////////////////////////////////////////////////

		public RegExRecognizer( GrandCentral gc )
			: base(gc)
		{
			//ThreadContext.WriteMessage( "new RegExRecognizer()" );
		}
	}



}
