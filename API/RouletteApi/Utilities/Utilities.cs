using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;

namespace Utilities
{
    public static class Utilities
    {
        private static readonly JsonSerializerSettings _settings = new()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.Indented
        };

        public static string JsonSerialize(this object obj, JsonSerializerSettings settings = null) => JsonConvert.SerializeObject(obj, settings ?? _settings);
        public static T ToEntity<T>(this string obj) where T : class, new()
        {
            PropertyInfo[] piPropiedades = typeof(T).GetProperties();
            T returnObj = new();
            JObject jObj = JObject.Parse(obj);
            foreach (PropertyInfo piPropiedad in piPropiedades)
                try
                {
                    string stringValue = (string)jObj.SelectToken(piPropiedad.Name);
                    TypeConverter tc = TypeDescriptor.GetConverter(piPropiedad.PropertyType);
                    if (stringValue != null)
                    {
                        object value = tc.ConvertFromString(null, CultureInfo.InvariantCulture, stringValue);
                        piPropiedad.SetValue(returnObj, value);
                    }
                }
                catch { }

            return returnObj;
        }
        public static T ToEntitySimple<T>(this string obj) => JsonConvert.DeserializeObject<T>(obj);
        public static List<T> ToEntityList<T, D>(this string objs) where T : class, new() where D : class, new()
        {
            PropertyInfo[] piPropiedades = typeof(T).GetProperties();
            List<T> returnObjList = new();
            var jArray = JsonConvert.DeserializeObject<dynamic[]>(objs);
            foreach (var obj in jArray)
            {
                JObject jObj = JObject.Parse(obj.ToString());
                T preReturnObj = new();
                foreach (PropertyInfo piPropiedad in piPropiedades)
                    if (!typeof(System.Collections.IList).IsAssignableFrom(piPropiedad.PropertyType))
                    {
                        string stringValue = (string)(jObj.SelectToken(piPropiedad.Name));
                        TypeConverter tc = TypeDescriptor.GetConverter(piPropiedad.PropertyType);
                        if (stringValue != null)
                        {
                            object value = tc.ConvertFromString(null, CultureInfo.InvariantCulture, stringValue);
                            piPropiedad.SetValue(preReturnObj, value);
                        }
                    }
                    else if (!typeof(T).Equals(typeof(D)) && !string.IsNullOrWhiteSpace(jObj.SelectToken(piPropiedad.Name).ToString()))
                    {
                        JArray jArraySub = JArray.Parse(jObj.SelectToken(piPropiedad.Name).ToString());
                        List<D> asingObjSub = ToEntityList<D, D>(jArraySub.ToString());
                        piPropiedad.SetValue(preReturnObj, asingObjSub);
                    }
                returnObjList.Add(preReturnObj);
            }

            return returnObjList;
        }
        public static List<T> ToEntityListSimple<T>(this string objs) => JsonConvert.DeserializeObject<List<T>>(objs);
        public static Dictionary<T, D> ToDictionary<T, D>(this string objs) => JsonConvert.DeserializeObject<Dictionary<T, D>>(objs);
        public static bool TryParseJson<T>(this string json, out T result)
        {
            bool success = true;
            if (string.IsNullOrEmpty(json))
            {
                success = false;
                result = (T)(object)string.Empty;
                return success;
            }
            var settings = new JsonSerializerSettings
            {
                Error = (sender, args) => { success = false; args.ErrorContext.Handled = true; },
                MissingMemberHandling = MissingMemberHandling.Error
            };
            result = JsonConvert.DeserializeObject<T>(json, settings);

            return success;
        }
        public static TResponse TryCatch<TResponse>(this Func<TResponse> func, Action<Exception> handleError = null)
        {
            try
            {
                return func();
            }
            catch (Exception e)
            {
                handleError?.Invoke(e);
                throw;
            }
        }
        public static TResponse TryCatch<TResponse>(this Func<TResponse> func, Func<Exception, TResponse> handleError)
        {
            try
            {
                return func();
            }
            catch (Exception e)
            {
                return handleError(e);
            }
        }
        public static async Task<TResponse> TryCatchAsync<TResponse>(this Func<Task<TResponse>> func, Action<Exception> handleError = null)
        {
            try
            {
                return await func();
            }
            catch (Exception e)
            {
                handleError?.Invoke(e);
                throw;
            }
        }
        public static async Task<TResponse> TryCatchAsync<TResponse>(this Func<Task<TResponse>> func, Func<Exception, Task<TResponse>> handleError)
        {
            try
            {
                return await func();
            }
            catch (Exception e)
            {
                return await handleError(e);
            }
        }
    }
}
