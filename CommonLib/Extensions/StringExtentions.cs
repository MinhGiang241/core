using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using CommonLibCore.CommonLib;

namespace CommonLibCore.CommonLib
{
    public static class StringExtentions
    {
        private static string validURLCharacters =
            "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIKLMNOPQRSTUVWXYZ-_";
        private static string validFileNameCharacters =
            "abcdefghijklmnopqrstuvwxyz0123456789.ABCDEFGHIKLMNOPQRSTUVWXYZ/";
        private static string validKeyNameCharacters =
            "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIKLMNOPQRSTUVWXYZ_";
        private static string validGraphQLKeyNameCharacters =
            "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIKLMNOPQRSTUVWXYZ_";

        public static string MergeWithObject(this string inputStr, object rawdata)
        {
            ///phân tích cú pháp ${xxx}

            // Nếu rawdata là null, trả về inputStrputStr
            if (rawdata == null)
                return inputStr;

            // Chuyển đổi rawdata thành một Dictionary<string, object>object>
            Dictionary<string, object> dic = rawdata.ConvertToDictionary();

            // Mẫu regex để tìm các placeholder trong inputStr
            string pattern = "\\${([^{}\\$]+)}";
            var matches = Regex.Matches(inputStr, pattern);

            // Lặp qua từng match tìm được trong inputStr
            foreach (Match match in matches)
            {
                try
                {
                    // Tìm placeholder để thay thế (ví dụ: ${name})
                    string replaceKey = "\\" + match.Groups[0].Value;
                    // Lấy tên khóa từ placeholder (ví dụ: name từ ${name})� ${name})
                    string key = match.Groups[1].Value;
                    // Lấy giá trị của key từ rawdata rawdata rawdata rawdata
                    var value = rawdata.GetFieldValue(key);
                    // Chuyển đổi giá trị thành chuỗi JSON
                    var datastr = value?.ToJson();
                    // Nếu giá trị là boolean, chuyển đổi nó thành chữ thường
                    if (value?.GetType() == typeof(bool))
                        datastr = datastr?.ToLower();
                    //trong trường hợp boolean hàm ToString bị chuyển thành chữ hoa chữ cái đầu --> có thể gây lỗi
                    // Thay thế tất cả các placeholder bằng giá trị tương ứng
                    inputStr = inputStr.ReplaceAll(replaceKey, datastr);
                }
                catch (Exception ex) { }
            }
            return inputStr;
        }

        public static string ReplaceAll(this string content, string pattern, string contentReplace)
        {
            return Regex.Replace(content, pattern, contentReplace);
        }

        public static string RemoveHtmlTag(this string text)
        {
            string output = text.ReplaceAll("<.*?>", "");
            return output;
        }

        public static string GetTextEncodeTransform(
            this string content,
            Encoding sourceEncode,
            Encoding destEncode
        )
        {
            // Kiểm tra nếu chuỗi content là null hoặc rỗng, trả về content� về content
            if (content.IsNullOrEmpty())
                return content;

            // Đăng ký provider để hỗ trợ các mã hóa khác ngoài UTF-8, Unicode, ASCII.nicode, ASCII.
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            // Chuyển đổi chuỗi content thành mảng byte sử dụng mã hóa nguồn.
            byte[] sourceBytes = sourceEncode.GetBytes(content);
            //Chuyển đổi mảng byte từ mã hóa nguồn sang mã hóa đích:mã hóa đích:
            byte[] destBytes = Encoding.Convert(sourceEncode, destEncode, sourceBytes);

            // Tạo chuỗi mới từ mảng byte sử dụng mã hóa đích. mã hóa đích.
            string transformedStr = destEncode.GetString(destBytes);
            // Trả về chuỗi đã được chuyển đổi mã hóa.
            return transformedStr;
        }

