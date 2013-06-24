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

		[Macro]
		public bool fixResultOn { get { return gc.FixResults; } }

		[Macro]
		public void setFixtResult( bool value )
		{
			gc.FixResults = value;
		}


		/////////////////////////////////////////////////////////////////////////////

		[Macro]
		public bool macroTraceOn { get { return gc.MacroTraceOn; } }

		[Macro]
		public void setMacroTraceOn( bool value )
		{
			gc.MacroTraceOn = value;
		}


		/////////////////////////////////////////////////////////////////////////////

		[Macro]
		public int macroTraceLevel { get { return gc.MacroTraceLevel; } }

		[Macro]
		public void setMacroTraceLevel( int value )
		{
			gc.MacroTraceLevel = value;
		}
		
		
		/////////////////////////////////////////////////////////////////////////////

		[Macro]
		public bool pushbackResult { get { return gc.PushbackResult; } }

		[Macro]
		public void setPushbackResult( bool value )
		{
			gc.PushbackResult = value;
		}
		

		/////////////////////////////////////////////////////////////////////////////

		[Macro]
		public bool expandAndScan { get { return gc.ExpandAndScan; } }

		[Macro]
		public void setExpandAndScan( bool value )
		{
			gc.ExpandAndScan = value;
		}
		

		/////////////////////////////////////////////////////////////////////////////

		[Macro]
		public int maxMacroScanDepth { get { return gc.MaxMacroScanDepth; } }

		[Macro]
		public void setMaxMacroScanDepth( int value )
		{
			gc.MaxMacroScanDepth = value;
		}


		/////////////////////////////////////////////////////////////////////////////

		[Macro]
		public int maxRunTime { get { return gc.MaxRunTime; } }

		[Macro]
		public void setMaxRunTime( int value )
		{
			gc.MaxRunTime = value;
		}


		/////////////////////////////////////////////////////////////////////////////

		[Macro]
		public int maxExecTime { get { return gc.MaxExecTime; } }

		[Macro]
		public void setMaxExecTime( int value )
		{
			gc.MaxExecTime = value;
		}


		/////////////////////////////////////////////////////////////////////////////

		[Macro]
		public bool breakNext { get { return gc.BreakNext; } }

		[Macro]
		public void setBreakNext( bool value )
		{
			gc.BreakNext = value;
		}
		

		/////////////////////////////////////////////////////////////////////////////

		[Macro]
		public bool altTokenFmtOnly { get { return gc.AltTokenFmtOnly; } }

		[Macro]
		public void setAltTokenFmtOnly( bool value )
		{
			gc.AltTokenFmtOnly = value;
		}


		/////////////////////////////////////////////////////////////////////////////

		[Macro]
		public string openQuote { get { return gc.OpenQuote; } }

		[Macro]
		public string closeQuote { get { return gc.CloseQuote; } }

		[Macro]
		public void setQuotes( string openQuoteStr, string closeQuoteStr )
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
