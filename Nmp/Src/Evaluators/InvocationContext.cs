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

namespace NmpExpressions {


	/////////////////////////////////////////////////////////////////////////////

	//public class InvocationContext : UsingHelper {
	//
	//	IMacroInvocationRecord mir;
	//	//bool doPushPop = true;
	//
	//
	//	/////////////////////////////////////////////////////////////////////////////
	//
	//	public override void DoDispose()
	//	{
	//		//if( doPushPop ) {
	//			ThreadContext.PopInvocationData();
	//		//}
	//	}
	//
	//
	//	/////////////////////////////////////////////////////////////////////////////
	//
	//	public override void Init()
	//	{
	//		//if( doPushPop ) {
	//			ThreadContext.PushInvocationData( mir );
	//		//}
	//	}
	//
	//
	//	/////////////////////////////////////////////////////////////////////////////
	//
	//	public InvocationContext( IMacroInvocationRecord mir )	//, bool doPushPop )
	//	{
	//		this.mir = mir;
	//		//this.doPushPop = doPushPop;
	//		Init();
	//	}
	//
	//}
	//

	class InvocationContext : IDisposable {

		InvocationStack stack;


		/////////////////////////////////////////////////////////////////////////////

		public void Dispose()
		{
			stack.Pop();
		}


		/////////////////////////////////////////////////////////////////////////////

		public InvocationContext Init( IMacroInvocationRecord mir )
		{
			stack.Push( mir );
			return this;
		}


		/////////////////////////////////////////////////////////////////////////////
		
		//
		// should be injected
		// 

		public InvocationContext( InvocationStack stack )
		{
			this.stack = stack;
		}

	}

}