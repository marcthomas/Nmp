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
using System.Text.RegularExpressions;


namespace NmpBase {


	/////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Simple class for use in "using" statements where you want to execute
	/// some arbitrary code on object creation and disposal (when the block is
	/// exited)
	/// </summary>

	// ?? DisposeBlock - DisposeHelper ??

	public class UsingHelper : IDisposable {

		//
		// allow derived classes access
		//
		protected Action initAction;
		protected Action disposeAction;


		/////////////////////////////////////////////////////////////////////////////

		public virtual void DoDispose()
		{
			if( null != disposeAction ) {
				disposeAction();
			}
		}


		/////////////////////////////////////////////////////////////////////////////
		
		public virtual void Init()
		{
			if( null != initAction ) {
				initAction();
			}
		}
		

		/////////////////////////////////////////////////////////////////////////////

		public void Dispose()
		{
			DoDispose();
		}
	

		/////////////////////////////////////////////////////////////////////////////

		protected UsingHelper()
		{
		}


		/////////////////////////////////////////////////////////////////////////////

		public UsingHelper( Action dispose = null )
		{
			// ******
			initAction = null;
			disposeAction = dispose;
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="init"> null or an Action to be executed from within the constructor</param>
		/// <param name="dispose">null or an Action to be executed when the object is disposed</param>
		/// <example>
		/// <code>
		/// 
		/// public class ContainerSuppressError : UsingWrapper {
		///		public ContainerSuppressError( IServiceContainer c, bool suppress )
		///		{
		///			bool iv = c.SuppressErrors;
		///			disposeAction = () => c.SuppressErrors = iv;
		///			c.SuppressErrors = suppress;
		///		}
		///	}
		///	
		/// </code>
		/// </example>

		public UsingHelper( Action init, Action dispose )
		{
			// ******
			initAction = init;
			disposeAction = dispose;

			// ******
			Init();
		}
	}


}

