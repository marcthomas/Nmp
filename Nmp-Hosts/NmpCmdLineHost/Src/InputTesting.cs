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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;


using NmpBase;
using Nmp;


namespace NmpHost {


	/////////////////////////////////////////////////////////////////////////////

	class InputTest {

		delegate void TestMethod( string fileName );


		/////////////////////////////////////////////////////////////////////////////

		private void FileRead( string fileName )
		{
			// ******
			string text = File.ReadAllText( fileName );
			ParseStringReader reader = new ParseStringReader( text );

			// ******
			while( !reader.AtEnd ) {
				char ch = reader.Next();

				Console.Write( ch );
			}

			// ******
			Console.Write( "\n\n\n" );
		}


		/////////////////////////////////////////////////////////////////////////////

		public bool Run()
		{


			// ******
			return false;
		}


		/////////////////////////////////////////////////////////////////////////////
		
		public InputTest( INmpHost host, CmdLineParams cmds )
		{
			// ******
			TestMethod testMethod = FileRead;

			// ******
			for( int iCmd = 0; iCmd < cmds.Count; iCmd++ ) {
				string cmd;
				string value;
			
				// ******
				if( cmds.GetCommand(iCmd, out cmd, out value) ) {
					string key = string.Empty;
					
					// ******
					if( null == value ) {
						value = string.Empty;
					}
					
					// ******
					//WriteLine( "command {0}, value {1}", cmd, value );
			
					// ******
					switch( cmd ) {
						case "--????--":
							break;

						default:
							host.Die( "unknown command line switch \"{0}\"", cmd );
							break;
					}
				}
				else {
					//
					// file to run test against
					//
					string fileName = value.ToLower().Trim();
					fileName = Path.GetFullPath( fileName );

					// ******
					if( ! File.Exists(fileName) ) {
						host.Die( "InputTest: unable to locate file \"{0}\"", fileName );
					}

					// ******
					testMethod( fileName );
					return;
				}
			}
		}
		
		
	}

}