        public static string FromAnsiToUtf8(this string content)
        {
            // Kiểm tra nếu chuỗi đầu vào là null hoặc rỗng thì trả về ngay chuỗi đó về ngay chuỗi đó
            if (content.IsNullOrEmpty())
                return content;
            // Đăng ký bộ mã hóa để hỗ trợ các mã mã hóa không chuẩn hóa không chuẩn
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            // Xác định bộ mã hóa nguồn là Windows-1252 (mã ANSI mặc định cho các  ngôn ngữ Tây Âu
            Encoding sourceEncode = Encoding.GetEncoding(1252);
            // Xác định bộ mã hóa đích là UTF-8 là UTF-8
            Encoding destEncode = Encoding.UTF8;
            // Chuyển đổi chuỗi từ mã ANSI sang mảng byte theo mã hóa nguồn
            byte[] sourceBytes = sourceEncode.GetBytes(content);
            //byte[] destBytes = Encoding.Convert(sourceEncode, destEncode, sourceBytes);
            // Chuyển đổi mảng byte từ mã hóa nguồn sang chuỗi UTF-8
            string utf8str = destEncode.GetString(sourceBytes);
            // Trả về chuỗi UTF-8 đã chuyển đổiyển đổi
            return utf8str;
        }

        public static Type GetType(this string field, Type cursorType)
        {
            // Tách chuỗi field thành mảng các phần tử dựa trên dấu "."trên dấu "."
            string[] listId = field.Split(".".ToCharArray());
            // Lấy thuộc tính (property) đầu tiên của cursorType có tên là listId[0]
            var property = cursorType.GetProperty(listId[0]);
            // Lấy kiểu dữ liệu của thuộc tính này�c tính này
            cursorType = property.PropertyType;
            //Type cursorType = typeof(TObject);
            // Nếu field chỉ có một phần tử (không có dấu "."), trả về kiểu dữ liệu của thuộc tính
            if (listId.Length == 1)
            {
                return cursorType;
            }
            else
            {
                // Nếu field có nhiều hơn một phần tử, lấy phần tiếp theo của field
                string subField = field.Substring(listId[0].Length + 1);
                // Đệ quy gọi lại phương thức GetType với subField và cursorTypeà cursorType
                return subField.GetType(cursorType);
            }
        }

        public static PropertyInfo GetPropertyInfo(this string field, Type cursorType)
        {
            // Tách chuỗi field thành mảng các phần tử dựa trên dấu "."trên dấu "."
            string[] listId = field.Split(".".ToCharArray());
            // Lấy thông tin thuộc tính đầu tiên của cursorType dựa trên tên trong listId[0]
            var property = cursorType.GetPropertyIgnoreCase(listId[0]);
            // Nếu không tìm thấy thuộc tính, trả về null
            if (property == null)
                return null;
            // Cập nhật cursorType thành kiểu dữ liệu của thuộc tínha thuộc tính
            cursorType = property.PropertyType;
            //Type cursorType = typeof(TObject);
            // Nếu field chỉ có một phần tử (không có dấu "."), trả về thông tin của thuộc tính propertya thuộc tính property
            if (listId.Length == 1)
            {
                return property;
            }
            else
            {
                // Nếu field có nhiều hơn một phần tử, lấy phần tiếp theo của field
                string subField = field.Substring(listId[0].Length + 1);
                // Nếu cursorType là một generic type, lấy thông tin thuộc tính của generic argument đầu tiênent đầu tiên
                if (cursorType.GenericTypeArguments.Length > 0)
                {
                    return subField.GetPropertyInfo(cursorType.GenericTypeArguments[0]);
                }
                else
                    // Ngược lại, lấy thông tin thuộc tính của cursorTypea cursorType
                    return subField.GetPropertyInfo(cursorType);
            }
        }

        public static PropertyInfo? GetPropertyIgnoreCase(this Type type, string key)
        {
            // Lấy danh sách các thuộc tính của đối tượng Type
            var properties = type.GetProperties();
            // Duyệt qua từng thuộc tính
            foreach (var prop in properties)
            {
                // So sánh tên của thuộc tính (bỏ qua sự phân biệt chữ hoa chữ thường)
                if (prop.Name.ToLower() == key.ToLower())
                {
                    // Nếu tìm thấy thuộc tính có tên tương đương, trả về thuộc tính đóvề thuộc tính đó
                    return prop;
                }
            }

            // Nếu không tìm thấy, trả về nullvề null
            return null;
        }

