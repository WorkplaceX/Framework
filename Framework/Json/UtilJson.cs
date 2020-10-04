using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Framework.Test")]

namespace Framework.Json
{
    using Framework.DataAccessLayer;
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Text.Json;

    /// <summary>
    /// By default, properties and fields are serialized to jsonSession (for server session store) and jsonClient (for Angular client).
    /// References to ComponentJson are not sent to client by default. But can be enabled exclusively with serialize client attribute.
    /// If ComponentJson.IsHidden is true, it is not sent to jsonClient.
    /// </summary>
    [Flags]
    internal enum SerializeEnum
    {
        /// <summary>
        /// Do not send property or field to client and do not store in session.
        /// </summary>
        None = 0,

        /// <summary>
        /// Store property or field in session.
        /// </summary>
        Session = 1,

        /// <summary>
        /// Send property or field to client. Use for example for render output.
        /// </summary>
        Client = 2,

        /// <summary>
        /// Send property or field to client and do store in session. Attribute can be omitted.
        /// </summary>
        Both = 3,
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    internal class SerializeAttribute : Attribute 
    {
        public SerializeAttribute(SerializeEnum serializeEnum)
        {
            this.SerializeEnum = serializeEnum;
        }

        public readonly SerializeEnum SerializeEnum;
    }

    /// <summary>
    /// Write session and client json. See method Return(); for jsonSession and jsonClient return values.
    /// </summary>
    internal sealed class Writer : IDisposable
    {
        public Writer(JsonWriterOptions options)
        {
            this.streamSession = new MemoryStream();
            this.writerSession = new Utf8JsonWriter(streamSession, options);
            this.streamClient = new MemoryStream();
            this.writerClient = new Utf8JsonWriter(streamClient, options);
        }

        private Stack<(bool? IsSerializeSession, bool? IsSerializeClient)> serializeStack = new Stack<(bool? isSerializeSession, bool? isSerializeClient)>();

        public int SerializeStackCount
        {
            get
            {
                return serializeStack.Count;
            }
        }

        private int? stackRootIndexSession;
        private int? stackRootIndexClient;

        /// <summary>
        /// Make sure max one object is written to root.
        /// </summary>
        public void StackRootValidate()
        {
            if (IsSerializeSession)
            {
                if (stackRootIndexSession == null)
                {
                    stackRootIndexSession = serializeStack.Count - 1;
                }
                else
                {
                    if (stackRootIndexSession.Value == serializeStack.Count - 1)
                    {
                        throw new Exception("JsonSession can only have one ComponentJson graph!");
                    }
                }
            }
            if (IsSerializeClient)
            {
                if (stackRootIndexClient == null)
                {
                    stackRootIndexClient = serializeStack.Count - 1;
                }
                else
                {
                    if (stackRootIndexClient.Value == serializeStack.Count - 1)
                    {
                        throw new Exception("JsonClient can only have one ComponentJson graph!");
                    }
                }
            }
        }

        /// <summary>
        /// Gets IsSerializeSession. If true, writer writes to jsonSession. See also method SerializeStart();
        /// </summary>
        public bool IsSerializeSession { get; private set; }

        /// <summary>
        /// Gets IsSerializeClient. If true, writer writes to jsonClient. See also method SerializeStart();
        /// </summary>
        public bool IsSerializeClient { get; private set; }

        /// <summary>
        /// Configure writer. Method SerializeStart(); has to be followed by method SerializeEnd();
        /// </summary>
        public void SerializeStart(bool? isSerializeSession, bool? isSerializeClient)
        {
            var result = (IsSerializeSession: (bool?)null, IsSerializeClient: (bool?)null);
            if (serializeStack.TryPeek(out var serialize))
            {
                result = serialize;
            }
            if (isSerializeSession != null && result.IsSerializeSession != false)
            {
                result.IsSerializeSession = isSerializeSession;
            }
            if (isSerializeClient != null && result.IsSerializeClient != false)
            {
                result.IsSerializeClient = isSerializeClient;
            }
            serializeStack.Push(result);
            IsSerializeSession = result.IsSerializeSession == true;
            IsSerializeClient = result.IsSerializeClient == true;
        }

        public void SerializeEnd()
        {
            serializeStack.Pop();
            if (serializeStack.TryPeek(out var result))
            {
                IsSerializeSession = result.IsSerializeSession == true;
                IsSerializeClient = result.IsSerializeClient == true;
            }
        }

        private MemoryStream streamSession;

        private readonly Utf8JsonWriter writerSession;

        private MemoryStream streamClient;

        private readonly Utf8JsonWriter writerClient;

        public void WritePropertyName(string propertyName)
        {
            if (IsSerializeSession)
                writerSession.WritePropertyName(propertyName);
            if (IsSerializeClient)
                writerClient.WritePropertyName(propertyName);
        }

        public void WriteNumber(string propertyName, int value)
        {
            if (IsSerializeSession)
                writerSession.WriteNumber(propertyName, value);
            if (IsSerializeClient)
                writerClient.WriteNumber(propertyName, value);
        }

        public void WriteNull(string propertyName)
        {
            if (IsSerializeSession)
                writerSession.WriteNull(propertyName);
            if (IsSerializeClient)
                writerClient.WriteNull(propertyName);
        }

        public void WriteStartObject()
        {
            if (IsSerializeSession)
                writerSession.WriteStartObject();
            if (IsSerializeClient)
                writerClient.WriteStartObject();
        }

        public void WriteStartArray()
        {
            if (IsSerializeSession)
                writerSession.WriteStartArray();
            if (IsSerializeClient)
                writerClient.WriteStartArray();
        }

        public void WriteEndArray()
        {
            if (IsSerializeSession)
                writerSession.WriteEndArray();
            if (IsSerializeClient)
                writerClient.WriteEndArray();
        }

        public void WriteEndObject()
        {
            if (IsSerializeSession)
                writerSession.WriteEndObject();
            if (IsSerializeClient)
                writerClient.WriteEndObject();
        }

