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
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;


#pragma warning disable 414, 219

using NmpBase;
using Nmp;


namespace Nmp.Expressions {


	/////////////////////////////////////////////////////////////////////////////

	interface IStaticStandin {
		
		Type	GetStaticClassType();

	}


	/////////////////////////////////////////////////////////////////////////////
		
	class StaticStandin : IStaticStandin {

		Type type;

		public override string ToString()
		{
			return string.Format( "[Static reference for {0}]", type.FullName );
		}

		public new Type GetType()
		{
			return type;
		}

		public Type GetStaticClassType()
		{
			return type;
		}

		public StaticStandin( Type typeIn )
		{
			type = typeIn;
		}

	}


	/////////////////////////////////////////////////////////////////////////////

	class ObjectInfo {

		// ******
		protected object	_obj;
		protected Type		_objType;
		protected string	_memberName;

		// ******
		protected List<MemberInfo>	_members = new List<MemberInfo>();
		protected MemberTypes				_membersType = MemberTypes.All;
		protected bool							_haveMembers = false;
		protected bool							isIndexer = false;
		protected bool							isStatic = false;

		/////////////////////////////////////////////////////////////////////////////

		public object			Object					{ get { return isStatic ? null : _obj; } }
		public Type				ObjectType			{ get { return _objType; } }
		public string			MemberName			{ get { return _memberName; } }
		
		public List<MemberInfo>	Members			{ get { return _members; } }

		public bool				IsStatic				{ get { return isStatic; } }

		/////////////////////////////////////////////////////////////////////////////

