/* * * * *
 * A simple JSON Parser / builder
 * ------------------------------
 * 
 * It mainly has been written as a simple JSON parser. It can build a JSON string
 * from the node-tree, or generate a node tree from any valid JSON string.
 * 
 * If you want to use compression when saving to file / stream / B64 you have to include
 * SharpZipLib ( http://www.icsharpcode.net/opensource/sharpziplib/ ) in your project and
 * define "USE_SharpZipLib" at the top of the file
 * 
 * Written by Bunny83 
 * 2012-06-09
 * 
 * [2012-06-09 First Version]
 * - provides strongly typed node classes and lists / dictionaries
 * - provides easy access to class members / array items / data values
 * - the parser now properly identifies types. So generating JSON with this framework should work.
 * - only double quotes (") are used for quoting strings.
 * - provides "casting" properties to easily convert to / from those types:
 *   int / float / double / bool
 * - provides a common interface for each node so no explicit casting is required.
 * - the parser tries to avoid errors, but if malformed JSON is parsed the result is more or less undefined
 * - It can serialize/deserialize a node tree into/from an experimental compact binary format. It might
 *   be handy if you want to store things in a file and don't want it to be easily modifiable
 * 
 * [2012-12-17 Update]
 * - Added internal JSONLazyCreator class which simplifies the construction of a JSON tree
 *   Now you can simple reference any item that doesn't exist yet and it will return a JSONLazyCreator
 *   The class determines the required type by it's further use, creates the type and removes itself.
 * - Added binary serialization / deserialization.
 * - Added support for BZip2 zipped binary format. Requires the SharpZipLib ( http://www.icsharpcode.net/opensource/sharpziplib/ )
 *   The usage of the SharpZipLib library can be disabled by removing or commenting out the USE_SharpZipLib define at the top
 * - The serializer uses different types when it comes to store the values. Since my data values
 *   are all of type string, the serializer will "try" which format fits best. The order is: int, float, double, bool, string.
 *   It's not the most efficient way but for a moderate amount of data it should work on all platforms.
 * 
 * [2017-03-08 Update]
 * - Optimised parsing by using a StringBuilder for token. This prevents performance issues when large
 *   string data fields are contained in the json data.
 * - Finally refactored the badly named JSONClass into JSONObject.
 * - Replaced the old JSONData class by distict typed classes ( JSONString, JSONNumber, JSONBool, JSONNull ) this
 *   allows to propertly convert the node tree back to json without type information loss. The actual value
 *   parsing now happens at parsing time and not when you actually access one of the casting properties.
 * 
 * [2017-04-11 Update]
 * - Fixed parsing bug where empty string values have been ignored.
 * - Optimised "ToString" by using a StringBuilder internally. This should heavily improve performance for large files
 * - Changed the overload of "ToString(string aIndent)" to "ToString(int aIndent)"
 * 
 * [2017-11-29 Update]
 * - Removed the IEnumerator implementations on JSONArray & JSONObject and replaced it with a common
 *   struct Enumerator in JSONNode that should avoid garbage generation. The enumerator always works
 *   on KeyValuePair<string, JSONNode>, even for JSONArray.
 * - Added two wrapper Enumerators that allows for easy key or value enumeration. A JSONNode now has
 *   a "Keys" and a "Values" enumerable property. Those are also struct enumerators / enumerables
 * - A KeyValuePair<string, JSONNode> can now be implicitly converted into a JSONNode. This allows
 *   a foreach loop over a JSONNode to directly access the values only. Since KeyValuePair as well as
 *   all the Enumerators are structs, no garbage is allocated.
 * - To add Linq support another "LinqEnumerator" is available through the "Linq" property. This
 *   enumerator does implement the generic IEnumerable interface so most Linq extensions can be used
 *   on this enumerable object. This one does allocate memory as it's a wrapper class.
 * - The Escape method now escapes all control characters (# < 32) in strings as uncode characters
 *   (\uXXXX) and if the static bool JSONNode.forceASCII is set to true it will also escape all
 *   characters # > 127. This might be useful if you require an ASCII output. Though keep in mind
 *   when your strings contain many non-ascii characters the strings become much longer (x6) and are
 *   no longer human readable.
 * - The node types JSONObject and JSONArray now have an "Inline" boolean switch which will default to
 *   false. It can be used to serialize this element inline even you serialize with an indented format
 *   This is useful for arrays containing numbers so it doesn't place every number on a new line
 * - Extracted the binary serialization code into a seperate extension file. All classes are now declared
 *   as "partial" so an extension file can even add a new virtual or abstract method / interface to
 *   JSONNode and override it in the concrete type classes. It's of course a hacky approach which is
 *   generally not recommended, but i wanted to keep everything tightly packed.
 * - Added a static CreateOrGet method to the JSONNull class. Since this class is immutable it could
 *   be reused without major problems. If you have a lot null fields in your data it will help reduce
 *   the memory / garbage overhead. I also added a static setting (reuseSameInstance) to JSONNull
 *   (default is true) which will change the behaviour of "CreateOrGet". If you set this to false
 *   CreateOrGet will not reuse the cached instance but instead create a new JSONNull instance each time.
 *   I made the JSONNull constructor private so if you need to create an instance manually use
 *   JSONNull.CreateOrGet()
 * 
 * [2018-01-09 Update]
 * - Changed all double.TryParse and double.ToString uses to use the invariant culture to avoid problems
 *   on systems with a culture that uses a comma as decimal point.
 * 
 * [2018-01-26 Update]
 * - Added AsLong. Note that a JSONNumber is stored as double and can't represent all long values. However
 *   storing it as string would work.
 * - Added static setting "JSONNode.longAsString" which controls the default type that is used by the
 *   LazyCreator when using AsLong
 * 
 * 
 * The MIT License (MIT)
 * 
 * Copyright (c) 2012-2017 Markus Göbel (Bunny83)
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * 
 * * * * */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ThirdParty.SimpleJSON
{
    public enum JSONNodeType
    {
        Array = 1,
        Object = 2,
        String = 3,
        Number = 4,
        NullValue = 5,
        Boolean = 6,
        None = 7,
        Custom = 0xFF,
    }
    public enum JSONTextMode
    {
        Compact,
        Indent
    }

    public abstract partial class JSONNode
    {
        #region Enumerators
        public struct Enumerator
        {
            enum Type { None, Array, Object }

            Type type;
            Dictionary<string, global::ThirdParty.SimpleJSON.JSONNode>.Enumerator m_Object;
            List<global::ThirdParty.SimpleJSON.JSONNode>.Enumerator m_Array;
            public bool IsValid { get { return type != Type.None; } }
            public Enumerator(List<global::ThirdParty.SimpleJSON.JSONNode>.Enumerator aArrayEnum)
            {
                type = Type.Array;
                m_Object = default(Dictionary<string, global::ThirdParty.SimpleJSON.JSONNode>.Enumerator);
                m_Array = aArrayEnum;
            }
            public Enumerator(Dictionary<string, global::ThirdParty.SimpleJSON.JSONNode>.Enumerator aDictEnum)
            {
                type = Type.Object;
                m_Object = aDictEnum;
                m_Array = default(List<global::ThirdParty.SimpleJSON.JSONNode>.Enumerator);
            }
            public KeyValuePair<string, global::ThirdParty.SimpleJSON.JSONNode> Current
            {
                get {
                    if (type == Type.Array)
                        return new KeyValuePair<string, global::ThirdParty.SimpleJSON.JSONNode>(string.Empty, m_Array.Current);
                    else if (type == Type.Object)
                        return m_Object.Current;
                    return new KeyValuePair<string, global::ThirdParty.SimpleJSON.JSONNode>(string.Empty, null);
                }
            }
            public bool MoveNext()
            {
                if (type == Type.Array)
                    return m_Array.MoveNext();
                else if (type == Type.Object)
                    return m_Object.MoveNext();
                return false;
            }
        }
        public struct ValueEnumerator
        {
            Enumerator m_Enumerator;
            public ValueEnumerator(List<global::ThirdParty.SimpleJSON.JSONNode>.Enumerator aArrayEnum) : this(new Enumerator(aArrayEnum)) { }
            public ValueEnumerator(Dictionary<string, global::ThirdParty.SimpleJSON.JSONNode>.Enumerator aDictEnum) : this(new Enumerator(aDictEnum)) { }
            public ValueEnumerator(Enumerator aEnumerator) { m_Enumerator = aEnumerator; }
            public global::ThirdParty.SimpleJSON.JSONNode Current { get { return m_Enumerator.Current.Value; } }
            public bool MoveNext() { return m_Enumerator.MoveNext(); }
            public ValueEnumerator GetEnumerator() { return this; }
        }
        public struct KeyEnumerator
        {
            Enumerator m_Enumerator;
            public KeyEnumerator(List<global::ThirdParty.SimpleJSON.JSONNode>.Enumerator aArrayEnum) : this(new Enumerator(aArrayEnum)) { }
            public KeyEnumerator(Dictionary<string, global::ThirdParty.SimpleJSON.JSONNode>.Enumerator aDictEnum) : this(new Enumerator(aDictEnum)) { }
            public KeyEnumerator(Enumerator aEnumerator) { m_Enumerator = aEnumerator; }
            public global::ThirdParty.SimpleJSON.JSONNode Current { get { return m_Enumerator.Current.Key; } }
            public bool MoveNext() { return m_Enumerator.MoveNext(); }
            public KeyEnumerator GetEnumerator() { return this; }
        }

        public class LinqEnumerator : IEnumerator<KeyValuePair<string, global::ThirdParty.SimpleJSON.JSONNode>>, IEnumerable<KeyValuePair<string, global::ThirdParty.SimpleJSON.JSONNode>>
        {
            global::ThirdParty.SimpleJSON.JSONNode m_Node;
            Enumerator m_Enumerator;
            internal LinqEnumerator(global::ThirdParty.SimpleJSON.JSONNode aNode)
            {
                m_Node = aNode;
                if (m_Node != null)
                    m_Enumerator = m_Node.GetEnumerator();
            }
            public KeyValuePair<string, global::ThirdParty.SimpleJSON.JSONNode> Current { get { return m_Enumerator.Current; } }
            object IEnumerator.Current { get { return m_Enumerator.Current; } }
            public bool MoveNext() { return m_Enumerator.MoveNext(); }

            public void Dispose()
            {
                m_Node = null;
                m_Enumerator = new Enumerator();
            }

            public IEnumerator<KeyValuePair<string, global::ThirdParty.SimpleJSON.JSONNode>> GetEnumerator()
            {
                return new LinqEnumerator(m_Node);
            }

            public void Reset()
            {
                if (m_Node != null)
                    m_Enumerator = m_Node.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new LinqEnumerator(m_Node);
            }
        }

        #endregion Enumerators

        #region common interface

        public static bool forceASCII = false; // Use Unicode by default
        public static bool longAsString = false; // lazy creator creates a JSONString instead of JSONNumber

        public abstract JSONNodeType Tag { get; }

        public virtual global::ThirdParty.SimpleJSON.JSONNode this[int aIndex] { get { return null; } set { } }

        public virtual global::ThirdParty.SimpleJSON.JSONNode this[string aKey] { get { return null; } set { } }

        public virtual string Value { get { return ""; } set { } }

        public virtual int Count { get { return 0; } }

        public virtual bool IsNumber { get { return false; } }
        public virtual bool IsString { get { return false; } }
        public virtual bool IsBoolean { get { return false; } }
        public virtual bool IsNull { get { return false; } }
        public virtual bool IsArray { get { return false; } }
        public virtual bool IsObject { get { return false; } }

        public virtual bool Inline { get { return false; } set { } }

        public virtual void Add(string aKey, global::ThirdParty.SimpleJSON.JSONNode aItem)
        {
        }
        public virtual void Add(global::ThirdParty.SimpleJSON.JSONNode aItem)
        {
            Add("", aItem);
        }

        public virtual global::ThirdParty.SimpleJSON.JSONNode Remove(string aKey)
        {
            return null;
        }

        public virtual global::ThirdParty.SimpleJSON.JSONNode Remove(int aIndex)
        {
            return null;
        }

        public virtual global::ThirdParty.SimpleJSON.JSONNode Remove(global::ThirdParty.SimpleJSON.JSONNode aNode)
        {
            return aNode;
        }

        public virtual IEnumerable<global::ThirdParty.SimpleJSON.JSONNode> Children
        {
            get
            {
                yield break;
            }
        }

        public IEnumerable<global::ThirdParty.SimpleJSON.JSONNode> DeepChildren
        {
            get
            {
                foreach (global::ThirdParty.SimpleJSON.JSONNode C in Children)
                    foreach (global::ThirdParty.SimpleJSON.JSONNode D in C.DeepChildren)
                        yield return D;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            WriteToStringBuilder(sb, 0, 0, JSONTextMode.Compact);
            return sb.ToString();
        }

        public virtual string ToString(int aIndent)
        {
            StringBuilder sb = new StringBuilder();
            WriteToStringBuilder(sb, 0, aIndent, JSONTextMode.Indent);
            return sb.ToString();
        }
        internal abstract void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode);

        public abstract Enumerator GetEnumerator();
        public IEnumerable<KeyValuePair<string, global::ThirdParty.SimpleJSON.JSONNode>> Linq { get { return new LinqEnumerator(this); } }
        public KeyEnumerator Keys { get { return new KeyEnumerator(GetEnumerator()); } }
        public ValueEnumerator Values { get { return new ValueEnumerator(GetEnumerator()); } }

        #endregion common interface

        #region typecasting properties


        public virtual double AsDouble
        {
            get
            {
                double v = 0.0;
                if (double.TryParse(Value,NumberStyles.Float, CultureInfo.InvariantCulture, out v))
                    return v;
                return 0.0;
            }
            set
            {
                Value = value.ToString(CultureInfo.InvariantCulture);
            }
        }

        public virtual int AsInt
        {
            get { return (int)AsDouble; }
            set { AsDouble = value; }
        }

        public virtual float AsFloat
        {
            get { return (float)AsDouble; }
            set { AsDouble = value; }
        }

        public virtual bool AsBool
        {
            get
            {
                bool v = false;
                if (bool.TryParse(Value, out v))
                    return v;
                return !string.IsNullOrEmpty(Value);
            }
            set
            {
                Value = (value) ? "true" : "false";
            }
        }

        public virtual long AsLong
        {
            get
            {
                long val = 0;
                if (long.TryParse(Value, out val))
                    return val;
                return 0L;
            }
            set
            {
                Value = value.ToString();
            }
        }

        public virtual global::ThirdParty.SimpleJSON.JSONArray AsArray
        {
            get
            {
                return this as global::ThirdParty.SimpleJSON.JSONArray;
            }
        }

        public virtual global::ThirdParty.SimpleJSON.JSONObject AsObject
        {
            get
            {
                return this as global::ThirdParty.SimpleJSON.JSONObject;
            }
        }


        #endregion typecasting properties

        #region operators

        public static implicit operator global::ThirdParty.SimpleJSON.JSONNode(string s)
        {
            return new global::ThirdParty.SimpleJSON.JSONString(s);
        }
        public static implicit operator string(global::ThirdParty.SimpleJSON.JSONNode d)
        {
            return (d == null) ? null : d.Value;
        }

        public static implicit operator global::ThirdParty.SimpleJSON.JSONNode(double n)
        {
            return new global::ThirdParty.SimpleJSON.JSONNumber(n);
        }
        public static implicit operator double(global::ThirdParty.SimpleJSON.JSONNode d)
        {
            return (d == null) ? 0 : d.AsDouble;
        }

        public static implicit operator global::ThirdParty.SimpleJSON.JSONNode(float n)
        {
            return new global::ThirdParty.SimpleJSON.JSONNumber(n);
        }
        public static implicit operator float(global::ThirdParty.SimpleJSON.JSONNode d)
        {
            return (d == null) ? 0 : d.AsFloat;
        }

        public static implicit operator global::ThirdParty.SimpleJSON.JSONNode(int n)
        {
            return new global::ThirdParty.SimpleJSON.JSONNumber(n);
        }
        public static implicit operator int(global::ThirdParty.SimpleJSON.JSONNode d)
        {
            return (d == null) ? 0 : d.AsInt;
        }

        public static implicit operator global::ThirdParty.SimpleJSON.JSONNode(long n)
        {
            return new global::ThirdParty.SimpleJSON.JSONNumber(n);
        }
        public static implicit operator long(global::ThirdParty.SimpleJSON.JSONNode d)
        {
            return (d == null) ? 0L : d.AsLong;
        }

        public static implicit operator global::ThirdParty.SimpleJSON.JSONNode(bool b)
        {
            return new global::ThirdParty.SimpleJSON.JSONBool(b);
        }
        public static implicit operator bool(global::ThirdParty.SimpleJSON.JSONNode d)
        {
            return (d == null) ? false : d.AsBool;
        }

        public static implicit operator global::ThirdParty.SimpleJSON.JSONNode(KeyValuePair<string, global::ThirdParty.SimpleJSON.JSONNode> aKeyValue)
        {
            return aKeyValue.Value;
        }

        public static bool operator ==(global::ThirdParty.SimpleJSON.JSONNode a, object b)
        {
            if (ReferenceEquals(a, b))
                return true;
            bool aIsNull = a is global::ThirdParty.SimpleJSON.JSONNull || ReferenceEquals(a, null) || a is global::ThirdParty.SimpleJSON.JSONLazyCreator;
            bool bIsNull = b is global::ThirdParty.SimpleJSON.JSONNull || ReferenceEquals(b, null) || b is global::ThirdParty.SimpleJSON.JSONLazyCreator;
            if (aIsNull && bIsNull)
                return true;
            return !aIsNull && a.Equals(b);
        }

        public static bool operator !=(global::ThirdParty.SimpleJSON.JSONNode a, object b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion operators

        [ThreadStatic]
        static StringBuilder m_EscapeBuilder;
        internal static StringBuilder EscapeBuilder
        {
            get {
                if (m_EscapeBuilder == null)
                    m_EscapeBuilder = new StringBuilder();
                return m_EscapeBuilder;
            }
        }
        internal static string Escape(string aText)
        {
            StringBuilder sb = EscapeBuilder;
            sb.Length = 0;
            if (sb.Capacity < aText.Length + aText.Length / 10)
                sb.Capacity = aText.Length + aText.Length / 10;
            foreach (char c in aText)
            {
                switch (c)
                {
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '\"':
                        sb.Append("\\\"");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    default:
                        if (c < ' ' || (forceASCII && c > 127))
                        {
                            ushort val = c;
                            sb.Append("\\u").Append(val.ToString("X4"));
                        }
                        else
                            sb.Append(c);
                        break;
                }
            }
            string result = sb.ToString();
            sb.Length = 0;
            return result;
        }

        static void ParseElement(global::ThirdParty.SimpleJSON.JSONNode ctx, string token, string tokenName, bool quoted)
        {
            if (quoted)
            {
                ctx.Add(tokenName, token);
                return;
            }
            string tmp = token.ToLower();
            if (tmp == "false" || tmp == "true")
                ctx.Add(tokenName, tmp == "true");
            else if (tmp == "null")
                ctx.Add(tokenName, null);
            else
            {
                double val;
                if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out val))
                    ctx.Add(tokenName, val);
                else
                    ctx.Add(tokenName, token);
            }
        }

        public static global::ThirdParty.SimpleJSON.JSONNode Parse(string aJSON)
        {
            Stack<global::ThirdParty.SimpleJSON.JSONNode> stack = new Stack<global::ThirdParty.SimpleJSON.JSONNode>();
            global::ThirdParty.SimpleJSON.JSONNode ctx = null;
            int i = 0;
            StringBuilder Token = new StringBuilder();
            string TokenName = "";
            bool QuoteMode = false;
            bool TokenIsQuoted = false;
            while (i < aJSON.Length) {
                switch (aJSON[i])
                {
                    case '{':
                        if (QuoteMode)
                        {
                            Token.Append(aJSON[i]);
                            break;
                        }
                        stack.Push(new global::ThirdParty.SimpleJSON.JSONObject());
                        if (ctx != null)
                        {
                            ctx.Add(TokenName, stack.Peek());
                        }
                        TokenName = "";
                        Token.Length = 0;
                        ctx = stack.Peek();
                        break;

                    case '[':
                        if (QuoteMode)
                        {
                            Token.Append(aJSON[i]);
                            break;
                        }

                        stack.Push(new global::ThirdParty.SimpleJSON.JSONArray());
                        if (ctx != null)
                        {
                            ctx.Add(TokenName, stack.Peek());
                        }
                        TokenName = "";
                        Token.Length = 0;
                        ctx = stack.Peek();
                        break;

                    case '}':
                    case ']':
                        if (QuoteMode)
                        {

                            Token.Append(aJSON[i]);
                            break;
                        }
                        if (stack.Count == 0)
                            throw new Exception("JSON Parse: Too many closing brackets");

                        stack.Pop();
                        if (Token.Length > 0 || TokenIsQuoted)
                        {
                            ParseElement(ctx, Token.ToString(), TokenName, TokenIsQuoted);
                            TokenIsQuoted = false;
                        }
                        TokenName = "";
                        Token.Length = 0;
                        if (stack.Count > 0)
                            ctx = stack.Peek();
                        break;

                    case ':':
                        if (QuoteMode)
                        {
                            Token.Append(aJSON[i]);
                            break;
                        }
                        TokenName = Token.ToString();
                        Token.Length = 0;
                        TokenIsQuoted = false;
                        break;

                    case '"':
                        QuoteMode ^= true;
                        TokenIsQuoted |= QuoteMode;
                        break;

                    case ',':
                        if (QuoteMode)
                        {
                            Token.Append(aJSON[i]);
                            break;
                        }
                        if (Token.Length > 0 || TokenIsQuoted)
                        {
                            ParseElement(ctx, Token.ToString(), TokenName, TokenIsQuoted);
                            TokenIsQuoted = false;
                        }
                        TokenName = "";
                        Token.Length = 0;
                        TokenIsQuoted = false;
                        break;

                    case '\r':
                    case '\n':
                        break;

                    case ' ':
                    case '\t':
                        if (QuoteMode)
                            Token.Append(aJSON[i]);
                        break;

                    case '\\':
                        ++i;
                        if (QuoteMode)
                        {
                            char C = aJSON[i];
                            switch (C)
                            {
                                case 't':
                                    Token.Append('\t');
                                    break;
                                case 'r':
                                    Token.Append('\r');
                                    break;
                                case 'n':
                                    Token.Append('\n');
                                    break;
                                case 'b':
                                    Token.Append('\b');
                                    break;
                                case 'f':
                                    Token.Append('\f');
                                    break;
                                case 'u':
                                    {
                                        string s = aJSON.Substring(i + 1, 4);
                                        Token.Append((char)int.Parse(
                                            s,
                                            System.Globalization.NumberStyles.AllowHexSpecifier));
                                        i += 4;
                                        break;
                                    }
                                default:
                                    Token.Append(C);
                                    break;
                            }
                        }
                        break;

                    default:
                        Token.Append(aJSON[i]);
                        break;
                }
                ++i;
            }
            if (QuoteMode)
            {
                throw new Exception("JSON Parse: Quotation marks seems to be messed up.");
            }
            return ctx;
        }

    }
    // End of JSONNode

    public partial class JSONArray : global::ThirdParty.SimpleJSON.JSONNode
    {
        List<global::ThirdParty.SimpleJSON.JSONNode> m_List = new List<global::ThirdParty.SimpleJSON.JSONNode>();
        bool inline = false;
        public override bool Inline
        {
            get { return inline; }
            set { inline = value; }
        }

        public override JSONNodeType Tag { get { return JSONNodeType.Array; } }
        public override bool IsArray { get { return true; } }
        public override Enumerator GetEnumerator() { return new Enumerator(m_List.GetEnumerator()); }

        public override global::ThirdParty.SimpleJSON.JSONNode this[int aIndex]
        {
            get
            {
                if (aIndex < 0 || aIndex >= m_List.Count)
                    return new global::ThirdParty.SimpleJSON.JSONLazyCreator(this);
                return m_List[aIndex];
            }
            set
            {
                if (value == null)
                    value = global::ThirdParty.SimpleJSON.JSONNull.CreateOrGet();
                if (aIndex < 0 || aIndex >= m_List.Count)
                    m_List.Add(value);
                else
                    m_List[aIndex] = value;
            }
        }

        public override global::ThirdParty.SimpleJSON.JSONNode this[string aKey]
        {
            get { return new global::ThirdParty.SimpleJSON.JSONLazyCreator(this); }
            set
            {
                if (value == null)
                    value = global::ThirdParty.SimpleJSON.JSONNull.CreateOrGet();
                m_List.Add(value);
            }
        }

        public override int Count
        {
            get { return m_List.Count; }
        }

        public override void Add(string aKey, global::ThirdParty.SimpleJSON.JSONNode aItem)
        {
            if (aItem == null)
                aItem = global::ThirdParty.SimpleJSON.JSONNull.CreateOrGet();
            m_List.Add(aItem);
        }

        public override global::ThirdParty.SimpleJSON.JSONNode Remove(int aIndex)
        {
            if (aIndex < 0 || aIndex >= m_List.Count)
                return null;
            global::ThirdParty.SimpleJSON.JSONNode tmp = m_List[aIndex];
            m_List.RemoveAt(aIndex);
            return tmp;
        }

        public override global::ThirdParty.SimpleJSON.JSONNode Remove(global::ThirdParty.SimpleJSON.JSONNode aNode)
        {
            m_List.Remove(aNode);
            return aNode;
        }

        public override IEnumerable<global::ThirdParty.SimpleJSON.JSONNode> Children
        {
            get
            {
                foreach (global::ThirdParty.SimpleJSON.JSONNode N in m_List)
                    yield return N;
            }
        }


        internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode)
        {
            aSB.Append('[');
            int count = m_List.Count;
            if (inline)
                aMode = JSONTextMode.Compact;
            for (int i = 0; i < count; i++)
            {
                if (i > 0)
                    aSB.Append(',');
                if (aMode == JSONTextMode.Indent)
                    aSB.AppendLine();

                if (aMode == JSONTextMode.Indent)
                    aSB.Append(' ', aIndent + aIndentInc);
                m_List[i].WriteToStringBuilder(aSB, aIndent + aIndentInc, aIndentInc, aMode);
            }
            if (aMode == JSONTextMode.Indent)
                aSB.AppendLine().Append(' ', aIndent);
            aSB.Append(']');
        }
    }
    // End of JSONArray

    public partial class JSONObject : global::ThirdParty.SimpleJSON.JSONNode
    {
        Dictionary<string, global::ThirdParty.SimpleJSON.JSONNode> m_Dict = new Dictionary<string, global::ThirdParty.SimpleJSON.JSONNode>();

        bool inline = false;
        public override bool Inline
        {
            get { return inline; }
            set { inline = value; }
        }

        public override JSONNodeType Tag { get { return JSONNodeType.Object; } }
        public override bool IsObject { get { return true; } }

        public override Enumerator GetEnumerator() { return new Enumerator(m_Dict.GetEnumerator()); }


        public override global::ThirdParty.SimpleJSON.JSONNode this[string aKey]
        {
            get
            {
                if (m_Dict.ContainsKey(aKey))
                    return m_Dict[aKey];
                else
                    return new global::ThirdParty.SimpleJSON.JSONLazyCreator(this, aKey);
            }
            set
            {
                if (value == null)
                    value = global::ThirdParty.SimpleJSON.JSONNull.CreateOrGet();
                if (m_Dict.ContainsKey(aKey))
                    m_Dict[aKey] = value;
                else
                    m_Dict.Add(aKey, value);
            }
        }

        public override global::ThirdParty.SimpleJSON.JSONNode this[int aIndex]
        {
            get
            {
                if (aIndex < 0 || aIndex >= m_Dict.Count)
                    return null;
                return m_Dict.ElementAt(aIndex).Value;
            }
            set
            {
                if (value == null)
                    value = global::ThirdParty.SimpleJSON.JSONNull.CreateOrGet();
                if (aIndex < 0 || aIndex >= m_Dict.Count)
                    return;
                string key = m_Dict.ElementAt(aIndex).Key;
                m_Dict[key] = value;
            }
        }

        public override int Count
        {
            get { return m_Dict.Count; }
        }

        public override void Add(string aKey, global::ThirdParty.SimpleJSON.JSONNode aItem)
        {
            if (aItem == null)
                aItem = global::ThirdParty.SimpleJSON.JSONNull.CreateOrGet();

            if (!string.IsNullOrEmpty(aKey))
            {
                if (m_Dict.ContainsKey(aKey))
                    m_Dict[aKey] = aItem;
                else
                    m_Dict.Add(aKey, aItem);
            }
            else
                m_Dict.Add(Guid.NewGuid().ToString(), aItem);
        }

        public override global::ThirdParty.SimpleJSON.JSONNode Remove(string aKey)
        {
            if (!m_Dict.ContainsKey(aKey))
                return null;
            global::ThirdParty.SimpleJSON.JSONNode tmp = m_Dict[aKey];
            m_Dict.Remove(aKey);
            return tmp;
        }

        public override global::ThirdParty.SimpleJSON.JSONNode Remove(int aIndex)
        {
            if (aIndex < 0 || aIndex >= m_Dict.Count)
                return null;
            KeyValuePair<string, global::ThirdParty.SimpleJSON.JSONNode> item = m_Dict.ElementAt(aIndex);
            m_Dict.Remove(item.Key);
            return item.Value;
        }

        public override global::ThirdParty.SimpleJSON.JSONNode Remove(global::ThirdParty.SimpleJSON.JSONNode aNode)
        {
            try
            {
                KeyValuePair<string, global::ThirdParty.SimpleJSON.JSONNode> item = m_Dict.Where(k => k.Value == aNode).First();
                m_Dict.Remove(item.Key);
                return aNode;
            }
            catch
            {
                return null;
            }
        }

        public override IEnumerable<global::ThirdParty.SimpleJSON.JSONNode> Children
        {
            get
            {
                foreach (KeyValuePair<string, global::ThirdParty.SimpleJSON.JSONNode> N in m_Dict)
                    yield return N.Value;
            }
        }

        internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode)
        {
            aSB.Append('{');
            bool first = true;
            if (inline)
                aMode = JSONTextMode.Compact;
            foreach (KeyValuePair<string, global::ThirdParty.SimpleJSON.JSONNode> k in m_Dict)
            {
                if (!first)
                    aSB.Append(',');
                first = false;
                if (aMode == JSONTextMode.Indent)
                    aSB.AppendLine();
                if (aMode == JSONTextMode.Indent)
                    aSB.Append(' ', aIndent + aIndentInc);
                aSB.Append('\"').Append(Escape(k.Key)).Append('\"');
                if (aMode == JSONTextMode.Compact)
                    aSB.Append(':');
                else
                    aSB.Append(" : ");
                k.Value.WriteToStringBuilder(aSB, aIndent + aIndentInc, aIndentInc, aMode);
            }
            if (aMode == JSONTextMode.Indent)
                aSB.AppendLine().Append(' ', aIndent);
            aSB.Append('}');
        }

    }
    // End of JSONObject

    public partial class JSONString : global::ThirdParty.SimpleJSON.JSONNode
    {
        string m_Data;

        public override JSONNodeType Tag { get { return JSONNodeType.String; } }
        public override bool IsString { get { return true; } }

        public override Enumerator GetEnumerator() { return new Enumerator(); }


        public override string Value
        {
            get { return m_Data; }
            set
            {
                m_Data = value;
            }
        }

        public JSONString(string aData)
        {
            m_Data = aData;
        }

        internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode)
        {
            aSB.Append('\"').Append(Escape(m_Data)).Append('\"');
        }
        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
                return true;
            string s = obj as string;
            if (s != null)
                return m_Data == s;
            global::ThirdParty.SimpleJSON.JSONString s2 = obj as global::ThirdParty.SimpleJSON.JSONString;
            if (s2 != null)
                return m_Data == s2.m_Data;
            return false;
        }
        public override int GetHashCode()
        {
            return m_Data.GetHashCode();
        }
    }
    // End of JSONString

    public partial class JSONNumber : global::ThirdParty.SimpleJSON.JSONNode
    {
        double m_Data;

        public override JSONNodeType Tag { get { return JSONNodeType.Number; } }
        public override bool IsNumber { get { return true; } }
        public override Enumerator GetEnumerator() { return new Enumerator(); }

        public override string Value
        {
            get { return m_Data.ToString(CultureInfo.InvariantCulture); }
            set
            {
                double v;
                if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out v))
                    m_Data = v;
            }
        }

        public override double AsDouble
        {
            get { return m_Data; }
            set { m_Data = value; }
        }
        public override long AsLong
        {
            get { return (long)m_Data; }
            set { m_Data = value; }
        }

        public JSONNumber(double aData)
        {
            m_Data = aData;
        }

        public JSONNumber(string aData)
        {
            Value = aData;
        }

        internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode)
        {
            aSB.Append(Value);
        }

        static bool IsNumeric(object value)
        {
            return value is int || value is uint
                || value is float || value is double
                || value is decimal
                || value is long || value is ulong
                || value is short || value is ushort
                || value is sbyte || value is byte;
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (base.Equals(obj))
                return true;
            global::ThirdParty.SimpleJSON.JSONNumber s2 = obj as global::ThirdParty.SimpleJSON.JSONNumber;
            if (s2 != null)
                return m_Data == s2.m_Data;
            if (IsNumeric(obj))
                return Convert.ToDouble(obj) == m_Data;
            return false;
        }
        public override int GetHashCode()
        {
            return m_Data.GetHashCode();
        }
    }
    // End of JSONNumber

    public partial class JSONBool : global::ThirdParty.SimpleJSON.JSONNode
    {
        bool m_Data;

        public override JSONNodeType Tag { get { return JSONNodeType.Boolean; } }
        public override bool IsBoolean { get { return true; } }
        public override Enumerator GetEnumerator() { return new Enumerator(); }

        public override string Value
        {
            get { return m_Data.ToString(); }
            set
            {
                bool v;
                if (bool.TryParse(value, out v))
                    m_Data = v;
            }
        }
        public override bool AsBool
        {
            get { return m_Data; }
            set { m_Data = value; }
        }

        public JSONBool(bool aData)
        {
            m_Data = aData;
        }

        public JSONBool(string aData)
        {
            Value = aData;
        }

        internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode)
        {
            aSB.Append((m_Data) ? "true" : "false");
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj is bool)
                return m_Data == (bool)obj;
            return false;
        }
        public override int GetHashCode()
        {
            return m_Data.GetHashCode();
        }
    }
    // End of JSONBool

    public partial class JSONNull : global::ThirdParty.SimpleJSON.JSONNode
    {
        static global::ThirdParty.SimpleJSON.JSONNull m_StaticInstance = new global::ThirdParty.SimpleJSON.JSONNull();
        public static bool reuseSameInstance = true;
        public static global::ThirdParty.SimpleJSON.JSONNull CreateOrGet()
        {
            if (reuseSameInstance)
                return m_StaticInstance;
            return new global::ThirdParty.SimpleJSON.JSONNull();
        }

        JSONNull() { }

        public override JSONNodeType Tag { get { return JSONNodeType.NullValue; } }
        public override bool IsNull { get { return true; } }
        public override Enumerator GetEnumerator() { return new Enumerator(); }

        public override string Value
        {
            get { return "null"; }
            set { }
        }
        public override bool AsBool
        {
            get { return false; }
            set { }
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
                return true;
            return (obj is global::ThirdParty.SimpleJSON.JSONNull);
        }
        public override int GetHashCode()
        {
            return 0;
        }

        internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode)
        {
            aSB.Append("null");
        }
    }
    // End of JSONNull

    internal partial class JSONLazyCreator : global::ThirdParty.SimpleJSON.JSONNode
    {
        global::ThirdParty.SimpleJSON.JSONNode m_Node = null;
        string m_Key = null;
        public override JSONNodeType Tag { get { return JSONNodeType.None; } }
        public override Enumerator GetEnumerator() { return new Enumerator(); }

        public JSONLazyCreator(global::ThirdParty.SimpleJSON.JSONNode aNode)
        {
            m_Node = aNode;
            m_Key = null;
        }

        public JSONLazyCreator(global::ThirdParty.SimpleJSON.JSONNode aNode, string aKey)
        {
            m_Node = aNode;
            m_Key = aKey;
        }

        void Set(global::ThirdParty.SimpleJSON.JSONNode aVal)
        {
            if (m_Key == null)
            {
                m_Node.Add(aVal);
            }
            else
            {
                m_Node.Add(m_Key, aVal);
            }
            m_Node = null; // Be GC friendly.
        }

        public override global::ThirdParty.SimpleJSON.JSONNode this[int aIndex]
        {
            get
            {
                return new global::ThirdParty.SimpleJSON.JSONLazyCreator(this);
            }
            set
            {
                global::ThirdParty.SimpleJSON.JSONArray tmp = new global::ThirdParty.SimpleJSON.JSONArray();
                tmp.Add(value);
                Set(tmp);
            }
        }

        public override global::ThirdParty.SimpleJSON.JSONNode this[string aKey]
        {
            get
            {
                return new global::ThirdParty.SimpleJSON.JSONLazyCreator(this, aKey);
            }
            set
            {
                global::ThirdParty.SimpleJSON.JSONObject tmp = new global::ThirdParty.SimpleJSON.JSONObject();
                tmp.Add(aKey, value);
                Set(tmp);
            }
        }

        public override void Add(global::ThirdParty.SimpleJSON.JSONNode aItem)
        {
            global::ThirdParty.SimpleJSON.JSONArray tmp = new global::ThirdParty.SimpleJSON.JSONArray();
            tmp.Add(aItem);
            Set(tmp);
        }

        public override void Add(string aKey, global::ThirdParty.SimpleJSON.JSONNode aItem)
        {
            global::ThirdParty.SimpleJSON.JSONObject tmp = new global::ThirdParty.SimpleJSON.JSONObject();
            tmp.Add(aKey, aItem);
            Set(tmp);
        }

        public static bool operator ==(global::ThirdParty.SimpleJSON.JSONLazyCreator a, object b)
        {
            if (b == null)
                return true;
            return System.Object.ReferenceEquals(a, b);
        }

        public static bool operator !=(global::ThirdParty.SimpleJSON.JSONLazyCreator a, object b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return true;
            return System.Object.ReferenceEquals(this, obj);
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override int AsInt
        {
            get
            {
                global::ThirdParty.SimpleJSON.JSONNumber tmp = new global::ThirdParty.SimpleJSON.JSONNumber(0);
                Set(tmp);
                return 0;
            }
            set
            {
                global::ThirdParty.SimpleJSON.JSONNumber tmp = new global::ThirdParty.SimpleJSON.JSONNumber(value);
                Set(tmp);
            }
        }

        public override float AsFloat
        {
            get
            {
                global::ThirdParty.SimpleJSON.JSONNumber tmp = new global::ThirdParty.SimpleJSON.JSONNumber(0.0f);
                Set(tmp);
                return 0.0f;
            }
            set
            {
                global::ThirdParty.SimpleJSON.JSONNumber tmp = new global::ThirdParty.SimpleJSON.JSONNumber(value);
                Set(tmp);
            }
        }

        public override double AsDouble
        {
            get
            {
                global::ThirdParty.SimpleJSON.JSONNumber tmp = new global::ThirdParty.SimpleJSON.JSONNumber(0.0);
                Set(tmp);
                return 0.0;
            }
            set
            {
                global::ThirdParty.SimpleJSON.JSONNumber tmp = new global::ThirdParty.SimpleJSON.JSONNumber(value);
                Set(tmp);
            }
        }

        public override long AsLong
        {
            get
            {
                if (longAsString)
                    Set(new global::ThirdParty.SimpleJSON.JSONString("0"));
                else
                    Set(new global::ThirdParty.SimpleJSON.JSONNumber(0.0));
                return 0L;
            }
            set
            {
                if (longAsString)
                    Set(new global::ThirdParty.SimpleJSON.JSONString(value.ToString()));
                else
                    Set(new global::ThirdParty.SimpleJSON.JSONNumber(value));
            }
        }

        public override bool AsBool
        {
            get
            {
                global::ThirdParty.SimpleJSON.JSONBool tmp = new global::ThirdParty.SimpleJSON.JSONBool(false);
                Set(tmp);
                return false;
            }
            set
            {
                global::ThirdParty.SimpleJSON.JSONBool tmp = new global::ThirdParty.SimpleJSON.JSONBool(value);
                Set(tmp);
            }
        }

        public override global::ThirdParty.SimpleJSON.JSONArray AsArray
        {
            get
            {
                global::ThirdParty.SimpleJSON.JSONArray tmp = new global::ThirdParty.SimpleJSON.JSONArray();
                Set(tmp);
                return tmp;
            }
        }

        public override global::ThirdParty.SimpleJSON.JSONObject AsObject
        {
            get
            {
                global::ThirdParty.SimpleJSON.JSONObject tmp = new global::ThirdParty.SimpleJSON.JSONObject();
                Set(tmp);
                return tmp;
            }
        }
        internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode)
        {
            aSB.Append("null");
        }
    }
    // End of JSONLazyCreator

    public static class JSON
    {
        public static global::ThirdParty.SimpleJSON.JSONNode Parse(string aJSON)
        {
            return global::ThirdParty.SimpleJSON.JSONNode.Parse(aJSON);
        }
    }
}
