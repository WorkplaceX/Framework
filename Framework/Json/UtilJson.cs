using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("Framework.Test")]

namespace Framework.Json
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal class JsonException : Exception
    {
        public JsonException(string message)
            : base(message)
        {

        }
    }

    internal static class UtilJson
    {
        private class UtilValue
        {
            public object Obj;

            public string FieldName;

            public Type FieldType;

            public object Index;

            public object Value;
        }

        private static List<UtilValue> ValueListGet(object obj)
        {
            List<UtilValue> result = new List<UtilValue>();
            TypeGroup typeGroup;
            Type valueType;
            UtilJson.TypeInfo(obj, obj.GetType(), out typeGroup, out valueType);
            switch (typeGroup)
            {
                case TypeGroup.Value:
                    break;
                case TypeGroup.Object:
                    {
                        foreach (var fieldIndo in obj.GetType().GetTypeInfo().GetFields())
                        {
                            object value = fieldIndo.GetValue(obj);
                            result.Add(new UtilValue() { Obj = obj, FieldName = fieldIndo.Name, FieldType = fieldIndo.FieldType, Index = null, Value = value });
                        }
                        break;
                    }
                case TypeGroup.List:
                    {
                        IList list = (IList)obj;
                        for (int i = 0; i < list.Count; i++)
                        {
                            object value = list[i];
                            result.Add(new UtilValue() { Obj = obj, FieldName = null, FieldType = valueType, Index = i, Value = value });
                        }
                        break;
                    }
                case TypeGroup.Dictionary:
                    {
                        IDictionary list = (IDictionary)obj;
                        foreach (DictionaryEntry item in list)
                        {
                            object value = item.Value;
                            result.Add(new UtilValue() { Obj = obj, FieldName = null, FieldType = valueType, Index = item.Key, Value = value });
                        }
                        break;
                    }
                default:
                    throw new Exception("Type unknown!");
            }
            return result;
        }

        private static void ValueListSet(object obj, List<UtilValue> valueList)
        {
            TypeGroup typeGroup;
            Type valueType;
            UtilJson.TypeInfo(obj, obj.GetType(), out typeGroup, out valueType);
            switch (typeGroup)
            {
                case TypeGroup.Object:
                    {
                        // (FieldName, UtilValue)
                        Dictionary<string, UtilValue> valueListIndexed = new Dictionary<string, UtilValue>();
                        foreach (var item in valueList)
                        {
                            valueListIndexed.Add(item.FieldName, item);
                        }
                        foreach (var fieldIndo in obj.GetType().GetTypeInfo().GetFields())
                        {
                            object value = valueListIndexed[fieldIndo.Name].Value;
                            fieldIndo.SetValue(obj, value);
                        }
                        break;
                    }
                case TypeGroup.List:
                    {
                        // (Index, UtilValue)
                        Dictionary<int, UtilValue> valueListIndexed = new Dictionary<int, UtilValue>();
                        foreach (var item in valueList)
                        {
                            valueListIndexed.Add((int)item.Index, item);
                        }
                        IList list = (IList)obj;
                        for (int i = 0; i < list.Count; i++)
                        {
                            object value = valueListIndexed[i].Value;
                            list[i] = value;
                        }
                        break;
                    }
                case TypeGroup.Dictionary:
                    {
                        // (Key, UtilValue)
                        Dictionary<object, UtilValue> valueListIndexed = new Dictionary<object, UtilValue>();
                        foreach (var item in valueList)
                        {
                            valueListIndexed.Add(item.Index, item);
                        }
                        IDictionary list = (IDictionary)obj;
                        object[] keyList = new object[list.Count];
                        list.Keys.CopyTo(keyList, 0);
                        foreach (var key in keyList)
                        {
                            object value = valueListIndexed[key].Value;
                            list[key] = value;
                        }
                        break;
                    }
                default:
                    throw new Exception("Type unknown!");
            }
        }

        private enum TypeGroup { None, Value, Object, List, Dictionary }

        private static void TypeInfo(object value, Type fieldType, out TypeGroup typeGroup, out Type valueType)
        {
            if (fieldType == typeof(object))
            {
                if (value != null)
                {
                    Type valueGetType = value.GetType();
                    if (valueGetType == typeof(string))
                    {
                        typeGroup = TypeGroup.Value;
                        valueType = typeof(string);
                        return;
                    }
                    if (valueGetType == typeof(int) || valueGetType == typeof(double) || valueGetType == typeof(Int64))
                    {
                        typeGroup = TypeGroup.Value;
                        valueType = typeof(double);
                        return;
                    }
                    if (valueGetType == typeof(bool))
                    {
                        typeGroup = TypeGroup.Value;
                        valueType = typeof(bool);
                        return;
                    }
                }
            }
            TypeInfo fieldTypeGetTypeInfo = fieldType.GetTypeInfo();
            if (fieldTypeGetTypeInfo.IsValueType)
            {
                typeGroup = TypeGroup.Value;
                valueType = fieldType;
                return;
            }
            if (fieldTypeGetTypeInfo.IsGenericType && fieldTypeGetTypeInfo.GetGenericTypeDefinition() == typeof(List<>))
            {
                typeGroup = TypeGroup.List;
                valueType = fieldTypeGetTypeInfo.GetGenericArguments().First();
                return;
            }
            if (fieldTypeGetTypeInfo.IsGenericType && fieldTypeGetTypeInfo.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                typeGroup = TypeGroup.Dictionary;
                valueType = fieldTypeGetTypeInfo.GetGenericArguments()[1];
                return;
            }
            if (fieldType == typeof(string))
            {
                typeGroup = TypeGroup.Value;
                valueType = fieldType;
                return;
            }
            if (fieldTypeGetTypeInfo.GetConstructors().Count() > 0)
            {
                typeGroup = TypeGroup.Object;
                valueType = fieldType;
                return;
            }
            valueType = null;
            typeGroup = TypeGroup.None;
        }

        private static void SerializePrepareListReset(UtilValue value, bool isAfter)
        {
            TypeGroup typeGroup;
            Type valueType;
            UtilJson.TypeInfo(value.Value, value.FieldType, out typeGroup, out valueType);
            switch (typeGroup)
            {
                case TypeGroup.Value:
                    break;
                case TypeGroup.Object:
                    break;
                case TypeGroup.List:
                    {
                        var list = (IList)value.Value;
                        if (isAfter == false)
                        {
                            if (list != null && list.Count == 0)
                            {
                                value.Value = null;
                            }
                        }
                        else
                        {
                            if (list == null)
                            {
                                value.Value = Activator.CreateInstance(value.FieldType);
                            }
                        }
                    }
                    break;
                case TypeGroup.Dictionary:
                    {
                        var list = (IDictionary)value.Value;
                        if (isAfter == false)
                        {
                            if (list != null && list.Count == 0)
                            {
                                value.Value = null;
                            }
                        }
                        else
                        {
                            if (list == null)
                            {
                                value.Value = Activator.CreateInstance(value.FieldType);
                            }
                        }
                    }
                    break;
                default:
                    throw new Exception("Type unknown!");
            }
        }

        private static void SerializePrepareDerivedList(Type type)
        {
            if (type.GetTypeInfo().IsGenericType)
            {
                Type genericType = type.GetGenericTypeDefinition();
                if (!(genericType == typeof(List<>) || genericType == typeof(Dictionary<,>)))
                {
                    throw new JsonException("No derived list or dictionary for json!");
                }
            }
        }

        /// <summary>
        /// Prepare objects for serialization.
        /// </summary>
        /// <param name="isAfter">After serialization or before serialization.</param>
        private static void SerializePrepare(object value, Type fieldType, bool isAfter)
        {
            if (value != null)
            {
                TypeGroup typeGroup;
                Type valueType;
                UtilJson.TypeInfo(value, fieldType, out typeGroup, out valueType);
                switch (typeGroup)
                {
                    case TypeGroup.Value:
                        {
                            if (fieldType == typeof(object))
                            {
                                if (!(value.GetType() == typeof(string) || value.GetType() == typeof(double) || value.GetType() == typeof(bool)))
                                {
                                    throw new JsonException("Allowed types: string, double or bool!");
                                }
                            }
                        }
                        break;
                    case TypeGroup.Object:
                        {
                            SerializePrepareDerivedList(value.GetType());
                            bool isSetType = value.GetType() != fieldType;
                            var valueList = ValueListGet(value);
                            foreach (UtilValue item in valueList)
                            {
                                object itemValue = item.Value;
                                if (isSetType && item.FieldName == "Type")
                                {
                                    isSetType = false;
                                    item.Value = value.GetType().Name;
                                }
                                if (item.FieldName == "Type" && itemValue is string && (string)itemValue != value.GetType().Name) // Type has been overwritten for Angular Selector
                                {
                                    valueList.Where(item2 => item2.FieldName == "Type").First().Value = itemValue; // For example Label
                                    UtilValue utilValue = valueList.Where(item2 => item2.FieldName == "TypeCSharp").FirstOrDefault();
                                    if (utilValue == null)
                                    {
                                        throw new JsonException("Object has no TypeCSharp field!");
                                    }
                                    utilValue.Value = value.GetType().Name; // For example MyLabel
                                }
                                SerializePrepareListReset(item, isAfter);
                                SerializePrepare(item.Value, item.FieldType, isAfter);
                            }
                            if (isSetType)
                            {
                                throw new JsonException("Object has no Type field!"); // Add this code: "public string Type;"
                            }
                            ValueListSet(value, valueList);
                        }
                        break;
                    case TypeGroup.List:
                        {
                            SerializePrepareDerivedList(value.GetType());
                            var valueList = ValueListGet(value);
                            foreach (UtilValue item in valueList)
                            {
                                SerializePrepareListReset(item, isAfter);
                                SerializePrepare(item.Value, item.FieldType, isAfter);
                            }
                            ValueListSet(value, valueList);
                        }
                        break;
                    case TypeGroup.Dictionary:
                        {
                            var valueList = ValueListGet(value);
                            foreach (UtilValue item in valueList)
                            {
                                if (item.Index.GetType() != typeof(string))
                                {
                                    throw new JsonException("Dictionary key needs to be of type string!");
                                }
                                SerializePrepareListReset(item, isAfter);
                                SerializePrepare(item.Value, item.FieldType, isAfter);
                            }
                            ValueListSet(value, valueList);
                        }
                        break;
                    default:
                        throw new Exception("Type unknown!");
                }
            }
        }

        private static string Serialize(object obj, Type rootType, Type[] typeInNamespaceList)
        {
            SerializePrepare(obj, rootType, false);
            string result = Newtonsoft.Json.JsonConvert.SerializeObject(obj, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore });
            SerializePrepare(obj, rootType, true);
            // TODO Enable, Disable Debug
            //{
            //    string debugSource = Newtonsoft.Json.JsonConvert.SerializeObject(obj, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All });
            //    object debugObj = Deserialize(result, rootType, typeInNamespaceList);
            //    SerializePrepare(debugObj, rootType, true);
            //    string debugDest = Newtonsoft.Json.JsonConvert.SerializeObject(debugObj, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All });
            //    UtilJson.Assert(debugSource == debugDest);
            //}
            //
            return result;
        }

        /// <summary>
        /// Returns json text of serialized fields and properties of CSharp object.
        /// </summary>
        /// <param name="obj">Object to serialize.</param>
        /// <param name="typeInNamespaceList">Needed internally for debug, to deserialize and compare.</param> // TODO Remove
        public static string Serialize(object obj, params Type[] typeInNamespaceList)
        {
            if (UtilFramework.IsJson2 == false)
            {
                // SerializeClient
                UtilStopwatch.TimeStart("SerializeClient");
                string jsonClient = Serialize(obj, obj.GetType(), typeInNamespaceList);
                UtilStopwatch.TimeStop("SerializeClient");

                return jsonClient;
            }
            else
            {
                // SerializeClient
                UtilStopwatch.TimeStart("SerializeClient2");
                string jsonClient = UtilJson2.Serialize(obj);
                UtilStopwatch.TimeStop("SerializeClient2");

                return jsonClient;
            }
        }

        private static object DeserializeObjectConvert(object value, Type type)
        {
            if (value == null)
            {
                return value;
            }
            if (type == typeof(object))
            {
                return value;
            }
            if (type == typeof(Guid))
            {
                return Guid.Parse((string)value);
            }
            if (value.GetType().GetTypeInfo().IsSubclassOf(type))
            {
                return value;
            }
            if (type.GetTypeInfo().IsEnum)
            {
                return Enum.ToObject(type, value);
            }
            if (Nullable.GetUnderlyingType(type) != null)
            {
                type = Nullable.GetUnderlyingType(type);
                if (type.GetTypeInfo().IsEnum)
                {
                    return Enum.ToObject(type, value);
                }
                return DeserializeObjectConvert(value, type);
            }
            else
            {
                return System.Convert.ChangeType(value, type);
            }
        }

        /// <summary>
        /// Returns type found in exact namespace. Not only in assembly.
        /// </summary>
        /// <param name="objectTypeString">Type as string. For example "MyComponent".</param>
        /// <param name="typeInNamespace">A type defined in namespace in which to search.</param>
        private static Type TypeGetExact(string objectTypeString, Type typeInNamespace)
        {
            string ns = typeInNamespace.Namespace + ".";
            if (typeInNamespace.DeclaringType != null)
            {
                ns = typeInNamespace.DeclaringType.FullName + "+";
            }
            Type result = Type.GetType(ns + objectTypeString + ", " + typeInNamespace.GetTypeInfo().Assembly.FullName);
            return result;
        }

        /// <summary>
        /// Returns type. Searches for type in rootType's assembly and typeInNamespaceList assembly and namespace.
        /// </summary>
        private static Type TypeGet(string objectTypeString, Type rootType, Type[] typeInNamespaceList)
        {
            List<Type> resultList = new List<Type>();
            Type result = TypeGetExact(objectTypeString, rootType);
            if (result != null)
            {
                resultList.Add(result);
            }
            foreach (Type type in typeInNamespaceList)
            {
                result = TypeGetExact(objectTypeString, type); // Search in exact namespace. Not just in assembly.
                if (result != null && !resultList.Contains(result))
                {
                    resultList.Add(result);
                }
            }
            if (resultList.Count == 0)
            {
                UtilJson.Assert(false, "Type not found!");
            }
            if (resultList.Count > 1)
            {
                UtilJson.Assert(false, "More than one type found!"); // Type with same name defined in more than one namespace (not assembly!)
            }
            return resultList.Single();
        }

        private static Type DeserializeTokenObjectType(JObject jObject, Type fieldType, Type rootType, Type[] typeInNamespaceList)
        {
            Type result = null;
            if (jObject != null)
            {
                result = fieldType;
                if (jObject.Property("Type") != null)
                {
                    JValue jValue = jObject.Property("Type").Value as JValue;
                    string objectTypeString = jValue.Value as string;
                    if (objectTypeString != null)
                    {
                        result = UtilJson.TypeGet(objectTypeString, rootType, typeInNamespaceList);
                    }
                }
                //
                if (jObject.Property("TypeCSharp") != null)
                {
                    JValue jValue = jObject.Property("TypeCSharp").Value as JValue;
                    string objectTypeString = jValue.Value as string;
                    if (objectTypeString != null)
                    {
                        result = UtilJson.TypeGet(objectTypeString, rootType, typeInNamespaceList);
                    }
                }
            }
            return result;
        }

        private static object DeserializeToken(JToken jToken, Type fieldType, Type rootType, Type[] typeInNamespaceList)
        {
            object result = null;
            //
            object value = null;
            JValue jValue = jToken as JValue;
            if (jValue != null)
            {
                value = jValue.Value;
            }
            //
            TypeGroup typeGroup;
            Type valueType;
            UtilJson.TypeInfo(value, fieldType, out typeGroup, out valueType);
            switch (typeGroup)
            {
                case TypeGroup.Value:
                    {
                        result = DeserializeObjectConvert(value, valueType);
                    }
                    break;
                case TypeGroup.Object:
                    {
                        JObject jObject = jToken as JObject;
                        Type objectType = DeserializeTokenObjectType(jObject, fieldType, rootType, typeInNamespaceList);
                        if (objectType != null)
                        {
                            result = Activator.CreateInstance(objectType);
                            // Deserialize field
                            foreach (var fieldInfo in result.GetType().GetTypeInfo().GetFields())
                            {
                                if (jObject != null)
                                {
                                    JProperty jProperty = jObject.Property(fieldInfo.Name);
                                    if (jProperty != null)
                                    {
                                        JToken jTokenChild = jProperty.Value;
                                        Type fieldTypeChild = fieldInfo.FieldType;
                                        object valueChild = DeserializeToken(jTokenChild, fieldTypeChild, rootType, typeInNamespaceList);
                                        fieldInfo.SetValue(result, valueChild);
                                    }
                                }
                            }
                            // Deserialize property
                            foreach (var propertyInfo in result.GetType().GetTypeInfo().GetProperties())
                            {
                                if (jObject != null)
                                {
                                    JProperty jProperty = jObject.Property(propertyInfo.Name);
                                    if (jProperty != null)
                                    {
                                        JToken jTokenChild = jProperty.Value;
                                        Type fieldTypeChild = propertyInfo.PropertyType;
                                        object valueChild = DeserializeToken(jTokenChild, fieldTypeChild, rootType, typeInNamespaceList);
                                        propertyInfo.SetValue(result, valueChild);
                                    }
                                }
                            }
                        }
                    }
                    break;
                case TypeGroup.List:
                    {
                        var list = (IList)Activator.CreateInstance(fieldType);
                        JArray jArray = jToken as JArray;
                        if (jArray != null)
                        {
                            foreach (var jTokenChild in jArray)
                            {
                                Type fieldTypeChild = valueType;
                                object valueChild = DeserializeToken(jTokenChild, fieldTypeChild, rootType, typeInNamespaceList);
                                list.Add(valueChild);
                            }
                        }
                        result = list;
                    }
                    break;
                case TypeGroup.Dictionary:
                    {
                        var list = (IDictionary)Activator.CreateInstance(fieldType);
                        JObject jObject = jToken as JObject;
                        if (jObject != null)
                        {
                            foreach (var jKeyValue in jObject)
                            {
                                Type fieldTypeChild = valueType;
                                JToken jTokenChild = jKeyValue.Value;
                                object valueChild = DeserializeToken(jTokenChild, fieldTypeChild, rootType, typeInNamespaceList);
                                list.Add(jKeyValue.Key, valueChild);
                            }
                        }
                        result = list;
                    }
                    break;
                default:
                    throw new Exception("Type unknown!");
            }
            return result;
        }

        private static object Deserialize(string json, Type rootType, Type[] typeInNamespaceList)
        {
            JObject jObject = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            object result = DeserializeToken(jObject, rootType, rootType, typeInNamespaceList);
            SerializePrepare(result, rootType, true);
            return result;
        }

        /// <summary>
        /// Deserialize fields and properties.
        /// </summary>
        /// <typeparam name="T">Type of root object.</typeparam>
        /// <param name="json">Json text to deserialize.</param>
        /// <param name="typeInNamespaceList">Additional namespaces to search for classes.</param>
        public static T Deserialize<T>(string json, params Type[] typeInNamespaceList)
        {
            if (UtilFramework.IsJson2 == false)
            {
                // DeserializeClient
                UtilStopwatch.TimeStart("DeserializeClient");
                var result = (T)Deserialize(json, typeof(T), typeInNamespaceList);
                UtilStopwatch.TimeStop("DeserializeClient");
                return result;
            }
            else
            {
                // DeserializeClient
                UtilStopwatch.TimeStart("DeserializeClient2");
                var result = (T)UtilJson2.Deserialize(json);
                UtilStopwatch.TimeStop("DeserializeClient2");
                return result;
            }

            // UtilJson2.DebugValidateJson(result, result2);
        }

        private static void Assert(bool isAssert, string exceptionText)
        {
            if (!isAssert)
            {
                throw new Exception(exceptionText);
            }
        }

        private static void Assert(bool isAssert)
        {
            Assert(isAssert, "Assert!");
        }
    }
}

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

    internal static class UtilJson2
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
                    DeclarationProperty declarationProperty = new DeclarationProperty(propertyInfo);
                    PropertyList.Add(declarationProperty.PropertyName, declarationProperty);
                }
                // Field
                foreach (var fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (fieldInfo.Attributes != FieldAttributes.Private)
                    {
                        DeclarationProperty declarationProperty = new DeclarationProperty(fieldInfo);
                        PropertyList.Add(declarationProperty.PropertyName, declarationProperty);
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

        private class DeclarationProperty
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
                    if (propertyType.Assembly == typeof(UtilFramework).Assembly) // Dto
                        result = converterObjectDto;
            UtilFramework.Assert(result != null, "Type not supported!");
            return result;
        }

        private abstract class ConverterBase
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

            protected virtual void SerializeValue(DeclarationProperty declarationProperty, object value, Utf8JsonWriter writer)
            {
                // writer.WriteStringValue(string.Format("{0}", value));
            }

            protected virtual void SerializeObjectType(DeclarationProperty declarationProperty, object obj, Utf8JsonWriter writer)
            {
                // writer.WriteString("$typeRoot", UtilFramework.TypeToName(obj.GetType()));
            }

            private void SerializeObject(DeclarationProperty declarationProperty, object obj, Utf8JsonWriter writer)
            {
                DeclarationObject declarationObject;
                declarationObject = DeclarationObjectGet(obj.GetType());
                writer.WriteStartObject();
                SerializeObjectType(declarationProperty, obj, writer);
                foreach (var item in declarationObject.PropertyList.Values)
                {
                    if (item.IsList == false)
                    {
                        object value = item.ValueGet(obj);
                        ConverterBase converter = item.Converter;
                        if (!converter.IsValueDefault(value))
                        {
                            writer.WritePropertyName(item.PropertyName);
                            converter.Serialize(item, value, writer);
                        }
                    }
                    else
                    {
                        IList valueList = item.ValueListGet(obj);
                        if (valueList?.Count > 0)
                        {
                            writer.WritePropertyName(item.PropertyName);
                            ConverterBase converter = item.Converter;
                            writer.WriteStartArray();
                            foreach (var value in valueList)
                            {
                                if (!converter.IsValueDefault(value))
                                {
                                    converter.Serialize(item, value, writer);
                                }
                                else
                                {
                                    if (converter.ValueDefault == null)
                                    {
                                        writer.WriteNullValue(); // Serialize null
                                    }
                                    else
                                    {
                                        converter.Serialize(item, value, writer);
                                    }
                                }
                            }
                            writer.WriteEndArray();
                        }
                    }
                }
                writer.WriteEndObject();
            }

            internal void Serialize(DeclarationProperty declarationProperty, object obj, Utf8JsonWriter writer)
            {
                if (IsObject == false)
                {
                    SerializeValue(declarationProperty, obj, writer);
                }
                else
                {
                    SerializeObject(declarationProperty, obj, writer);
                }
            }

            protected virtual object DeserializeValue(DeclarationProperty declarationProperty, Utf8JsonReader reader)
            {
                throw new NotImplementedException(); // return reader.GetString();
            }

            protected virtual object DeserializeValue(DeclarationProperty declarationProperty, JsonElement jsonElement)
            {
                throw new NotImplementedException(); // return jsonElement.GetString();
            }

            protected virtual string DeserializeObjectType(DeclarationProperty declarationProperty, JsonElement jsonElement)
            {
                // return jsonElement.GetProperty("$typeRoot").GetString();
                return null;
            }

            protected virtual string DeserializeObjectType(DeclarationProperty declarationProperty, Utf8JsonReader reader)
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

            private object DeserializeObject(DeclarationProperty declarationProperty, Utf8JsonReader reader)
            {
                reader.Read();
                UtilFramework.Assert(reader.TokenType == JsonTokenType.StartObject);
                Type type = PropertyType;
                string typeName = DeserializeObjectType(declarationProperty, reader);
                if (typeName != null)
                {
                    type = UtilFramework.TypeFromName(typeName);
                }
                var result = FormatterServices.GetUninitializedObject(type);
                var declarationObject = DeclarationObjectGet(type);
                return result;
            }

            private object DeserializeObject(DeclarationProperty declarationProperty, JsonElement jsonElement)
            {
                Type type = PropertyType;
                string typeName = DeserializeObjectType(declarationProperty, jsonElement);
                if (typeName != null)
                {
                    type = UtilFramework.TypeFromName(typeName);
                }
                if (type == null)
                {
                    type = declarationProperty.PropertyType; // Dto
                }
                var result = FormatterServices.GetUninitializedObject(type);
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
                            object value = item.Converter.Deserialize(item, jsonElementProperty.Value);
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
                                    value = item.Converter.Deserialize(item, jsonElementValue);
                                }
                                valueList.Add(value);
                            }
                            item.ValueListSet(result, valueList);
                        }
                    }
                }
                return result;
            }

            internal object Deserialize(DeclarationProperty declarationProperty, JsonElement jsonElement)
            {
                if (IsObject == false)
                {
                    return DeserializeValue(declarationProperty, jsonElement);
                }
                else
                {
                    return DeserializeObject(declarationProperty, jsonElement);
                }
            }

            internal object Deserialize(DeclarationProperty declarationProperty, Utf8JsonReader reader)
            {
                if (IsObject == false)
                {
                    return DeserializeValue(declarationProperty, reader);
                }
                else
                {
                    return DeserializeObject(declarationProperty, reader);
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

            public Type PropertyTypeGeneric(DeclarationProperty declarationProperty)
            {
                var result = propertyTypeGeneric;
                if (result == null)
                {
                    result = TypeGenericList.GetOrAdd(declarationProperty.PropertyType, (Type type) => typeof(List<>).MakeGenericType(declarationProperty.PropertyType));
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

            protected override void SerializeValue(DeclarationProperty declarationProperty, object value, Utf8JsonWriter writer)
            {
                writer.WriteNumberValue((int)value);
            }

            protected override object DeserializeValue(DeclarationProperty declarationProperty, JsonElement jsonElement)
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

            protected override void SerializeValue(DeclarationProperty declarationProperty, object value, Utf8JsonWriter writer)
            {
                writer.WriteNumberValue((int)value);
            }

            protected override object DeserializeValue(DeclarationProperty declarationProperty, JsonElement jsonElement)
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

            protected override void SerializeValue(DeclarationProperty declarationProperty, object value, Utf8JsonWriter writer)
            {
                writer.WriteStringValue((string)value);
            }

            protected override object DeserializeValue(DeclarationProperty declarationProperty, JsonElement jsonElement)
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

            protected override void SerializeValue(DeclarationProperty declarationProperty, object value, Utf8JsonWriter writer)
            {
                writer.WriteBooleanValue((bool)value);
            }

            protected override object DeserializeValue(DeclarationProperty declarationProperty, JsonElement jsonElement)
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

            protected override void SerializeValue(DeclarationProperty declarationProperty, object value, Utf8JsonWriter writer)
            {
                writer.WriteBooleanValue((bool)value);
            }

            protected override object DeserializeValue(DeclarationProperty declarationProperty, JsonElement jsonElement)
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

            protected override void SerializeValue(DeclarationProperty declarationProperty, object value, Utf8JsonWriter writer)
            {
                writer.WriteNumberValue((double)value);
            }

            protected override object DeserializeValue(DeclarationProperty declarationProperty, JsonElement jsonElement)
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

            protected override void SerializeValue(DeclarationProperty declarationProperty, object value, Utf8JsonWriter writer)
            {
                writer.WriteNumberValue((double)value);
            }

            protected override object DeserializeValue(DeclarationProperty declarationProperty, JsonElement jsonElement)
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

            protected override void SerializeValue(DeclarationProperty declarationProperty, object value, Utf8JsonWriter writer)
            {
                string typeName = UtilFramework.TypeToName((Type)value, true);
                writer.WriteStringValue(typeName);
            }

            protected override object DeserializeValue(DeclarationProperty declarationProperty, JsonElement jsonElement)
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

            protected override void SerializeValue(DeclarationProperty declarationProperty, object value, Utf8JsonWriter writer)
            {
                writer.WriteStartObject();
                writer.WriteString("$typeRow", UtilFramework.TypeToName(value.GetType(), true));
                writer.WritePropertyName("Row");
                JsonSerializer.Serialize(writer, value, value.GetType());
                writer.WriteEndObject();
            }

            protected override object DeserializeValue(DeclarationProperty declarationProperty, JsonElement jsonElement)
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

            protected override void SerializeObjectType(DeclarationProperty declarationProperty, object obj, Utf8JsonWriter writer)
            {
                writer.WriteString("$typeComponent", UtilFramework.TypeToName(obj.GetType(), true));
            }

            protected override string DeserializeObjectType(DeclarationProperty declarationProperty, JsonElement jsonElement)
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

            protected override void SerializeValue(DeclarationProperty declarationProperty, object value, Utf8JsonWriter writer)
            {
                Type propertyType = value.GetType();
                ConverterBase converter = ConverterGet(propertyType);
                UtilFramework.Assert(converter.IsObject == false, "Property of type object needs to store a value type!");
                UtilFramework.Assert(!(converter.GetType() == typeof(ConverterEnum) || converter.GetType() == typeof(ConverterEnumNullable)), "Enum not allowed in property of type object!"); 
                writer.WriteStartObject();
                writer.WriteString("$typeValue", UtilFramework.TypeToName(propertyType, true));
                writer.WritePropertyName("Value");
                converter.Serialize(declarationProperty, value, writer);
                writer.WriteEndObject();
            }

            protected override object DeserializeValue(DeclarationProperty declarationProperty, JsonElement jsonElement)
            {
                string typeName = jsonElement.GetProperty("$typeValue").GetString();
                Type type = UtilFramework.TypeFromName(typeName);
                var converter = ConverterGet(type);
                var result = converter.Deserialize(declarationProperty, jsonElement.GetProperty("Value"));
                return result;
            }
        }

        private sealed class ConverterObjectRoot : ConverterBase
        {
            public ConverterObjectRoot() 
                : base(true, null)
            {

            }

            protected override void SerializeObjectType(DeclarationProperty declarationProperty, object obj, Utf8JsonWriter writer)
            {
                writer.WriteString("$typeRoot", UtilFramework.TypeToName(obj.GetType(), true));
            }

            protected override string DeserializeObjectType(DeclarationProperty declarationProperty, JsonElement jsonElement)
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

            protected override void SerializeObjectType(DeclarationProperty declarationProperty, object obj, Utf8JsonWriter writer)
            {
                UtilFramework.Assert(declarationProperty.PropertyType == obj.GetType(), "Property type and object type not equal!");
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

            protected override void SerializeValue(DeclarationProperty declarationProperty, object value, Utf8JsonWriter writer)
            {
                writer.WriteNumberValue((int)value);
            }

            protected override object DeserializeValue(DeclarationProperty declarationProperty, JsonElement jsonElement)
            {
                var resultInt = jsonElement.GetInt32();
                var resultEnum = Enum.ToObject(declarationProperty.PropertyType, resultInt);
                return resultEnum;
            }
        }

        private sealed class ConverterEnumNullable: ConverterBase
        {
            public ConverterEnumNullable() 
                : base(false, null)
            {

            }

            protected override void SerializeValue(DeclarationProperty declarationProperty, object value, Utf8JsonWriter writer)
            {
                writer.WriteNumberValue((int)value);
            }

            protected override object DeserializeValue(DeclarationProperty declarationProperty, JsonElement jsonElement)
            {
                var resultInt = jsonElement.GetInt32();
                var resultEnum = Enum.ToObject(UtilFramework.TypeUnderlying(declarationProperty.PropertyType), resultInt);
                return resultEnum;
            }
        }

        private static readonly JsonWriterOptions options = new JsonWriterOptions() { Indented = true };

        public static string Serialize(object obj)
        {
            string json;

            using (var stream = new MemoryStream())
            {
                using (var writer = new Utf8JsonWriter(stream, options))
                {
                    converterObjectRoot.Serialize(null, obj, writer);
                }

                json = Encoding.UTF8.GetString(stream.ToArray());
            }

            // DebugValidateJson(obj, json);

            return json;
        }

        /// <summary>
        /// Validate json by deserializing it and comparing to obj.
        /// </summary>
        public static void DebugValidateJson(object obj, string json)
        {
            string jsonSource = Newtonsoft.Json.JsonConvert.SerializeObject(obj, new Newtonsoft.Json.JsonSerializerSettings() { NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore, DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore });
            object objDest = Deserialize(json);
            string jsonDest = Newtonsoft.Json.JsonConvert.SerializeObject(objDest, new Newtonsoft.Json.JsonSerializerSettings() { NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore, DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore });
            UtilFramework.Assert(jsonSource == jsonDest);
        }

        public static void DebugValidateJson(object source, object dest)
        {
            string jsonSource = Newtonsoft.Json.JsonConvert.SerializeObject(source, new Newtonsoft.Json.JsonSerializerSettings() { TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All });
            string jsonDest = Newtonsoft.Json.JsonConvert.SerializeObject(dest, new Newtonsoft.Json.JsonSerializerSettings() { TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All });
            UtilFramework.Assert(jsonSource == jsonDest);
        }

        public static object Deserialize(string json)
        {
            bool isUtf8JsonReader = false; // Use JsonDocument or Utf8JsonReader

            object result;
            if (isUtf8JsonReader == false)
            {
                JsonDocument document = JsonDocument.Parse(json);
                result = converterObjectRoot.Deserialize(null, document.RootElement);
            }
            else
            {
                var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
                result = converterObjectRoot.Deserialize(null, reader);
            }

            return result;
        }
    }
}
