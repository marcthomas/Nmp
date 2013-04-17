using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.Remoting.Messaging;
using System.Web;
using NmpBase;
using NmpBase.Reflection;
using NmpExpressions;

namespace Nmp {
	/////////////////////////////////////////////////////////////////////////////

	class Hub : IHub {

		// ******
		//InvocationStack invocationStack;

		//INotifications notifications;
		//GrandCentral grandCentral;
		//NMP nmp;
		//IScanner scanner;
		//INmpHost nmpHost;
		//IMacroTracer macroTracer;
		//IRecognizer recognizer;
		//IMacroProcessor macroProcessor;

		Dictionary<Type, object> instances = new Dictionary<Type, object>();


		/////////////////////////////////////////////////////////////////////////////

		InvocationStack _invocationStack;

		//
		// one and only (singleton)
		//

		public InvocationStack InvocationStack
		{
			get
			{
				if( null == _invocationStack ) {
					_invocationStack = new InvocationStack { };
				}
				return _invocationStack;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public InvocationContext NewInvocationContext
		{
			get
			{
				return new InvocationContext( InvocationStack );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		//public TypeHelperDictionary NewTypeHelperDictionary
		//{
		//	get
		//	{
		//		return new TypeHelperDictionary { };
		//	}
		//}


		/////////////////////////////////////////////////////////////////////////////

		public object Get( Type type )
		{
			// ******
			object value = null;

			// ******
			if( typeof( InvocationContext ) == type ) {
				value = NewInvocationContext;
			}
			//else if( typeof( TypeHelperDictionary ) == type ) {
			//	value = NewTypeHelperDictionary;
			//}
			else if( typeof( InvocationStack ) == type ) {
				value = InvocationStack;
			}
			else {
				instances.TryGetValue( type, out value );
			}

			// ******
			if( null == value ) {

				Debug.WriteLine( "NEED TO SEARCH FOR TYPE {0}", type.Name );

				value = TypeCreator.Create( type, null );

				if( null == value ) {
					Debugger.Break();
				}
			}

			return value;
		}


		/////////////////////////////////////////////////////////////////////////////

		public T Get<T>() where T : class
		{
			return Get( typeof( T ) ) as T;
		}


		/////////////////////////////////////////////////////////////////////////////

		public void SetInstance<T>( object value )
		{
			//Debug.Assert( null == value || value.GetType() == typeof( T ), "object does not match type T" );
			instances [ typeof( T ) ] = value;
		}


		/////////////////////////////////////////////////////////////////////////////

		public void Initialize( GrandCentral grandCentral, NMP nmp, INmpHost nmpHost )
		{
			// ******
			//
			// GrandCentral holds all of the non static objects that use to live 
			// in ThreadContext, they are our "global" data - that stuff that lots
			// of disasociated parts need access to - mostly in a read only way
			//
			SetInstance<GrandCentral>( grandCentral );

			// ******
			//
			// this is the interface that globaly handles errors, warnings and messages;
			// currently the instance is maintined in ThreadContext which is swapped in
			// and out as different instances of NMP are used
			//
			SetInstance<INotifications>( new Notifications( nmpHost, Get<InvocationStack>() ) );

			// ******
			//
			// NMP, IScanHelpers
			//
			SetInstance<NMP>( nmp );
			SetInstance<IScanner>( nmp );

			// ******
			//
			// INmpHost
			//
			SetInstance<INmpHost>( nmpHost );

			// ******
			//
			// IMacroTracer
			//
			SetInstance<IMacroTracer>( nmpHost.Tracer );

			// ******
			//
			// IRecognizer
			//
			// normaly accessed via GrandCentral who updates Hub when recognizer is changed
			//
			var recognizer = null != nmpHost.Recognizer ? nmpHost.Recognizer : new DefaultRecognizer( grandCentral );
			SetInstance<IRecognizer>( recognizer );

			// ******
			//
			// TypeHelperDictionary
			//
			// MUST be initialized before DefaultMacroProcessor (dependency)
			//
			SetInstance<TypeHelperDictionary>( new TypeHelperDictionary { } );
			
			// ******
			//
			// DefaultMacroProcessor and IMacroProcessor
			//
			var macroProcessor = new DefaultMacroProcessor( this, nmpHost );
			SetInstance<IMacroProcessor>( macroProcessor );
			SetInstance<DefaultMacroProcessor>( macroProcessor );

			SetInstance<IMacroProcessorBase>( macroProcessor );
		}


		/////////////////////////////////////////////////////////////////////////////

		public Hub()
		{
		}


		/////////////////////////////////////////////////////////////////////////////

		//public Hub( GrandCentral grandCentral, NMP nmp, INmpHost nmpHost )
		//{
		//	Initialize( grandCentral, nmp, nmpHost );
		//}

	}
}
