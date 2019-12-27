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

    internal class SerializeIgnoreAttribute : Attribute { }

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
                    if (propertyInfo.GetCustomAttribute<SerializeIgnoreAttribute>() == null)
                    {
                        DeclarationProperty property = new DeclarationProperty(propertyInfo);
                        PropertyList.Add(property.PropertyName, property);
                    }
                }
                // Field
                foreach (var fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (fieldInfo.GetCustomAttribute<SerializeIgnoreAttribute>() == null)
                    {
                        if (fieldInfo.Attributes != FieldAttributes.Private)
                        {
                            DeclarationProperty property = new DeclarationProperty(fieldInfo);
                            PropertyList.Add(property.PropertyName, property);
                        }
                    }
                }
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
            public DeclarationProperty(PropertyInfo propertyInfo)
            {
                this.PropertyInfo = propertyInfo;
                this.PropertyName = propertyInfo.Name;
                this.PropertyType = propertyInfo.PropertyType;

                Constructor(ref this.PropertyType, ref this.IsList);

                this.Converter = ConverterGet(this.PropertyType);
            }

            public DeclarationProperty(FieldInfo fieldInfo)
            {
                this.FieldInfo = fieldInfo;
                this.PropertyName = fieldInfo.Name;
                this.PropertyType = fieldInfo.FieldType;

                Constructor(ref this.PropertyType, ref this.IsList);
                
                this.Converter = ConverterGet(this.PropertyType);
            }

            private void Constructor(ref Type propertyType, ref bool isList)
            {
                if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(List<>))
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
            new KeyValuePair<Type, ConverterBase>(new ConverterDouble().PropertyType, new ConverterBoolean()),
            new KeyValuePair<Type, ConverterBase>(new ConverterDoubleNullable().PropertyType, new ConverterDoubleNullable()),
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

            protected virtual void SerializeValue(object obj, DeclarationProperty property, object value, Utf8JsonWriter writer, Utf8JsonWriter writerClient, bool? isWriteClient)
            {
                // writer.WriteStringValue(string.Format("{0}", value));
            }

            protected virtual void SerializeObjectType(DeclarationProperty property, object obj, Utf8JsonWriter writer, Utf8JsonWriter writerClient, bool? isWriteClient)
            {
                // writer.WriteString("$typeRoot", UtilFramework.TypeToName(obj.GetType()));
            }

            private bool ReferenceSerialize(object obj, DeclarationProperty property, object value, ref ComponentJson componentJsonRoot, Utf8JsonWriter writer, Utf8JsonWriter writerClient, bool? isWriteClient)
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
                            // Dto referenced ComponentJson in object gwith.
                            result = true;
                        }
                    }

                    if (result)
                    {
                        UtilFramework.Assert(valueComponentJson.Root == componentJsonRoot, "Referenced ComponentJson not in same object graph!");
                        result = true;
                        writer.WriteStartObject();
                        writer.WriteNumber("$referenceId", valueComponentJson.Id);
                        writer.WriteEndObject();
                        if (isWriteClient == true)
                        {
                            // throw new Exception(); // Do not send reference to client.
                            writerClient.WriteNullValue(); // TODO Check if reference, before calling serialize.
                        }
                    }
                }
                return result;
            }

            private void SerializeObject(object obj, DeclarationProperty property, object value, ComponentJson componentJsonRoot, Utf8JsonWriter writer, Utf8JsonWriter writerClient, bool? isWriteClient)
            {
                if (ReferenceSerialize(obj, property, value, ref componentJsonRoot, writer, writerClient, isWriteClient))
                {
                    return;
                }
                DeclarationObject declarationObject;
                declarationObject = DeclarationObjectGet(value.GetType());
                if (isWriteClient == null && value is ComponentJson)
                {
                    isWriteClient = true;
                }
                writer.WriteStartObject();
                if (isWriteClient == true)
                {
                    writerClient.WriteStartObject();
                }
                SerializeObjectType(property, value, writer, writerClient, isWriteClient);
                foreach (var valueProperty in declarationObject.PropertyList.Values)
                {
                    if (valueProperty.IsList == false)
                    {
                        object propertyValue = valueProperty.ValueGet(value);
                        ConverterBase converter = valueProperty.Converter;
                        if (!converter.IsValueDefault(propertyValue))
                        {
                            writer.WritePropertyName(valueProperty.PropertyName);
                            if (isWriteClient == true)
                            {
                                writerClient.WritePropertyName(valueProperty.PropertyName);
                            }
                            converter.Serialize(value, valueProperty, propertyValue, componentJsonRoot, writer, writerClient, isWriteClient);
                        }
                    }
                    else
                    {
                        IList propertyValueList = valueProperty.ValueListGet(value);
                        if (propertyValueList?.Count > 0)
                        {
                            writer.WritePropertyName(valueProperty.PropertyName);
                            if (isWriteClient == true)
                            {
                                writerClient.WritePropertyName(valueProperty.PropertyName);
                            }
                            ConverterBase converter = valueProperty.Converter;
                            writer.WriteStartArray();
                            if (isWriteClient == true)
                            {
                                writerClient.WriteStartArray();
                            }
                            foreach (var propertyValue in propertyValueList)
                            {
                                if (!converter.IsValueDefault(propertyValue))
                                {
                                    converter.Serialize(value, valueProperty, propertyValue, componentJsonRoot, writer, writerClient, isWriteClient);
                                }
                                else
                                {
                                    if (converter.ValueDefault == null)
                                    {
                                        writer.WriteNullValue(); // Serialize null
                                        if (isWriteClient == true)
                                        {
                                            writerClient.WriteNullValue(); // Serialize null
                                        }
                                    }
                                    else
                                    {
                                        converter.Serialize(value, valueProperty, propertyValue, componentJsonRoot, writer, writerClient, isWriteClient);
                                    }
                                }
                            }
                            writer.WriteEndArray();
                            if (isWriteClient == true)
                            {
                                writerClient.WriteEndArray();
                            }
                        }
                    }
                }
                writer.WriteEndObject();
                if (isWriteClient == true)
                {
                    writerClient.WriteEndObject();
                }
            }

            internal void Serialize(object obj, DeclarationProperty property, object value, ComponentJson componentJsonRoot, Utf8JsonWriter writer, Utf8JsonWriter writerClient, bool? isWriteClient)
            {
                if (IsObject == false)
                {
                    SerializeValue(obj, property, value, writer, writerClient, isWriteClient);
                }
                else
                {
                    SerializeObject(obj, property, value, componentJsonRoot, writer, writerClient, isWriteClient);
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
                    int id = jsonElementReference.GetInt32();
                    componentJsonRoot.RootReferenceList.Add((obj, property, id)); // Register reference to solve later.
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

            protected override void SerializeValue(object obj, DeclarationProperty property, object value, Utf8JsonWriter writer, Utf8JsonWriter writerClient, bool? isWriteClient)
            {
                writer.WriteNumberValue((int)value);
                if (isWriteClient == true)
                {
                    writerClient.WriteNumberValue((int)value);
                }
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

            protected override void SerializeValue(object obj, DeclarationProperty property, object value, Utf8JsonWriter writer, Utf8JsonWriter writerClient, bool? isWriteClient)
            {
                writer.WriteNumberValue((int)value);
                if (isWriteClient == true)
                {
                    writerClient.WriteNumberValue((int)value);
                }
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

            protected override void SerializeValue(object obj, DeclarationProperty property, object value, Utf8JsonWriter writer, Utf8JsonWriter writerClient, bool? isWriteClient)
            {
                writer.WriteStringValue((string)value);
                if (isWriteClient == true)
                {
                    writerClient.WriteStringValue((string)value);
                }
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

            protected override void SerializeValue(object obj, DeclarationProperty property, object value, Utf8JsonWriter writer, Utf8JsonWriter writerClient, bool? isWriteClient)
            {
                writer.WriteBooleanValue((bool)value);
                if (isWriteClient == true)
                {
                    writerClient.WriteBooleanValue((bool)value);
                }
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

            protected override void SerializeValue(object obj, DeclarationProperty property, object value, Utf8JsonWriter writer, Utf8JsonWriter writerClient, bool? isWriteClient)
            {
                writer.WriteBooleanValue((bool)value);
                if (isWriteClient == true)
                {
                    writerClient.WriteBooleanValue((bool)value);
                }
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

            protected override void SerializeValue(object obj, DeclarationProperty property, object value, Utf8JsonWriter writer, Utf8JsonWriter writerClient, bool? isWriteClient)
            {
                writer.WriteNumberValue((double)value);
                if (isWriteClient == true)
                {
                    writerClient.WriteNumberValue((double)value);
                }
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

            protected override void SerializeValue(object obj, DeclarationProperty property, object value, Utf8JsonWriter writer, Utf8JsonWriter writerClient, bool? isWriteClient)
            {
                writer.WriteNumberValue((double)value);
                if (isWriteClient == true)
                {
                    writerClient.WriteNumberValue((double)value);
                }
            }

            protected override object DeserializeValue(object obj, DeclarationProperty property, JsonElement jsonElement)
            {
                return jsonElement.GetDouble();
            }
        }

        private sealed class ConverterType : ConverterBase<Type>
        {
            public ConverterType() 
                : base(false)
            {

            }

            protected override void SerializeValue(object obj, DeclarationProperty property, object value, Utf8JsonWriter writer, Utf8JsonWriter writerClient, bool? isWriteClient)
            {
                string typeName = UtilFramework.TypeToName((Type)value, true);
                writer.WriteStringValue(typeName);
                if (isWriteClient == true)
                {
                    writerClient.WriteStringValue(typeName);
                }
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

            protected override void SerializeValue(object obj, DeclarationProperty property, object value, Utf8JsonWriter writer, Utf8JsonWriter writerClient, bool? isWriteClient)
            {
                writer.WriteStartObject();
                writer.WriteString("$typeRow", UtilFramework.TypeToName(value.GetType(), true));
                writer.WritePropertyName("Row");
                JsonSerializer.Serialize(writer, value, value.GetType());
                writer.WriteEndObject();
                if (isWriteClient == true)
                {
                    throw new Exception(); // Do not send row to client
                }
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

            protected override void SerializeObjectType(DeclarationProperty property, object obj, Utf8JsonWriter writer, Utf8JsonWriter writerClient, bool? isWriteClient)
            {
                writer.WriteString("$typeComponent", UtilFramework.TypeToName(obj.GetType(), true));
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

            protected override void SerializeValue(object obj, DeclarationProperty property, object value, Utf8JsonWriter writer, Utf8JsonWriter writerClient, bool? isWriteClient)
            {
                Type propertyType = value.GetType();
                ConverterBase converter = ConverterGet(propertyType);
                UtilFramework.Assert(converter.IsObject == false, "Property of type object needs to store a value type!");
                UtilFramework.Assert(!(converter.GetType() == typeof(ConverterEnum) || converter.GetType() == typeof(ConverterEnumNullable)), "Enum not allowed in property of type object!"); 
                writer.WriteStartObject();
                writer.WriteString("$typeValue", UtilFramework.TypeToName(propertyType, true));
                writer.WritePropertyName("Value");
                converter.Serialize(obj, property, value, null, writer, writerClient, isWriteClient);
                writer.WriteEndObject();
                if (isWriteClient == true)
                {
                    throw new Exception(); // Do not use object type for client.
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

            protected override void SerializeObjectType(DeclarationProperty property, object obj, Utf8JsonWriter writer, Utf8JsonWriter writerClient, bool? isWriteClient)
            {
                writer.WriteString("$typeRoot", UtilFramework.TypeToName(obj.GetType(), true));
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

            protected override void SerializeObjectType(DeclarationProperty property, object obj, Utf8JsonWriter writer, Utf8JsonWriter writerClient, bool? isWriteClient)
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

            protected override void SerializeValue(object obj, DeclarationProperty property, object value, Utf8JsonWriter writer, Utf8JsonWriter writerClient, bool? isWriteClient)
            {
                writer.WriteNumberValue((int)value);
                if (isWriteClient == true)
                {
                    writerClient.WriteNumberValue((int)value);
                }
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

            protected override void SerializeValue(object obj, DeclarationProperty property, object value, Utf8JsonWriter writer, Utf8JsonWriter writerClient, bool? isWriteClient)
            {
                writer.WriteNumberValue((int)value);
                if (isWriteClient == true)
                {
                    writerClient.WriteNumberValue((int)value);
                }
            }

            protected override object DeserializeValue(object obj, DeclarationProperty property, JsonElement jsonElement)
            {
                var resultInt = jsonElement.GetInt32();
                var resultEnum = Enum.ToObject(UtilFramework.TypeUnderlying(property.PropertyType), resultInt);
                return resultEnum;
            }
        }

        private static readonly JsonWriterOptions options = new JsonWriterOptions(); // { Indented = true };

        public static void Serialize(object obj, out string json, out string jsonClient)
        {
            using (var stream = new MemoryStream())
            using (var writer = new Utf8JsonWriter(stream, options))
            using (var streamClient = new MemoryStream())
            using (var writerClient = new Utf8JsonWriter(streamClient, options))
            {
                converterObjectRoot.Serialize(obj: null, property: null, obj, componentJsonRoot: null, writer, writerClient, isWriteClient: null);
                writer.Flush();
                writerClient.Flush();
                json = Encoding.UTF8.GetString(stream.ToArray());
                jsonClient = Encoding.UTF8.GetString(streamClient.ToArray());
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
