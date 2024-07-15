using System.Text.RegularExpressions;
using CommonLibCore.CommonLib.Serialization;

namespace CommonLibCore.CommonLib
{
    public class MathUtils
    {
        public const int ZeroNum = 0;

        public static DateTime SqlMinDate = new DateTime(1753, 1, 1);

        private const long UnixEpoch = 621355968000000000L;
        /// <summary>
        /// Unix timestamp, val:1970-01-01 00:00:00 UTC
        /// </summary>
        public static readonly DateTime UnixEpochDateTime = new DateTime(UnixEpoch);

        /// <summary>
        /// 
        /// </summary>
        public static DateTime Now
        {
            get { return DateTime.Now; }
        }

        /// <summary>
        /// Use Utc time
        /// </summary>
        public static TimeSpan UnixEpochTimeSpan
        {
            get { return (DateTime.UtcNow - UnixEpochDateTime); }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static TimeSpan GetUnixEpochTimeSpan(DateTime date)
        {
            return date - UnixEpochDateTime;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ts"></param>
        /// <returns></returns>
        public static DateTime ToTimeFromUnixEpoch(TimeSpan ts)
        {
            return UnixEpochDateTime.Add(ts);
        }

        public static int WeekOfYear
        {
            get { return ToWeekOfYear(Now); }
        }

        public static int ToWeekOfYear(DateTime date)
        {
            int dayOfYear = date.DayOfYear;
            DateTime tempDate = new DateTime(date.Year, 1, 1);
            int tempDayOfWeek = (int)tempDate.DayOfWeek;
            tempDayOfWeek = tempDayOfWeek == 0 ? 7 : tempDayOfWeek;
            int index = (int)date.DayOfWeek;
            index = index == 0 ? 7 : index;
            DateTime retStartDay = date.AddDays(-(index - 1));
            DateTime retEndDay = date.AddDays(6 - index);
            int weekIndex = (int)Math.Ceiling(((double)dayOfYear + tempDayOfWeek - 1) / 7);
            if (retStartDay.Year < retEndDay.Year)
            {
                weekIndex = 1;
            }
            return weekIndex;
        }
        public static TimeSpan DiffDate(DateTime date)
        {
            return DiffDate(Now, date);
        }

        public static TimeSpan DiffDate(DateTime date1, DateTime date2)
        {
            return date1 - date2;
        }

        public static byte[] CharToByte(char c)
        {
            byte[] b = new byte[2];
            b[0] = (byte)((c & 0xFF00) >> 8);
            b[1] = (byte)(c & 0xFF);
            return b[0] == 0 ? new byte[] { b[1] } : b;
        }

        public static char ByteToChar(byte[] bytes, int startIndex = 0)
        {
            if (bytes == null || bytes.Length == 0)
            {
                throw new ArgumentOutOfRangeException("bytes");
            }
            if (startIndex > bytes.Length - 1)
            {
                throw new ArgumentOutOfRangeException("startIndex");
            }
            return bytes.Length > 1
                ? (char)(((bytes[startIndex] & 0xFF) << 8) | (bytes[startIndex + 1] & 0xFF))
                : (char)(bytes[startIndex] & 0xFF);
        }

        public static byte[] Join(params byte[][] args)
        {
            Int32 length = 0;
            foreach (byte[] tempbyte in args)
            {
                length += tempbyte.Length;
            }
            Byte[] bytes = new Byte[length];
            Int32 tempLength = 0;
            foreach (byte[] tempByte in args)
            {
                tempByte.CopyTo(bytes, tempLength);
                tempLength += tempByte.Length;
            }
            return bytes;
        }

        public static int IndexOf(byte[] bytes, byte[] pattern)
        {
            return IndexOf(bytes, 0, bytes.Length, pattern);
        }

        public static int IndexOf(byte[] bytes, int offset, int length, byte[] pattern)
        {
            int index = -1;
            int pos = offset;
            if (length > bytes.Length)
            {
                length = bytes.Length;
            }
            while (pos < length)
            {
                if (bytes[pos] == pattern[0])
                {
                    index = pos;
                    for (int i = 1; i < pattern.Length; i++)
                    {
                        if (pos + i >= length || pattern[i] != bytes[pos + i])
                        {
                            index = -1;
                            break;
                        }
                    }
                    if (index > -1)
                    {
                        break;
                    }
                }
                pos++;
            }
            return index;
        }

        public static bool IsEquals(string a, string b, bool ignoreCase)
        {
            return ignoreCase ? string.Equals(a.ToLower(), b.ToLower(), StringComparison.Ordinal) : string.Equals(a, b);
        }

        public static bool StartsWith(string a, string b, bool ignoreCase)
        {
            if (a == null) throw new ArgumentNullException("a");
            return ignoreCase ? a.ToLower().StartsWith(b.ToLower(), StringComparison.Ordinal) : a.StartsWith(b);
        }

        /// <summary>
        /// Convert to default value by type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object ToDefaultValue(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : "";
        }

        //private static string _runtimeBinPath;
        ///// <summary>
        ///// Get runtime bin path.
        ///// </summary>
        //public static string RuntimeBinPath
        //{
        //    get { return _runtimeBinPath ?? (_runtimeBinPath = AppDomain.CurrentDomain.SetupInformation.PrivateBinPath ?? RuntimePath); }
        //    set { _runtimeBinPath = value; }
        //}

        //private static string _runtimePath;
        ///// <summary>
        ///// Get runtime path.
        ///// </summary>
        //public static string RuntimePath
        //{
        //    get
        //    {
        //        return _runtimePath ?? (_runtimePath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase);
        //    }
        //}

        public static string ToHexMd5Hash(string str)
        {
            return CommonLibCore.CommonLib.Security.CryptoHelper.ToMd3Hash(str);
        }

        public static string ToHex(byte[] bytes)
        {
            return ToHex(bytes, 0, bytes.Length);
        }

        public static string ToHex(byte[] bytes, int offset, int count)
        {
            char[] c = new char[count * 2];
            byte b;

            for (int bx = 0, cx = 0; bx < count; ++bx, ++cx)
            {
                b = ((byte)(bytes[offset + bx] >> 4));
                c[cx] = (char)(b > 9 ? b + 0x37 + 0x20 : b + 0x30);

                b = ((byte)(bytes[offset + bx] & 0x0F));
                c[++cx] = (char)(b > 9 ? b + 0x37 + 0x20 : b + 0x30);
            }

            return new string(c);
        }

        public static decimal RoundCustom(decimal value, int decimals)
        {
            return (decimal)RoundCustom((double)value, decimals);
        }

        public static double RoundCustom(double value, int decimals)
        {
            if (value < 0)
            {
                return Math.Round(value + 5 / Math.Pow(10, decimals + 1), decimals, MidpointRounding.AwayFromZero);
            }
            return Math.Round(value, decimals, MidpointRounding.AwayFromZero);
        }

        public static int RoundCustom(decimal value)
        {
            return (int)RoundCustom(value, 0);
        }
        public static int RoundCustom(double value)
        {
            return (int)RoundCustom(value, 0);
        }

        public static bool IsMach(string pattern, string value)
        {
            if (pattern != null && value != null)
            {
                return new Regex(pattern).IsMatch(value);
            }
            return false;
        }

        public static bool IsMachVarName(string value)
        {
            return IsMach("[A-Za-z_][A-Za-z0-9_]*", value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="success"></param>
        /// <returns></returns>
        public static bool TryAdd(uint a, uint b, Action<uint> success)
        {
            uint increment;
            return TryAdd(a, b, success, out increment);
        }

        public static bool TryAdd(uint a, uint b, Action<uint> success, out uint increment)
        {
            increment = 0;
            uint r = a + b;
            if (r >= a)
            {
                increment = b;
                success(r);
                return true;
            }
            return false;
        }

        public static bool TryAdd(ushort a, ushort b, Action<ushort> success)
        {
            ushort increment;
            return TryAdd(a, b, success, out increment);
        }

        public static bool TryAdd(ushort a, ushort b, Action<ushort> success, out ushort increment)
        {
            increment = 0;
            if (a + b <= ushort.MaxValue)
            {
                var r = (ushort)(a + b);
                increment = b;
                success(r);
                return true;
            }
            return false;
        }
        public static bool TrySub(uint a, uint b, Action<uint> success)
        {
            uint decrement;
            return TrySub(a, b, success, out decrement);
        }

        public static bool TrySub(uint a, uint b, Action<uint> success, out uint decrement)
        {
            decrement = 0;
            if (a < b) return false;

            uint r = a - b;
            if (r <= a)
            {
                decrement = b;
                success(r);
                return true;
            }
            return false;
        }

        public static bool TrySub(ushort a, ushort b, Action<uint> success)
        {
            ushort decrement;
            return TrySub(a, b, success, out decrement);
        }

        public static bool TrySub(ushort a, ushort b, Action<uint> success, out ushort decrement)
        {
            decrement = 0;
            if (a < b) return false;

            int r = a - b;
            if (r >= 0 && r <= a)
            {
                decrement = b;
                success((ushort)r);
                return true;
            }
            return false;
        }

        public static long Addition(long value, long addValue)
        {
            return Addition(value, addValue, long.MaxValue);
        }

        public static long Addition(long value, long addValue, long maxValue)
        {
            long t = value + addValue;
            return t < -1 || t > maxValue ? maxValue : t;
        }
        public static int Addition(int value, int addValue)
        {
            return Addition(value, addValue, int.MaxValue);
        }
        public static int Addition(int value, int addValue, int maxValue)
        {
            int t = value + addValue;
            return t < -1 || t > maxValue ? maxValue : t;
        }
        public static short Addition(short value, short addValue)
        {
            return Addition(value, addValue, short.MaxValue);
        }

        public static short Addition(short value, short addValue, short maxValue)
        {
            short t = (short)(value + addValue);
            return t < -1 || t > maxValue ? maxValue : t;
        }

        public static byte Addition(byte value, byte addValue)
        {
            return Addition(value, addValue, byte.MaxValue);
        }

        public static byte Addition(byte value, byte addValue, byte maxValue)
        {
            byte t = (byte)(value + addValue);
            return t > maxValue ? maxValue : t;
        }

        public static float Addition(float value, float addValue)
        {
            return Addition(value, addValue, float.MaxValue);
        }

        public static float Addition(float value, float addValue, float maxValue)
        {
            float t = value + addValue;
            return t < -1 || t > maxValue ? maxValue : t;
        }

        public static double Addition(double value, double addValue)
        {
            return Addition(value, addValue, double.MaxValue);
        }

        public static double Addition(double value, double addValue, double maxValue)
        {
            double t = value + addValue;
            return t < -1 || t > maxValue ? maxValue : t;
        }

        public static decimal Addition(decimal value, decimal addValue)
        {
            return Addition(value, addValue, decimal.MaxValue);
        }

        public static decimal Addition(decimal value, decimal addValue, decimal maxValue)
        {
            decimal t = value + addValue;
            return t < -1 || t > maxValue ? maxValue : t;
        }

        public static long Subtraction(long value, long subValue)
        {
            return Subtraction(value, subValue, 0);
        }


        public static long Subtraction(long value, long subValue, long minValue)
        {
            long t = value - subValue;
            return t < minValue ? minValue : t;
        }

        public static int Subtraction(int value, int subValue)
        {
            return Subtraction(value, subValue, 0);
        }

        public static int Subtraction(int value, int subValue, int minValue)
        {
            int t = value - subValue;
            return t < minValue ? minValue : t;
        }

        public static short Subtraction(short value, short subValue)
        {
            return Subtraction(value, subValue, (short)0);
        }

        public static short Subtraction(short value, short subValue, short minValue)
        {
            short t = (short)(value - subValue);
            return t < minValue ? minValue : t;
        }

        public static byte Subtraction(byte value, byte subValue)
        {
            return Subtraction(value, subValue, (byte)0);
        }

        public static byte Subtraction(byte value, byte subValue, byte minValue)
        {
            byte t = (byte)(value - subValue);
            return t < minValue ? minValue : t;
        }

        public static float Subtraction(float value, float subValue)
        {
            return Subtraction(value, subValue, (float)0);
        }

        public static float Subtraction(float value, float subValue, float minValue)
        {
            float t = value - subValue;
            return t < minValue ? minValue : t;
        }

        public static double Subtraction(double value, double subValue)
        {
            return Subtraction(value, subValue, (double)0);
        }

        public static double Subtraction(double value, double subValue, double minValue)
        {
            double t = value - subValue;
            return t < minValue ? minValue : t;
        }

        public static decimal Subtraction(decimal value, decimal subValue)
        {
            return Subtraction(value, subValue, (decimal)0);
        }

        public static decimal Subtraction(decimal value, decimal subValue, decimal minValue)
        {
            decimal t = value - subValue;
            return t < minValue ? minValue : t;
        }


        public static List<T> GetPaging<T>(List<T> list, int pageIndex, int pageSize, out int pageCount, out int recordCount)
        {
            List<T> result = new List<T>();
            int recordNum = 0;
            int pageNum = 0;

            pageIndex = pageIndex <= 0 ? 1 : pageIndex;
            pageSize = pageSize <= 0 ? 20 : pageSize;
            int fromIndex = (pageIndex - 1) * pageSize;
            int toIndex = pageIndex * pageSize;
            recordNum = list.Count;
            pageNum = (recordNum + pageSize - 1) / pageSize;

            if (recordNum > 0)
            {
                int size = 0;
                if (fromIndex < toIndex && toIndex <= recordNum)
                {
                    size = toIndex - fromIndex;
                }
                else if (fromIndex < recordNum && toIndex > recordNum)
                {
                    size = recordNum - fromIndex;
                }

                if (size > 0 && fromIndex < list.Count && (fromIndex + size) <= list.Count)
                {
                    result = new List<T>(list.GetRange(fromIndex, size));
                }
            }
            recordCount = recordNum;
            pageCount = pageNum > 0 ? pageNum : 1;
            return result;
        }


        public static bool IsEmpty(string value)
        {
            return string.IsNullOrEmpty(value);
        }


        public static bool IsNullOrDbNull(object value)
        {
            if (value != null && ((value.GetType() == typeof(double) ||
                value.GetType() == typeof(int) ||
                value.GetType() == typeof(float) ||
                value.GetType() == typeof(Double) ||
                value.GetType() == typeof(Int64)) && value.ToString() == "0" ||
                string.IsNullOrEmpty(Convert.ToString(value)) ||
                value.GetType() == typeof(object) ||
                (value.GetType().Name.ToLower().StartsWith("list") && ((dynamic)value).Count == 0)))
            {
                return true;
            }
            return value == null;
        }


        public static void InsertSort<T>(List<T> list, T item, Comparison<T> comparison)
        {
            if (list.Count == 0)
            {
                list.Add(item);
                return;
            }
            int index = 0;
            int startIndex = 0;
            int endIndex = list.Count - 1;

            while (endIndex >= startIndex)
            {
                int middle = (startIndex + endIndex) / 2;
                int nextMiddle = middle + 1;
                var value = list[middle];

                int result = comparison(value, item);
                if (result <= 0 &&
                    (nextMiddle >= list.Count || comparison(list[nextMiddle], item) > 0))
                {
                    startIndex = middle + 1;
                    index = nextMiddle;
                }
                else if (result > 0)
                {
                    endIndex = middle - 1;
                }
                else
                {
                    startIndex = middle + 1;
                }
            }
            list.Insert(index, item);
        }

        public static List<T> QuickSort<T>(List<T> list, Comparison<T> comparison)
        {
            DoQuickSort(list, 0, list.Count - 1, comparison);
            return list;
        }

        private static void DoQuickSort<T>(List<T> list, int low, int high, Comparison<T> compareTo)
        {
            T pivot;
            int l, r;
            int mid;
            if (high < 0 || high <= low)
            {
                return;
            }
            if (high == low + 1)
            {
                if (compareTo(list[low], list[high]) > 0)
                {
                    QuickSwap(list, low, high);
                }
                return;
            }
            mid = (low + high) >> 1;
            pivot = list[mid];
            QuickSwap(list, low, mid);
            l = low + 1;
            r = high;
            do
            {
                while (l <= r && compareTo(list[l], pivot) <= 0)
                {
                    l++;
                }
                while (r > 0 && compareTo(list[r], pivot) > 0)
                {
                    r--;
                }
                if (l < r)
                {
                    QuickSwap(list, l, r);
                }
            } while (l < r);

            list[low] = list[r];
            list[r] = pivot;
            if (low + 1 < r)
            {
                DoQuickSort(list, low, r - 1, compareTo);
            }
            if (r + 1 < high)
            {
                DoQuickSort(list, r + 1, high, compareTo);
            }
        }

        private static void QuickSwap<T>(List<T> list, int low, int high)
        {
            T temp = list[low];
            list[low] = list[high];
            list[high] = temp;
        }

        public static string ToNotNullString(object value)
        {
            return ToNotNullString(value, string.Empty);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="defValue"></param>
        /// <returns></returns>
        public static string ToNotNullString(object value, string defValue)
        {
            defValue = defValue.IsNullOrEmpty() ? string.Empty : defValue;
            return value.IsNullOrEmpty() || value.ToString().IsEmpty() ? defValue : value.ToString();
        }

        public static int ToCeilingInt(object value)
        {
            if (value is decimal)
            {
                return (int)Math.Ceiling((decimal)value);
            }
            else if (value is double)
            {
                return (int)Math.Ceiling((double)value);
            }
            else if (value is float)
            {
                return (int)Math.Ceiling((float)value);
            }
            else
            {
                return (int)Math.Ceiling(ToDouble(value));
            }
        }


        public static int ToFloorInt(object value)
        {
            if (value is decimal)
            {
                return (int)Math.Floor((decimal)value);
            }
            else if (value is double)
            {
                return (int)Math.Floor((double)value);
            }
            else if (value is float)
            {
                return (int)Math.Floor((float)value);
            }
            else
            {
                return (int)Math.Floor(ToDouble(value));
            }
        }

        public static long ToLong(object value)
        {
            try
            {
                return Convert.ToInt64(value.IsNullOrEmpty() ? 0 : value);
            }
            catch
            {
                throw new ArgumentException(string.Format("\"{0}\" converted to type long fail.", value));
            }
        }

        public static int ToInt(object value)
        {
            try
            {
                if (value.IsNullOrEmpty() || false.Equals(value))
                {
                    return 0;
                }
                if (true.Equals(value))
                {
                    return 1;
                }
                return Convert.ToInt32(value);
            }
            catch
            {
                throw new ArgumentException(string.Format("\"{0}\" converted to type int fail.", value));
            }
        }

        public static short ToShort(object value)
        {
            try
            {
                return Convert.ToInt16(value.IsNullOrEmpty() ? 0 : value);
            }
            catch
            {
                throw new ArgumentException(string.Format("\"{0}\" converted to type short fail.", value));
            }
        }

        public static double ToDouble(object value)
        {
            try
            {
                return Convert.ToDouble(value.IsNullOrEmpty() ? 0 : value);
            }
            catch
            {
                throw new ArgumentException(string.Format("\"{0}\" converted to type double fail.", value));
            }
        }
        public static decimal ToDecimal(object value)
        {
            try
            {
                return Convert.ToDecimal(value.IsNullOrEmpty() ? 0 : value);
            }
            catch
            {
                throw new ArgumentException(string.Format("\"{0}\" converted to type decimal fail.", value));
            }
        }
        public static float ToFloat(object value)
        {
            try
            {
                return Convert.ToSingle(value.IsNullOrEmpty() ? 0 : value);
            }
            catch
            {
                throw new ArgumentException(string.Format("\"{0}\" converted to type decimal fail.", value));
            }
        }

        public static bool ToBool(object value)
        {
            try
            {
                if (value.IsNullOrEmpty() || "0".Equals(value))
                {
                    return false;
                }
                if ("1".Equals(value))
                {
                    return true;
                }
                return Convert.ToBoolean(value);
            }
            catch
            {
                throw new ArgumentException(string.Format("\"{0}\" converted to type bool fail.", value));
            }
        }

        public static byte ToByte(object value)
        {
            try
            {
                return Convert.ToByte(value.IsNullOrEmpty() ? 0 : value);
            }
            catch
            {
                throw new ArgumentException(string.Format("\"{0}\" converted to type byte fail.", value));
            }
        }

        public static UInt64 ToUInt64(object value)
        {
            try
            {
                return Convert.ToUInt64(value.IsNullOrEmpty() ? 0 : value);
            }
            catch
            {
                throw new ArgumentException(string.Format("\"{0}\" converted to type UInt64 fail.", value));
            }
        }

        public static UInt32 ToUInt32(object value)
        {
            try
            {
                return Convert.ToUInt32(value.IsNullOrEmpty() ? 0 : value);
            }
            catch
            {
                throw new ArgumentException(string.Format("\"{0}\" converted to type ToUInt32 fail.", value));
            }
        }
        public static UInt16 ToUInt16(object value)
        {
            try
            {
                return Convert.ToUInt16(value.IsNullOrEmpty() ? 0 : value);
            }
            catch
            {
                throw new ArgumentException(string.Format("\"{0}\" converted to type ToUInt16 fail.", value));
            }
        }

        public static DateTime ToDateTime(object value)
        {
            return ToDateTime(value, MathUtils.SqlMinDate);
        }


        public static DateTime ToDateTime(object value, DateTime defValue)
        {
            try
            {
                if (value.IsNullOrEmpty()) return defValue;
                return Convert.ToDateTime(value);
            }
            catch
            {
                throw new ArgumentException(string.Format("\"{0}\" converted to type datetime fail.", value));
            }
        }

        public static T ToEnum<T>(object value)
        {
            return (T)ToEnum(value, typeof(T));
        }

        public static object ToEnum(object value, Type type)
        {
            try
            {
                if (value is string)
                {
                    string tempValue = value.ToNotNullString();
                    return tempValue.IsEmpty() ? 0 : Enum.Parse(type, tempValue, true);
                }
                return Enum.ToObject(type, (value == null || value == DBNull.Value) ? 0 : value);
            }
            catch
            {
                throw new ArgumentException(string.Format("\"{0}\" converted to Enum {1} fail.", value, type.Name));
            }
        }
        public static T ParseJson<T>(string jsonStr)
        {
            return JsonUtils.DeserializeCustom<T>(jsonStr);
        }

        public static string ToJson(object value)
        {
            return JsonUtils.SerializeCustom(value);
        }


    }
}
