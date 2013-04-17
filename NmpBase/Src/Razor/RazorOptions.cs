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
using System.Linq;
using System.Text;

//using System.Globalization;
using System.IO;
//using System.Reflection;

using NmpBase;
using NmpBase.Reflection;
//using Nmp;

//using Csx;

namespace NmpBase.Razor {


	/////////////////////////////////////////////////////////////////////////////

	public class RazorOptions {

		/*

			these are the lines that we process from the top of a razor input file

			we also remove all lines that begin with a '-' or '#'

		*/

		const string START_SINGLE_LINE_COMMENT	=	";;";

		const string RAZOR_ASMINC		= "@RazorAssembly";
		const string RAZOR_ASMINC2	= "@Assembly";
		
		const string KEEP_TEMPS			= "@KeepTempFiles";

		const string RAZOR_DEBUG		= "@Debug";
		
		const string CODE_NAMESPACE	= "@Namespace";
		const string CODE_CLASSNAME	= "@ClassName";

		const string INJECT					= "@Inject";

		const string PREPROCESS			= "@Preprocess";
		const string POSTPROCESS		= "@Postprocess";

		const string COMMENTS_FULL_REMOVE		= "@SemiColonCommentsFullRemove";

		const string INCLUDE				= "@Include";


		/////////////////////////////////////////////////////////////////////////////

		public bool						Debug								{ get; private set; }
		public bool						KeepTempFiles				{ get; private set; }
		public NmpStringList	RefAssemblies 			{ get; private set; }

		public string					Namespace						{ get; private set; }
		public string					ClassName						{ get; private set; }

		public bool						Inject							{ get; private set; }

		public bool						PreProcess					{ get; private set; }
		public bool						PostProcess					{ get; private set; }

		bool CommentsFullRemove = false;

		/////////////////////////////////////////////////////////////////////////////

