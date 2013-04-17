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
using System.Linq;
using System.Text;

using System.Runtime.Remoting.Messaging;

using System.Web;

using Nmp;
using NmpBase;
using NmpBase.Reflection;
using NmpExpressions;

namespace Nmp {


	/////////////////////////////////////////////////////////////////////////////

	partial class GrandCentral : IGrandCentral {	//IHub {

		// ******
		Hub	hub;

		// ******
		public CharSequence	SeqOpenQuote { get; set; }
		public CharSequence SeqCloseQuote { get; set; }


		/////////////////////////////////////////////////////////////////////////////

		public ExecutionSecurity		Security				{ get; private set; }



		/////////////////////////////////////////////////////////////////////////////

		public T Get<T>() where T : class
		{
			return hub.Get<T>();
		}


		/////////////////////////////////////////////////////////////////////////////

		public IRecognizer Recognizer
		{
			get
			{
				return Get<IRecognizer>();
			}
			set
			{
				hub.SetInstance<IRecognizer>( value );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		//
		// currently StringObjectHelper and StringArrayObjectHelper
		//

		//TypeHelperDictionary	_typeHelpers = new TypeHelperDictionary();

		//public TypeHelperDictionary GetTypeHelpers()
		//{
		//	return hub.Get<TypeHelperDictionary>();
		//}


		//public TypeHelperDictionary TypeHelpers
		//{
		//	get
		//	{
		//		return hub.Get<TypeHelperDictionary>();
		//	}
		//}

	
		/////////////////////////////////////////////////////////////////////////////

		public bool	RunningUnderAspNet
		{
			get {
				return null != HttpContext.Current;
			}
		}


		///////////////////////////////////////////////////////////////////////////

		bool _macroTraceOn = false;

		public bool MacroTraceOn
		{
			get {
				return _macroTraceOn;
			}

			set {
				_macroTraceOn = value;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		int _macroTraceLevel = 1;

		public int MacroTraceLevel
		{
			get {
				return _macroTraceLevel;
			}

			set {
				_macroTraceLevel = value;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		bool _pushbackResult = false;

		public bool PushbackResult
		{
			get {
				return _pushbackResult;
			}

			set {
				_pushbackResult = value;
			}
		}


		/////////////////////////////////////////////////////////////////////////////
		
		bool _expandAndScan = true;

		public bool ExpandAndScan
		{
			get {
				return _expandAndScan;
			}
		
			set {
				_expandAndScan = value;
			}
		}
		

		///////////////////////////////////////////////////////////////////////////

		int _maxMacroScanDepth = 128;

		public int MaxMacroScanDepth
		{
			get {
				return _maxMacroScanDepth;
			}

			set {
				_maxMacroScanDepth = value;
			}
		}


		///////////////////////////////////////////////////////////////////////////

		int _maxRunTime = 10 * 1024;
		
		public int MaxRunTime
		{
			get {
				return _maxRunTime;
			}

			set {
				_maxRunTime = value;
			}
		}


		///////////////////////////////////////////////////////////////////////////

		int _maxExecTime = 5 * 1024;
		
		public int MaxExecTime
		{
			get {
				return _maxExecTime;
			}

			set {
				_maxExecTime = value;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		bool _breakNext = false;

		public bool BreakNext
		{
			get {
				return _breakNext;
			}

			set {
				_breakNext = value;
			}
		}


		///////////////////////////////////////////////////////////////////////////

		bool _altTokenFmtOnly = false;

		public bool AltTokenFmtOnly
		{
			get {
				return _altTokenFmtOnly;
			}

			set {
				_altTokenFmtOnly = value;
			}
		}


		///////////////////////////////////////////////////////////////////////////

		string _openQuote = SC.DEF_OPEN_QUOTE_STR;

		public string OpenQuote
		{
			get {
				return _openQuote;
			}

			private set {
				_openQuote = value;
				SeqOpenQuote = new CharSequence( _openQuote );
			}
		}


		///////////////////////////////////////////////////////////////////////////

		string _closeQuote = SC.DEF_CLOSE_QUOTE_STR;
		
		public string CloseQuote
		{
			get {
				return _closeQuote;
			}

			private set {
				_closeQuote = value;
				SeqCloseQuote = new CharSequence( _closeQuote );
			}
		}


		/////////////////////////////////////////////////////////////////////////////
		//
		//
		//
		///////////////////////////////////////////////////////////////////////////

		public void SetQuotes( string open, string close )
		{
			OpenQuote = open;
			CloseQuote = close;
		}


		/////////////////////////////////////////////////////////////////////////////

		public string QuoteWrapString( string str )
		{
			return string.Format( "{0}{1}{2}", SeqOpenQuote.Sequence, str, SeqCloseQuote.Sequence );
		}


		/////////////////////////////////////////////////////////////////////////////

		NamedTextBlocks _namedTextBlocks = new NamedTextBlocks();

		public NamedTextBlocks GetTextBlocks()
		{
			return _namedTextBlocks;
		}


		/////////////////////////////////////////////////////////////////////////////
			
		EscapedCharList _escapedChars = new EscapedCharList();

		public EscapedCharList GetEscapedChars()
		{
			return _escapedChars;
		}


		/////////////////////////////////////////////////////////////////////////////

		NmpStringList _searchPaths = new NmpStringList();

		public NmpStringList GetSearchPaths()
		{
			return _searchPaths;
		}


		/////////////////////////////////////////////////////////////////////////////

		NmpStack<string> _directoryStack = new NmpStack<string>();

		public NmpStack<string> GetDirectoryStack()
		{
			return _directoryStack;
		}


		/////////////////////////////////////////////////////////////////////////////

		public GrandCentral( NMP nmp, INmpHost nmpHost )
		{
			// ******
			//
			// we have to do it this way because DefaultMacroProcessor is initialized
			// in Hub.Initialize() and DefaultMacroProcessor depends on GrandCentral to
			// access Hub.Get<>()
			//
			// yea, bad design but this is how it worked out after some refactoring,
			// for NOW we know that this works
			//
			//this.hub = new Hub( this, nmp, nmpHost );
			this.hub = new Hub { };
			hub.Initialize( this, nmp, nmpHost );

			// ******
			Security = new ExecutionSecurity();
			GetDirectoryStack().Push( Directory.GetCurrentDirectory() );
		}


	}


}

