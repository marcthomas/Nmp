#region License
// 
// Author: Joe McLain <nmp.developer@outlook.com>
// Copyright (c) 2013, Joe McLain and Digital Writing
// 
// Licensed under Eclipse Public License, Version 1.0 (EPL-1.0)
// See the file LICENSE.txt for details.
// 
#endregion
// ErrorsWarningsTrace.cs
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Web;

using NmpBase;
using Nmp;


	/////////////////////////////////////////////////////////////////////////////

	public static partial class ThreadContext {


		///////////////////////////////////////////////////////////////////////////

		public static void MacroError( string fmt, params object [] args )
		{
			if( null != Notifications ) {
				Notifications.MacroError( fmt, args );
			}
			else {
				throw new Exception( Helpers.SafeStringFormat(fmt, args) );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public static void MacroError( Notifier notifier, string fmt, params object [] args )
		{								
			if( null != Notifications ) {
				Notifications.MacroError( notifier, fmt, args );
			}
			else {
				throw new Exception( Helpers.SafeStringFormat(fmt, args) );
			}
		}
		

		///////////////////////////////////////////////////////////////////////////////

		public static void MacroWarning( string fmt, params object [] args )
		{
			if( null != Notifications ) {
				Notifications.MacroWarning( fmt, args );
			}
			else {
				throw new Exception( Helpers.SafeStringFormat(fmt, args) );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public static void MacroWarning( Notifier notifier, string fmt, params object [] args )
		{								
			if( null != Notifications ) {
				Notifications.MacroWarning( notifier, fmt, args );
			}
			else {
				throw new Exception( Helpers.SafeStringFormat(fmt, args) );
			}
		}
		

		/////////////////////////////////////////////////////////////////////////////

		public static void WriteMessage( string fmt, params object [] args )
		{					
			if( null != Notifications ) {
				Notifications.WriteMessage( fmt, args );
			}
			else {
				throw new Exception( Helpers.SafeStringFormat(fmt, args) );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public static void Trace( string fmt, params object [] args )
		{	
			if( null != Notifications ) {
				Notifications.Trace( fmt, args );
			}
			else {
				throw new Exception( Helpers.SafeStringFormat(fmt, args) );
			}
		}


	}


