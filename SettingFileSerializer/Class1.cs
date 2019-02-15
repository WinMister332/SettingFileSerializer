using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace System.Settings.Serialization
{
    public sealed class FCubedParser
    {
        public static string WriteFCubed(ValueSet<object>[] set)
        {
            string s = "";
            foreach (ValueSet<object> o in set)
            {
                if (o.GetValue().GetType() == typeof(string))
                {
                    if (s == "" || s == null)
                        s = $"{o.GetKey().ToString()}=\"{(string)o.GetValue()}\";";
                    else
                        s += $"\n{o.GetKey().ToString()}=\"{(string)o.GetValue()}\";";
                }
                else if (o.GetValue().GetType() == typeof(char))
                {
                    if (s == "" || s == null)
                        s = $"{o.GetKey().ToLower()}=\'{(char)o.GetValue()}\';";
                    else
                        s += $"\n{o.GetKey().ToLower()}=\'{(char)o.GetValue()}\';";
                }
                else if (o.GetValue().GetType() == typeof(bool))
                {
                    if (s == "" || s == null)
                        s = $"{o.GetKey().ToLower()}=\"{(bool)o.GetValue()}\";";
                    else
                        s += $"\n{o.GetKey().ToLower()}=\"{(bool)o.GetValue()}\";";
                }
                else if (o.GetValue().GetType() == typeof(int))
                {
                    if (s == "" || s == null)
                        s = $"{o.GetKey().ToLower()}={(int)o.GetValue()};";
                    else
                        s += $"\n{o.GetKey().ToLower()}={(int)o.GetValue()};";
                }
                else
                {
                    if (s == "" || s == null)
                        s = $"{o.GetKey().ToLower()}=[NONE];";
                    else
                        s += $"\n{o.GetKey().ToLower()}=[NONE];";
                }


            }
            return s;
        }

        public static ValueSet<object>[] ReadFCubed(string data)
        {
            List<ValueSet<object>> sets = new List<ValueSet<object>>();
            var s = data.Split('\n');
            foreach (string sx in s)
            {
                var x = sx.Split('=');
                //Strip the ending -> ;
                var x1 = x[0];
                var x2 = x[1];
                x2 = x2.Replace(";", "");
                if (x2.StartsWith("\"") && x2.EndsWith("\""))
                {
                    //Bool or String.
                    x2.Replace("\"", "");
                    var b = bool.TryParse(x2, out bool bx);
                    if (b)
                    {
                        sets.Add(new ValueSet<object>(x1, bx));
                    }
                    else
                    {
                        sets.Add(new ValueSet<object>(x1, x2));
                    }
                }
                else if (x2.StartsWith("\'") && x2.EndsWith("\'"))
                {
                    //Char only.
                    //Strip out '.
                    x2.Replace("\'", "");
                    sets.Add(new ValueSet<object>(x1, char.Parse(x2)));
                }
                else
                {
                    //Int
                    var b = int.TryParse(x2, out int i);
                    if (b)
                        sets.Add(new ValueSet<object>(x1, i));
                    else
                        sets.Add(new ValueSet<object>(x1, null));
                }
            }
            return sets.ToArray();
        }

        /* 
         FCubed only supports the following object types:
            - String (string)
            - Chararacter (char)
            - Int32 (int)
            - Boolean (bool)
            - and Null (null) ~> [NONE] or [*nul*]
         */
    }

    public class ValueSet<T>
    {
        private string key = "";
        private T value = default(T);

        public ValueSet(string key, T value)
        {
            if (!(key == "" || key == null))
            {
                this.key = key;
                this.value = value;
            }
        }
    
        public string GetKey() => key;
        public T GetValue() => value;
    }
    public sealed class ArgValueSet<T> : ValueSet<T>
    {
        private ArgSet<object> argSet = new ArgSet<object>();

        public ArgValueSet(string key, T value, ArgSet<object> argSet)
            : base(key, value)
        {
            if (!(argSet == null))
                this.argSet = argSet;
        }

        public ArgValueSet(string key, ArgSet<object> argSet)
            : base(key, default(T))
        {
            if (!(argSet == null))
                this.argSet = argSet;

        }

        public ArgSet<object> GetArgSet() => argSet;

        public bool IsArgSetEmpty() => (argSet.GetArgSet().Length <= 0);
        public bool IsValueEmpty() => (GetValue() == null);

        public A GetValueByArgKey<A>(string key)
        {
            foreach (ValueSet<object> vs in GetArgSet().GetArgSet())
                if (vs.GetKey().Equals(key, StringComparison.CurrentCultureIgnoreCase)
                    && vs.GetValue().GetType() == typeof(A))
                    return (A)vs.GetValue();
            return default(A);

        }
        public bool ContainsArgValue<A>(string key, A o)
        {
            var x = GetArgSet().KeyHasValue(key, o);
            return x;
        }
    }
    public sealed class ArgSet<T>
    {
        ValueSet<T>[] vsa;

        public ArgSet(params ValueSet<T>[] sets)
        {
            if (!(sets == null || sets.Length <= 0 || sets == new ValueSet<T>[0]))
                vsa = sets;
        }

        public ValueSet<T>[] GetArgSet() => vsa;
        public T GetValueByKey(string key)
        {
            foreach (ValueSet<T> vs in GetArgSet())
                if (vs.GetKey().Equals(key, StringComparison.CurrentCultureIgnoreCase))
                    return vs.GetValue();
            return default(T);
        }
        public bool KeyHasValue(string key, T value)
        {
            var tx = GetValueByKey(key);
            if (tx.Equals(value))
                return true;
            return false;
        }
    }

    public static class ClassUtils
    {
        public static T GetAttribute<T>(this object o) where T: Attribute
        {
            var attrib = (T)o.GetType().GetCustomAttributes(typeof(T), true)[0];
            return attrib;
        }
        internal static string ToString<T>(this List<T> tlist)
        {
            return ToString(tlist.ToArray());
        }
        internal static string ToString<T>(this T[] tarr)
        {
            string s = "";
            foreach (T t in tarr)
            {
                if (s == "" || s == null)
                    s = $"{t.ToString()}";
                else
                    s += $", {t.ToString()}";
            }
            return s;
        }
    }
}