        public static string ValidJavaScriptString(this string text)
        {
            if (text == null)
                return "";
            text = text.Replace("'null'", "")
                .Replace("null", "")
                .Replace("undefined", "")
                .Replace("Other", "");
            return text;
        }

        //chuyen sang tieng viet khong dau
        public static string ToUnsign(this string kq)
        {
            if (kq == null)
                return "";
            string[,] Mang = new string[14, 18];
            byte i = 0;
            byte j = 0;
            string? Chuỗi = null;
            string? Thga = null;
            string? Thge = null;
            string? Thgo = null;
            string? Thgu = null;
            string? Thgi = null;
            string? Thgd = null;
            string? Thgy = null;
            string? HoaA = null;
            string? HoaE = null;
            string? HoaO = null;
            string? HoaU = null;
            string? HoaI = null;
            string? HoaD = null;
            string? HoaY = null;
            Chuỗi = "aAeEoOuUiIdDyY";
            Thga = "áàạảãâấầậẩẫăắằặẳẵ";
            HoaA = "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ";
            Thge = "éèẹẻẽêếềệểễeeeeee";
            HoaE = "ÉÈẸẺẼÊẾỀỆỂỄEEEEEE";
            Thgo = "óòọỏõôốồộổỗơớờợởỡ";
            HoaO = "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ";
            Thgu = "úùụủũưứừựửữuuuuuu";
            HoaU = "ÚÙỤỦŨƯỨỪỰỬỮUUUUUU";
            Thgi = "íìịỉĩiiiiiiiiiiii";
            HoaI = "ÍÌỊỈĨIIIIIIIIIIII";
            Thgd = "đdddddddddddddddd";
            HoaD = "ĐDDDDDDDDDDDDDDDD";
            Thgy = "ýỳỵỷỹyyyyyyyyyyyy";
            HoaY = "ÝỲỴỶỸYYYYYYYYYYYY";

            for (i = 0; i <= 13; i++)
            {
                Mang[i, 0] = Chuỗi.Substring(i, 1);
            }
            for (j = 1; j <= 17; j++)
            {
                for (i = 1; i <= 17; i++)
                {
                    Mang[0, i] = Thga.Substring(i - 1, 1);
                    //Nạp từng ký tự trong chuỗi Thga vào từng ô trong hàng 0
                    Mang[1, i] = HoaA.Substring(i - 1, 1);
                    //Nạp từng ký tự trong chuỗi HoaA vào từng ô trong  hàng 1
                    Mang[2, i] = Thge.Substring(i - 1, 1);
                    //Nạp từng ký tự trong chuỗi Thge vào từng ô trong  hàng 2
                    Mang[3, i] = HoaE.Substring(i - 1, 1);
                    //Nạp từng ký tự trong chuỗi HoaE vào từng ô trong  hàng 3
                    Mang[4, i] = Thgo.Substring(i - 1, 1);
                    //Nạp từng ký tự trong chuỗi Thgo vào từng ô trong  hàng 4
                    Mang[5, i] = HoaO.Substring(i - 1, 1);
                    //Nạp từng ký tự trong chuỗi HoaO vào từng ô trong  hàng 5
                    Mang[6, i] = Thgu.Substring(i - 1, 1);
                    //Nạp từng ký tự trong chuỗi Thgu vào từng ô trong  hàng 6
                    Mang[7, i] = HoaU.Substring(i - 1, 1);
                    //Nạp từng ký tự trong chuỗi HoaU vào từng ô trong  hàng 7
                    Mang[8, i] = Thgi.Substring(i - 1, 1);
                    //Nạp từng ký tự trong chuỗi Thgi vào từng ô trong  hàng 8
                    Mang[9, i] = HoaI.Substring(i - 1, 1);
                    //Nạp từng ký tự trong chuỗi HoaI vào từng ô trong  hàng 9
                    Mang[10, i] = Thgd.Substring(i - 1, 1);
                    //Nạp từng ký tự trong chuỗi Thgd vào từng ô trong  hàng 10
                    Mang[11, i] = HoaD.Substring(i - 1, 1);
                    //Nạp từng ký tự trong chuỗi HoaD vào từng ô trong  hàng 11
                    Mang[12, i] = Thgy.Substring(i - 1, 1);
                    //Nạp từng ký tự trong chuỗi Thgy vào từng ô trong  hàng 12
                    Mang[13, i] = HoaY.Substring(i - 1, 1);
                    //Nạp từng ký tự trong chuỗi HoaY vào từng ô trong  hàng 13
                }
            }

            string? Tạm1 = null;
            string? Tạm2 = null;
            Tạm1 = kq;
            for (j = 0; j <= 13; j++)
            {
                for (i = 1; i <= 17; i++)
                {
                    Tạm2 = Tạm1.Replace(Mang[j, i], Mang[j, 0]);
                    Tạm1 = Tạm2;
                }
            }
            return Tạm1;
        }