        public void WriteNullValue()
        {
            if (IsSerializeSession)
                writerSession.WriteNullValue();
            if (IsSerializeClient)
                writerClient.WriteNullValue();
        }

        public void WriteNumberValue(int value)
        {
            if (IsSerializeSession)
                writerSession.WriteNumberValue(value);
            if (IsSerializeClient)
                writerClient.WriteNumberValue(value);
        }

        internal void WriteNumberValue(double value)
        {
            if (IsSerializeSession)
                writerSession.WriteNumberValue(value);
            if (IsSerializeClient)
                writerClient.WriteNumberValue(value);
        }

        public void WriteStringValue(string value)
        {
            if (IsSerializeSession)
                writerSession.WriteStringValue(value);
            if (IsSerializeClient)
                writerClient.WriteStringValue(value);
        }

        public void WriteBooleanValue(bool value)
        {
            if (IsSerializeSession)
                writerSession.WriteBooleanValue(value);
            if (IsSerializeClient)
                writerClient.WriteBooleanValue(value);
        }

        public void WriteString(string propertyName, string value)
        {
            if (IsSerializeSession)
                writerSession.WriteString(propertyName, value);
            if (IsSerializeClient)
                writerClient.WriteString(propertyName, value);
        }

        internal void Serialize(object value)
        {
            if (IsSerializeSession)
                JsonSerializer.Serialize(writerSession, value, value.GetType());
            if (IsSerializeClient)
                JsonSerializer.Serialize(writerClient, value, value.GetType());
        }

        public void Return(out string jsonSession, out string jsonClient)
        {
            UtilFramework.Assert(serializeStack.Count == 0);
            writerSession.Flush();
            writerClient.Flush();
            jsonSession = Encoding.UTF8.GetString(streamSession.ToArray());
            jsonClient = Encoding.UTF8.GetString(streamClient.ToArray());
        }

        public void Dispose()
        {
            writerClient.Dispose();
            streamClient.Dispose();
            writerSession.Dispose();
            streamSession.Dispose();
        }
    }

    internal static class UtilJson
    {
        /// <summary>
        /// (TypeName, DeclarationObject)
        /// </summary>
        private static readonly ConcurrentDictionary<string, DeclarationObject> declarationObjectList = new ConcurrentDictionary<string, DeclarationObject>();

        private static DeclarationObject DeclarationObjectGet(string typeName)
        {
            return declarationObjectList.GetOrAdd(typeName, (key) =>
            {
                Type type = Type.GetType(typeName);
                bool isComponentJson = UtilFramework.IsSubclassOf(type, typeof(ComponentJson));
                bool isRow = UtilFramework.IsSubclassOf(type, typeof(Row));
                bool isDto = type.Assembly == typeof(UtilFramework).Assembly; // Dto declared in UtilFramework
                isDto = isDto || type.Namespace.StartsWith("Framework.Test"); // Dto declared in Framework.Test
                UtilFramework.Assert(isComponentJson | isRow | isDto);
                return new DeclarationObject(type);
            });
        }

        private static DeclarationObject DeclarationObjectGet(Type type)
        {
            return DeclarationObjectGet(UtilFramework.TypeToName(type, true));
        }

        private class DeclarationObject
        {
            public DeclarationObject(Type type)
            {
                this.Type = type;
                this.TypeName = type.FullName;
                // Property
                foreach (var propertyInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (propertyInfo.CanWrite) // If property has no setter do not add it.
                    {
                        if (SerializeAttribute(propertyInfo.GetCustomAttribute<SerializeAttribute>(), out bool isSerializeSession, out bool isSerializeClient, out bool isAttribute)) // If SerializeEnum.Both, do not add property
                        {
                            DeclarationProperty property = new DeclarationProperty(propertyInfo, isSerializeSession, isSerializeClient, isAttribute);
                            PropertyList.Add(property.PropertyName, property);
                        }
                    }
                }
                // Field
                foreach (var fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (SerializeAttribute(fieldInfo.GetCustomAttribute<SerializeAttribute>(), out bool isSerializeSession, out bool isSerializeClient, out bool isAttribute)) // If SerializeEnum.Both, do not add property
                    {
                        if (fieldInfo.Attributes != FieldAttributes.Private)
                        {
                            DeclarationProperty property = new DeclarationProperty(fieldInfo, isSerializeSession, isSerializeClient, isAttribute);
                            PropertyList.Add(property.PropertyName, property);
                        }
                    }
                }
            }

            private bool SerializeAttribute(SerializeAttribute attribute, out bool isSerializeSession, out bool isSerializeClient, out bool isAttribute)
            {
                SerializeEnum serializeEnum = SerializeEnum.Both;
                isAttribute = false;
                if (attribute != null)
                {
                    isAttribute = true;
                    serializeEnum = attribute.SerializeEnum;
                }
                isSerializeSession = (serializeEnum & SerializeEnum.Session) > 0;
                isSerializeClient = (serializeEnum & SerializeEnum.Client) > 0;
                return (serializeEnum != SerializeEnum.None);
            }

            public readonly Type Type;

            public readonly string TypeName;

            /// <summary>
            /// (PropertyName, DeclarationProperty).
            /// </summary>
            public Dictionary<string, DeclarationProperty> PropertyList = new Dictionary<string, DeclarationProperty>();
        }

        internal class DeclarationProperty
        {
            public DeclarationProperty(PropertyInfo propertyInfo, bool isSerializeSession, bool isSerializeClient, bool isAttribute)
            {
                this.PropertyInfo = propertyInfo;
                this.PropertyName = propertyInfo.Name;
                this.PropertyType = propertyInfo.PropertyType;
                this.IsSerializeSession = isSerializeSession;
                this.IsSerializeClient = isSerializeClient;
                this.IsAttribute = isAttribute;
                this.IsSerializeClientExclusive = IsSerializeClient && IsAttribute;

                Constructor(ref this.PropertyType, ref this.IsList);

                this.Converter = ConverterGet(this.PropertyType);
            }

