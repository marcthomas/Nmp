#region License
// 
// Author: Joe McLain <nmp.developer@outlook.com>
// Copyright (c) 2013, Joe McLain and Digital Writing
// 
// Licensed under Eclipse Public License, Version 1.0 (EPL-1.0)
// See the file LICENSE.txt for details.
// 
#endregion
// NmpTuple3.cs
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

[Serializable]
public class NmpTuple<T1, T2, T3> : IStructuralEquatable, IStructuralComparable, IComparable, INmpTuple
{
  // ******
  private readonly T1 m_Item1;
  private readonly T2 m_Item2;
  private readonly T3 m_Item3;



  /////////////////////////////////////////////////////////////////////////////
	
  //[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
  public NmpTuple( T1 item1, T2 item2, T3 item3 )
  {
    this.m_Item1 = item1;
    this.m_Item2 = item2;
    this.m_Item3 = item3;
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
    NmpTuple<T1, T2, T3> tuple = other as NmpTuple<T1, T2, T3>;
    if (tuple == null) {
      //throw new ArgumentException(Environment.GetResourceString("ArgumentException_TupleIncorrectType", new object[] { base.GetType().ToString() }), "other");
      throw new ArgumentException( "not an NmpTuple" );
    }

    int num = 0;
    num = comparer.Compare(this.m_Item1, tuple.m_Item1);
    if (num != 0) {
      return num;
    }
    num = comparer.Compare(this.m_Item2, tuple.m_Item2);
    if (num != 0) {
      return num;
    }

    return comparer.Compare(this.m_Item3, tuple.m_Item3);
  }


  /////////////////////////////////////////////////////////////////////////////

  bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
  {
    if (other == null) {
      return false;
    }
    NmpTuple<T1, T2, T3> tuple = other as NmpTuple<T1, T2, T3>;
    if (tuple == null) {
      return false;
    }
    return (comparer.Equals(this.m_Item1, tuple.m_Item1) && comparer.Equals(this.m_Item2, tuple.m_Item2) && comparer.Equals(this.m_Item3, tuple.m_Item3));
  }

	
  /////////////////////////////////////////////////////////////////////////////

  int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
  {
    int hc = comparer.GetHashCode( this.m_Item1 );
    hc = NmpTuple.CombineHashCodes( hc, NmpTuple.CombineHashCodes( comparer.GetHashCode(this.m_Item2), comparer.GetHashCode(this.m_Item3) ));
    return hc;
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
    sb.Append(this.m_Item2);  sb.Append(", ");
    sb.Append(this.m_Item3); 
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
  /////////////////////////////////////////////////////////////////////////////

  public T3 Item3
  {
    //[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
    get
    {
      return this.m_Item3;
    }
  }


  int INmpTuple.Size
  {
    get
    {
      return 3;
    }
  }
}