        public static string ToValidFileName(this string text)
        {
            String unsigntext = ToUnsign(text);
            String validurl = "";
            char[] listch = unsigntext.ToCharArray();
            char space = ' ';
            char lastchar = space;
            foreach (char ch in listch)
            {
                if (validFileNameCharacters.Contains(ch))
                {
                    lastchar = ch;
                    validurl += lastchar;
                }
                else
                {
                    if (lastchar != space)
                    {
                        lastchar = space;
                        validurl += lastchar;
                    }
                }
            }

            return validurl.Trim();
        }

        public static string ToValidKeyName(this string text)
        {
            // Chuyển đổi chuỗi thành chuỗi không dấu
            string unsigntext = ToUnsign(text);

            // Chuỗi kết quả là chuỗi rỗng ban đầu
            string validKeyName = "";

            // Chuyển đổi chuỗi thành mảng các ký tự
            char[] characters = unsigntext.ToCharArray();

            // Ký tự được sử dụng để thay thế khoảng trắng
            char spaceReplacement = '_';

            // Ký tự cuối cùng được sử dụng để theo dõi xem đã thêm ký tự nào vào chuỗi kết quả chưa
            char lastChar = spaceReplacement;

            // Duyệt qua từng ký tự trong mảng ký tự
            foreach (char ch in characters)
            {
                // Kiểm tra xem ký tự có nằm trong danh sách các ký tự hợp lệ của tên khóa không
                if (validKeyNameCharacters.Contains(ch))
                {
                    // Nếu là ký tự hợp lệ, thêm vào chuỗi kết quả và cập nhật ký tự cuối cùng
                    lastChar = ch;
                    validKeyName += lastChar;
                }
                else
                {
                    // Nếu không phải là ký tự hợp lệ
                    if (lastChar != spaceReplacement)
                    {
                        // Nếu ký tự cuối cùng đã được thêm vào chuỗi kết quả, thêm ký tự thay thế (spaceReplacement) vào chuỗi kết quả
                        lastChar = spaceReplacement;
                        validKeyName += lastChar;
                    }
                }
            }

            // Trả về chuỗi kết quả đã được chuẩn hóa và loại bỏ khoảng trắng ở đầu và cuối
            return validKeyName.Trim();
        }

        public static string ToValidGraphTypeKeyName(this string text)
        {
            // Kiểm tra nếu chuỗi đầu vào là null hoặc rỗng thì trả về chuỗi rỗng
            if (string.IsNullOrEmpty(text))
                return "";

            // Chuyển đổi chuỗi đầu vào thành chuỗi không dấu (unsigned string)
            string unsigntext = ToUnsign(text);

            // Khởi tạo một StringBuilder để xây dựng chuỗi kết quả
            StringBuilder validGraphTypeKeyNameBuilder = new StringBuilder();

            // Ký tự thay thế khoảng trắng
            char spaceReplacement = '_';

            // Biến để lưu trữ ký tự cuối cùng được xử lý
            char lastChar = spaceReplacement;

            // Duyệt từng ký tự trong chuỗi không dấu
            foreach (char ch in unsigntext)
            {
                // Nếu ký tự hiện tại thuộc danh sách các ký tự hợp lệ cho GraphQL key name
                if (validGraphQLKeyNameCharacters.Contains(ch))
                {
                    // Cập nhật ký tự cuối cùng và thêm vào StringBuilder
                    lastChar = ch;
                    validGraphTypeKeyNameBuilder.Append(lastChar);
                }
                else if (lastChar != spaceReplacement)
                {
                    // Nếu ký tự cuối cùng không phải là ký tự thay thế khoảng trắng,
                    // thay thế bằng ký tự thay thế khoảng trắng và thêm vào StringBuilder
                    lastChar = spaceReplacement;
                    validGraphTypeKeyNameBuilder.Append(lastChar);
                }
            }

            // Chuyển StringBuilder thành chuỗi và loại bỏ các ký tự thay thế khoảng trắng ở đầu và cuối chuỗi
            return validGraphTypeKeyNameBuilder.ToString().Trim('_');
        }