            public DeclarationProperty(FieldInfo fieldInfo, bool isSerializeSession, bool isSerializeClient, bool isAttribute)
            {
                this.FieldInfo = fieldInfo;
                this.PropertyName = fieldInfo.Name;
                this.PropertyType = fieldInfo.FieldType;
                this.IsSerializeSession = isSerializeSession;
                this.IsSerializeClient = isSerializeClient;
                this.IsAttribute = isAttribute;
                this.IsSerializeClientExclusive = IsSerializeClient && IsAttribute;

                Constructor(ref this.PropertyType, ref this.IsList);
                
                this.Converter = ConverterGet(this.PropertyType);
            }

            private void Constructor(ref Type propertyType, ref bool isList)
            {
                if (propertyType.IsGenericType && (propertyType.GetGenericTypeDefinition() == typeof(List<>) || propertyType.GetGenericTypeDefinition() == typeof(IReadOnlyList<>)))
                {
                    isList = true;
                    propertyType = propertyType.GetGenericArguments()[0];
                }

                if (propertyType.IsArray)
                {
                    isList = true;
                    propertyType = this.PropertyType.GetElementType();
                }
            }

            public readonly FieldInfo FieldInfo;

            public readonly PropertyInfo PropertyInfo;

            public readonly string PropertyName;

            /// <summary>
            /// Gets PropertyType. Declaring type for property and list.
            /// </summary>
            public readonly Type PropertyType;

            public readonly ConverterBase Converter;

            /// <summary>
            /// Gets IsList. Field is either single field or list.
            /// </summary>
            public readonly bool IsList;

            public readonly bool IsSerializeSession;

            public readonly bool IsSerializeClient;

            /// <summary>
            /// Gets IsSerializeClientExclusive. If true, property has client serialization attribute declared exclusively.
            /// </summary>
            public readonly bool IsSerializeClientExclusive;

            /// <summary>
            /// Gets IsAttribute. If true, serialize attribute is explicitly declared on this property.
            /// </summary>
            public readonly bool IsAttribute;

            public object ValueGet(object obj)
            {
                UtilFramework.Assert(this.IsList == false);
                object result;
                if (PropertyInfo != null)
                {
                    result = PropertyInfo.GetValue(obj);
                }
                else
                {
                    result = FieldInfo.GetValue(obj);

                    // TypedReference typedReference = __makeref(obj);
                    // result = FieldInfo.GetValueDirect(typedReference);
                }
                return result;
            }

            public void ValueSet(object obj, object value)
            {
                UtilFramework.Assert(this.IsList == false);
                if (PropertyInfo != null)
                {
                    PropertyInfo.SetValue(obj, value);
                }
                else
                {
                    FieldInfo.SetValue(obj, value);
                }
            }

            public IList ValueListGet(object obj)
            {
                UtilFramework.Assert(this.IsList == true);
                IList result;
                if (PropertyInfo != null)
                {
                    result = (IList)PropertyInfo.GetValue(obj);
                }
                else
                {
                    result = (IList)FieldInfo.GetValue(obj);
                }
                return result;
            }

            public void ValueListSet(object obj, IList valueList)
            {
                UtilFramework.Assert(this.IsList == true);

                if (PropertyInfo?.PropertyType.IsArray == true || FieldInfo?.FieldType.IsArray == true)
                {
                    var valueListArray = Array.CreateInstance(PropertyType, valueList.Count);
                    valueList.CopyTo(valueListArray, 0);
                    valueList = valueListArray;
                }

                if (PropertyInfo != null)
                {
                    PropertyInfo.SetValue(obj, valueList);
                }
                else
                {
                    FieldInfo.SetValue(obj, valueList);
                }
            }
        }

        /// <summary>
        /// (PropertyType, Converter)
        /// </summary>
        private static readonly ConcurrentDictionary<Type, ConverterBase> converterList = new ConcurrentDictionary<Type, ConverterBase>(new KeyValuePair<Type, ConverterBase>[] { 
            // Value types
            new KeyValuePair<Type, ConverterBase>(new ConverterInt().PropertyType, new ConverterInt()),
            new KeyValuePair<Type, ConverterBase>(new ConverterIntNullable().PropertyType, new ConverterIntNullable()),
            new KeyValuePair<Type, ConverterBase>(new ConverterBoolean().PropertyType, new ConverterBoolean()),
            new KeyValuePair<Type, ConverterBase>(new ConverterBooleanNullable().PropertyType, new ConverterBooleanNullable()),
            new KeyValuePair<Type, ConverterBase>(new ConverterDouble().PropertyType, new ConverterDouble()),
            new KeyValuePair<Type, ConverterBase>(new ConverterDoubleNullable().PropertyType, new ConverterDoubleNullable()),
            new KeyValuePair<Type, ConverterBase>(new ConverterDateTime().PropertyType, new ConverterDateTime()),
            new KeyValuePair<Type, ConverterBase>(new ConverterDateTimeNullable().PropertyType, new ConverterDateTimeNullable()),
            new KeyValuePair<Type, ConverterBase>(new ConverterString().PropertyType, new ConverterString()),
            
            // Special types
            new KeyValuePair<Type, ConverterBase>(new ConverterObjectValue().PropertyType, new ConverterObjectValue()), // Value object
            new KeyValuePair<Type, ConverterBase>(new ConverterType().PropertyType, new ConverterType()), // Type
        });

        private static readonly ConverterObjectRoot converterObjectRoot = new ConverterObjectRoot();
        private static readonly ConverterObjectDto converterObjectDto = new ConverterObjectDto();
        private static readonly ConverterObjectRow converterObjectRow = new ConverterObjectRow();
        private static readonly ConverterObjectComponentJson converterObjectComponentJson = new ConverterObjectComponentJson();
        private static readonly ConverterEnum converterEnum = new ConverterEnum();
        private static readonly ConverterEnumNullable converterEnumNullable = new ConverterEnumNullable();

