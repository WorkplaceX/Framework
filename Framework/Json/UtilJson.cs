﻿namespace Framework.Json
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
            // TODO Remove
            {
                string debugSource = Newtonsoft.Json.JsonConvert.SerializeObject(obj, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All });
                object debugObj = Deserialize(result, rootType, typeInNamespaceList);
                SerializePrepare(debugObj, rootType, true);
                string debugDest = Newtonsoft.Json.JsonConvert.SerializeObject(debugObj, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All });
                UtilJson.Assert(debugSource == debugDest);
            }
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
            return Serialize(obj, obj.GetType(), typeInNamespaceList);
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
            return (T)Deserialize(json, typeof(T), typeInNamespaceList);
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
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Linq;
    using Framework.DataAccessLayer;
    using Framework.Server;
    using Microsoft.Extensions.Caching.Memory;

    /// <summary>
    /// Property attribute for properties of class ComponentJson.
    /// </summary>
    public enum JsonSerialize 
    {
        /// <summary>
        /// Default behavior (serialize).
        /// </summary>
        None = 0, 

        /// <summary>
        /// Do not serialize property.
        /// </summary>
        Exclude = 1, 

        /// <summary>
        /// Serialize property only on server side. It's not sent to the client.
        /// </summary>
        ServerOnly = 2 
    }

    /// <summary>
    /// Property attribute for properties of class ComponentJson.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class JsonSerializeAttribute : Attribute 
    {
        public JsonSerializeAttribute(JsonSerialize jsonSerialize)
        {
            this.JsonSerialize = jsonSerialize;
        }

        public readonly JsonSerialize JsonSerialize;
    }

    /// <summary>
    /// AppJson serializer and deserializer.
    /// </summary>
    internal static class UtilJson2
    {
        /// <summary>
        /// Serialize AppJson for server (session state) and client (Angular).
        /// </summary>
        public static void Serialize(AppJson2 appJson, out string jsonServer, out string jsonClient)
        {
            // Serialize with System.Text.Json (Init)
            var options = new JsonSerializerOptions();
            var converterFactory = new ConverterFactory();
            options.Converters.Add(converterFactory);
            options.WriteIndented = true;
            options.IgnoreNullValues = true;

            // Serialize AppJson for server
            converterFactory.IsClient = false;
            jsonServer = JsonSerializer.Serialize(appJson, appJson.GetType(), options);

            // Serialize AppJson for client
            converterFactory.IsClient = true;
            jsonClient = JsonSerializer.Serialize(appJson, appJson.GetType(), options);
        }

        /// <summary>
        /// Set ComponentJson.Owner and ComponentJson.Root of every deserialized component.
        /// </summary>
        private static void Deserialize(ComponentJson2 componentJson)
        {
            foreach (var item in componentJson.List)
            {
                item.ConstructorDeserialize(componentJson);
                Deserialize(item);
            }
        }

        /// <summary>
        /// Deserialize AppJson object from jsonServer (session state).
        /// </summary>
        public static AppJson2 Deserialize(string jsonServer)
        {
            // Deserialize with System.Text.Json (Init)
            var options = new JsonSerializerOptions();
            var converterFactory = new ConverterFactory();
            options.Converters.Add(converterFactory);

            // Deserialize AppJson
            var result = JsonSerializer.Deserialize<AppJson2>(jsonServer, options);

            // Resolve ComponentJson references
            foreach (var item in converterFactory.ComponentJsonReferenceList)
            {
                PropertyInfo propertyInfo = item.PropertyInfo;
                object propertyObject = item.PropertyObject;
                ComponentJson2 componentJsonReference = converterFactory.ComponentJsonList[item.IdReference];
                propertyInfo.SetValue(propertyObject, componentJsonReference);
            }

            Deserialize(result);

            return result;
        }

        /// <summary>
        /// Returns for example: "Application.AppMain2, Application"
        /// </summary>
        public static string TypeToName(Type type)
        {
            return type.FullName + ", " + type.Assembly.GetName().Name;
        }

        /// <summary>
        /// Returns type of for example "Application.AppMain2, Application".
        /// </summary>
        public static Type TypeFromName(string typeName)
        {
            string key = string.Format("{0}; TypeNameList; {1};", typeof(UtilJson2).FullName, typeName);
             
            return UtilServer.MemoryCache.GetOrCreate(key, (entry) => Type.GetType(typeName));
        }
    }

    /// <summary>
    /// Factory to serialize, deserialize ComponentJson object.
    /// </summary>
    internal class ConverterFactory : JsonConverterFactory
    {
        /// <summary>
        /// Returns true for ComponentJson, data Row and Type objects.
        /// </summary>
        public override bool CanConvert(Type typeToConvert)
        {
            // Handle inheritance of ComponentJson and Row classes. Also handle Type object.
            var result =
                UtilFramework.IsSubclassOf(typeToConvert, typeof(ComponentJson2)) ||
                UtilFramework.IsSubclassOf(typeToConvert, typeof(Row)) ||
                typeToConvert == typeof(Type);

            // For in Framework declared DTO's enable DefaultValueHandling.Ignore and ComponentJsonReference
            if (result == false)
            {
                if (typeToConvert.Assembly == typeof(UtilFramework).Assembly)
                {
                    result = true;
                }
            }

            return result;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var converterType = typeof(Converter<>).MakeGenericType(typeToConvert);
            return (JsonConverter)Activator.CreateInstance(converterType, this);
        }

        /// <summary>
        /// Gets or sets IsClient. If false, everithing is serialized to store on server. If true, data Row and ComponentJson references are excluded.
        /// </summary>
        public bool IsClient;

        /// <summary>
        /// List of deserialized ComponentJson references.
        /// </summary>
        public List<ComponentJsonReference> ComponentJsonReferenceList = new List<ComponentJsonReference>();

        /// <summary>
        /// List of deserialized ComponentJson objects.
        /// </summary>
        public Dictionary<int, ComponentJson2> ComponentJsonList = new Dictionary<int, ComponentJson2>();
    }

    /// <summary>
    /// Not yet resolved ComponentJson reference.
    /// </summary>
    internal class ComponentJsonReference
    {
        public PropertyInfo PropertyInfo;

        /// <summary>
        /// Gets or sets PropertyObject. This is the object on which property is declared.
        /// </summary>
        public object PropertyObject; // ComponentJson or Framework declared DTO

        /// <summary>
        /// Gets or sets IdReference. This is the ComponentJson.Id reference.
        /// </summary>
        public int IdReference;
    }

    internal enum PropertyEnum { None = 0, Property = 1, List = 2, Dictionary = 3 }

    /// <summary>
    /// Access property, list or dictionary.
    /// </summary>
    internal class Property
    {
        public Property(PropertyInfo propertyInfo, object propertyValue)
        {
            // Property
            this.PropertyEnum = PropertyEnum.Property;
            this.PropertyType = propertyInfo.PropertyType;
            this.IsComponentJsonList = propertyInfo.DeclaringType == typeof(ComponentJson2) && propertyInfo.Name == nameof(ComponentJson2.List);
            this.PropertyValue = propertyValue;
            this.PropertyValueList = new List<object>(new object[] { propertyValue });

            var interfaceList = propertyInfo.PropertyType.GetInterfaces();

            // List
            if (interfaceList.Contains(typeof(IList)))
            {
                this.PropertyEnum = PropertyEnum.List;
                this.PropertyType = propertyInfo.PropertyType.GetGenericArguments()[0]; // List type
                this.PropertyValueList = (IList)propertyValue;
            }

            // Dictionary
            if (interfaceList.Contains(typeof(IDictionary)))
            {
                this.PropertyEnum = PropertyEnum.Dictionary;
                this.PropertyType = propertyInfo.PropertyType.GetGenericArguments()[1]; // Key type
                this.PropertyDictionary = (IDictionary)propertyValue;
                this.PropertyValueList = PropertyDictionary?.Values;
            }
        }

        /// <summary>
        /// Gets PropertyEnum (property, list or dictionary)
        /// </summary>
        public PropertyEnum PropertyEnum;

        /// <summary>
        /// Gets PropertyType for property, list and dictionary.
        /// </summary>
        public Type PropertyType;

        /// <summary>
        /// Gets IsComponentJsonList. If true, property is ComponentJson.List property.
        /// </summary>
        public bool IsComponentJsonList;

        /// <summary>
        /// Gets PropertyValueList for property, list and dictionary.
        /// </summary>
        public ICollection PropertyValueList;

        /// <summary>
        /// Gets PropertyValue for property.
        /// </summary>
        public object PropertyValue;

        /// <summary>
        /// Gets PropertyDictionary for dictionary to access key, value pair with DictionaryEntry.
        /// </summary>
        public IDictionary PropertyDictionary;
    }

    internal class Converter<T> : JsonConverter<T>
    {
        public Converter(ConverterFactory converterFactory)
        {
            this.ConverterFactory = converterFactory;
        }

        public readonly ConverterFactory ConverterFactory;

        /// <summary>
        /// Deserialize ComponentJson, Row or Type objects.
        /// </summary>
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Deserialize Type object
            if (typeToConvert == typeof(Type))
            {
                var typeName = JsonSerializer.Deserialize<string>(ref reader);
                return (T)(object)UtilJson2.TypeFromName(typeName);
            }

            // Deserialize ComponentJson or Row object
            var valueList = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(ref reader);

            // Deserialize data row object
            if (UtilFramework.IsSubclassOf(typeToConvert, typeof(Row)))
            {
                var typeRowName = valueList["$typeRow"].GetString();
                string rowJson = valueList["$row"].GetRawText();
                Type typeRow = UtilJson2.TypeFromName(typeRowName);
                var resultRow = JsonSerializer.Deserialize(rowJson, typeRow); // Native deserialization for data row.
                return (T)(object)resultRow;
            }

            // Read type information
            string typeNameComponentJson = valueList["$type"].GetString();
            Type type = UtilJson2.TypeFromName(typeNameComponentJson); // TODO Cache on factory

            // Create AppJson, ComponentJson or Framework declared DTO
            object result;
            if (UtilFramework.IsSubclassOf(type, typeof(AppJson2)))
            {
                result = Activator.CreateInstance(type); // Create AppJson
            }
            else
            {
                if (UtilFramework.IsSubclassOf(type, typeof(ComponentJson2)))
                {
                    result = Activator.CreateInstance(type, new object[] { null }); // Create ComponentJson
                }
                else
                {
                    UtilFramework.Assert(!UtilFramework.IsSubclassOf(type, typeof(Row))); // Not a data row!
                    result = Activator.CreateInstance(type); // Create in Framework declared DTO
                }
            }

            // Loop through created ComponentJson properties
            foreach (var propertyInfo in result.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                if (valueList.ContainsKey(propertyInfo.Name))
                {
                    // Deserialize ComponentJsonReference
                    if (IsComponentJsonReference(propertyInfo))
                    {
                        var componentJsonReferenceValueList = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(valueList[propertyInfo.Name].GetRawText());
                        UtilFramework.Assert(componentJsonReferenceValueList["$type"].GetString() == "$componentJsonReference");
                        int id = componentJsonReferenceValueList["$id"].GetInt32();
                        ConverterFactory.ComponentJsonReferenceList.Add(new ComponentJsonReference { PropertyInfo = propertyInfo, PropertyObject = result, IdReference = id });
                        continue;
                    }

                    // Deserialize property value
                    var propertyValue = JsonSerializer.Deserialize(valueList[propertyInfo.Name].GetRawText(), propertyInfo.PropertyType, options);
                    propertyInfo.SetValue(result, propertyValue);
                }
            }

            // Add ComponentJson for ComponentJsonReference resolve.
            if (result is ComponentJson2 componentJson)
            {
                ConverterFactory.ComponentJsonList.Add(componentJson.Id, componentJson); // Exception item with same key happens, if DTO not declared in Framework references ComponentJson.
            }

            return (T)(object)result;
        }

        /// <summary>
        /// PropertyType has to be (ComponentJson, Row or Type). Or PropertyType and PropertyValue type need to match. Applies also for list or dictionary.
        /// </summary>
        private void ValidatePropertyAndValueType(PropertyInfo propertyInfo, object propertyValue)
        {
            var property = new Property(propertyInfo, propertyValue);

            Type propertyType = property.PropertyType;
            ICollection propertyValueList = property.PropertyValueList;

            // Property type is of type ComponentJson or Row. For example property type object would throw exception.
            if (UtilFramework.IsSubclassOf(propertyType, typeof(ComponentJson2)) || UtilFramework.IsSubclassOf(propertyType, typeof(Row)))
            {
                return;
            }

            // Property type is class Type. Property value typeof(int) is class RuntimeType (which derives from class Type)
            if (propertyType == typeof(Type))
            {
                return;
            }

            // Property is ComponentJson.List
            if (property.IsComponentJsonList)
            {
                return;
            }

            foreach (var item in propertyValueList)
            {
                if (item != null)
                {
                    // Property type is equal to value. No inheritance.
                    if (!(UtilFramework.TypeUnderlying(propertyType) == item.GetType()))
                    {
                        throw new Exception(string.Format("Combination property type and value type not supported! (PropertyName={0}.{1}; PropertyType={2}; ValueType={3}; Value={4};)", propertyInfo.DeclaringType.Name, propertyInfo.Name, propertyType.Name, item.GetType().Name, item));
                    }
                }
            }
        }

        /// <summary>
        /// Returns true, if property is a reference to ComponentJson. 
        /// </summary>
        private bool IsComponentJsonReference(PropertyInfo propertyInfo)
        {
            bool result = false;
            Property property = new Property(propertyInfo, null);
            bool isComponentJson = UtilFramework.IsSubclassOf(property.PropertyType, typeof(ComponentJson2));
            if (!property.IsComponentJsonList && isComponentJson) // Is it a component reference?
            {
                if (property.PropertyEnum == PropertyEnum.List || property.PropertyEnum == PropertyEnum.Dictionary)
                {
                    throw new Exception("ComponentJson reference supported only for property! Not for list and dictionary!");
                }
                result = true;
            }
            return result;
        }

        /// <summary>
        /// Serialize ComponentJson or Row objects.
        /// </summary>
        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            // Serialize Type object
            if (typeof(T) == typeof(Type))
            {
                JsonSerializer.Serialize(writer, UtilJson2.TypeToName(value as Type));
                return;
            }

            // Serialize data row object
            if (UtilFramework.IsSubclassOf(typeof(T), typeof(Row)))
            {
                if (ConverterFactory.IsClient)
                {
                    return; // Exclude data Row for client (Angular)-
                }
                writer.WriteStartObject();
                writer.WritePropertyName("$typeRow");
                JsonSerializer.Serialize(writer, UtilJson2.TypeToName(value.GetType()));
                writer.WritePropertyName("$row");
                JsonSerializer.Serialize(writer, value, value.GetType()); // Native serialization of data row values (no inheritance)
                writer.WriteEndObject();
                return;
            }

            // ComponentJson or Row object start
            writer.WriteStartObject();

            // Type information
            if (ConverterFactory.IsClient == false)
            {
                writer.WritePropertyName("$type"); // Note: Type information could be omitted if property type is equal to property value type
                JsonSerializer.Serialize(writer, UtilJson2.TypeToName(value.GetType()));
            }

            // Loop through properties
            foreach (var propertyInfo in value.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                var propertyValue = propertyInfo.GetValue(value);
                if (propertyValue == null) // IgnoreNullValues
                {
                    continue;
                }
                if (propertyValue is ICollection list && list.Count == 0)
                {
                    continue;
                }
                if (propertyValue is int propertyValueInt && propertyValueInt == 0) // DefaultValueHandling.Ignore
                {
                    continue;
                }
                if (propertyValue is bool propertyValueBool && propertyValueBool == false) // DefaultValueHandling.Ignore
                {
                    continue;
                }
                var attribute = propertyInfo.GetCustomAttribute<JsonSerializeAttribute>();
                if (attribute != null)
                {
                    if (attribute.JsonSerialize == JsonSerialize.Exclude || (ConverterFactory.IsClient && attribute.JsonSerialize == JsonSerialize.ServerOnly))
                    {
                        continue;
                    }
                }

                // Property has value
                ValidatePropertyAndValueType(propertyInfo, propertyValue);

                if (IsComponentJsonReference(propertyInfo))
                {
                    // Serialize ComponentJson reference
                    if (ConverterFactory.IsClient)
                    {
                        continue; // Exclude ComponentJson reference for client
                    }
                    writer.WritePropertyName(propertyInfo.Name);
                    writer.WriteStartObject();
                    writer.WritePropertyName("$type");
                    JsonSerializer.Serialize(writer, "$componentJsonReference");
                    writer.WritePropertyName("$id");
                    var id = ((ComponentJson2)propertyValue).Id;
                    JsonSerializer.Serialize<int>(writer, id);
                    writer.WriteEndObject();
                }
                else
                {
                    // Serialize property value
                    writer.WritePropertyName(propertyInfo.Name);
                    JsonSerializer.Serialize(writer, propertyValue, propertyInfo.PropertyType, options);
                }
            }

            // ComponentJson or Row object end
            writer.WriteEndObject();
        }
    }
}