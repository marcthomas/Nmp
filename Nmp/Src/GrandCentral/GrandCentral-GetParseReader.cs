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

		public NonEscapingParseReader CreateNonEscapingParseReader( string text )
		{
			return new NonEscapingParseReader( this, GetDirectoryStack(), text );
		}


		///////////////////////////////////////////////////////////////////////////
		
		public ParseReader CreateParseReader()
		{
			return new ParseReader( this, GetDirectoryStack() );
		}


		///////////////////////////////////////////////////////////////////////////

		public ParseReader CreateParseReader( string text )
		{
			return new ParseReader( this, GetDirectoryStack(), text );
		}


		///////////////////////////////////////////////////////////////////////////
		
		public ParseReader CreateParseReader( IBaseReader reader, string context )
		{
			return new ParseReader( this, GetDirectoryStack(), reader, context );
		}
		

		///////////////////////////////////////////////////////////////////////////
		
		public ParseReader CreateParseReader( string text, string sourceFile, string context )
		{
			return new ParseReader( this, GetDirectoryStack(), text, sourceFile, context );
		}
		

		///////////////////////////////////////////////////////////////////////////
		
		public MasterParseReader CreateMasterParseReader( IBaseReader reader )
		{
			return new MasterParseReader( this, GetDirectoryStack(), reader );
		}
		

	}

}
