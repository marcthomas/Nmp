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
using System.Diagnostics.Eventing;
using System.Linq;
using System.Text;

using fastJSON;

namespace NmpBase {


	/////////////////////////////////////////////////////////////////////////////

	//
	// TraceEventType
	//

	// Critical Fatal error or application crash. 
	// Error Recoverable error. 
	// Warning Noncritical problem. 
	// Information Informational message. 
	// Verbose Debugging trace. 
	// Start Starting of a logical operation. 
	// Stop Stopping of a logical operation. 
	// Suspend Suspension of a logical operation. 
	// Resume Resumption of a logical operation. 
	// Transfer Changing of correlation identity 


	/////////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// 
	/// </summary>
	/// <param name="level"></param>
	/// <param name="keyWords"></param>
	/// <param name="obj">instance of a message class (or struct), or string message </param>
	/// 
	public delegate void InnerProcHandlerDelegate( EventLevel level, EventKey keyWords, object obj );

	public enum EventLevel : byte {
		Critical,
		Error,
		Warning,
		Information,
		Verbose,
		
		//Start,
		//Stop,
		//Suspend,
		//Resume,
		//Transfer,
	}

	[Flags]
	public enum EventKey : long {
		/// <summary>
		/// no flag set
		/// </summary>
		None = 0,

		/// <summary>
		/// Message is a simple string
		/// </summary>
		SimpleString = 1 << 0,

		/// <summary>
		/// Message is a JSON string
		/// </summary>
		JSON = 1 << 1,

		/// <summary>
		/// Only used for InnerProcHandlerDelegate(), indicates the object being passed
		/// is a message object and not a 'SimpleString'
		/// </summary>
		Object = 1 << 2,

		/// <summary>
		/// Indicates message should NOT be passed to TEWriter.Provider
		/// </summary>
		NoProvider = 1 << 3,

	}


	/////////////////////////////////////////////////////////////////////////////

	public static class EventWriter {

		class TEWriter {

			public Guid ProviderId { get; private set; }
			public EventProvider Provider { get; private set; }
			public InnerProcHandlerDelegate InnerProcHandler { get; private set; }
			public object Instance { get; private set; }

			/////////////////////////////////////////////////////////////////////////////

			public TEWriter( Guid providerId, InnerProcHandlerDelegate innerProcHandler, object instance )
			{
				ProviderId = providerId;
				Provider = new EventProvider( providerId );
				InnerProcHandler = innerProcHandler;
				Instance = instance;
			}
		}

		// ******
		static Dictionary<object, TEWriter> writers = new Dictionary<object, TEWriter> { };
		static NmpStack<TEWriter> writerStack = new NmpStack<TEWriter> { };
		static TEWriter writer;


		/////////////////////////////////////////////////////////////////////////////