		public PropertyInfo GetPropertyInfo
		{
			get {
				return _members[0] as PropertyInfo;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public FieldInfo GetFieldInfo
		{
			get {
				return _members[0] as FieldInfo;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public T MemberAs<T>() where T : class
		{
			return _members[0] as T;
		}


		/////////////////////////////////////////////////////////////////////////////

		public List<T> MembersAs<T>() where T : class
		{
			var list = new List<T>();
			foreach( MemberInfo mi in _members ) {
				list.Add( mi as T );
			}
			return list;
		}


		/////////////////////////////////////////////////////////////////////////////

		public MemberTypes MembersType
		{
			get {
				//
				// don't call unles HaveMembers is true
				//
				if( MemberTypes.All == _membersType ) {
					Debug.Fail( "ObjectInfo: there a no valid members" );
				}

				// ******
				return _membersType;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public bool HaveMembers
		{
			get {
				return _haveMembers;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public bool IsField
		{
			// ******
			get {
				return _haveMembers && MemberTypes.Field == _membersType;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public bool IsMethod
		{
			// ******
			get {
				return _haveMembers && MemberTypes.Method == _membersType;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public bool IsIndexer
		{
			// ******
			get {
				return _haveMembers && isIndexer;
			}
		}

	
		/////////////////////////////////////////////////////////////////////////////

		public bool IsProperty
		{
			// ******
			get {
				return _haveMembers && MemberTypes.Property == _membersType && ! isIndexer;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public bool IsDelegate
		{
			// ******
			get {
				if( IsField || IsProperty ) {
					object value = GetFieldOrProperty();
					return null != value && value is Delegate;
				}
				return false;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		private List<MemberInfo> PruneMembers( Type objType, List<MemberInfo> members )
		{
			// ******
			//
			// order by most to least derived classes and then pick the member type
			// for the top of the list and remove any other member types that do not
			// match it
			//
			// we do this because at the class level there can be no duplicate member
			// names of different member type; however, using 'new' you can override
			// members from base classes that have differnt types
			//
			// we only want member types of the "topmost" definition
			//
			// ?? is this cheating ??
			//
			members.Sort( (x, y) => x.DeclaringType == y.DeclaringType ? 0 : (x.DeclaringType.IsSubclassOf(y.DeclaringType) ? -1 : 1) );

			var memberType = members[0].MemberType;

			for( int iEntry = members.Count - 1; iEntry >= 1; iEntry-- ) {
				//
				// backward through list remvoving any of a different memberType
				//
				if( memberType != members[iEntry].MemberType ) {
					members.RemoveAt( iEntry );
				}
			}

			// ******
			return members;
		}


		/////////////////////////////////////////////////////////////////////////////

		protected bool GetMembers()
		{
			// ******
			_members.Clear();
			isIndexer = false;

			// ******
			MemberInfo [] membersInfo = _objType.GetMember( _memberName );
			//
			// filter out anything that is not a Field, Method or Property
			//
			foreach( MemberInfo mi in membersInfo ) {
				switch( mi.MemberType ) {
					case MemberTypes.Field:
					case MemberTypes.Method:
					case MemberTypes.Property:
						_members.Add( mi );
						break;
				}
			}

			// ******
			if( 0 == _members.Count ) {
				_haveMembers = false;
				return false;
			}
			
			// ******
			//
			// should all be the same type - I hope
			//
			//_membersType = _members[0].MemberType;
			//
			//if( _members.Count > 1 ) {
			//	foreach( MemberInfo mi in membersInfo ) {
			//		Debug.Assert( _membersType == mi.MemberType, "ObjectInfo: mismatched member types" );
			//	}
			//	if( MemberTypes.Field == _membersType ) {
			//		Debug.Fail( "didn't know you could overload a Field ???" );
			//	}
			//}
			
			if( _members.Count > 1 ) {
				//
				// more than one member
				//
				_membersType = _members[0].MemberType;

				if( MemberTypes.Field == _members[0].MemberType ) {
					Debug.Fail( "didn't know you could overload a Field ???" );
				}

				//for( int iMember = 0; iMember < _members.Count; iMember++ ) {
				//	MemberInfo mi = _members[ iMember ];
				//	
				//	if( _membersType != mi.MemberType ) {
				//		_members = PruneMembers( _objType, _members );
				//		break;
				//	}
				//}

				_members = PruneMembers( _objType, _members );
			}

			_membersType = _members[0].MemberType;


			if( MemberTypes.Property == _membersType ) {
				//
				// under the hood properties are just methods, so we want
				// to replace the contents of _members the MemberInfo instances
				// that reference the properties methods
				//
				for( int iMember = 0; iMember < _members.Count; iMember++ ) {
					//
					// foreach MemberInfo - which we know is a PropertyInfo - we
					// cast to PropertyInfo, get the MethodInfo for the method 
					// that implements the property and then set _members[]
					// with the MethodInfo instance
					//
					// having done this, _members looks as if we'd gotten MemberInfo
					// for: "get_" + memberName
					//
					// of course we could have: membersInfo = _objType.GetMember( "get_" + _memberName )
					// but then we'd have to copy that to the _members
					//
					// this way is quicker
					//
					PropertyInfo pi = _members[iMember] as PropertyInfo;
					_members[iMember] = pi.GetGetMethod();
				}

				isIndexer = MemberAs<MethodInfo>().GetParameters().Length > 0;
			}
			else if( MemberTypes.Method == _membersType && _objType.IsArray && "GetValue" == _memberName) {
				//
				// .net Array
				//
				isIndexer = true;
			}
			
			// ******
			_haveMembers = true;
			return true;
		}


		/////////////////////////////////////////////////////////////////////////////

		public object GetFieldValue()
		{
			return FieldInvoker.Invoke( this );
		}


		/////////////////////////////////////////////////////////////////////////////

		public object GetPropertyValue()
		{
			return PropertyInvoker.Invoke( this );
		}


		/////////////////////////////////////////////////////////////////////////////

		public object GetIndexerValue( object [] args )
		{
			return IndexerInvoker.Invoke( this, args );
		}


		/////////////////////////////////////////////////////////////////////////////

		public object GetMethodValue( object [] args )
		{
			return MethodInvoker.Invoke( this, args );
		}


		/////////////////////////////////////////////////////////////////////////////

		public object GetDelegateValue( object [] args )
		{
			return DelegateInvoker.Invoke( this, args );
		}


		/////////////////////////////////////////////////////////////////////////////

		public object GetFieldOrProperty()
		{
			// ******
			if( IsField ) {
				return GetFieldValue();
			}
			else if( IsProperty ) {
				return GetPropertyValue();
			}
			else {
				return null;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public object GetValue()
		{
			// ******
			if( IsMethod ) {
				//
				// only way to return a method is to wrap it
				//
				return new MethodInvoker( this );
			}

			if( IsIndexer ) {
				//
				// only way to return an indexer is to wrap it
				//
				return new IndexerInvoker( this );
			}

			if( IsField ) {
				//
				// fields have returnable values
				//
				return GetFieldValue();
			}

			if( IsProperty ) {
				//
				// properties (but not indexers) have returnable values
				//
				return GetPropertyValue();
			}

			//
			// a delegate is stored as a field or property and will
			// be handled by returning that
			//
			//if( IsDelegate ) {
			//	//
			//	// only way to return a method is to wrap it
			//	//
			//	return new DelegateInvoker( this );
			//}

			// ******
			return null;
		}


		/////////////////////////////////////////////////////////////////////////////

		public ObjectInfo( object obj, string memberName )
		{
			// ******
			_obj = obj;
			_memberName = memberName;

			var ss = obj as IStaticStandin;
			if( null != ss ) {
				//
				// IStaticStandin represents a static class or a class that
				// has static members that we want to access
				//
				_objType = ss.GetStaticClassType();
				isStatic = true;
			}
			else {
				_objType = obj.GetType();
				isStatic = false;
			}

			// ******
			GetMembers();
		}


		/////////////////////////////////////////////////////////////////////////////

		/// exists so we can support IStaticStandin

		public static string GetTypeName( object obj )
		{
			// ******
			var ss = obj as IStaticStandin;
			if( null != ss ) {
				//
				// IStaticStandin represents a static class or a class that
				// has static members that we want to access
				//
				return ss.GetStaticClassType().Name;
			}
			else {
				return obj.GetType().Name;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		/// exists so we can support IStaticStandin

		public static Type GetObjectType( object obj )
		{
			var ss = obj as IStaticStandin;
			if( null != ss ) {
				//
				// IStaticStandin represents a static class or a class that
				// has static members that we want to access
				//
				return ss.GetStaticClassType();
			}
			else {
				return obj.GetType();
			}
		}


	}


}
