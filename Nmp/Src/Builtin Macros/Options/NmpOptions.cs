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
using System.Linq;
using System.Text;

//using Microsoft.JScript;
//using Microsoft.JScript.Vsa;

using System.Threading;

using NmpBase;

#pragma warning disable 618

namespace Nmp.Builtin.Macros {

	
	/////////////////////////////////////////////////////////////////////////////


	class NmpOptions {

		IMacroProcessor mp;
		IGrandCentral gc;


		/////////////////////////////////////////////////////////////////////////////

		public bool	MacroTraceOn		{ get { return gc.MacroTraceOn; } }

		public void SetMacroTraceOn( bool value )
		{
			gc.MacroTraceOn = value;
		}


		/////////////////////////////////////////////////////////////////////////////
		
		public int	MacroTraceLevel		{ get { return gc.MacroTraceLevel; } }
		
		public void SetMacroTraceLevel( int value )
		{
			gc.MacroTraceLevel = value;
		}
		
		
		/////////////////////////////////////////////////////////////////////////////
		
		public bool	PushbackResult		{ get { return gc.PushbackResult; } }
		
		public void SetPushbackResult( bool value )
		{
			gc.PushbackResult = value;
		}
		

		/////////////////////////////////////////////////////////////////////////////
		
		public bool	ExpandAndScan		{ get { return gc.ExpandAndScan; } }
		
		public void SetExpandAndScan( bool value )
		{
			gc.ExpandAndScan = value;
		}
		

		/////////////////////////////////////////////////////////////////////////////

		public int	MaxMacroScanDepth		{ get { return gc.MaxMacroScanDepth; } }

		public void SetMaxMacroScanDepth( int value )
		{
			gc.MaxMacroScanDepth = value;
		}


		/////////////////////////////////////////////////////////////////////////////

		public int	MaxRunTime		{ get { return gc.MaxRunTime; } }

		public void SetMaxRunTime( int value )
		{
			gc.MaxRunTime = value;
		}


		/////////////////////////////////////////////////////////////////////////////

		public int	MaxExecTime		{ get { return gc.MaxExecTime; } }

		public void SetMaxExecTime( int value )
		{
			gc.MaxExecTime = value;
		}


		/////////////////////////////////////////////////////////////////////////////
		
		public bool	BreakNext		{ get { return gc.BreakNext; } }
		
		public void SetBreakNext( bool value )
		{
			gc.BreakNext = value;
		}
		

		/////////////////////////////////////////////////////////////////////////////

		public bool	AltTokenFmtOnly		{ get { return gc.AltTokenFmtOnly; } }

		public void SetAltTokenFmtOnly( bool value )
		{
			gc.AltTokenFmtOnly = value;
		}


		/////////////////////////////////////////////////////////////////////////////

		public string	OpenQuote		{ get { return gc.OpenQuote; } }
		public string	CloseQuote	{ get { return gc.CloseQuote; } }

		public void SetQuotes( string openQuoteStr, string closeQuoteStr )
		{
			gc.SetQuotes( openQuoteStr, closeQuoteStr );
		}


		/////////////////////////////////////////////////////////////////////////////

		public NmpOptions( IMacroProcessor mp )
		{
			this.mp = mp;
			gc = mp.GrandCentral;
		}
	}

}
