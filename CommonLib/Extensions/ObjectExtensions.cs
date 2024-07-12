
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CommonLibCore.CommonLib.Attributes;
using CommonLibCore.CommonLib.Reflect;
using CommonLibCore.CommonLib.Serialization;
using Confluent.Kafka;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
namespace CommonLibCore.CommonLib
{
    public static class ObjectExtention
    {
        public static string Filter(this string str, List<char> charsToRemove)
        {
            charsToRemove.ForEach(c => str = str.Replace(c.ToString(), String.Empty));
            return str;
        }
        public static List<string> GetAllFieldNameWithoutSubFields(this object value, bool ignoreNull = false)
        {
            List<string> fields = new List<string>();
            if (value.GetType() == typeof(ExpandoObject))
            {
                IDictionary<string, object> propertyValues = (ExpandoObject)value;
                foreach (var property in propertyValues.Keys)
                {
                    object fieldValue = propertyValues[property];
                    if (fieldValue == null)
                    {
                        if (!ignoreNull)
                            fields.Add(property);
                        continue;
                    }
                    fields.Add(property);
                }
            }
            else
            {

                foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(value.GetType()))
                {

                    object fieldValue = property.GetValue(value);
                    if (fieldValue == null)
                    {
                        if (!ignoreNull)
                            fields.Add(property.Name);
                        continue;
                    }
                    fields.Add(property.Name);
                }
            }
            return fields;
        }
        public static List<string> GetAllFieldName(this object value, bool ignoreNull = false)
        {
            List<string> fields = new List<string>();
            if (value.GetType() == typeof(ExpandoObject))
            {
                IDictionary<string, object> propertyValues = (ExpandoObject)value;
                foreach (var property in propertyValues.Keys)
                {
                    object fieldValue = propertyValues[property];
                    if (fieldValue == null)
                    {
                        if (!ignoreNull)
                            fields.Add(property);
                        continue;
                    }
                    Type fieldType = fieldValue.GetType();

                    if (fieldType == typeof(int) || fieldType == typeof(string) || fieldType == typeof(long) || fieldType == typeof(DateTime) || fieldType == typeof(ObjectId) || fieldType == typeof(double) || fieldType == typeof(float) || fieldType == typeof(bool) || fieldType.BaseType == typeof(Enum))
                        fields.Add(property);
                    else
                    {
                        var subFields = GetAllFieldName(fieldValue);
                        fields.AddRange(subFields.Select(s => $"{property}.{s}"));
                    }
                }
            }
            else
            {

                foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(value.GetType()))
                {

                    object fieldValue = property.GetValue(value);
                    if (fieldValue == null)
                    {
                        if (!ignoreNull)
                            fields.Add(property.Name);
                        continue;
                    }
                    Type fieldType = property.PropertyType;
                    if (fieldType == typeof(int) || fieldType == typeof(string) || fieldType == typeof(long) || fieldType == typeof(DateTime) || fieldType == typeof(ObjectId) || fieldType == typeof(double) || fieldType == typeof(float) || fieldType == typeof(bool) || fieldType.BaseType == typeof(Enum))
                        fields.Add(property.Name);
                    else
                    {
                        var subFields = GetAllFieldName(fieldValue);
                        fields.AddRange(subFields.Select(s => $"{property.Name}.{s}"));
                    }
                }
            }
            return fields;
        }
        public static bool IsList(this object o)
        {
            if (o == null) return false;
            return o is IList &&
                   o.GetType().IsGenericType &&
                   o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
        }
        public static bool IsDictionary(this object o)
        {
            if (o == null) return false;
            return o is IDictionary &&
                   o.GetType().IsGenericType &&
                   o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>));
        }
        public static dynamic ToDynamic(this object value)
        {
            IDictionary<string, object> expando = new ExpandoObject();

            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(value.GetType()))
                expando.Add(property.Name, property.GetValue(value));
            return expando as ExpandoObject;
        }
        public static dynamic JsonStringToDynamic(this string value)
        {
            try
            {
                var converter = new ExpandoObjectConverter();
                dynamic message = JsonConvert.DeserializeObject<ExpandoObject>(value, converter);
                return message;
            }
            catch (Exception ex)
            {
                return value;
            }
        }

        public static Type GetFieldType(this Type T, string fieldName)
        {
            List<string> PropertyList = new List<string>();
            PropertyInfo[] props = T.GetProperties();
            foreach (PropertyInfo prop in props)
            {
                if (prop.Name == fieldName)
                {
                    return prop.PropertyType;
                }
            }
            return null;
        }

        public static List<string> GetSecondKeys<T>()
        {
            List<string> PropertyList = new List<string>();
            PropertyInfo[] props = typeof(T).GetProperties();
            foreach (PropertyInfo prop in props)
            {
                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    SecondaryIdAttribute authAttr = attr as SecondaryIdAttribute;
                    if (authAttr != null)
                    {
                        PropertyList.Add(prop.Name);
                    }
                }
            }
            return PropertyList;
        }
        public static bool HasSecondKey<T>()
        {
            PropertyInfo[] props = typeof(T).GetProperties();
            foreach (PropertyInfo prop in props)
            {
                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    SecondaryIdAttribute authAttr = attr as SecondaryIdAttribute;
                    if (authAttr != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        //tìm giá trị của thuộc tính Id --> phục vụ cho cache
        public static object GetKeyValue(this object obj)
        {
            String KeyName = obj.GetType().Name + "Id";
            var IndexProp = obj.GetType().GetProperties().Where(p => p.Name == "Id" || p.Name == KeyName).SingleOrDefault();
            if (IndexProp != null && !IndexProp.IsNullOrEmpty())
            {
                return IndexProp.GetValue(obj);
            }
            return "";
        }
        /// <summary>
        /// check xem đối tượng có thuộc tính key không
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool HasKey(this object obj, string KeyName, bool ignoreCase = true)
        {
            PropertyInfo property = null;
            if (ignoreCase)
            {
                property = obj.GetType().GetProperties().Where(p => p.Name.ToLower() == KeyName.ToLower()).SingleOrDefault();
            }
            else
                property = obj.GetType().GetProperties().Where(p => p.Name == KeyName).SingleOrDefault();
            return property != null;
        }
        /// <summary>
        /// cungx tương tự cách getFieldvalue thôi nhưng csai này lâ từ nguồn trên mạng và thây cách xử lý nó ngắn hơn? 
        /// Tuy nhiên cái này chưa xử lý case như hàm kia
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static object GetDeepPropertyValue(this object instance, string path, bool ignoreCase = true)
        {
            var pp = path.Split('.');
            Type t = instance.GetType();
            foreach (var prop in pp)
            {
                PropertyInfo propInfo;
                if (!ignoreCase)
                    propInfo = t.GetProperty(prop);
                else
                {
                    propInfo = t.GetProperties().Where(p => p.Name.ToLower() == prop.ToLower()).FirstOrDefault();
                }
                if (propInfo != null)
                {
                    instance = propInfo.GetValue(instance, null);
                    t = propInfo.PropertyType;
                }
                else
                    throw new ArgumentException("Properties path is not correct");
            }
            return instance;
        }
        /// <summary>
        /// Tìm giá trị của thuôc tín KeyName
        /// Hàm này dùng được cho mọi loại đối tượng bao gồm cả dynamic, anonymous chứ không như hàm Deep ở trên
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="KeyName"></param>
        /// <returns></returns>
        public static object GetFieldValue(this object obj, string KeyName, bool ignoreCase = true)
        {
            string subName = "";
            if (obj == null)
                return null;
            if (obj.GetType() == typeof(Dictionary<string, object>))
            {
                Dictionary<string, object> dic = (Dictionary<string, object>)obj;
                if (dic.ContainsKey(KeyName))
                    return dic[KeyName];
                else return null;
            }
            if (KeyName.IndexOf('.') >= 0)
            {
                int dot = KeyName.IndexOf('.');
                subName = KeyName.Substring(dot + 1);
                KeyName = KeyName.Substring(0, dot);
            }
            if (obj.GetType() == typeof(ExpandoObject))
            {
                IDictionary<string, object> propertyValues = (ExpandoObject)obj;

                var value = propertyValues[KeyName];
                if (subName.IsNullOrEmpty())
                {
                    return value;
                }
                else
                {
                    return value.GetFieldValue(subName, ignoreCase);
                }

            }
            else
            {
                PropertyInfo property = null;
                if (ignoreCase)
                {
                    property = obj.GetType().GetProperties().Where(p => p.Name.ToLower() == KeyName.ToLower()).SingleOrDefault();
                }
                else
                    property = obj.GetType().GetProperties().Where(p => p.Name == KeyName).SingleOrDefault();
                if (property != null && !property.IsNullOrEmpty())
                {
                    var result = property.GetValue(obj);
                    if (subName.IsNullOrEmpty())
                        return result;
                    else
                        return result.GetFieldValue(subName);
                }
            }
            //}

            return "";
        }
        public static void Set(this object obj, String FieldName, object Value, bool ignoreCase = false)
        {
            var IndexProp = obj.GetType().GetProperties().Where(p => (p.Name == FieldName || (ignoreCase && p.Name.ToLower() == FieldName.ToLower()))).SingleOrDefault();
            if (IndexProp != null && !IndexProp.IsNullOrEmpty())
            {
                IndexProp.SetValue(obj, Value);
            }
        }

        public static string GetMemberName<T, TValue>(Expression<Func<T, TValue>> memberAccess)
        {
            return ((MemberExpression)memberAccess.Body).Member.Name;
        }
        public static void UpdateField<T>(this IEnumerable<T> entities, Func<T, T> func)
        {
            foreach (var entity in entities)
            {
                func(entity);
            }
        }

        public static void CloneAllFieldTo<T>(this object Source, T DestObj)
        {
            PropertyInfo[] SourceProperties = Source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo[] DestProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo[] selectProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            //Expression.Property(Expression.Constant(Source,typeof(T)),)
            foreach (var selectProp in selectProperties)
            {
                String Name = selectProp.Name;
                try
                {
                    var FindInSource = SourceProperties.Where(s => s.Name == Name).SingleOrDefault();
                    var FindInDest = DestProperties.Where(s => s.Name == Name).SingleOrDefault();
                    if (FindInSource != null && FindInDest != null)
                    {
                        FindInDest.SetValue(DestObj, FindInSource.GetValue(Source));
                    }
                }
                catch
                {
                    //có thể xảy ra lỗi không giống kiểu dữ liệu ở đấy --> sẽ xử lý riêng

                }
            }
        }

        //hàm này sẽ copy toàn bộ các trường được khai báo từ T sang TObject
        /// <summary>
        /// data.CloneField(input, x => new { x.password, x.userName, x.Id });
        /// Copy các trường từ data sang input 
        /// Nếu ignore = true: Copy TRỪ các trường: password, userName, Id
        /// Nếu ignore = false: Copy các trường được khai báo sang.
        /// Sử dụng trong trường hợp không cho thay đổi các field này, kể cả client có gửi lên 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TField"></typeparam>
        /// <param name="Source"></param>
        /// <param name="DestObj"></param>
        /// <param name="member"></param>
        /// <param name="cloneIfNotEmpty"></param>
        public static void CloneField<T, TField>(this object Source, T DestObj, Expression<Func<T, TField>> member, bool cloneIfNotEmpty = false, bool ignore = false)
        {
            PropertyInfo[] SourceProperties = Source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo[] DestProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo[] selectProperties = typeof(TField).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            //Expression.Property(Expression.Constant(Source,typeof(T)),)

            if (ignore == false)
            {
                foreach (var selectProp in selectProperties)
                {
                    string Name = selectProp.Name;
                    try
                    {
                        var FindInSource = SourceProperties.Where(s => s.Name == Name).SingleOrDefault();
                        var FindInDest = DestProperties.Where(s => s.Name == Name).SingleOrDefault();
                        if (FindInSource != null && FindInDest != null)
                        {
                            var valueOfSource = FindInSource.GetValue(Source);
                            if (cloneIfNotEmpty && !valueOfSource.IsNullOrEmpty())
                            {
                                FindInDest.SetValue(DestObj, FindInSource.GetValue(Source));
                            }
                            else
                                FindInDest.SetValue(DestObj, FindInSource.GetValue(Source));
                        }
                    }
                    catch
                    {
                        //có thể xảy ra lỗi không giống kiểu dữ liệu ở đấy --> sẽ xử lý riêng

                    }
                }
            }
            else
            {
                //ignore == true thì bỏ qua các trường trong member
                foreach (var selectProp in SourceProperties)
                {
                    string Name = selectProp.Name;
                    if (selectProperties.Select(x => x.Name == Name) == null)
                    {
                        try
                        {
                            var FindInSource = SourceProperties.Where(s => s.Name == Name).SingleOrDefault();
                            var FindInDest = DestProperties.Where(s => s.Name == Name).SingleOrDefault();
                            if (FindInSource != null && FindInDest != null)
                            {
                                var valueOfSource = FindInSource.GetValue(Source);
                                if (cloneIfNotEmpty && !valueOfSource.IsNullOrEmpty())
                                {
                                    FindInDest.SetValue(DestObj, FindInSource.GetValue(Source));
                                }
                                else
                                    FindInDest.SetValue(DestObj, FindInSource.GetValue(Source));
                            }
                        }
                        catch
                        {
                            //có thể xảy ra lỗi không giống kiểu dữ liệu ở đấy --> sẽ xử lý riêng

                        }
                    }
                }
            }

        }

        public static void CloneDictionary(this Dictionary<string, object> source, Dictionary<string, object> dest)
        {
            foreach (var key in source.Keys)
            {
                dest[key] = source[key];
            }
        }
        public static void CloneField<T>(this object Source, T DestObj, params string[] member)
        {
            PropertyInfo[] SourceProperties = Source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo[] DestProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            //PropertyInfo[] selectProperties = typeof(TField).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            //Expression.Property(Expression.Constant(Source,typeof(T)),)
            foreach (var name in member)
            {
                string Name = name.GetRealName(typeof(T));
                if (Name.IsNullOrEmpty())
                    continue;
                try
                {
                    var FindInSource = SourceProperties.Where(s => s.Name == Name).SingleOrDefault();
                    var FindInDest = DestProperties.Where(s => s.Name == Name).SingleOrDefault();
                    if (FindInSource != null && FindInDest != null)
                    {
                        FindInDest.SetValue(DestObj, FindInSource.GetValue(Source));
                    }
                }
                catch
                {
                    //có thể xảy ra lỗi không giống kiểu dữ liệu ở đấy --> sẽ xử lý riêng

                }
            }
        }
        /// <summary>
        /// điền đủ thông tin vào các trường dữ liệu sẵn có nếu như nó null hoặc rỗng
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Source"></param>
        /// <returns></returns>
        public static void CloneIfNull<T>(this object Source, T DestObj)
        {
            PropertyInfo[] SourceProperties = Source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo[] DestProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var DestProp in DestProperties)
            {
                String Name = DestProp.Name;
                try
                {
                    var FindInSource = SourceProperties.Where(s => s.Name == Name).SingleOrDefault();
                    var FindInDest = DestProperties.Where(s => s.Name == Name).SingleOrDefault();
                    if (FindInSource != null && FindInDest != null)
                    {
                        var oldValue = FindInDest.GetValue(Source);
                        if (oldValue.IsNullOrEmpty())
                            FindInDest.SetValue(DestObj, FindInSource.GetValue(Source));
                    }
                }
                catch
                {
                    //có thể xảy ra lỗi không giống kiểu dữ liệu ở đấy --> sẽ xử lý riêng

                }
            }
        }

        public static T ConvertFromExpando<T>(this ExpandoObject source)
        {
            var obj = (T)Activator.CreateInstance(typeof(T));//new instance of T
            PropertyInfo[] DestProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (KeyValuePair<string, object> kvp in source)
            {
                Console.WriteLine(kvp.Key + ": " + kvp.Value);
                var DestProp = DestProperties.Where(d => d.Name.ToLower() == kvp.Key.ToLower()).SingleOrDefault();
                try
                {
                    if (DestProp.PropertyType.FullName == "System.DateTime")
                    {
                        if (kvp.Value.GetType() == typeof(DateTime))
                        {
                            DestProp.SetValue(obj, kvp.Value);
                        }
                        else
                            DestProp.SetValue(obj, ((string)kvp.Value).ToDateTime(DateTime.Now));
                    }
                    else if (DestProp.PropertyType == typeof(int))
                    {
                        DestProp.SetValue(obj, (int)kvp.Value);
                    }
                    else if (DestProp.PropertyType == typeof(bool))
                    {
                        DestProp.SetValue(obj, (bool)kvp.Value);
                    }
                    else if (DestProp.PropertyType == typeof(long))
                    {
                        DestProp.SetValue(obj, (long)kvp.Value);
                    }
                    else if (DestProp.PropertyType == typeof(double))
                    {
                        DestProp.SetValue(obj, (double)kvp.Value);
                    }
                    else if (DestProp.PropertyType == typeof(string))
                    {
                        DestProp.SetValue(obj, (string)kvp.Value);
                    }
                    else if (DestProp.PropertyType.BaseType == typeof(Enum))
                    {
                        try
                        {
                            DestProp.SetValue(obj, kvp.Value);
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                DestProp.SetValue(obj, Enum.Parse(DestProp.PropertyType, (string)kvp.Value, true));
                            }
                            catch (Exception exx)
                            {

                            }
                        }

                    }

                    else
                    {
                        try
                        {
                            DestProp.SetValue(obj, kvp.Value);
                        }
                        catch
                        {
                            var value = kvp.Value.ConvertFromAnonymous(DestProp.PropertyType);
                            DestProp.SetValue(obj, value);
                        }

                    }
                }
                catch (Exception ex)
                {

                }
            }
            return obj;
        }
        public static T CloneFromDictionary<T>(this IDictionary<string, object> Dic, bool igNoreWhiteCard = false)
        {
            var obj = (T)Activator.CreateInstance(typeof(T));//new instance of T
            PropertyInfo[] DestProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var DestProp in DestProperties)
            {
                string Name = DestProp.Name;
                var checkKey = Dic.Where(d => d.Key.ToLower() == Name.ToLower());
                if (checkKey.Count() > 0)
                    Name = checkKey.ElementAt(0).Key;
                try
                {
                    try
                    {
                        if (DestProp.PropertyType.FullName == "System.DateTime")
                        {
                            DestProp.SetValue(obj, ((string)Dic[Name]).ToDateTime(DateTime.Now));
                        }
                        else if (DestProp.PropertyType == typeof(int))
                        {
                            DestProp.SetValue(obj, (int)Dic[Name]);
                        }
                        else if (DestProp.PropertyType == typeof(bool))
                        {
                            DestProp.SetValue(obj, (bool)Dic[Name]);
                        }
                        else if (DestProp.PropertyType == typeof(long))
                        {
                            DestProp.SetValue(obj, (long)Dic[Name]);
                        }
                        else if (DestProp.PropertyType == typeof(double))
                        {
                            DestProp.SetValue(obj, (double)Dic[Name]);
                        }
                        if (DestProp.PropertyType == typeof(string))
                        {
                            DestProp.SetValue(obj, (string)Dic[Name]);
                        }
                        else
                            DestProp.SetValue(obj, Dic[Name]);
                    }
                    catch (Exception ex)
                    {

                    }
                }
                catch
                {
                    //có thể xảy ra lỗi không giống kiểu dữ liệu ở đấy --> sẽ xử lý riêng
                }
            }
            return obj;
        }

        /// <summary>
        /// tạo ra 1 đối tượng mới hoàn toàn
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Source"></param>
        /// <returns></returns>
        public static T Clone<T>(this object Source, bool igNoreWhiteCard = false)
        {
            var obj = (T)Activator.CreateInstance(typeof(T));//new instance of T
            var sourceType = Source.GetType().GetProperties();
            PropertyInfo[] SourceProperties = Source.GetType().GetProperties();
            PropertyInfo[] DestProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var DestProp in DestProperties)
            {
                string Name = DestProp.Name;
                try
                {

                    var FindInSource = SourceProperties.Where(s => s.Name == Name || (igNoreWhiteCard && s.Name.ToLower().Replace("_", "") == Name.ToLower().Replace("_", ""))).SingleOrDefault();
                    if (FindInSource != null)
                    {
                        //if (FindInSource.PropertyType.FullName == DestProp.PropertyType.FullName)
                        //    DestProp.SetValue(obj, FindInSource.GetValue(Source));
                        if (DestProp.PropertyType.FullName == "System.DateTime" && FindInSource.PropertyType.FullName == "System.String")
                        {
                            DestProp.SetValue(obj, ((string)FindInSource.GetValue(Source)).ToDateTime(DateTime.Now));
                        }
                        else if (FindInSource.PropertyType.FullName == "System.DateTime" && DestProp.PropertyType.FullName == "System.String")
                        {
                            DestProp.SetValue(obj, FindInSource.GetValue(Source).ToString());
                        }
                        else
                        {
                            try
                            {
                                DestProp.SetValue(obj, FindInSource.GetValue(Source));
                            }
                            catch { }
                        }
                    }
                }
                catch
                {
                    //có thể xảy ra lỗi không giống kiểu dữ liệu ở đấy --> sẽ xử lý riêng
                }
            }
            return obj;
        }
        public static Dictionary<string, object> NameValueToDictionary(this NameValueCollection a)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            foreach (var k in a.AllKeys)
            {
                try
                {
                    object value = a[k];
                    dict.Add(k, value);
                }
                catch (Exception ex) { }
            }
            return dict;
        }
        public static Dictionary<string, object> ConvertToDictionary(this object value)
        {
            if (value == null)
                return new Dictionary<string, object>();
            if (value.GetType() == typeof(ExpandoObject))
            {
                return new Dictionary<string, object>((ExpandoObject)value);
            }

            //check nếu T là expando oject thì dùng JsonConvert luôn uôn 
            if (value.GetType() == typeof(object))
            {
                string str = value.ToJson();
                return JsonConvert.DeserializeObject<Dictionary<string, object>>(str);
            }
            else if (value.GetType() == typeof(Dictionary<string, object>))
            {
                return (Dictionary<string, object>)value;
            }
            var dic = new Dictionary<string, object>();
            PropertyInfo[] DestProperties = value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in DestProperties)
            {
                try
                {
                    dic.Add(property.Name, property.GetValue(value));
                }
                catch (Exception ex)
                {

                }
            }
            return dic;
        }
        public static dynamic ToSafeDynamic(this object obj)
        {
            //would be nice to restrict to anonymous types - but alas no.
            IDictionary<string, object> toReturn = new ExpandoObject();
            foreach (var prop in obj.GetType().GetProperties(
              BindingFlags.Public | BindingFlags.Instance)
              .Where(p => p.CanRead))
            {
                try
                {
                    toReturn[prop.Name] = prop.GetValue(obj, null);
                }
                catch (Exception ex)
                {

                }
            }

            return toReturn;
        }
        public static ExpandoObject ConvertToExpando(this object value)
        {
            if (value == null)
                return null;
            if (value.GetType() == typeof(ExpandoObject))
                return (ExpandoObject)value;
            return JsonConvert.SerializeObject(value).JsonStringToDynamic();
        }
        public static object ConvertFromAnonymous(this object value, Type type)
        {
            return JsonConvert.DeserializeObject(JsonConvert.SerializeObject(value), type);
        }
        public static T ConvertFromDictionary<T>(this Dictionary<string, object> dic, bool stringifyObjectId = true)
        {
            try
            {
                IDictionary<string, object> expando = new ExpandoObject();
                ExpandoObject dynamic;
                foreach (var key in dic.Keys)
                {
                    var value = dic[key];
                    if (value is ObjectId && stringifyObjectId)
                    {
                        expando.Add(key, value.ToString());
                    }
                    else
                        expando.Add(key, value);
                }
                dynamic = expando as ExpandoObject;
                T result = default(T);

                result = dynamic.ConvertFromExpando<T>();
                return result;
            }
            catch (Exception ex)
            {
                return JsonConvert.DeserializeObject<T>(dic.ToJson());
            }
        }
        /// <summary>
        /// Convert from anonimousObject to an Object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T ConvertFromAnonymous<T>(this object value)
        {
            try
            {
                IDictionary<string, object> expando = new ExpandoObject();
                ExpandoObject dynamic;
                if (value.GetType() == typeof(ExpandoObject))
                {
                    dynamic = (ExpandoObject)value;
                }
                else if (value.GetType() == typeof(Dictionary<string, object>))
                {
                    Dictionary<string, object> dic = (Dictionary<string, object>)(value);
                    foreach (var key in dic.Keys)
                    {
                        expando.Add(key, dic[key]);
                    }
                    dynamic = expando as ExpandoObject;
                }
                else
                {
                    foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(value.GetType()))
                    {
                        var objValue = property.GetValue(value);
                        expando.Add(property.Name, property.GetValue(value));
                    }
                    dynamic = expando as ExpandoObject;
                }
                T result = default(T);

                result = dynamic.ConvertFromExpando<T>();
                return result;
            }
            catch (Exception ex)
            {
                return JsonConvert.DeserializeObject<T>(value.ToJson());
            }
        }
        public static object ToObject<T>(this T obj, Func<T, object> predicate)
        {
            return predicate(obj);
        }
        public static bool IsSimple(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // nullable type, check if the nested type is simple.
                return IsSimple(typeInfo.GetGenericArguments()[0]);
            }
            return typeInfo.IsPrimitive
              || typeInfo.IsEnum
              || type.Equals(typeof(string))
              || type.Equals(typeof(decimal));
        }
        public static T CreateInstance<T>(this Type type, params object[] args)
        {
            return (T)FastActivator.Create(type, args);
        }

        public static object CreateInstance(this Type type, params object[] args)
        {
            return FastActivator.Create(type, args);
        }

        public static bool IsEquals(this string a, string b, bool ignoreCase)
        {
            return MathUtils.IsEquals(a, b, ignoreCase);
        }

        public static bool StartsWith(this string a, string b, bool ignoreCase)
        {
            return MathUtils.StartsWith(a, b, ignoreCase);
        }
        public static bool IsEmpty(this string value)
        {
            return MathUtils.IsEmpty(value);
        }

        public static bool IsNullOrEmpty<T>(this T value)
        {
            return MathUtils.IsNullOrDbNull(value);
        }

        public static List<T> GetPaging<T>(this List<T> list, int pageIndex, int pageSize, out int pageCount)
        {
            int recordCount;
            return GetPaging(list, pageIndex, pageSize, out pageCount, out recordCount);
        }

        public static List<T> GetPaging<T>(this List<T> list, int pageIndex, int pageSize, out int pageCount, out int recordCount)
        {
            return MathUtils.GetPaging<T>(list, pageIndex, pageSize, out pageCount, out recordCount);
        }

        public static void InsertSort<T>(this List<T> list, T item, Comparison<T> comparison)
        {
            MathUtils.InsertSort<T>(list, item, comparison);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static List<T> QuickSort<T>(this List<T> list) where T : IComparable<T>
        {
            return MathUtils.QuickSort(list, (x, y) =>
            {
                if (x == null && y == null) return 0;
                if (x == null && y != null) return -1;
                if (x != null && y == null) return 1;

                return x.CompareTo(y);
            });
        }
        public static void QuickSort<T>(this List<T> list, Comparison<T> comparison)
        {
            MathUtils.QuickSort(list, comparison);
        }

        public static T ParseJson<T>(this string jsonStr)
        {
            return JsonUtils.DeserializeCustom<T>(jsonStr);
        }
        public static string ToJson<T>(this List<T> list)
        {
            return JsonUtils.SerializeCustom(list);
        }

        public static string ToJson(this object value, bool ignoreNull = true)
        {
            return JsonConvert.SerializeObject(value, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }
        public static int IndexOf(this byte[] bytes, byte[] pattern)
        {
            return MathUtils.IndexOf(bytes, pattern);
        }
        public static int IndexOf(this byte[] bytes, int offset, int length, byte[] pattern)
        {
            return MathUtils.IndexOf(bytes, offset, length, pattern);
        }
        public static T[] RandomSort<T>(this T[] array)
        {
            return RandomUtils.RandomSort(array);
        }

        public static List<T> RandomSort<T>(this List<T> list)
        {
            return RandomUtils.RandomSort(list);
        }

        public static string ToNotNullString(this object value)
        {
            return MathUtils.ToNotNullString(value);
        }

        public static string ToNotNullString(this object value, string defValue)
        {
            return MathUtils.ToNotNullString(value, defValue);
        }

        public static int ToCeilingInt(this object value)
        {
            return MathUtils.ToCeilingInt(value);
        }

        public static int ToFloorInt(this object value)
        {
            return MathUtils.ToFloorInt(value);
        }

        public static long ToLong(this object value)
        {
            return MathUtils.ToLong(value);
        }

        public static int ToInt(this object value)
        {
            return MathUtils.ToInt(value);
        }

        public static short ToShort(this object value)
        {
            return MathUtils.ToShort(value);
        }

        public static double ToDouble(this object value)
        {
            return MathUtils.ToDouble(value);
        }
        public static decimal ToDecimal(this object value)
        {
            return MathUtils.ToDecimal(value);
        }

        public static float ToFloat(this object value)
        {
            return MathUtils.ToFloat(value);
        }

        public static bool ToBool(this object value)
        {
            return MathUtils.ToBool(value);
        }

        public static byte ToByte(this object value)
        {
            return MathUtils.ToByte(value);
        }

        public static UInt64 ToUInt64(this object value)
        {
            return MathUtils.ToUInt64(value);
        }

        public static UInt32 ToUInt32(this object value)
        {
            return MathUtils.ToUInt32(value);
        }

        public static UInt16 ToUInt16(this object value)
        {
            return MathUtils.ToUInt16(value);
        }

        //public static DateTime ToDateTime(this object value)
        //{
        //    return ToDateTime(value, MathUtils.SqlMinDate);
        //}

        //public static DateTime ToDateTime(this object value, DateTime defValue)
        //{
        //    return MathUtils.ToDateTime(value, defValue);
        //}

        public static T ToEnum<T>(this object value)
        {
            return MathUtils.ToEnum<T>(value);
        }

        public static object ToEnum(this object value, Type type)
        {
            return MathUtils.ToEnum(value, type);
        }

        public static bool IsValid(this long value)
        {
            return IsValid(value, MathUtils.ZeroNum, long.MaxValue);
        }

        public static bool IsValid(this long value, long minValue, long maxValue)
        {
            return value >= minValue && value <= maxValue;
        }

        public static bool IsValid(this int value)
        {
            return IsValid(value, MathUtils.ZeroNum, int.MaxValue);
        }

        public static bool IsValid(this int value, int minValue, int maxValue)
        {
            return value >= minValue && value <= maxValue;
        }

        public static bool IsValid(this short value)
        {
            return IsValid(value, (short)MathUtils.ZeroNum, short.MaxValue);
        }

        public static bool IsValid(this short value, short minValue, short maxValue)
        {
            return value >= minValue && value <= maxValue;
        }

        public static bool IsValid(this byte value)
        {
            return IsValid(value, (byte)MathUtils.ZeroNum, byte.MaxValue);
        }

        public static bool IsValid(this byte value, byte minValue, byte maxValue)
        {
            return value >= minValue && value <= maxValue;
        }

        public static bool IsValid(this double value)
        {
            return IsValid(value, MathUtils.ZeroNum, double.MaxValue);
        }

        public static bool IsValid(this double value, double minValue, double maxValue)
        {
            return value >= minValue && value <= maxValue;
        }

        public static bool IsValid(this decimal value)
        {
            return IsValid(value, MathUtils.ZeroNum, decimal.MaxValue);
        }

        public static bool IsValid(this decimal value, decimal minValue, decimal maxValue)
        {
            return value >= minValue && value <= maxValue;
        }
    }

}

