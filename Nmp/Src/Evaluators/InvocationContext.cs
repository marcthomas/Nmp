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

namespace NmpEvaluators {


	/////////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Used to push/pop value on InvocationStack
	/// </summary>
	/// 
	class InvocationContext : IDisposable {

		InvocationStack stack;


		/////////////////////////////////////////////////////////////////////////////

		public void Dispose()
		{
			stack.Pop();
		}


		/////////////////////////////////////////////////////////////////////////////

		public InvocationContext Initialize( IMacroInvocationRecord mir )
		{
			stack.Push( mir );
			return this;
		}


		/////////////////////////////////////////////////////////////////////////////
		
		public InvocationContext( InvocationStack stack )
		{
			this.stack = stack;
		}

	}

}