		private static void ReadToEOL( StringIndexer reader )
		{
			while( true ) {
				char ch = reader.NextChar();
				
				if( SC.CR == ch || SC.NEWLINE == ch || SC.NO_CHAR == ch ) {
					if( SC.CR == ch && SC.NEWLINE == reader.Peek() ) {
						//
						// skip the newline if preceeded by cr
						//
						reader.Skip( 1 );
					}
						
					return;
				}
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		private static string GetText( StringIndexer reader )
		{
			// ******
			var sb = new StringBuilder();
			
			// ******
			while( true ) {
				char ch = reader.NextChar();
				
				if( SC.CR == ch || SC.NEWLINE == ch || SC.NO_CHAR == ch ) {
					if( SC.CR == ch && SC.NEWLINE == reader.Peek() ) {
						//
						// skip the newline if preceeded by cr
						//
						reader.Skip( 1 );
					}
						
					break;
				}

				sb.Append( ch );
			}

			// ******
			return sb.ToString();
		}


		/////////////////////////////////////////////////////////////////////////////

		private void GetAssemblyName( StringIndexer reader )
		{
			//// ******
			//var sb = new StringBuilder();
			//
			//// ******
			//while( true ) {
			//	char ch = reader.Next();
			//	
			//	if( SC.CR == ch || SC.NEWLINE == ch || SC.NO_CHAR == ch ) {
			//		if( SC.CR == ch && SC.NEWLINE == reader.Peek() ) {
			//			//
			//			// skip the newline if preceeded by cr
			//			//
			//			reader.Next();
			//		}
			//			
			//		break;
			//	}
			//
			//	sb.Append( ch );
			//}
			//
			//// ******
			//RefAssemblies.Add( sb.ToString() );
			
			// ******
			RefAssemblies.Add( GetText(reader).Trim() );
		}


		///////////////////////////////////////////////////////////////////////////////
		//
		//private Type GetTypeOfData( string description, StringIndexer reader )
		//{
		//	// ******
		//	string typeName = GetText( reader ).Trim();
		//	if( string.IsNullOrEmpty(typeName) ) {
		//		CSX.WriteWarning( "empty type name processing {0}", description );
		//	}
		//
		//	// ******
		//	int pos = typeName.IndexOf( '.' );
		//	bool fullTypeName = pos >= 0;
		//	if( 0 == pos ) {
		//		//
		//		// if a dot is the first char the intent is that this
		//		// is a full type name search and the type we're looking
		//		// for does NOT live in a namespace
		//		//
		//		typeName = typeName.Substring( 1 );
		//	}
		//
		//	// ******
		//	Type type = TypeLoader.GetType( typeName );
		//	if( null != type ) {
		//		return type;
		//	}
		//
		//	// ******
		//	foreach( var path in RefAssemblies ) {
		//		TypeList list = TypeLoader.GetType( path, typeName, fullTypeName );
		//		if( list.Count > 0 ) {
		//			return list[ 0 ];
		//		}
		//	}
		//
		//	// ******
		//	return null;
		//}
		//

		/////////////////////////////////////////////////////////////////////////////

		private string CleanComments( StringIndexer reader )
		{
			// ******
			if( PreProcess ) {
				//
				// let Nmp remove any single line comments - we leave
				// the end of line behind, Nmp will remove the entire
				// line
				//
				return reader.Remainder;
			}

			// ******
			var sb = new StringBuilder();

			while( ! reader.AtEnd ) {
				//
				// a space or a pair of commas also make it go away
				//
				if( reader.StartsWith(START_SINGLE_LINE_COMMENT) ) {
					ReadToEOL( reader );
					if( CommentsFullRemove ) {
						//
						// remove the whole thing - do not add end of line
						//
						continue;
					}
				}
				else {
					sb.Append( GetText(reader) );
				}

				//
				// GetText() does not return an end-of-line so we need to add it,
				// also we want to leave a blank line where the comment was stripped
				// out so line counts remain the same as the source
				//
				sb.AppendLine();

			}

			// ******
			return sb.ToString();
		}


		/////////////////////////////////////////////////////////////////////////////

		public string GetOptions( string textIn )
		{


/*

	we should split the text into lines and then do our checks

	the caller can call us to reassemble the line


*/

			// ******
			//
			// NOTE: the text assigned to the StringIndexer has NOT been "fixed" as
			// it would be in NMP so we have to deal with "\r\n" instead of just
			// newlines
			//
			StringIndexer reader = new StringIndexer( textIn );

			while( true ) {
				//
				// if the line starts with a dask or hash it goes way!
				//
				//if( SC.DASH == reader.Peek() || SC.HASH == reader.Peek() ) {
				//	ReadToEOL( reader );
				//}

				//
				// a space or a pair of commas also make it go away
				//
				//else if( char.IsWhiteSpace(reader.Peek()) || reader.StartsWith(START_SINGLE_LINE_COMMENT) ) {
				//	ReadToEOL( reader );
				//}

				if( reader.StartsWith( START_SINGLE_LINE_COMMENT ) ) {
					ReadToEOL( reader );
				}



				else if( reader.StartsWith(RAZOR_ASMINC) ) {
					reader.Skip( RAZOR_ASMINC.Length );
					GetAssemblyName( reader );
				}

				else if( reader.StartsWith(RAZOR_ASMINC2) ) {
					reader.Skip( RAZOR_ASMINC2.Length );
					GetAssemblyName( reader );
				}

				else if( reader.StartsWith(KEEP_TEMPS) ) {
					ReadToEOL( reader );
					KeepTempFiles = true;
				}

				else if( reader.StartsWith(RAZOR_DEBUG) ) {
					ReadToEOL( reader );
					Debug = true;
				}

				//else if( reader.StartsWith(ASSERT_DATA_NAME) ) {
				//	ReadToEOL( reader );
				//	AssertDataName = true;
				//}
				//
				//else if( reader.StartsWith(ASSERT_DATA_FULLNAME) ) {
				//	ReadToEOL( reader );
				//	AssertDataFullName = true;
				//}
				//
				//else if( reader.StartsWith(ASSERT_ENUM_NAME) ) {
				//	ReadToEOL( reader );
				//	AssertEnumDataName = true;
				//}
				//
				//else if( reader.StartsWith(ASSERT_ENUM_FULLNAME) ) {
				//	ReadToEOL( reader );
				//	AssertEnumDataFullName = true;
				//}


				//else if( reader.StartsWith(TYPE_OF_DATA) ) {
				//	reader.Next( TYPE_OF_DATA.Length );
				//	TypeOfData = GetTypeOfData( "@TypeOfData", reader );
				//}
				//
				//else if( reader.StartsWith(TYPE_OF_ENUMDATA) ) {
				//	reader.Next( TYPE_OF_ENUMDATA.Length );
				//	TypeOfEnumData = GetTypeOfData( "@TypeOfEnumData", reader );
				//}
				//
				//else if( reader.StartsWith(NO_NULL_DATA) ) {
				//	ReadToEOL( reader );
				//	AllowNullData = false;
				//}
				//
				//else if( reader.StartsWith(NO_NULL_ENUMDATA) ) {
				//	ReadToEOL( reader );
				//	AllowNullEnumData = false;
				//}


				else if( reader.StartsWith(CODE_NAMESPACE) ) {
					reader.Skip( CODE_NAMESPACE.Length );
					Namespace = GetText( reader ).Trim();
				}

				else if( reader.StartsWith(CODE_CLASSNAME) ) {
					reader.Skip( CODE_CLASSNAME.Length );
					ClassName = GetText( reader ).Trim();
				}


				else if( reader.StartsWith(INJECT) ) {
					ReadToEOL( reader );
					Inject = true;
				}
				

				else if( reader.StartsWith(PREPROCESS) ) {
					ReadToEOL( reader );
					PreProcess = true;
				}
				
				else if( reader.StartsWith(POSTPROCESS) ) {
					ReadToEOL( reader );
					PostProcess = true;
				}

				else if( reader.StartsWith(COMMENTS_FULL_REMOVE) ) {
					ReadToEOL( reader );
					CommentsFullRemove = true;
				}
				
				else {
					//return reader.Remainder;
					return CleanComments( reader );
				}
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public static void CallerOptions(	string textIn, 
																			StringBuilder textOut, 
																			List<string> targets, 
																			Action<string, string, StringBuilder> handler
																		)
		{
			// ******
			if( null == handler ) {
				throw new ArgumentNullException( "handler" );
			}

			// ******
			var reader = new StringIndexer( textIn );

			while( ! reader.AtEnd ) {
				foreach( string target in targets ) {
					if( reader.StartsWith(target) ) {
						reader.Skip( target.Length );
						string remainder = GetText( reader ).Trim();
						handler( target, remainder, textOut );
					}
					else {
						if( null != textOut ) {
							textOut.Append( GetText(reader) );
							textOut.Append( SC.NEWLINE );
						}
					}
				}

			}

		}



//		/////////////////////////////////////////////////////////////////////////////
//
//		public static string ProcessText(	string textIn, 
//																			StringBuilder textOut, 
//																			List<string> targets, Action<string, string, StringBuilder> handler
//																		)
//		{
//			// ******
//			var reader = new StringIndexer( textIn );
//
//			while( true ) {
//				foreach( string target in targets ) {
//					if( reader.StartsWith(target) ) {
//						reader.Skip( target.Length );
//						string remainder = GetText( reader ).Trim();
//						if( null != handler ) {
//							handler( target, remainder, textOut );
//						}
//					}
//					else {
//						if( null != textOut ) {
//							textOut.Append( GetText(reader) );
//						}
//					}
//				}
//
//			}
//		}
//
//
//		/////////////////////////////////////////////////////////////////////////////
//
//		public static bool CheckCallerOptions(	StringIndexer reader,
//																						StringBuilder textOut, 
//																						List<string> targets, 
//																						Action<string, string, StringBuilder> handler
//																					)
//		{
//			foreach( string target in targets ) {
//				if( reader.StartsWith(target) ) {
//					reader.Skip( target.Length );
//					string remainder = GetText( reader ).Trim();
//					if( null != handler ) {
//						handler( target, remainder, textOut );
//					}
//				}
//				else {
//					if( null != textOut ) {
//						textOut.Append( GetText(reader) );
//					}
//				}
//			}
//		}
//
//
//		/////////////////////////////////////////////////////////////////////////////
//
//		public string _GetOptions(	string textIn,
//																StringBuilder textOut, 
//																List<string> targets,
//																Action<string, string, StringBuilder> handler
//															)
//		{
//
//			// ******
//			var sb = new StringBuilder();
//			var reader = new StringIndexer( textIn );
//
//			bool checkTopOfFileOptions = true;
//
//			while( true ) {
//				if( checkTopOfFileOptions ) {
//					if( reader.StartsWith( START_SINGLE_LINE_COMMENT ) ) {
//						ReadToEOL( reader );
//					}
//
//					else if( reader.StartsWith(RAZOR_ASMINC) ) {
//						reader.Skip( RAZOR_ASMINC.Length );
//						GetAssemblyName( reader );
//					}
//
//					else if( reader.StartsWith(RAZOR_ASMINC2) ) {
//						reader.Skip( RAZOR_ASMINC2.Length );
//						GetAssemblyName( reader );
//					}
//
//					else if( reader.StartsWith(KEEP_TEMPS) ) {
//						ReadToEOL( reader );
//						KeepTempFiles = true;
//					}
//
//					else if( reader.StartsWith(RAZOR_DEBUG) ) {
//						ReadToEOL( reader );
//						Debug = true;
//					}
//
//					else if( reader.StartsWith(CODE_NAMESPACE) ) {
//						reader.Skip( CODE_NAMESPACE.Length );
//						Namespace = GetText( reader ).Trim();
//					}
//
//					else if( reader.StartsWith(CODE_CLASSNAME) ) {
//						reader.Skip( CODE_CLASSNAME.Length );
//						ClassName = GetText( reader ).Trim();
//					}
//
//					else if( reader.StartsWith(INJECT) ) {
//						ReadToEOL( reader );
//						Inject = true;
//					}
//					
//					else if( reader.StartsWith(PREPROCESS) ) {
//						ReadToEOL( reader );
//						PreProcess = true;
//					}
//					
//					else if( reader.StartsWith(POSTPROCESS) ) {
//						ReadToEOL( reader );
//						PostProcess = true;
//					}
//
//					else if( reader.StartsWith(COMMENTS_FULL_REMOVE) ) {
//						ReadToEOL( reader );
//						CommentsFullRemove = true;
//					}
//					
//					else {
//						checkTopOfFileOptions = false;
//						if( ! CheckCallerOptions(reader, sb, targets, handler) ) {
//							sb.Append( GetText(reader) );
//							sb.Append( SC.NEWLINE );
//						}
//					}
//				}
//
// ***** INCOMPLETE - NEVER FINISHED - MORE THINK THROUGH
//
//				else {
//				}
//			}
//		}
//

		/////////////////////////////////////////////////////////////////////////////

		public RazorOptions()
		{
			KeepTempFiles = false;
			RefAssemblies = new NmpStringList();

			//AllowNullData = true;
			//AllowNullEnumData = true;

			Namespace = string.Empty;
			ClassName = "_" + Guid.NewGuid().ToString().Replace( "-", "_" );


		}


	}


}
