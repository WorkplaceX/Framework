namespace Framework.Server.Json
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class JsonException : Exception
    {
        public JsonException(string message)
            : base(message)
        {

        }
    }

    public static class Util
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
            Util.TypeInfo(obj, obj.GetType(), out typeGroup, out valueType);
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
            Util.TypeInfo(obj, obj.GetType(), out typeGroup, out valueType);
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
                    if (value.GetType() == typeof(string))
                    {
                        typeGroup = TypeGroup.Value;
                        valueType = typeof(string);
                        return;
                    }
                    if (value.GetType() == typeof(int) || value.GetType() == typeof(double) || value.GetType() == typeof(Int64))
                    {
                        typeGroup = TypeGroup.Value;
                        valueType = typeof(double);
                        return;
                    }
                    if (value.GetType() == typeof(bool))
                    {
                        typeGroup = TypeGroup.Value;
                        valueType = typeof(bool);
                        return;
                    }
                }
            }
            if (fieldType.GetTypeInfo().IsValueType)
            {
                typeGroup = TypeGroup.Value;
                valueType = fieldType;
                return;
            }
            if (fieldType.GetTypeInfo().IsGenericType && fieldType.GetTypeInfo().GetGenericTypeDefinition() == typeof(List<>))
            {
                typeGroup = TypeGroup.List;
                valueType = fieldType.GetTypeInfo().GetGenericArguments().First();
                return;
            }
            if (fieldType.GetTypeInfo().IsGenericType && fieldType.GetTypeInfo().GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                typeGroup = TypeGroup.Dictionary;
                valueType = fieldType.GetTypeInfo().GetGenericArguments()[1];
                return;
            }
            if (fieldType == typeof(string))
            {
                typeGroup = TypeGroup.Value;
                valueType = fieldType;
                return;
            }
            if (fieldType.GetTypeInfo().GetConstructors().Count() > 0)
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
            Util.TypeInfo(value.Value, value.FieldType, out typeGroup, out valueType);
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
                Util.TypeInfo(value, fieldType, out typeGroup, out valueType);
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
                                throw new JsonException("Object has no Type field!"); // Add this code: "public string Type;" // TODO derived object can not be in different assembly.
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

        private static string Serialize(object obj, Type rootType)
        {
            SerializePrepare(obj, rootType, false);
            string result = JsonConvert.SerializeObject(obj, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore });
            SerializePrepare(obj, rootType, true);
            // TODO Disable Debug
            {
                string debugSource = JsonConvert.SerializeObject(obj, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All });
                object debugObj = Deserialize(result, rootType);
                SerializePrepare(debugObj, rootType, true);
                string debugDest = JsonConvert.SerializeObject(debugObj, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All });
                Util.Assert(debugSource == debugDest);
            }
            //
            return result;
        }

        public static string Serialize(object obj)
        {
            return Serialize(obj, obj.GetType());
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
            if (Nullable.GetUnderlyingType(type) != null)
            {
                type = Nullable.GetUnderlyingType(type);
            }
            return Convert.ChangeType(value, type);
        }

        public static Type TypeGet(string objectTypeString, Type rootType)
        {
            string ns = rootType.Namespace + ".";
            if (rootType.DeclaringType != null)
            {
                ns = rootType.DeclaringType.FullName + "+";
            }
            Type result = Type.GetType(ns + objectTypeString + ", " + rootType.GetTypeInfo().Assembly.FullName);
            Util.Assert(result != null);
            return result;
        }

        private static Type DeserializeTokenObjectType(JObject jObject, Type fieldType, Type rootType)
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
                        result = Util.TypeGet(objectTypeString, rootType);
                    }
                }
                //
                if (jObject.Property("TypeCSharp") != null)
                {
                    JValue jValue = jObject.Property("TypeCSharp").Value as JValue;
                    string objectTypeString = jValue.Value as string;
                    if (objectTypeString != null)
                    {
                        result = Util.TypeGet(objectTypeString, rootType);
                    }
                }
            }
            return result;
        }

        private static object DeserializeToken(JToken jToken, Type fieldType, Type rootType)
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
            Util.TypeInfo(value, fieldType, out typeGroup, out valueType);
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
                        Type objectType = DeserializeTokenObjectType(jObject, fieldType, rootType);
                        if (objectType != null)
                        {
                            result = Activator.CreateInstance(objectType);
                            foreach (var fieldInfo in result.GetType().GetTypeInfo().GetFields())
                            {
                                if (jObject != null)
                                {
                                    JProperty jProperty = jObject.Property(fieldInfo.Name);
                                    if (jProperty != null)
                                    {
                                        JToken jTokenChild = jProperty.Value;
                                        Type fieldTypeChild = fieldInfo.FieldType;
                                        object valueChild = DeserializeToken(jTokenChild, fieldTypeChild, rootType);
                                        fieldInfo.SetValue(result, valueChild);
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
                                object valueChild = DeserializeToken(jTokenChild, fieldTypeChild, rootType);
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
                                object valueChild = DeserializeToken(jTokenChild, fieldTypeChild, rootType);
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

        private static object Deserialize(string json, Type rootType)
        {
            JObject jObject = (JObject)JsonConvert.DeserializeObject(json);
            object result = DeserializeToken(jObject, rootType, rootType);
            SerializePrepare(result, rootType, true);
            return result;
        }

        public static T Deserialize<T>(string json)
        {
            return (T)Deserialize(json, typeof(T));
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