        public static string ToValidUrl(this string text, string spliter = "-")
        {
            // Kiểm tra nếu chuỗi đầu vào là null hoặc rỗng thì trả về chuỗi rỗng
            if (string.IsNullOrEmpty(text))
                return "";

            // Chuyển đổi chuỗi đầu vào thành chuỗi không dấu và chuyển thành chữ thường (lowercase)
            string unsigntext = ToUnsign(text).ToLower();

            // Khởi tạo một StringBuilder để xây dựng chuỗi kết quả
            StringBuilder validUrlBuilder = new StringBuilder();

            // Biến để lưu trữ ký tự cuối cùng được xử lý
            char lastChar = (char)0;

            // Duyệt từng ký tự trong chuỗi không dấu và chữ thường
            foreach (char ch in unsigntext)
            {
                // Nếu ký tự hiện tại thuộc danh sách các ký tự hợp lệ cho URL
                if (validURLCharacters.Contains(ch))
                {
                    // Cập nhật ký tự cuối cùng và thêm vào StringBuilder
                    lastChar = ch;
                    validUrlBuilder.Append(lastChar);
                }
                else if (!spliter.IsNullOrEmpty() && lastChar != spliter[0])
                {
                    // Nếu ký tự cuối cùng không phải là ký tự phân cách và ký tự phân cách không rỗng
                    // Thêm ký tự phân cách vào StringBuilder và cập nhật ký tự cuối cùng
                    validUrlBuilder.Append(spliter[0]);
                    lastChar = spliter[0];
                }
            }

            // Trả về chuỗi kết quả từ StringBuilder
            return validUrlBuilder.ToString();
        }

        public static string Bash(
            this string cmd,
            string fileExecute = "sh",
            bool waitProcess = true
        )
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileExecute,
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            if (waitProcess)
                process.WaitForExit();
            return result;
        }

        public static string GetTelco(this string PhoneNumber)
        {
            string Vietel = "086-096-097-098-032-033-034-035-036-037-038-039";
            string Mobifone = "090-093-070-079-077-076-078";
            string Vinaphone = "091-094-083-084-085-081-082";
            string Vietnamemobile = "092-056-058";
            string GMobile = "099-059";
            string phonenumber = PhoneNumber.FormatPhonenumberStartWith0();
            foreach (string item in Vietel.Split('-'))
            {
                if (phonenumber.StartsWith(item))
                    return "VIETTEL";
            }
            foreach (string item in Mobifone.Split('-'))
            {
                if (phonenumber.StartsWith(item))
                    return "MOBIFONE";
            }
            foreach (string item in Vinaphone.Split('-'))
            {
                if (phonenumber.StartsWith(item))
                    return "VINAPHONE";
            }
            foreach (string item in Vietnamemobile.Split('-'))
            {
                if (phonenumber.StartsWith(item))
                    return "VIETNAMMOBILE";
            }
            foreach (string item in GMobile.Split('-'))
            {
                if (phonenumber.StartsWith(item))
                    return "GMOBILE";
            }

            return "";
        }

        public static bool IsBoolean(this string str)
        {
            try
            {
                bool.Parse(str);
                return true;
            }
            catch { }
            return false;
        }

        public static bool IsNumber(this string str)
        {
            if (String.IsNullOrEmpty(str))
                return false;
            return Regex.Match(str, "^\\d+$").Success;
        }

