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
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Threading;

using NmpBase;
using Nmp;



//#pragma warning disable 618

namespace Nmp.Builtin.Macros {

	
	/////////////////////////////////////////////////////////////////////////////

	partial class CoreMacros {


		/////////////////////////////////////////////////////////////////////////////

		public object TestOptionalArgs( string str, int i1 = 1, int i2 = 2, object o = null )
		{
			return string.Format( "TestOptional {0} {1} {2} {3}", str, i1, i2, null == o ? "null" : o  );
		}


		/////////////////////////////////////////////////////////////////////////////
		//
		// #nmpRegion( name [ options , ] )
		//
		/////////////////////////////////////////////////////////////////////////////

		public string nmpRegion( string name, params object [] optionalArgs )
		{
			//
			// method is a placeholder that can be used by editors
			//
			return string.Empty;
		}


		/////////////////////////////////////////////////////////////////////////////
		//
		// #endNmpRegion( name [ options , ] )
		//
		/////////////////////////////////////////////////////////////////////////////

		public string endNmpRegion( string name, params object [] optionalArgs )
		{
			//
			// method is a placeholder that can be used by editors
			//
			return string.Empty;
		}


		/////////////////////////////////////////////////////////////////////////////
		//
		// #sleep( milliseconds )
		//
		/////////////////////////////////////////////////////////////////////////////

		public object sleep( int ms )
		{
			// ******
			if( ms < 100 ) {
				ms = 100;
			}
			else if( ms > 1000 ) {
				ms = 1000;
			}

			// ******
			Thread.Sleep( ms );
			return string.Empty;
		}
		
		
		/////////////////////////////////////////////////////////////////////////////
		//
		// #newDateTime
		//
		/////////////////////////////////////////////////////////////////////////////

		public object CreateDateTime( Int64 ticksOrYear,
																	Int32 month, 
																	Int32 day, 
																	Int32 hour, 
																	Int32 minute, 
																	Int32 second, 
																	Int32 ms, 
																	bool utc
																)
		{
			// ******
			if( -1 == ticksOrYear ) {
				return utc ? DateTime.UtcNow : DateTime.Now;
			}

			// ******
			DateTimeKind kind = utc ? DateTimeKind.Utc : DateTimeKind.Local;

			if( -1 == month ) {
				return new DateTime( ticksOrYear, kind );
			}

			if( -1 == hour ) {
				return new DateTime( (Int32) ticksOrYear, month, day, 0, 0, 0, kind );
			}

			return new DateTime( (Int32) ticksOrYear, month, day, hour, minute, second, ms, kind );
		}
		
		
		/////////////////////////////////////////////////////////////////////////////

		public object newDateTime( Int64 ticksOrYear = -1, Int32 month = -1, Int32 day = 1, Int32 hour = -1, Int32 minute = 0, Int32 second = 0, Int32 ms = 0 )
		{
			return CreateDateTime( ticksOrYear, month, day, hour, minute, second, ms, false );
		}
		
		
		/////////////////////////////////////////////////////////////////////////////

		public object newDateTimeUtc( Int64 ticksOrYear = -1, Int32 month = -1, Int32 day = 1, Int32 hour = -1, Int32 minute = 0, Int32 second = 0, Int32 ms = 0 )
		{
			return CreateDateTime( ticksOrYear, month, day, hour, minute, second, ms, true );
		}
		
		
		/////////////////////////////////////////////////////////////////////////////
		//
		// #datetime
		//
		/////////////////////////////////////////////////////////////////////////////

		public object dateTime( string format = "" )
		{
			// ******
			if( string.IsNullOrEmpty(format) ) {
				//
				// ISO8601 is year-month-dayThour:minute:second:thousanthsZ ==>  "yyyy-MM-ddTHH:mm:ss.000Z"
				//
				//format = "yyyy-MM-ddTHH:mm:ss.000Z";
				format = string.Empty;
			}
			
			// ******
			return DateTime.Now.ToString( format );
		}
		
		
		/////////////////////////////////////////////////////////////////////////////
		//
		// #datetimeutc
		//
		/////////////////////////////////////////////////////////////////////////////

		public object dateTimeUtc( string format = "" )
		{
			// ******
			if( string.IsNullOrEmpty(format) ) {
				//
				// ISO8601 is year-month-dayThour:minute:second:thousanthsZ ==>  "yyyy-MM-ddTHH:mm:ss.000Z"
				//
				//format = "yyyy-MM-ddTHH:mm:ss.000Z";
				format = string.Empty;
			}
			
			// ******
			return DateTime.UtcNow.ToString( format );
		}
		
		


	}
}
