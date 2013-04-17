#region License
// 
// Author: Joe McLain <nmp.developer@outlook.com>
// Copyright (c) 2013, Joe McLain and Digital Writing
// 
// Licensed under Eclipse Public License, Version 1.0 (EPL-1.0)
// See the file LICENSE.txt for details.
// 
#endregion
// NmpTuple2.cs
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

[Serializable]
public class NmpTuple<T1, T2> : IStructuralEquatable, IStructuralComparable, IComparable, INmpTuple
{
  // ******
  private readonly T1 m_Item1;
  private readonly T2 m_Item2;



  /////////////////////////////////////////////////////////////////////////////
	
  //[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
  public NmpTuple( T1 item1, T2 item2 )
  {
    this.m_Item1 = item1;
    this.m_Item2 = item2;
  }


  /////////////////////////////////////////////////////////////////////////////

  public override bool Equals(object obj)
  {
    return ((IStructuralEquatable) this).Equals(obj, EqualityComparer<object>.Default);
  }


  /////////////////////////////////////////////////////////////////////////////

  public override int GetHashCode()
  {
    return ((IStructuralEquatable) this).GetHashCode(EqualityComparer<object>.Default);
  }


  /////////////////////////////////////////////////////////////////////////////

  int IStructuralComparable.CompareTo(object other, IComparer comparer)
  {
    if (other == null) {
      return 1;
    }
    NmpTuple<T1, T2> tuple = other as NmpTuple<T1, T2>;
    if (tuple == null) {
      //throw new ArgumentException(Environment.GetResourceString("ArgumentException_TupleIncorrectType", new object[] { base.GetType().ToString() }), "other");
      throw new ArgumentException( "not an NmpTuple" );
    }

    int num = 0;
    num = comparer.Compare(this.m_Item1, tuple.m_Item1);
    if (num != 0) {
      return num;
    }

    return comparer.Compare(this.m_Item2, tuple.m_Item2);
  }


  /////////////////////////////////////////////////////////////////////////////

  bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
  {
    if (other == null) {
      return false;
    }
    NmpTuple<T1, T2> tuple = other as NmpTuple<T1, T2>;
    if (tuple == null) {
      return false;
    }
    return (comparer.Equals(this.m_Item1, tuple.m_Item1) && comparer.Equals(this.m_Item2, tuple.m_Item2));
  }

	
  /////////////////////////////////////////////////////////////////////////////

  int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
  {
    return NmpTuple.CombineHashCodes( comparer.GetHashCode(this.m_Item1), comparer.GetHashCode(this.m_Item2) );
  }

	
  /////////////////////////////////////////////////////////////////////////////

  int IComparable.CompareTo(object obj)
  {
    return ((IStructuralComparable) this).CompareTo(obj, Comparer<object>.Default);
  }


  /////////////////////////////////////////////////////////////////////////////

  int INmpTuple.GetHashCode(IEqualityComparer comparer)
  {
    return ((IStructuralEquatable) this).GetHashCode(comparer);
  }


  /////////////////////////////////////////////////////////////////////////////

  string INmpTuple.ToString(StringBuilder sb)
  {
    sb.Append("(");
    sb.Append(this.m_Item1);  sb.Append(", ");
    sb.Append(this.m_Item2); 
    sb.Append(")");
    return sb.ToString();
  }


  /////////////////////////////////////////////////////////////////////////////

  public override string ToString()
  {
    return ((INmpTuple) this).ToString(new StringBuilder());
  }

  // Properties
  /////////////////////////////////////////////////////////////////////////////

  public T1 Item1
  {
    //[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
    get
    {
      return this.m_Item1;
    }
  }
  /////////////////////////////////////////////////////////////////////////////

  public T2 Item2
  {
    //[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
    get
    {
      return this.m_Item2;
    }
  }


  int INmpTuple.Size
  {
    get
    {
      return 2;
    }
  }
}