        public static string FormatPhonenumberStartWith84(this string PhoneNumber)
        {
            PhoneNumber = PhoneNumber.Replace("+", "").Replace("(", "").Replace(")", "");
            PhoneNumber = Regex.Replace(PhoneNumber, "^0", "84");
            return PhoneNumber;
        }

        public static string ConvertToString(this long longvalue, int length)
        {
            string value = longvalue.ToString();
            while (value.Length < length)
            {
                value = "0" + value;
            }
            return value;
        }

        public static string FormatPhonenumberStartWith0(this string PhoneNumber)
        {
            PhoneNumber = PhoneNumber.Replace("+", "").Replace("(", "").Replace(")", "");
            PhoneNumber = Regex.Replace(PhoneNumber, "^84", "0");
            return PhoneNumber;
        }

        public static bool IsPhoneNumber(this string PhoneNumber)
        {
            if (string.IsNullOrEmpty(PhoneNumber))
                return false;
            PhoneNumber = PhoneNumber.FormatPhonenumberStartWith0();
            return Regex.Match(PhoneNumber, "^0[0-9]{9,12}$").Success;
        }

        public static string RemoveSpace(string input)
        {
            return input.Replace(" ", "");
        }

        public static bool IsEmail(this String source)
        {
            return (!String.IsNullOrEmpty(source) && source.IndexOf("@") > 0);
        }

        public static DateTime ToDateTime(this string datetime, string format, string DateSeparator)
        {
            try
            {
                DateTimeFormatInfo dtfi = new DateTimeFormatInfo();
                dtfi.ShortDatePattern = format;
                dtfi.DateSeparator = DateSeparator;
                DateTime objDate = Convert.ToDateTime(datetime, dtfi);
                return objDate;
            }
            catch (Exception ex)
            {
                return DateTime.MinValue;
            }
        }

        public static DateTime ToDateTime(this string datetime)
        {
            return datetime.ToDateTime("dd/MM/yyyy", "/");
        }

        public static DateTime ToDateTime(this string datetime, DateTime defDateTime)
        {
            try
            {
                //return DateTime.Parse(datetime);
                return datetime.ToDateTime("dd/MM/yyyy", "/");
            }
            catch
            {
                return defDateTime;
            }
        }

        public static void TryToDateTime(this string datetime, out DateTime? defDateTime)
        {
            defDateTime = null;
            try
            {
                defDateTime = DateTime.Parse(datetime);
            }
            catch { }
        }

        public static string ToMD5(this string input)
        {
            MD5 md5Hash = MD5.Create();
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        public static string EncodeSHA256(this string plaintext)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(plaintext);
            SHA256Managed hashstring = new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(bytes);
            string hashString = string.Empty;
            foreach (byte x in hash)
            {
                hashString += String.Format("{0:x2}", x);
            }
            return hashString;
        }