        /// <summary>
        /// (Type, typeof(&lt;PropertyType&gt;))
        /// </summary>
        private static readonly ConcurrentDictionary<Type, Type> TypeGenericList = new ConcurrentDictionary<Type, Type>();

        /// <summary>
        /// Returns Converter.
        /// </summary>
        /// <param name="propertyType">Property type</param>
        private static ConverterBase ConverterGet(Type propertyType)
        {
            if (!converterList.TryGetValue(propertyType, out ConverterBase result)) // Value type
                if (propertyType.IsEnum) // Emum
                    result = converterEnum;
                else
                    if (UtilFramework.TypeUnderlying(propertyType).IsEnum) // EnumNullable
                        result = converterEnumNullable;
                else
                    if (UtilFramework.IsSubclassOf(propertyType, typeof(Row))) // Row
                        result = converterObjectRow;
                else
                    if (UtilFramework.IsSubclassOf(propertyType, typeof(ComponentJson))) // ComponentJson
                        result = converterObjectComponentJson;
                else
                    if (propertyType.Assembly == typeof(UtilFramework).Assembly || propertyType.Namespace.StartsWith("Framework.Test")) // Dto
                        result = converterObjectDto;
            UtilFramework.Assert(result != null, "Type not supported!");
            return result;
        }

        internal abstract class ConverterBase
        {
            public ConverterBase(Type propertyType, Type propertyTypeGeneric, bool isObject, object valueDefault)
            {
                this.PropertyType = propertyType;
                this.propertyTypeGeneric = propertyTypeGeneric;
                this.IsObject = isObject;
                this.ValueDefault = valueDefault;

                UtilFramework.Assert(!(this.PropertyType == null ^ this.propertyTypeGeneric == null));
            }

            public ConverterBase(bool isObject, object valueDefault) 
                : this(null, null, isObject, valueDefault)
            {

            }

            protected virtual bool IsValueDefault(object value)
            {
                return object.Equals(value, ValueDefault);
            }

            protected virtual void SerializeValue(object obj, DeclarationProperty property, object value, Writer writer)
            {
                // writer.WriteStringValue(string.Format("{0}", value));
            }

            protected virtual void SerializeObjectType(DeclarationProperty property, object obj, Writer writer)
            {
                // writer.WriteString("$typeRoot", UtilFramework.TypeToName(obj.GetType()));
            }

            /// <summary>
            /// Returns true if 'property' and 'value' is a reference to a ComponentJson and serializes it to jsonSession.
            /// </summary>
            private bool ReferenceSerialize(object obj, DeclarationProperty property, object value, ref ComponentJson componentJsonRoot, Writer writer)
            {
                bool result = false;
                if (value is ComponentJson valueComponentJson)
                {
                    if (obj is ComponentJson objComponentJson)
                    {
                        if (property.PropertyName == nameof(ComponentJson.List))
                        {
                            // ComponentJson.List
                        }
                        else
                        {
                            // ComponentJson.Property
                            result = true;
                        }
                    }
                    else
                    {
                        if (componentJsonRoot == null)
                        {
                            UtilFramework.Assert(valueComponentJson.Owner == null, "Referenced ComponentJson not root!");
                            componentJsonRoot = valueComponentJson;
                        }
                        else
                        {
                            // Dto referenced ComponentJson in object graph.
                            result = true;
                        }
                    }

                    if (result)
                    {
                        int? id = valueComponentJson.Id;
                        if (valueComponentJson.IsRemoved)
                        {
                            // UtilFramework.Assert(false, string.Format("Reference to removed ComponentJson! ({0}.{1})", UtilFramework.TypeToName(obj.GetType()), property.PropertyName));
                            id = null;
                        }
                        UtilFramework.Assert(valueComponentJson.Root == componentJsonRoot, "Referenced ComponentJson not in same object graph!");
                        result = true;
                        writer.SerializeStart(null, false); // Do not serialize reference to client
                        writer.WriteStartObject();
                        if (id != null)
                        {
                            writer.WriteNumber("$referenceId", id.Value);
                        }
                        else
                        {
                            writer.WriteNull("$referenceId");
                        }
                        writer.WriteEndObject();
                        writer.SerializeEnd();
                    }
                }
                return result;
            }

