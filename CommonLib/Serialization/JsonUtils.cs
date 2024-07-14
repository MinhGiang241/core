using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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
    }
}
