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
using NmpBase;

using Nmp;


//public interface IInvocationInfo {
//
//	protected string GetSourceFile( int depth )
//	public string ParentSourceFile()
//	public string CurrentSourceFile()
//	public string GetSourceFile( IMacroInvocationRecord record )
//	public IMacro GetExecutingMacro()
//	public IMacro GetCallingMacro( IMacroInvocationRecord mir )
//	public IMacro GetCallingMacro()
//
//}


namespace NmpEvaluators {



	/////////////////////////////////////////////////////////////////////////////

	class InvocationStack : NmpStack<IMacroInvocationRecord> {


		/////////////////////////////////////////////////////////////////////////////

		protected string GetSourceFile( int depth )
		{
			// ******
			int foundCount = 0;
			//foreach( IMacroInvocationRecord item in ThreadContext.InvocationStack ) {

			foreach( IMacroInvocationRecord item in this ) {
				string fileName = item.SourceName;

				if( ! string.IsNullOrEmpty(fileName) ) {
					if( depth == ++foundCount ) {
						return fileName;
					}
				}
			}

			// ******
			return string.Empty;
		}


		/////////////////////////////////////////////////////////////////////////////

		public string ParentSourceFile()
		{
			return GetSourceFile( 2 );
		}


		/////////////////////////////////////////////////////////////////////////////

		public string CurrentSourceFile()
		{
			return GetSourceFile( 1 );
		}


		/////////////////////////////////////////////////////////////////////////////

		public string _GetSourceFile( IMacroInvocationRecord record )
		{
			// ******
			int startIndex = FindIndex( r => record == r );

			for( int i = startIndex; i >= 0; i-- ) {
				string fileName = this[ i ].SourceName;

				if( ! string.IsNullOrEmpty(fileName) ) {
					return fileName;
				}
			}

			// ******
			return string.Empty;
		}


		/////////////////////////////////////////////////////////////////////////////

		//
		// macro at the top of the stack, null if its a file executing
		//

		public IMacro GetExecutingMacro()
		{
			var mir = base.Peek();
			return mir.Macro;
		}


		/////////////////////////////////////////////////////////////////////////////

		//
		// gets arbitrary "calling" macro	when provided a mir
		//

		public IMacro GetCallingMacro( IMacroInvocationRecord mir )
		{
			int index = base.IndexOf( mir, 0 );
			return index > 0 ? base[ index - 1 ].Macro : null;
		}


		/////////////////////////////////////////////////////////////////////////////

		//
		// macro at the second position on the stack, null if its a file executing
		//

		public IMacro GetCallingMacro()
		{
			// ******
			IMacroInvocationRecord mir;
			if( base.TryPeek(1, out mir) ) {
				return mir.Macro;
			}

			// ******
			return null;
		}

		
		/////////////////////////////////////////////////////////////////////////////

		public InvocationStack()
		{
		}

	}

}
