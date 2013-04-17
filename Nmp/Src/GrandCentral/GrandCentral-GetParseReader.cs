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

using NmpBase;
using Nmp.Input;

namespace Nmp {

	///////////////////////////////////////////////////////////////////////////

	partial class GrandCentral {


		///////////////////////////////////////////////////////////////////////////

		public NonEscapingParseReader GetNonEscapingParseReader( string text )
		{
			return new NonEscapingParseReader( this, GetDirectoryStack(), text );
		}


		///////////////////////////////////////////////////////////////////////////
		
		public ParseReader GetParseReader()
		{
			return new ParseReader( this, GetDirectoryStack() );
		}


		///////////////////////////////////////////////////////////////////////////

		public ParseReader GetParseReader( string text )
		{
			return new ParseReader( this, GetDirectoryStack(), text );
		}


		///////////////////////////////////////////////////////////////////////////
		
		public ParseReader GetParseReader( IBaseReader reader, string context )
		{
			return new ParseReader( this, GetDirectoryStack(), reader, context );
		}
		

		///////////////////////////////////////////////////////////////////////////
		
		public ParseReader GetParseReader( string text, string sourceFile, string context )
		{
			return new ParseReader( this, GetDirectoryStack(), text, sourceFile, context );
		}
		

		///////////////////////////////////////////////////////////////////////////
		
		public MasterParseReader GetMasterParseReader( IBaseReader reader )
		{
			return new MasterParseReader( this, GetDirectoryStack(), reader );
		}
		

	}

}