        public static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static string EncodeSHA256(this string plaintext, string secretKey)
        {
            secretKey = secretKey ?? "";
            var encoding = new System.Text.UTF8Encoding();
            byte[] keyByte = encoding.GetBytes(secretKey);
            byte[] messageBytes = encoding.GetBytes(plaintext);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hash = hmacsha256.ComputeHash(messageBytes);
                string hashString = string.Empty;
                foreach (byte x in hash)
                {
                    hashString += String.Format("{0:x2}", x);
                }
                return hashString;
            }
        }

        public static string EncodeAES(this string plaintext, string secretKey)
        {
            byte[] src = Encoding.UTF8.GetBytes(plaintext);
            byte[] key = Encoding.ASCII.GetBytes(secretKey);
            RijndaelManaged aes = new RijndaelManaged();
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;
            aes.KeySize = 128;
            aes.IV = new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef };

            string hashString = string.Empty;
            using (ICryptoTransform encrypt = aes.CreateEncryptor(key, null))
            {
                byte[] hash = encrypt.TransformFinalBlock(src, 0, src.Length);
                encrypt.Dispose();
                hashString = Convert.ToBase64String(hash);
            }
            return hashString;
        }

        public static string EncodeAES(this string plaintext, string secretKey, byte[] iv)
        {
            byte[] src = Encoding.UTF8.GetBytes(plaintext);
            byte[] key = Encoding.ASCII.GetBytes(secretKey);
            RijndaelManaged aes = new RijndaelManaged();
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;
            aes.KeySize = 128;
            aes.IV = iv;

            string hashString = string.Empty;
            using (ICryptoTransform encrypt = aes.CreateEncryptor(key, null))
            {
                byte[] hash = encrypt.TransformFinalBlock(src, 0, src.Length);
                encrypt.Dispose();
                hashString = Convert.ToBase64String(hash);
            }
            return hashString;
        }

        public static string DecodeAES(this string encryptedText, string secretKey, byte[] iv)
        {
            byte[] src = Convert.FromBase64String(encryptedText);
            byte[] key = Encoding.ASCII.GetBytes(secretKey);
            RijndaelManaged aes = new RijndaelManaged();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.KeySize = 128;
            aes.IV = iv;

            string plaintext = string.Empty;
            using (ICryptoTransform decrypt = aes.CreateDecryptor(key, iv))
            {
                byte[] hash = decrypt.TransformFinalBlock(src, 0, src.Length);
                decrypt.Dispose();
                plaintext = Encoding.UTF8.GetString(hash);
            }
            return plaintext;
        }

        public static string FaceBookSecret(this string content, string key)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] messageBytes = Encoding.UTF8.GetBytes(content);
            byte[] hash;
            using (HMACSHA256 hmacsha256 = new HMACSHA256(keyBytes))
            {
                hash = hmacsha256.ComputeHash(messageBytes);
            }
            string hashString = string.Empty;
            foreach (byte x in hash)
            {
                hashString += String.Format("{0:x2}", x);
            }
            return hashString;
        }

        public static class Base16
        {
            private static readonly char[] encoding;

            static Base16()
            {
                encoding = new char[16]
                {
                    '0',
                    '1',
                    '2',
                    '3',
                    '4',
                    '5',
                    '6',
                    '7',
                    '8',
                    '9',
                    'a',
                    'b',
                    'c',
                    'd',
                    'e',
                    'f'
                };
            }

            public static string Encode(byte[] data)
            {
                char[] text = new char[data.Length * 2];

                for (int i = 0, j = 0; i < data.Length; i++)
                {
                    text[j++] = encoding[data[i] >> 4];
                    text[j++] = encoding[data[i] & 0xf];
                }

                return new string(text);
            }

            public static byte[] Decode(string hex)
            {
                //return Enumerable.Range(0, hex.Length)
                //                 .Where(x => x % 2 == 0)
                //                 .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                //                 .ToArray();
                return UTF8Encoding.UTF8.GetBytes(hex);
            }
        }

        public static string GenerateAppSecretProof(this string accessToken, string appSecret)
        {
            byte[] key = Base16.Decode(appSecret);
            byte[] hash;
            using (HMAC hmacAlg = new HMACSHA1(key))
            {
                hash = hmacAlg.ComputeHash(Encoding.ASCII.GetBytes(accessToken));
            }
            return Base16.Encode(hash);
        }

        public static string EncodeBase64(this string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string DecodeBase64(this string base64EncodedData)
        {
            try
            {
                base64EncodedData = base64EncodedData
                    .Replace('-', '+')
                    .Replace('_', '/')
                    .PadRight(4 * ((base64EncodedData.Length + 3) / 4), '=');
                var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
                return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            }
            catch (Exception ex)
            {
                return base64EncodedData;
            }
        }

        public static bool IsNullOrEmpty(this string text)
        {
            return string.IsNullOrEmpty(text);
        }

        public static string ToUpperFirstChar(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";
            return text[0].ToString().ToUpper() + text.Substring(1);
        }

        public static string ToFormatName(this string text, params char[] separator)
        {
            if (string.IsNullOrEmpty(text))
                return "";
            string[] arr = Regex.Replace(text, "\\s+", " ").Split(separator);
            return string.Join(" ", arr.Select(s => s.ToUpperFirstChar()));
        }

        public static string ToLowerCase(this string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "";
            }
            return text.ToLower();
        }
    }
}
