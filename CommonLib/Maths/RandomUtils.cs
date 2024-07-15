namespace CommonLibCore.CommonLib
{
    public static class RandomUtils
    {
        private static int MinPercent = 1;
        private static int MaxPercent = 100;
        private static Random random = new Random();
        private static String ascii_uppercase = "A9B8CDEFGHI7JK6LMN5OPQ4RS3TUV2WX1YZ";//35 character

        /// <summary>
        /// Tạo mã số gồm 6 chữ số
        /// </summary>
        /// <returns></returns>
        public static String GenerateOTPCode()
        {
            Random r = new Random();
            int number = r.Next(999999);
            String value = number.ToString();
            while (value.Length < 6)
            {
                value = "0" + value;
            }
            return value;
        }
        /// <summary>
        /// kết quả trả về dạng chuỗi các chữ số
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static String GenerateOTPCode(int length, Func<string, int, string> checkCode = null, int loop = 0)
        {
            Random r = new Random();
            string MaxNumberStr = "";
            for (int i = 0; i < length; i++)
            {
                MaxNumberStr += "9";
            }
            int MaxNumber = int.Parse(MaxNumberStr);
            int number = r.Next(MaxNumber);
            String value = number.ToString();
            while (value.Length < length)
            {
                value = "0" + value;
            }
            if (checkCode != null)
            {
                value = checkCode(value, loop);
                if (value == null)
                {
                    return GenerateOTPCode(length, checkCode, loop + 1);
                }
            }
            return value;
        }
        /// <summary>
        /// Tạo mã voucher se chứa các ký tự phức tạp hơn mã pin bao gồm cả chữ và số
        /// </summary>
        /// <param name="len"></param>
        /// <param name="checkCode">để hệ thống sử dụng biết nên dừng vòng lặp khi nào</param>
        /// <returns></returns>
        public static string GenerateVoucherCode(int len, Func<string, int, string> checkCode = null, int loop = 0)
        {
            Random r = new Random();
            string value = null;
            int number = r.Next(int.MaxValue);
            value = GeneratePinCode(number, len);
            if (checkCode != null)
            {
                value = checkCode(value, loop);
                if (value == null)
                {
                    value = GenerateVoucherCode(len, checkCode, loop + 1);
                }
            }
            return value;
        }

        public static string ToValidCodeLength(string code, int length)
        {
            if (code == null)
                return "";
            while (code.Length < length)
            {
                code = "0" + code;
            }
            return code;
        }
        public static string GeneratePinCodeFromTickDay(int Len, Func<string, int, string> checkCode = null, int loop = 0)
        {
            long unixTime = DateTime.Now.Ticks - DateTime.Today.Ticks;
            return GeneratePinCode(unixTime, Len, checkCode, loop);
        }

        /// <summary>
        /// Pincode phải đảm bảo duy nhất trên toàn hệ thống
        /// </summary>
        /// <param name="Number">giá trị của voucher dạng số sẽ được mã hóa lại</param>
        /// <param name="Len">Độ dài của mã voucher</param>
        /// <param name="checkCode">hàm kiểm tra tính duy nhất</param>
        /// <returns></returns>
        public static string GeneratePinCode(long OriNumber, int Len, Func<string, int, string> checkCode = null, int loop = 0)
        {
            long Number = OriNumber;
            Char[] list_ascii = ascii_uppercase.ToUpper().ToCharArray();
            //vị trí ký tự chính là số dư khi chia cho tổng số ký tự của chuỗi ascii
            String Value = "";
            while (Number > 0)
            {
                long Result = Number % list_ascii.Length;
                Number = Number / list_ascii.Length;
                Value += list_ascii[Result];
            }
            while (Len > 0 && Value.Length < Len)
            {
                Value += "0";
            }
            if (checkCode != null)
            {
                Value = checkCode(Value, loop);
                if (Value == null)
                {//thực hiện lại vòng lặp sau khi cộng với 1 số ngẫu nhiên từ 1-100
                    return GeneratePinCode(OriNumber + GetRandom(), Len, checkCode, loop + 1);
                }
            }
            return Value;
        }
        public static double RevertFromPinCode(string Code)
        {
            var list_ascii = new List<char>(ascii_uppercase.ToUpper().ToCharArray());
            double ASCII_LENGTH = ascii_uppercase.Length;
            //List<char> 
            double Number = 0;
            char[] CodeArray = Code.ToCharArray();
            int Skip = 0;
            for (int i = CodeArray.Length - 1; i >= 0; i--)
            {
                double Pow = CodeArray.Length - i - 1 - Skip;
                int IndexInASCII = list_ascii.IndexOf(CodeArray[i]);
                if (IndexInASCII == -1)
                {
                    Skip++;
                    continue;
                }
                Number += IndexInASCII * Math.Pow(ASCII_LENGTH, i);
            }
            return Number;
        }
        /// <summary>
        /// gen ra code mới tiến lên dạng số từ 1 string dạng số
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string GetNextSequenceCode(string maxCode, int minLength, Func<string, int, string> checkCode = null, int loop = 0)
        {
            int NextCode = 1;
            if (!maxCode.IsNullOrEmpty())
            {
                NextCode = int.Parse(maxCode) + 1;
            }
            string value = NextCode.ToString();
            while (value.Length < minLength)
            {
                value = "0" + value;
            }
            if (checkCode != null)
            {
                string result = checkCode(value, loop);
                if (result == null)
                {
                    return GetNextSequenceCode(value, minLength, checkCode, loop + 1);
                }
                else
                    return result;
            }
            return value;
        }
        /// <summary>
        /// Gencode nhưng truyền cả đối tượng góc để tùy biến được nhiều hơn
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string GetNextSequenceCode<T>(string maxCode, int minLength, Func<string, int, T, string> checkCode = null, int loop = 0, T obj = default(T))
        {
            int NextCode = 1;
            if (!maxCode.IsNullOrEmpty())
            {
                NextCode = int.Parse(maxCode) + 1;
            }
            string value = NextCode.ToString();
            while (value.Length < minLength)
            {
                value = "0" + value;
            }
            if (checkCode != null)
            {

                string result = checkCode(value, loop, obj);
                if (result == null)
                {
                    return GetNextSequenceCode(value, minLength, checkCode, loop + 1, obj);
                }
                else
                    return result;
            }
            return value;
        }
        public static string RandomNickName(string[] firstNames, string[] lastNames)
        {
            if (firstNames == null)
            {
                throw new ArgumentNullException("firstNames");
            }
            if (lastNames == null)
            {
                throw new ArgumentNullException("lastNames");
            }
            string name = "";
            if (firstNames.Length > 0)
            {
                name = firstNames[GetRandom(0, firstNames.Length)];
            }
            int length = GetRandom(1, 3);
            for (int i = 0; i < length; i++)
            {
                if (lastNames.Length > 0)
                {
                    string temp = lastNames[GetRandom(0, lastNames.Length)];
                    if (!string.IsNullOrEmpty(temp))
                    {
                        name += temp.Substring(0, 1);
                    }
                }
            }
            return name;
        }

        public static bool NextBool(double rate)
        {
            return random.NextDouble() < rate;
        }
        /// <summary>
        /// 获取随机概率值1-100
        /// </summary>
        /// <returns></returns>
        public static int GetRandom()
        {
            return GetRandom(MinPercent, MaxPercent + 1);
        }

        public static int GetRandom(int minNum, int maxNum)
        {
            minNum = minNum < 0 ? 0 : minNum;
            if (minNum < maxNum)
            {
                return random.Next(minNum, maxNum);
            }
            throw new ArgumentOutOfRangeException("maxNum");
        }

        public static bool IsHitNew(double value)
        {
            return NextBool(value);
        }

        public static bool IsHitNew(double value, double increase)
        {
            return NextBool(value * (1 + increase));
        }

        public static bool IsHitNew(int value, int increase, int radix = 100)
        {
            return NextBool((value / radix) * (1 + increase / radix));
        }

        public static bool IsHit(int percent)
        {
            percent = percent > MaxPercent ? MaxPercent : percent;
            return NextBool(percent * 0.01);
        }

        public static bool IsHitByTh(decimal percent)
        {
            return NextBool((double)percent);
        }

        public static bool IsHit(double percent)
        {
            return NextBool(percent);
        }

        public static bool IsHit(decimal percent)
        {
            return NextBool((double)percent);
        }

        public static int HitIndex(double[] percents)
        {
            int index = -1;
            int hitIndex = -1;
            double max = 0;
            int maxIndex = 0;
            var r = random.NextDouble();
            double offset = 0;
            foreach (var p in percents)
            {
                index++;
                if (p > max)
                {
                    maxIndex = index;
                    max = p;
                }
                offset += p;
                if (r <= offset)
                {
                    hitIndex = index;
                    break;
                }
            }
            if (hitIndex == -1)
            {
                hitIndex = maxIndex;
            }
            return hitIndex;
        }

        public static int GetHitIndex(int[] percents)
        {
            int index = -1;
            int p = GetRandom();
            int num = 0;
            if (percents != null)
            {
                for (int i = 0; i < percents.Length; i++)
                {
                    num += percents[i];
                    if (p <= num)
                    {
                        index = i;
                        break;
                    }
                }
            }
            return index;
        }

        public static int GetHitIndex(double[] percents)
        {
            int index = 0;
            int totalNum = 1;
            if (percents == null)
            {
                return -1;
            }
            foreach (var percent in percents)
            {
                totalNum += (int)Math.Floor(Math.Abs(percent) * 10000);
            }

            int tempNum = 0;
            if (totalNum > 0)
            {
                tempNum = GetRandom(0, totalNum);
            }
            double num = 0;
            for (int i = 0; i < percents.Length; i++)
            {
                num += (int)Math.Floor(Math.Abs(percents[i]) * 10000);
                if (tempNum <= num)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }
        public static int GetHitIndexByTH(int[] percents)
        {
            int index = -1;
            if (percents == null)
            {
                return index;
            }
            int p = GetRandom(0, 1000 + 1);
            int num = 0;
            for (int i = 0; i < percents.Length; i++)
            {
                num += percents[i];
                if (p <= num)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        public static int[] GetRandom(int count, int randomMax, int totalMin, int totalMax)
        {
            int randomMin = (int)Math.Floor((double)totalMin / count);
            return GetRandom(count, randomMin, randomMax, totalMin, totalMax);
        }
        public static int[] GetRandom(int count, int randomMin, int randomMax, int totalMin, int totalMax)
        {
            if (count > 100)
            {
                throw new ArgumentOutOfRangeException("count", "<100");
            }
            count = count < 0 ? 0 : count;
            randomMin = randomMin < 0 ? 0 : randomMin;
            randomMax = randomMax < 0 ? 0 : randomMax;
            totalMin = totalMin < 0 ? 0 : totalMin;
            totalMax = totalMax < 0 ? 0 : totalMax;
            int currentTotal = 0;
            int[] values = new int[count];
            for (int i = 0; i < count; i++)
            {
                int tminVal = (count - i - 1) * randomMin;
                tminVal = tminVal == 0 ? randomMin : tminVal;

                int tmaxVal = totalMax - currentTotal - tminVal;
                tmaxVal = tmaxVal > randomMax ? randomMax : tmaxVal;
                tmaxVal = tmaxVal < tminVal ? tminVal : tmaxVal;

                int val = GetRandom(randomMin, tmaxVal + 1);
                currentTotal += val;

                values[i] = val;
            }

            return values;
        }

        public static T[] GetRandomArray<T>(T[] array, int count)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            List<T> list = new List<T>();
            if (count > array.Length)
            {
                return list.ToArray();
            }
            bool[] checkedList = new bool[array.Length];

            while (list.Count < count)
            {
                int index = GetRandom(0, array.Length);
                if (!checkedList[index])
                {
                    list.Add(array[index]);
                    checkedList[index] = true;
                }
            }

            return list.ToArray();
        }

        public static int[] GetRandomNew(int count, int randomMin, int randomMax, int totalMin, int totalMax)
        {
            var val = GetRandom(totalMin, totalMax);
            return GetRandomNew(count, randomMin, randomMax, val);
        }

        public static int[] GetRandomNew(int count, int randomMin, int randomMax, int total)
        {
            var realMax = randomMax;
            var rollMax = 0;
            var realMin = randomMin;
            var currentTotal = 0;
            var temTotal = total;
            var values = new int[count];
            for (var i = 0; i < count - 1; i++)
            {
                realMax = temTotal - randomMin;
                rollMax = randomMax * (count - i - 1);
                realMax = realMax < rollMax ? realMax : rollMax;


                realMin = MathUtils.Subtraction(temTotal, randomMax, randomMin * (count - i - 1));

                var val = GetRandom(realMin, realMax + 1);
                values[i] = MathUtils.Subtraction(temTotal, val);
                currentTotal = MathUtils.Addition(currentTotal, values[i]);
                temTotal = val;
            }
            values[count - 1] = total - currentTotal;
            return values;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] RandomSort<T>(T[] source)
        {
            int num = source.Length / 2;
            for (int i = 0; i < num; i++)
            {
                int num2 = GetRandom(0, source.Length);
                int num3 = GetRandom(0, source.Length);
                if (num2 != num3)
                {
                    T t = source[num3];
                    source[num3] = source[num2];
                    source[num2] = t;
                }
            }
            return source;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> RandomSort<T>(List<T> source)
        {
            int num = source.Count / 2;
            for (int i = 0; i < num; i++)
            {
                int num2 = GetRandom(0, source.Count);
                int num3 = GetRandom(0, source.Count);
                if (num2 != num3)
                {
                    T t = source[num3];
                    source[num3] = source[num2];
                    source[num2] = t;
                }
            }
            return source;
        }

    }
}

