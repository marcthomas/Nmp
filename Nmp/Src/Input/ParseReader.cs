#region License
// 
// Author: Joe McLain <nmp.developer@outlook.com>
// Copyright (c) 2013, Joe McLain and Digital Writing
// 
// Licensed under Eclipse Public License, Version 1.0 (EPL-1.0)
// See the file LICENSE.txt for details.
// 
#endregion
// ParseReader.cs
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.CodeDom.Compiler;
using System.CodeDom;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Linq;

using NmpBase;
using Nmp;

namespace Nmp.Input {


	///////////////////////////////////////////////////////////////////////////

	class NonEscapingParseReader : ParseReader {

		public NonEscapingParseReader( GrandCentral gc, NmpStack<string> dirStack, string text )
			:	base(gc, dirStack, text)
		{
			CheckEscapesAndSpecialChars = false;
		}

	}


	///////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// ParseReader is used to provide input for the scanner; it is important
	/// that it always be created within a using () {} statement because it
	/// manipulates the directory stack which must be kept balanced 
	/// </summary>
	
	[DebuggerDisplay("Remainder: {Remainder}")]

	class ParseReader : IInput {


		///////////////////////////////////////////////////////////////////////////

		protected GrandCentral gc;

		protected NmpStack<string> dirStack;
		protected bool initializePushedDirectory = false;

		protected NmpStack<Reader>	includeFiles = new NmpStack<Reader>();
		protected Reader current;


		public bool		CheckEscapesAndSpecialChars	{ get { return current.CheckEscapesAndSpecialChars; } set { current.CheckEscapesAndSpecialChars = value; } }

		public IReader	Current			{ get { return current; }}

		public int		Index						{ get { return current.Index; } }
		public int		RemainderCount	{ get { return current.RemainderCount; } }
		public string	Remainder				{ get { return current.Remainder; } }

		//
		// at end of input - including all pushed IBaseReader's
		//
		// only having GetMoreInput() in this routines is a bit scarry - works
		// ok because scanners use it but ...
		//
		public bool AtEnd 			{ get { return null == current || (0 == RemainderCount ? ! GetMoreInput() : false); } }

		//
		// _IInput
		//
		public	bool		PushbackCalled	{ get; private set; }
		public string		Context					{ get; private set; }

		public string		SourceName			{ get { return current.SourceName; } }
		public int			Line						{ get { return current.Line; } } 
		public int			Column					{ get { return current.Column; } }


		/////////////////////////////////////////////////////////////////////////////

		public void Dispose()
		{
			if( initializePushedDirectory ) {
				dirStack.Pop();
			}
		}
	

		///////////////////////////////////////////////////////////////////////////
		
		protected virtual bool GetMoreInput()
		{
			// ******
			if( includeFiles.NotEmpty ) {
				PreviousState();
				return true;
			}

			//
			// this is ugly: need to make sure we clear the last
			// path when we're out of data
			//
			if( initializePushedDirectory ) {
				dirStack.Pop();
				initializePushedDirectory = false;
			}

			// ******
			return false;
		}


		///////////////////////////////////////////////////////////////////////////

		protected void NewState( Reader newCurrent )
		{
			// ******
			includeFiles.Push( current );
			current = newCurrent;

			// ******
			string path = current.Source.DefaultPath;

			if( ! string.IsNullOrEmpty(path) ) {
				//
				// push path
				//
				dirStack.Push( path );
			}
		}


		///////////////////////////////////////////////////////////////////////////

		protected void PreviousState()
		{
			// ******
			if( ! string.IsNullOrEmpty(current.Source.DefaultPath) ) {
				//
				// pop path
				//
				dirStack.Pop();
			}

			// ******
			current = includeFiles.Pop();
		}


		///////////////////////////////////////////////////////////////////////////

		public string DefaultPath
		{
			get {
				return dirStack.Peek();
			}
		}
		

		///////////////////////////////////////////////////////////////////////////
		
		public void	PushBack( string text )
		{							
			// ******
			PushbackCalled = true;
			if( String.IsNullOrEmpty(text) ) {
				return;
			}
			
			// ******
			current.Pushback( text );
		}
		
		
		/////////////////////////////////////////////////////////////////////////////

		public bool Matches( int startIndex, string cmpStr )
		{
			return current.Matches( startIndex, cmpStr );
		}


		/////////////////////////////////////////////////////////////////////////////

		public bool StartsWith( string cmpStr )
		{
			return current.StartsWith( cmpStr );
		}


		/////////////////////////////////////////////////////////////////////////////

		public bool StartsWith( string cmpStr, bool ignoreCase )
		{
			return current.StartsWith( cmpStr, ignoreCase );
		}


		/////////////////////////////////////////////////////////////////////////////

		public void Skip( Predicate<char> cmp )
		{
			while( cmp(current.PeekChar(0)) ) {
				current.GetChar();
			}
		}
				

		/////////////////////////////////////////////////////////////////////////////

		public void Skip( int n )
		{
			current.SkipChars( n );
		}


		///////////////////////////////////////////////////////////////////////////

		public char Peek( int index )
		{
			return current.PeekChar( index );
		}


		///////////////////////////////////////////////////////////////////////////

		public char Peek()
		{
			return current.PeekChar( 0 );
		}


		///////////////////////////////////////////////////////////////////////////

		public char Next()
		{
			return current.GetChar();
		}