            private void SerializeObject(object obj, DeclarationProperty property, object value, ComponentJson componentJsonRoot, Writer writer)
            {
                bool isReference = ReferenceSerialize(obj, property, value, ref componentJsonRoot, writer);
                bool isSerializeClientExclusive = (property?.IsSerializeClientExclusive).GetValueOrDefault();
                if (isReference && !isSerializeClientExclusive)
                {
                    return;
                }
                DeclarationObject declarationObject;
                declarationObject = DeclarationObjectGet(value.GetType());
                bool? isSerializeSessionObject = isReference && isSerializeClientExclusive ? false : (bool?)null; // Session reference has already been serialized by method ReferenceSerialize();
                bool? isSerializeClientObject = (bool?)isSerializeClientExclusive | ((value is ComponentJson) ? true : (bool?)null); // Serialize to client if client attribute is declared on property.
                writer.SerializeStart(isSerializeSessionObject, isSerializeClientObject);
                writer.StackRootValidate();

                writer.WriteStartObject();
                SerializeObjectType(property, value, writer);
                foreach (var valueProperty in declarationObject.PropertyList.Values)
                {
                    ConverterBase converter = valueProperty.Converter;
                    bool? isSerializeClient = null;
                    bool? isSerializeSession = null;
                    if (converter is ConverterObjectRow)
                    {
                        isSerializeClient = false; // Do not send data row to client.
                    }
                    if (converter is ConverterObjectComponentJson)
                    {
                        if (!(value is ComponentJson)) // Property references ComponentJson
                        {
                            if (writer.IsSerializeClient && !valueProperty.IsSerializeClientExclusive)
                            {
                                isSerializeClient = false; // Do not send ComponentJson reference to client.
                            }
                        }
                        if (value is ComponentJson && valueProperty.PropertyName != nameof(ComponentJson.List)) // ComponentJson references ComponentJson
                        {
                            isSerializeClient = false; // Do not send ComponentJson reference to client.
                        }
                    }

                    // SerializeAttribute
                    if (valueProperty.IsSerializeSession == false) 
                    {
                        isSerializeSession = false;
                    }
                    if (valueProperty.IsSerializeClient == false)
                    {
                        isSerializeClient = false;
                    }

                    // Serialize property, list
                    if (valueProperty.IsList == false)
                    {
                        // Serialize property
                        object propertyValue = valueProperty.ValueGet(value);
                        if (!converter.IsValueDefault(propertyValue))
                        {
                            if (propertyValue is ComponentJson componentJson && componentJson.IsHide && !valueProperty.IsSerializeClientExclusive)
                            {
                                isSerializeClient = false; // No list entry for hidden object.
                            }
                            writer.SerializeStart(isSerializeSession, isSerializeClient);
                            writer.WritePropertyName(valueProperty.PropertyName);
                            converter.Serialize(value, valueProperty, propertyValue, componentJsonRoot, writer);
                            writer.SerializeEnd();
                        }
                    }
                    else
                    {
                        // Serialize list
                        IList propertyValueList = valueProperty.ValueListGet(value);
                        if (propertyValueList?.Count > 0)
                        {
                            writer.WritePropertyName(valueProperty.PropertyName);
                            writer.WriteStartArray();
                            foreach (var propertyValue in propertyValueList)
                            {
                                var isSerializeClientLocal = isSerializeClient;
                                if (propertyValue is IHide hide && hide.IsHide)
                                {
                                    isSerializeClientLocal = false; // No list entry for hidden object.
                                }
                                writer.SerializeStart(isSerializeSession, isSerializeClientLocal);
                                if (!converter.IsValueDefault(propertyValue))
                                {
                                    converter.Serialize(value, valueProperty, propertyValue, componentJsonRoot, writer);
                                }
                                else
                                {
                                    if (converter.ValueDefault == null)
                                    {
                                        writer.WriteNullValue(); // Serialize null
                                    }
                                    else
                                    {
                                        converter.Serialize(value, valueProperty, propertyValue, componentJsonRoot, writer);
                                    }
                                }
                                writer.SerializeEnd();
                            }
                            writer.WriteEndArray();
                        }
                    }
                }
                writer.WriteEndObject();
                writer.SerializeEnd();
            }

            internal void Serialize(object obj, DeclarationProperty property, object value, ComponentJson componentJsonRoot, Writer writer)
            {
                if (IsObject == false)
                {
                    SerializeValue(obj, property, value, writer);
                }
                else
                {
                    SerializeObject(obj, property, value, componentJsonRoot, writer);
                }
            }

            protected virtual object Utf8JsonReaderDeserializeValue(object obj, DeclarationProperty property, Utf8JsonReader reader)
            {
                throw new NotImplementedException(); // return reader.GetString();
            }

            protected virtual object DeserializeValue(object obj, DeclarationProperty property, JsonElement jsonElement)
            {
                throw new NotImplementedException(); // return jsonElement.GetString();
            }

            protected virtual string DeserializeObjectType(DeclarationProperty property, JsonElement jsonElement)
            {
                // return jsonElement.GetProperty("$typeRoot").GetString();
                return null;
            }

            protected virtual string Utf8JsonReaderDeserializeObjectType(DeclarationProperty property, Utf8JsonReader reader)
            {
                reader.Read();
                UtilFramework.Assert(reader.TokenType == JsonTokenType.PropertyName);
                string propertyName = reader.GetString();
                UtilFramework.Assert(propertyName == "$typeRoot");
                reader.Read();
                UtilFramework.Assert(reader.TokenType == JsonTokenType.String);
                string result = reader.GetString();
                return result;
            }

            private object Utf8JsonReaderDeserializeObject(DeclarationProperty property, Utf8JsonReader reader)
            {
                reader.Read();
                UtilFramework.Assert(reader.TokenType == JsonTokenType.StartObject);
                Type type = PropertyType;
                string typeName = Utf8JsonReaderDeserializeObjectType(property, reader);
                if (typeName != null)
                {
                    type = UtilFramework.TypeFromName(typeName);
                }
                var result = FormatterServices.GetUninitializedObject(type);
                var declarationObject = DeclarationObjectGet(type);
                return result;
            }

            private bool ReferenceDeserialize(object obj, DeclarationProperty property, JsonElement jsonElement, out object result, ComponentJson componentJsonRoot)
            {
                bool resultReturn = false;
                result = null;
                if (jsonElement.TryGetProperty("$referenceId", out JsonElement jsonElementReference))
                {
                    if (jsonElementReference.ValueKind != JsonValueKind.Null)
                    {
                        int id = jsonElementReference.GetInt32();
                        componentJsonRoot.RootReferenceList.Add((obj, property, id)); // Register reference to solve later.
                    }
                    resultReturn = true;
                }
                return resultReturn;
            }

            private void DeserializeObjectComponentJsonConstructor(object obj, DeclarationProperty property, object value)
            {
                if (value is ComponentJson componentJson)
                {
                    ComponentJson owner = null;
                    if (obj is ComponentJson && property.PropertyName == nameof(ComponentJson.List))
                    {
                        owner = (ComponentJson)obj;
                    }
                    componentJson.Constructor(owner, isDeserialize: true);
                }
            }

            private object DeserializeObject(object obj, DeclarationProperty property, JsonElement jsonElement, ComponentJson componentJsonRoot)
            {
                if (ReferenceDeserialize(obj, property, jsonElement, out object result, componentJsonRoot))
                {
                    return result;
                }