		static bool WriterIsValid
		{
			get
			{
				return null != writer;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		static bool ProviderIsValid
		{
			get
			{
				return null != writer && null != writer.Provider;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		static bool InnerProcessHandlerIsValid
		{
			get
			{
				return null != writer && null != writer.InnerProcHandler;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		static EventLevel _eventLevel = EventLevel.Information;

		public static EventLevel OutputLevel
		{
			get
			{
				return _eventLevel;
			}

			set
			{
				_eventLevel = value;
			}
		}

	
		/////////////////////////////////////////////////////////////////////////////
		//
		//
		//
		/////////////////////////////////////////////////////////////////////////////

		//public static void Event( byte level, long keyWords, string msg, params object [] args )
		//{
		//	// ******
		//	if( ProviderIsValid && writer.Provider.IsEnabled() ) {
		//		var message = args.Length > 0 ? Helpers.SafeStringFormat( msg, args ) : msg;
		//		writer.Provider.WriteMessageEvent( message, level, keyWords );
		//	}
		//}


		/////////////////////////////////////////////////////////////////////////////

		//public static void Event( string msg, params object [] args )
		//{
		//	// ******
		//	if( WriterIsValid ) {
		//		var message = Helpers.SafeStringFormat( msg, args );
		//		Event( message, EventLevel.Information, EventKey.SimpleString );
		//	}
		//}


		/////////////////////////////////////////////////////////////////////////////
		//
		//
		//
		/////////////////////////////////////////////////////////////////////////////

		static void Event( EventLevel level, string msg, params object [] args )
		{
			// ******
			if( WriterIsValid ) {
				var message = Helpers.SafeStringFormat( msg, args );

				// ******
				if( InnerProcessHandlerIsValid ) {
					writer.InnerProcHandler( level, EventKey.SimpleString, message );
				}

				// ******
				if( ProviderIsValid ) {
					writer.Provider.WriteMessageEvent( message, (byte) level, (long) EventKey.SimpleString );
				}
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public static void Critical( string fmt, object [] args )
		{
			Event( EventLevel.Critical, fmt, args );
		}


		/////////////////////////////////////////////////////////////////////////////

		public static void Error( string fmt, object [] args )
		{
			Event( EventLevel.Error, fmt, args );
		}


		/////////////////////////////////////////////////////////////////////////////

		public static void Warning( string fmt, object [] args )
		{
			Event( EventLevel.Warning, fmt, args );
		}


		/////////////////////////////////////////////////////////////////////////////

		public static void Information( string fmt, object [] args )
		{
			Event( EventLevel.Information, fmt, args );
		}


		/////////////////////////////////////////////////////////////////////////////

		public static void Verbose( string fmt, object [] args )
		{
			Event( EventLevel.Verbose, fmt, args );
		}


		/////////////////////////////////////////////////////////////////////////////

		//public static void Start( string fmt, object [] args )
		//{
		//	Event( EventLevel.Start, fmt, args );
		//}


		/////////////////////////////////////////////////////////////////////////////

		//public static void Stop( string fmt, object [] args )
		//{
		//	Event( EventLevel.Stop, fmt, args );
		//}


		/////////////////////////////////////////////////////////////////////////////

		//public static void Suspend( string fmt, object [] args )
		//{
		//	Event( EventLevel.Suspend, fmt, args );
		//}


		/////////////////////////////////////////////////////////////////////////////

		//public static void Resume( string fmt, object [] args )
		//{
		//	Event( EventLevel.Resume, fmt, args );
		//}


		/////////////////////////////////////////////////////////////////////////////

		//public static void Transfer( string fmt, object [] args )
		//{
		//	Event( EventLevel.Transfer, fmt, args );
		//}


		/////////////////////////////////////////////////////////////////////////////
		//
		//
		//
		/////////////////////////////////////////////////////////////////////////////

		static void Event( EventLevel level, object obj, params EventKey [] additionalKeyWords )
		{
			// ******
			if( WriterIsValid ) {

				// ******
				EventKey keys = EventKey.None;
				foreach( var key in additionalKeyWords ) {
					keys |= key;
				}

				// ******

				Debug.WriteLine( obj.ToString() );

				if( InnerProcessHandlerIsValid ) {
					writer.InnerProcHandler( level, EventKey.Object | keys, obj );
				}

				// ******
				if( ProviderIsValid ) {
					var json = Json.ToJSON( obj );
					writer.Provider.WriteMessageEvent( json, (byte) level, (long) (EventKey.JSON | keys) );
				}
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public static void Critical( object obj, params EventKey [] additionalKeyWords )
		{
			Event( EventLevel.Critical, obj, additionalKeyWords );
		}


		/////////////////////////////////////////////////////////////////////////////

		public static void Error( object obj, params EventKey [] additionalKeyWords )
		{
			Event( EventLevel.Error, obj, additionalKeyWords );
		}


		/////////////////////////////////////////////////////////////////////////////

		public static void Warning( object obj, params EventKey [] additionalKeyWords )
		{
			Event( EventLevel.Warning, obj, additionalKeyWords );
		}


		/////////////////////////////////////////////////////////////////////////////

		public static void Information( object obj, params EventKey [] additionalKeyWords )
		{
			Event( EventLevel.Information, obj, additionalKeyWords );
		}


		/////////////////////////////////////////////////////////////////////////////

		public static void Verbose( object obj, params EventKey [] additionalKeyWords )
		{
			Event( EventLevel.Verbose, obj, additionalKeyWords );
		}


		/////////////////////////////////////////////////////////////////////////////

		//public static void Start( object obj, params EventKey [] additionalKeyWords )
		//{
		//	Event( EventLevel.Start, obj, additionalKeyWords );
		//}


		/////////////////////////////////////////////////////////////////////////////

		//public static void Stop( object obj, params EventKey [] additionalKeyWords )
		//{
		//	Event( EventLevel.Stop, obj, additionalKeyWords );
		//}


		/////////////////////////////////////////////////////////////////////////////

		//public static void Suspend( object obj, params EventKey [] additionalKeyWords )
		//{
		//	Event( EventLevel.Suspend, obj, additionalKeyWords );
		//}


		/////////////////////////////////////////////////////////////////////////////

		//public static void Resume( object obj, params EventKey [] additionalKeyWords )
		//{
		//	Event( EventLevel.Resume, obj, additionalKeyWords );
		//}


		/////////////////////////////////////////////////////////////////////////////

		//public static void Transfer( object obj, params EventKey [] additionalKeyWords )
		//{
		//	Event( EventLevel.Transfer, obj, additionalKeyWords );
		//}



		/////////////////////////////////////////////////////////////////////////////


		public static object SetCurrentWriter( object instance )
		{
			// ******
			TEWriter tew;
			if( writers.TryGetValue( instance, out tew ) ) {
				writer = tew;
			}
			else {
				writer = null;
				throw new Exception( "unknown writer instance" );
			}

			// ******
			return writer;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static void PushWriter( object instanceOfOther )
		{
			writerStack.Push( writer );
			SetCurrentWriter( instanceOfOther );
		}


		/////////////////////////////////////////////////////////////////////////////

		public static void PopWriter()
		{
			writer = writerStack.Pop();
		}


		/////////////////////////////////////////////////////////////////////////////

		public static object RegisterWriter( Guid providerId, object instance, InnerProcHandlerDelegate innerProcHandler )
		{
			// ******
			writer = new TEWriter( providerId, innerProcHandler, instance );
			writers.Add( instance, writer );

			// ******
			return writer;
		}




	}


}
