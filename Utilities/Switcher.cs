using System;
using System.Collections.Generic;
using System.Linq;

namespace Utilities {
   /// <summary>
   ///    A Switcher where each case returns an instance of type R.  This is intended as a datatype switcher.  See the example
   ///    in the comments.
   ///    <para>There are some additional features that allow other uses (see comments on the members).</para>
   ///    <para>The basic use is illustrated in the example in the comments.</para>
   /// </summary>
   /// <example>
   ///    <code>
   /// object obj = MethodThatReturnsObjectsOfDifferentTypes();
   /// Switcher&lt;string&gt; switcher = new Switcher&lt;string&gt;().Case&lt;int&gt;(x => { Console.WriteLine("The object is an int."); return x.ToString(); })
   ///                                                   .Case&lt;string&gt;(x => { Console.WriteLine("The object is a string."); return x; });
   ///                                                   .Case&lt;StudentRecord&gt;(x => { Console.WriteLine("The object is a StudentRecord.); return x.FirstName + " " + x.LastName; });
   ///                                                   .Case&lt;Pickle&gt;(x => { Console.WriteLine("The object is a Pickle."; return x.IsDill ? "Disgusting" : "Delicious"; });
   ///                                                   .Default(x => { Console.WriteLine("I don't know what the object is!"); return "unknown"; });
   /// Console.Write("The object info is: " + switcher.Switch(obj));
   /// </code>
   /// </example>
   /// <typeparam name="R">The return type for all cases.</typeparam>
   // Example (without the markup):
   // object obj = MethodThatReturnsObjectsOfDifferentTypes();
   // Switcher<string> switcher = new Switcher<string>().Case<int>(x => { Console.WriteLine("The object is an int."); return x.ToString(); })
   //                                                   .Case<string>(x => { Console.WriteLine("The object is a string."); return x; });
   //                                                   .Case<StudentRecord>(x => { Console.WriteLine("The object is a StudentRecord.); return x.FirstName + " " + x.LastName; });
   //                                                   .Case<Pickle>(x => { Console.WriteLine("The object is a Pickle."; return x.IsDill ? "Disgusting" : "Delicious"; })
   //                                                   .Default(x => { Console.WriteLine("I don't know what the object is!"); return "unknown"; });
   // Console.Write("The object info is: " + switcher.Switch(obj));
   public class Switcher<R> {
      /*----------------------*/
      /* Methods              */
      /*----------------------*/
      /// <summary>
      ///    Adds a case for type T, with a Func that accepts one argument of type T.  When the object passed to Switch is type
      ///    T, the Func will be called with the object cast to T.
      /// </summary>
      public Switcher<R> Case<T>(Func<T, R> action) {
         _cases.Add(typeof (T), x => action((T) x));
         return this;
      }
      /// <summary>
      ///    <para>Adds a case for type T, but the Func passed takes one argument of type OT.  There are two uses for this:</para>
      ///    <para>
      ///       1. You have multiple cases for different derived types, but you want to cast the object to a base type when
      ///       passing it to the Func.
      ///    </para>
      ///    <para>
      ///       2. If you use the version of Switch that takes two arguments, you can specify a Type instance, rather than
      ///       using the actual type of the object passed to Switch.
      ///    </para>
      ///    <para>
      ///       This can be useful to create a different kind of Switcher that takes, say, a string and parses it to
      ///       different types based on what's passed to Switch.
      ///    </para>
      ///    <para>An example of the second usage is in the comments.</para>
      /// </summary>
      /// <typeparam name="T">The type to match on.</typeparam>
      /// <typeparam name="OT">The type of the object to pass to the Func.</typeparam>
      /// <param name="action">The Func to call when this case matches.</param>
      /// <returns>The Switcher for chaining.</returns>
      // Example of alternate usage with this type of case:
      // string str = SomeStringFromSomewhere();
      // Type type = DetermineTypeOfDataInString(str);
      // Switcher<object> switcher = new Switcher<object>().Case<string, string>(x => x) // It's already a string, so just return it.
      //                                                   .Case<int, string>(x => int.Parse(x)) // Parse it to an int.
      //                                                   .Case<bool, string>(x => bool.Parse(x)) // Parse it to a bool.
      //                                                   .Case<StudentRecord, string>(x => new StudentRecord(x)) // Deserialize a StudentRecord.
      //                                                   .Default(x => { throw new Exception("Invalid data type."); });
      // object obj = switcher.Switch(type, str);
      public Switcher<R> Case<T, OT>(Func<OT, R> action) {
         _cases.Add(typeof (T), x => action((OT) x));
         return this;
      }
      ///// <summary>
      ///// Creates a case with the same action as another case (like a fall-through).
      ///// </summary>
      //public Switcher<R> Case<T>(Type t) {
      //   Func<object, R> a = _cases[t];
      //   _cases.Add(typeof (T), a);
      //   return this;
      //}
      public Switcher<R> Default(Func<object, R> action) {
         _default = action;
         return this;
      }
      public R Switch(Type t, object x) {
         // First see if there's a specific case for the object's type.
         if (_cases.ContainsKey(t)) return _cases[t](x);
         // Now see if there's a case for a type this object's type is derived from.
         Type tcontenttype = GetContentTypeOfEnumerableType(t);
         foreach (Type tt in _cases.Keys) {
            Type ttcontenttype = GetContentTypeOfEnumerableType(tt);
            if (t.IsSubclassOf(tt) || t.GetInterfaces().Any(type => type == tt) ||
                (tcontenttype != null && ttcontenttype != null && (tcontenttype == ttcontenttype || tcontenttype.IsSubclassOf(ttcontenttype)))) {
               object o = _cases[tt](x);
               if (o.GetType() != typeof (R) && tcontenttype != null && tcontenttype != o.GetType() && GetContentTypeOfEnumerableType(o.GetType()) != tcontenttype &&
                   GetContentTypeOfEnumerableType(o.GetType()) != ttcontenttype && GetContentTypeOfEnumerableType(o.GetType()) != typeof (string))
                  return default(R);
               return (R) o;
            }
         }
         // Call the default handler, if there is one.
         if (_default != null) return _default(x);
         // Nothing else I can do.  Return null.
         return default(R);
      }
      public R Switch(object x) {
         Type t = x.GetType();
         return Switch(t, x);
      }
      public static Type GetContentTypeOfEnumerableType(Type et) {
         if (et == null) return null;
         Type genericEnumerableInterface = et.GetInterfaces().FirstOrDefault(i => {
                                                                                if (i.IsGenericType) return i.GetGenericTypeDefinition() == typeof (IEnumerable<>);
                                                                                return false;
                                                                             });
         if (genericEnumerableInterface == null && et.IsGenericType && et.GetGenericTypeDefinition() == typeof (IEnumerable<>)) genericEnumerableInterface = et;
         if (genericEnumerableInterface == null) return null;
         Type elementType = genericEnumerableInterface.GetGenericArguments()[0];
         return elementType.IsGenericTypeDefinition && elementType.GetGenericTypeDefinition() == typeof (Nullable<>) ? elementType.GetGenericArguments()[0] : elementType;
      }
      /*----------------------*/
      /* Data                 */
      /*----------------------*/
      /// <summary>
      ///    These are the cases.  They are stored in a dictionary where each Func is keyed by a datatype.
      /// </summary>
      private readonly Dictionary<Type, Func<object, R>> _cases = new Dictionary<Type, Func<object, R>>();
      /// <summary>
      ///    The default Func to call, if no cases match.  This is optional and is created by the Default method.
      /// </summary>
      private Func<object, R> _default;
   }
   ///// <summary>
   ///// This is a type of Switcher that allows you to make switch statements with dynamic cases.
   ///// </summary>
   ///// <typeparam name="R"></typeparam>
   ///// <typeparam name="K"></typeparam>
   //public class Switcher<R, K> {
   //   private readonly Dictionary<K, Func<object, R>> _matches = new Dictionary<K, Func<object, R>>();
   //   private Func<object, R> _default;
   //   public Switcher<R, K> Case(K key, Func<object, R> action) {
   //      _matches.Add(key, action);
   //      return this;
   //   }
   //   public Switcher<R, K> Case<T>(K key, Func<T, R> action) {
   //      _matches.Add(key, x => action((T) x));
   //      return this;
   //   }
   //   /// <summary>
   //   /// Creates a case with the same action as another case (like a fall-through).
   //   /// </summary>
   //   public Switcher<R, K> Case(K key, K dupkey) {
   //      Func<object, R> a = _matches[key];
   //      _matches.Add(dupkey, a);
   //      return this;
   //   }
   //   public Switcher<R, K> Default(Func<object, R> action) {
   //      _default = action;
   //      return this;
   //   }
   //   public R Switch(K key, object obj = null) {
   //      if (_matches.ContainsKey(key)) return _matches[key](obj);
   //      if (_default != null) return _default(obj);
   //      return default(R);
   //   }
   //}
}