                Type type = PropertyType;
                string typeName = DeserializeObjectType(property, jsonElement);
                if (typeName != null)
                {
                    type = UtilFramework.TypeFromName(typeName);
                }
                if (type == null)
                {
                    type = property.PropertyType; // Dto
                }
                result = FormatterServices.GetUninitializedObject(type);
                DeserializeObjectComponentJsonConstructor(obj, property, result);
                bool isReferenceSolve = false;
                if (result is ComponentJson componentJson && componentJson.Owner == null)
                {
                    componentJsonRoot = componentJson;
                    isReferenceSolve = true;
                }

                var declarationObject = DeclarationObjectGet(type);

                // Create empty lists.
                Dictionary<string, IList> valueListList = new Dictionary<string, IList>();
                foreach (var item in declarationObject.PropertyList.Values)
                {
                    if (item.IsList)
                    {
                        Type typeGeneric = item.Converter.PropertyTypeGeneric(item);
                        IList valueList = (IList)Activator.CreateInstance(typeGeneric);
                        valueListList.Add(item.PropertyName, valueList);
                        item.ValueListSet(result, valueList);
                    }
                }

                // Loop through json properties
                foreach (var jsonElementProperty in jsonElement.EnumerateObject())
                {
                    string propertyName = jsonElementProperty.Name;
                    if (declarationObject.PropertyList.TryGetValue(propertyName, out var item)) // Could be "$type"
                    {
                        if (item.IsList == false)
                        {
                            object value = item.Converter.Deserialize(result, item, jsonElementProperty.Value, componentJsonRoot);
                            item.ValueSet(result, value);
                        }
                        else
                        {
                            IList valueList = valueListList[item.PropertyName];
                            foreach (var jsonElementValue in jsonElementProperty.Value.EnumerateArray())
                            {
                                object value = null;
                                if (jsonElementValue.ValueKind == JsonValueKind.Null)
                                {
                                    UtilFramework.Assert(item.Converter.ValueDefault == null);
                                }
                                else
                                {
                                    value = item.Converter.Deserialize(result, item, jsonElementValue, componentJsonRoot);
                                }
                                valueList.Add(value);
                            }
                            item.ValueListSet(result, valueList);
                        }
                    }
                }

                if (result is ComponentJson resultComponentJson)
                {
                    resultComponentJson.Root.RootComponentJsonList.Add(resultComponentJson.Id, resultComponentJson);
                }
                if (isReferenceSolve)
                {
                    componentJsonRoot.RootReferenceSolve();
                }
                return result;
            }

            /// <summary>
            /// Deserialize value or object.
            /// </summary>
            /// <param name="obj">Object on which property is declared.</param>
            /// <param name="property">Property of object <paramref name="obj"/></param>
            internal object Deserialize(object obj, DeclarationProperty property, JsonElement jsonElement, ComponentJson componentJsonRoot)
            {
                if (IsObject == false)
                {
                    return DeserializeValue(obj, property, jsonElement);
                }
                else
                {
                    return DeserializeObject(obj, property, jsonElement, componentJsonRoot);
                }
            }

            internal object Utf8JsonReaderDeserialize(object obj, DeclarationProperty property, Utf8JsonReader reader)
            {
                if (IsObject == false)
                {
                    return Utf8JsonReaderDeserializeValue(obj, property, reader);
                }
                else
                {
                    return Utf8JsonReaderDeserializeObject(property, reader);
                }
            }

            /// <summary>
            /// Gets PropertyType. Declaring type. Can be null for example for Enum, Dto, Row and ComponentJson.
            /// </summary>
            public readonly Type PropertyType;

            /// <summary>
            /// Gets PropertyTypeGeneric. This is typeof(&lt;PropertyType&gt;).
            /// </summary>
            private readonly Type propertyTypeGeneric;

            public Type PropertyTypeGeneric(DeclarationProperty property)
            {
                var result = propertyTypeGeneric;
                if (result == null)
                {
                    result = TypeGenericList.GetOrAdd(property.PropertyType, (Type type) => typeof(List<>).MakeGenericType(property.PropertyType));
                }
                return result;
            }

            /// <summary>
            /// Gets IsObject. If false, it is a value type. If true it is an object.
            /// </summary>
            public readonly bool IsObject;

            /// <summary>
            /// Gets ValueDefault. Used to ignore default values.
            /// </summary>
            public readonly object ValueDefault;
        }

        private abstract class ConverterBase<T> : ConverterBase
        {
            public ConverterBase(bool isObject) 
                : base(typeof(T), typeof(List<T>), isObject, default(T))
            {

            }
        }

        private sealed class ConverterInt : ConverterBase<int>
        {
            public ConverterInt()
                : base(false)
            {

            }

            protected override void SerializeValue(object obj, DeclarationProperty property, object value, Writer writer)
            {
                writer.WriteNumberValue((int)value);
            }

            protected override object DeserializeValue(object obj, DeclarationProperty property, JsonElement jsonElement)
            {
                return jsonElement.GetInt32();
            }
        }

        private sealed class ConverterIntNullable : ConverterBase<int?>
        {
            public ConverterIntNullable()
                : base(false)
            {

            }

            protected override void SerializeValue(object obj, DeclarationProperty property, object value, Writer writer)
            {
                writer.WriteNumberValue((int)value);
            }

            protected override object DeserializeValue(object obj, DeclarationProperty property, JsonElement jsonElement)
            {
                return jsonElement.GetInt32();
            }
        }

        private sealed class ConverterString : ConverterBase<string>
        {
            public ConverterString()
                : base(false)
            {

            }

            protected override void SerializeValue(object obj, DeclarationProperty property, object value, Writer writer)
            {
                writer.WriteStringValue((string)value);
            }

            protected override object DeserializeValue(object obj, DeclarationProperty property, JsonElement jsonElement)
            {
                return jsonElement.GetString();
            }
        }