		///////////////////////////////////////////////////////////////////////////

		public string Next( int count )
		{
			// ******
			StringBuilder sb = new StringBuilder();
			while( count > 0 ) {
				sb.Append( current.GetChar() );
				--count;
			}

			// ******
			return sb.ToString();
		}


		///////////////////////////////////////////////////////////////////////////

		public string GetText( int start, int end )
		{
			return current.Source.GetText( start, end );
		}


		///////////////////////////////////////////////////////////////////////////

		public void IncludeText( string text, string includeName )
		{
			// ******
			if( ! String.IsNullOrEmpty( text ) ) {
				NewState( new Reader( gc, new ParseStringReader(text, includeName)) );
			}
		}


		///////////////////////////////////////////////////////////////////////////

		//public void IncludeFile( string fileNameIn )
		//{
		//	// ******
		//	string	text = null;
		//	string fileName = string.Empty;

		//	// ******
		//	if( null == (text = FileReader.ReadFile( fileNameIn, out fileName, null )) ) {
		//		ThreadContext.MacroError( "file not found \"{0}\"", fileNameIn );
		//		return;
		//	}

		//	// ******
		//	if( string.IsNullOrEmpty( text ) ) {
		//		//
		//		// empty file
		//		//
		//		return;
		//	}

		//	// ******
		//	int nInstances = includeFiles.Count( item => fileName == item.SourceName );
		//	if( nInstances >= 20 ) {
		//		ThreadContext.MacroError( "the file \"{0}\" is activly included {1} times, this exceeds the allowed limit, Nmp will terminate", fileName, nInstances );
		//	}

		//	// ******
		//	NewState( new Reader(new ParseStringReader( text, fileName)) );
		//}
		
		
		///////////////////////////////////////////////////////////////////////////
		
		//[DebuggerStepThrough]
		public void Initialize( IBaseReader reader, string context )
		{
			//
			// this is ugly
			//
			if( ! string.IsNullOrEmpty(reader.DefaultPath) ) {
				dirStack.Push( reader.DefaultPath );
				initializePushedDirectory = true;
			}

			// ******
			Context = context;
			PushbackCalled = false;

			// ******
			current = new Reader( gc, reader );
		}
		

		///////////////////////////////////////////////////////////////////////////
		
		[DebuggerStepThrough]
		public ParseReader( GrandCentral gc, NmpStack<string> dirStack )
		{
			this.gc = gc;
			this.dirStack = dirStack;
			Initialize( new ParseStringReader( string.Empty ), "empty reader" );
		}


		///////////////////////////////////////////////////////////////////////////

		[DebuggerStepThrough]
		public ParseReader( GrandCentral gc, NmpStack<string> dirStack, string text )
		{
			this.gc = gc;
			this.dirStack = dirStack;
			Initialize( new ParseStringReader( text ), "no context" );
		}


		///////////////////////////////////////////////////////////////////////////
		
		[DebuggerStepThrough]
		public ParseReader( GrandCentral gc, NmpStack<string> dirStack, IBaseReader reader, string context )
		{
			this.gc = gc;
			this.dirStack = dirStack;
			Initialize( reader, string.IsNullOrEmpty(context) ? "no context" : context );
		}
		

		///////////////////////////////////////////////////////////////////////////
		
		[DebuggerStepThrough]
		public ParseReader( GrandCentral gc, NmpStack<string> dirStack, string text, string sourceFile, string context )
			: this( gc, dirStack, new ParseStringReader(text, sourceFile), context )
		{
		}
		

	}


	///////////////////////////////////////////////////////////////////////////

	class MasterParseReader : ParseReader {
	
		// ******
		private StringBuilder savedForLater = new StringBuilder();

		public string	BaseFileName	{ get; private set; }


		///////////////////////////////////////////////////////////////////////////

		protected override bool GetMoreInput()
		{
			// ******
			if( base.GetMoreInput() ) {
				return true;
			}
			else if( savedForLater.Length > 0 ) {
				current = new Reader( gc, new ParseStringReader( savedForLater.ToString(), string.Empty) );
				savedForLater.Length = 0;
				return true;
			}

			// ******
			return false;
		}


		///////////////////////////////////////////////////////////////////////////
		
		public void SaveForLater( StringBuilder sb )
		{
			savedForLater.Append( sb );
		}
		
		
		///////////////////////////////////////////////////////////////////////////
		
		public void SaveForLater( string str )
		{
			savedForLater.Append( str );
		}
		
		
		///////////////////////////////////////////////////////////////////////////
		
		//public void	SetSearchPath( string path )
		//{
		//	// ******
		//	//
		//	// clear current path set, split path at ';' and put
		//	// the non emtpy results into SearchPath
		//	//
		//	NmpStringList searchPaths = ThreadContext.SearchPaths;
		//	searchPaths.Clear();

		//	string [] a = path.Split( new char[] {';'} );
		//	
		//	foreach( string s in a ) {
		//		if( s.Length > 0 ) {
		//			searchPaths.Add( s );
		//		}
		//	}
		//}
		
		
		///////////////////////////////////////////////////////////////////////////
		
		public MasterParseReader( GrandCentral gc, NmpStack<string> dirStack, IBaseReader reader )
			:	base( gc, dirStack, reader, "Root source" )
		{
			this.BaseFileName = current.SourceName;
		}
		
	}

}
