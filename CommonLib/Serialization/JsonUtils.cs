
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace CommonLibCore.CommonLib.Serialization
{
    public static class JsonUtils
    {
        public static string SerializeCustom(object entity)
        {
            return SerializeCustom(entity, "yyyy'-'MM'-'dd' 'HH':'mm':'ss");
        }

        public static string SerializeCustom(object entity, string formatDate)
        {
            // IsoDateTimeConverter sẻ chuyển đổi các trường dữ liệu dạng isoDateString
            return Serialize(entity, new IsoDateTimeConverter() { DateTimeFormat = formatDate });
        }

        public static string Serialize(object entity, params JsonConverter[] converters)
        {
            if (entity == null)
            {
                return string.Empty;
            }
            try
            {
                return JsonConvert.SerializeObject(entity, converters);
            }
            catch (Exception ex)
            {
                //TraceLog.WriteError("Serialize object:{0},Error:{1}", entity.GetType().FullName, ex);
                return string.Empty;
            }
        }

        public static T DeserializeCustom<T>(string entity, string formatDate = "yyyy'-'MM'-'dd' 'HH':'mm':'ss")
        {
            return Deserialize<T>(entity, new IsoDateTimeConverter() { DateTimeFormat = formatDate });
        }

        public static T Deserialize<T>(string entity, params JsonConverter[] converters)
        {
            if (string.IsNullOrEmpty(entity))
            {
                return default(T);
            }
            return JsonConvert.DeserializeObject<T>(entity, converters);
        }

        public static T Deserialize<T>(string entity)
        {
            if (string.IsNullOrEmpty(entity))
            {
                return default(T);
            }
            return JsonConvert.DeserializeObject<T>(entity, new JsonSerializerSettings() { Error = HandleDeserializationError });
        }
        public static void HandleDeserializationError(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs errorArgs)
        {
            var currentError = errorArgs.ErrorContext.Error.Message;
            errorArgs.ErrorContext.Handled = true;
        }
        public static object DeserializeCustom(string entity, Type type, string formatDate = "yyyy'-'MM'-'dd' 'HH':'mm':'ss")
        {
            return Deserialize(entity, type, new IsoDateTimeConverter() { DateTimeFormat = formatDate });
        }

        public static object Deserialize(string entity, Type type, params JsonConverter[] converters)
        {
            if (type == null || string.IsNullOrEmpty(entity))
            {
                return null;
            }
            return JsonConvert.DeserializeObject(entity, type, converters);
        }

    }
}
