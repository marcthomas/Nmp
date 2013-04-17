#region License
// 
// Author: Joe McLain <nmp.developer@outlook.com>
// Copyright (c) 2013, Joe McLain and Digital Writing
// 
// Licensed under Eclipse Public License, Version 1.0 (EPL-1.0)
// See the file LICENSE.txt for details.
// 
#endregion
// ThreadContext.cs
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Web;

using Nmp;


	/////////////////////////////////////////////////////////////////////////////

	public static partial class ThreadContext {

		public class ThreadData {

			// ******
			public INotifications				Notifications		{ get; set; }

			// ******
			public ThreadContextObjectDictionary		Storage	{ get; set; }


			///////////////////////////////////////////////////////////////////////////

			public ThreadData()
			{
				Storage = new ThreadContextObjectDictionary();
				Storage[ ThreadContext.THREAD_DATA_NAME ] = this;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public static INotifications Notifications
		{
			get {
				return threadData.Notifications;
			}
			set {
				threadData.Notifications = value;
			}
		}

}