        private sealed class ConverterBoolean : ConverterBase<bool>
        {
            public ConverterBoolean()
                : base(false)
            {

            }

            protected override void SerializeValue(object obj, DeclarationProperty property, object value, Writer writer)
            {
                writer.WriteBooleanValue((bool)value);
            }

            protected override object DeserializeValue(object obj, DeclarationProperty property, JsonElement jsonElement)
            {
                return jsonElement.GetBoolean();
            }
        }

        private sealed class ConverterBooleanNullable : ConverterBase<bool?>
        {
            public ConverterBooleanNullable()
                : base(false)
            {

            }

            protected override void SerializeValue(object obj, DeclarationProperty property, object value, Writer writer)
            {
                writer.WriteBooleanValue((bool)value);
            }

            protected override object DeserializeValue(object obj, DeclarationProperty property, JsonElement jsonElement)
            {
                return jsonElement.GetBoolean();
            }
        }

        private sealed class ConverterDouble : ConverterBase<double>
        {
            public ConverterDouble() 
                : base(false)
            {

            }

            protected override void SerializeValue(object obj, DeclarationProperty property, object value, Writer writer)
            {
                writer.WriteNumberValue((double)value);
            }

            protected override object DeserializeValue(object obj, DeclarationProperty property, JsonElement jsonElement)
            {
                return jsonElement.GetDouble();
            }
        }

        private sealed class ConverterDoubleNullable : ConverterBase<double?>
        {
            public ConverterDoubleNullable()
                : base(false)
            {

            }

            protected override void SerializeValue(object obj, DeclarationProperty property, object value, Writer writer)
            {
                writer.WriteNumberValue((double)value);
            }

            protected override object DeserializeValue(object obj, DeclarationProperty property, JsonElement jsonElement)
            {
                return jsonElement.GetDouble();
            }
        }

        private sealed class ConverterDateTime : ConverterBase<DateTime>
        {
            public ConverterDateTime()
                : base(false)
            {

            }

            protected override void SerializeValue(object obj, DeclarationProperty property, object value, Writer writer)
            {
                writer.WriteStringValue(UtilFramework.DateTimeToText((DateTime)value));
            }

            protected override object DeserializeValue(object obj, DeclarationProperty property, JsonElement jsonElement)
            {
                return UtilFramework.DateTimeFromText(jsonElement.GetString());
            }
        }

        private sealed class ConverterDateTimeNullable : ConverterBase<DateTime?>
        {
            public ConverterDateTimeNullable()
                : base(false)
            {

            }

            protected override void SerializeValue(object obj, DeclarationProperty property, object value, Writer writer)
            {
                writer.WriteStringValue(UtilFramework.DateTimeToText((DateTime?)value));
            }

            protected override object DeserializeValue(object obj, DeclarationProperty property, JsonElement jsonElement)
            {
                return UtilFramework.DateTimeFromText(jsonElement.GetString());
            }
        }

        private sealed class ConverterType : ConverterBase<Type>
        {
            public ConverterType() 
                : base(false)
            {

            }

            protected override void SerializeValue(object obj, DeclarationProperty property, object value, Writer writer)
            {
                string typeName = UtilFramework.TypeToName((Type)value, true);
                writer.WriteStringValue(typeName);
            }

            protected override object DeserializeValue(object obj, DeclarationProperty property, JsonElement jsonElement)
            {
                string typeName = jsonElement.GetString();
                var result = UtilFramework.TypeFromName(typeName);
                return result;
            }
        }

        private sealed class ConverterObjectRow : ConverterBase
        {
            public ConverterObjectRow() 
                : base(false, null)
            {

            }

            protected override void SerializeValue(object obj, DeclarationProperty property, object value, Writer writer)
            {
                if (writer.IsSerializeClient)
                {
                    throw new Exception("Can not send data row to client!");
                }
                writer.WriteStartObject();
                writer.WriteString("$typeRow", UtilFramework.TypeToName(value.GetType(), true));
                writer.WritePropertyName("Row");
                writer.Serialize(value);
                writer.WriteEndObject();
            }

            protected override object DeserializeValue(object obj, DeclarationProperty property, JsonElement jsonElement)
            {
                string typeRowName = jsonElement.GetProperty("$typeRow").GetString();
                Type typeRow = UtilFramework.TypeFromName(typeRowName);
                string json = jsonElement.GetProperty("Row").GetRawText();
                var result = JsonSerializer.Deserialize(json, typeRow);
                return result;
            }
        }

        private sealed class ConverterObjectComponentJson : ConverterBase
        {
            public ConverterObjectComponentJson() 
                : base(true, null)
            {

            }

            protected override void SerializeObjectType(DeclarationProperty property, object obj, Writer writer)
            {
                writer.SerializeStart(null, false);
                writer.WriteString("$typeComponent", UtilFramework.TypeToName(obj.GetType(), true));
                writer.SerializeEnd();
            }

            protected override string DeserializeObjectType(DeclarationProperty property, JsonElement jsonElement)
            {
                return jsonElement.GetProperty("$typeComponent").GetString();
            }
        }

        /// <summary>
        /// Serialize an object. Property type is object. Supports inheritance.
        /// </summary>
        private sealed class ConverterObjectValue : ConverterBase<object>
        {
            public ConverterObjectValue() 
                : base(false)
            {

            }

            protected override void SerializeValue(object obj, DeclarationProperty property, object value, Writer writer)
            {
                Type propertyType = value.GetType();
                ConverterBase converter = ConverterGet(propertyType);
                UtilFramework.Assert(converter.IsObject == false, "Property of type object needs to store a value type!");
                UtilFramework.Assert(!(converter.GetType() == typeof(ConverterEnum) || converter.GetType() == typeof(ConverterEnumNullable)), "Enum not allowed in property of type object!");

                writer.WriteStartObject();
                writer.WriteString("$typeValue", UtilFramework.TypeToName(propertyType, true));
                writer.WritePropertyName("Value");
                converter.Serialize(obj, property, value, null, writer);
                writer.WriteEndObject();
                if (writer.IsSerializeClient == true)
                {
                    throw new Exception(); // Property of type object not supported for client serialization.
                }
            }

            protected override object DeserializeValue(object obj, DeclarationProperty property, JsonElement jsonElement)
            {
                string typeName = jsonElement.GetProperty("$typeValue").GetString();
                Type type = UtilFramework.TypeFromName(typeName);
                var converter = ConverterGet(type);
                var result = converter.Deserialize(obj, property, jsonElement.GetProperty("Value"), null);
                return result;
            }
        }

        private sealed class ConverterObjectRoot : ConverterBase
        {
            public ConverterObjectRoot() 
                : base(true, null)
            {

            }

            protected override void SerializeObjectType(DeclarationProperty property, object obj, Writer writer)
            {
                writer.SerializeStart(null, false);
                writer.WriteString("$typeRoot", UtilFramework.TypeToName(obj.GetType(), true));
                writer.SerializeEnd();
            }

            protected override string DeserializeObjectType(DeclarationProperty property, JsonElement jsonElement)
            {
                return jsonElement.GetProperty("$typeRoot").GetString();
            }
        }

        /// <summary>
        /// Serialize a dto object. Property type and dto object type need to be identical.
        /// </summary>
        private sealed class ConverterObjectDto : ConverterBase
        {
            public ConverterObjectDto() 
                : base(true, null)
            {

            }

            protected override void SerializeObjectType(DeclarationProperty property, object obj, Writer writer)
            {
                UtilFramework.Assert(property.PropertyType == obj.GetType(), "Property type and object type not equal!");
            }
        }

        private sealed class ConverterEnum : ConverterBase
        {
            public ConverterEnum() 
                : base(false, 0)
            {

            }

            protected override bool IsValueDefault(object value)
            {
                return object.Equals((int)value, ValueDefault);
            }

            protected override void SerializeValue(object obj, DeclarationProperty property, object value, Writer writer)
            {
                writer.WriteNumberValue((int)value);
            }

            protected override object DeserializeValue(object obj, DeclarationProperty property, JsonElement jsonElement)
            {
                var resultInt = jsonElement.GetInt32();
                var resultEnum = Enum.ToObject(property.PropertyType, resultInt);
                return resultEnum;
            }
        }

        private sealed class ConverterEnumNullable: ConverterBase
        {
            public ConverterEnumNullable() 
                : base(false, null)
            {

            }

            protected override void SerializeValue(object obj, DeclarationProperty property, object value, Writer writer)
            {
                writer.WriteNumberValue((int)value);
            }

            protected override object DeserializeValue(object obj, DeclarationProperty property, JsonElement jsonElement)
            {
                var resultInt = jsonElement.GetInt32();
                var resultEnum = Enum.ToObject(UtilFramework.TypeUnderlying(property.PropertyType), resultInt);
                return resultEnum;
            }
        }

        private static readonly JsonWriterOptions options = new JsonWriterOptions { };
        // private static readonly JsonWriterOptions options = new JsonWriterOptions { Indented = true }; // For debug only

        /// <summary>
        /// Serializes public and internal properties and fiels.
        /// </summary>
        /// <param name="obj">Object to serialize.</param>
        /// <param name="json">Json stored in server session.</param>
        /// <param name="jsonClient">Json sent to Angular client.</param>
        public static void Serialize(object obj, out string jsonSession, out string jsonClient)
        {
            using (var writer = new Writer(options))
            {
                if (obj is ComponentJson componentJson && componentJson.IsHide)
                {
                    writer.SerializeStart(true, false); // If root ComponentJson.IsHide = true
                }
                else
                {
                    writer.SerializeStart(true, obj is ComponentJson ? true : (bool?)null);
                }

                converterObjectRoot.Serialize(obj: null, property: null, obj, componentJsonRoot: null, writer);
                writer.SerializeEnd();

                writer.Return(out jsonSession, out jsonClient);
            }

            // DebugValidateJson(obj, json);
        }

        /// <summary>
        /// Validate json by deserializing it and comparing to obj.
        /// </summary>
        public static void DebugValidateJson(object obj, string json)
        {
            // Needs Newtonsoft.Json

            //string jsonSource = Newtonsoft.Json.JsonConvert.SerializeObject(obj, new Newtonsoft.Json.JsonSerializerSettings() { NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore, DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore });
            //object objDest = Deserialize(json);
            //string jsonDest = Newtonsoft.Json.JsonConvert.SerializeObject(objDest, new Newtonsoft.Json.JsonSerializerSettings() { NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore, DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore });
            //UtilFramework.Assert(jsonSource == jsonDest);
        }

        public static void DebugValidateJson(object source, object dest)
        {
            // Needs Newtonsoft.Json

            //string jsonSource = Newtonsoft.Json.JsonConvert.SerializeObject(source, new Newtonsoft.Json.JsonSerializerSettings() { TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All });
            //string jsonDest = Newtonsoft.Json.JsonConvert.SerializeObject(dest, new Newtonsoft.Json.JsonSerializerSettings() { TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All });
            //UtilFramework.Assert(jsonSource == jsonDest);
        }

        /// <summary>
        /// Deserialize server session json.
        /// </summary>
        /// <param name="json">Server session json.</param>
        /// <returns>Returns deserialized session object.</returns>
        public static object Deserialize(string json)
        {
            bool isUtf8JsonReader = false; // Use JsonDocument or Utf8JsonReader

            object result;
            if (isUtf8JsonReader == false)
            {
                JsonDocument document = JsonDocument.Parse(json);
                result = converterObjectRoot.Deserialize(null, null, document.RootElement, null);
            }
            else
            {
                var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
                result = converterObjectRoot.Utf8JsonReaderDeserialize(null, null, reader);
            }

            return result;
        }
    }
}
