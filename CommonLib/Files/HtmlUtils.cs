using System.Net;
using System.Text;

namespace CommonLibCore.CommonLib.Files
{
    public class HtmlUtils
    {
        public static string GetHTML(string url)
        {
            try
            {
                HttpWebRequest myWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
                myWebRequest.Method = "GET";
                myWebRequest.UserAgent =
                    "Mozilla/5.0 (Windows NT 6.3; rv:36.0) Gecko/20100101 Firefox/36.0";
                myWebRequest.Timeout = 10000; //dat 10s
                myWebRequest.AllowAutoRedirect = true;
                // make request for web page
                HttpWebResponse myWebResponse = (HttpWebResponse)myWebRequest.GetResponse();
                StreamReader myWebSource = new StreamReader(myWebResponse.GetResponseStream());
                string myPageSource = myWebSource.ReadToEnd();
                myWebResponse.Close();

                return myPageSource;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static string GetHTML(string url, out HttpStatusCode responseCode)
        {
            try
            {
                HttpWebRequest myWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
                myWebRequest.Method = "GET";
                myWebRequest.UserAgent =
                    "Mozilla/5.0 (Windows NT 5.2; rv:2.0.1) Gecko/20100101 Firefox/4.0.1";
                myWebRequest.AllowAutoRedirect = false;
                myWebRequest.Timeout = 10000; //dat 10s
                // make request for web page
                HttpWebResponse myWebResponse = (HttpWebResponse)myWebRequest.GetResponse();
                responseCode = myWebResponse.StatusCode;
                StreamReader myWebSource = new StreamReader(myWebResponse.GetResponseStream());
                string myPageSource = myWebSource.ReadToEnd();
                myWebResponse.Close();

                return myPageSource;
            }
            catch (Exception ex)
            {
                responseCode = HttpStatusCode.BadRequest;
                return ex.Message;
            }
        }

        public static string GetHTMLNoRedirect(string url)
        {
            try
            {
                HttpWebRequest myWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
                myWebRequest.Method = "GET";
                myWebRequest.UserAgent =
                    "Mozilla/5.0 (Windows NT 5.2; rv:2.0.1) Gecko/20100101 Firefox/4.0.1";
                myWebRequest.Timeout = 10000; //dat 10s
                myWebRequest.AllowAutoRedirect = false;
                // make request for web page
                HttpWebResponse myWebResponse = (HttpWebResponse)myWebRequest.GetResponse();
                StreamReader myWebSource = new StreamReader(myWebResponse.GetResponseStream());
                string myPageSource = myWebSource.ReadToEnd();
                myWebResponse.Close();

                return myPageSource;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static String GetRedirectUrl(String url, CookieContainer cookies)
        {
            HttpStatusCode responseCode;
            try
            {
                HttpWebRequest myWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
                myWebRequest.Method = "GET";
                myWebRequest.UserAgent =
                    "Mozilla/5.0 (Windows NT 5.2; rv:2.0.1) Gecko/20100101 Firefox/4.0.1";
                myWebRequest.AllowAutoRedirect = false;
                myWebRequest.Timeout = 10000; //dat 10s
                myWebRequest.CookieContainer = cookies;
                // make request for web page
                HttpWebResponse myWebResponse = (HttpWebResponse)myWebRequest.GetResponse();
                responseCode = myWebResponse.StatusCode;
                StreamReader myWebSource = new StreamReader(myWebResponse.GetResponseStream());
                string myPageSource = myWebSource.ReadToEnd();
                myWebResponse.Close();
                String redirectURL = myWebRequest.GetResponse().Headers["Location"];
                return redirectURL;
            }
            catch (Exception ex)
            {
                responseCode = HttpStatusCode.BadRequest;
                return ex.Message;
            }
        }

        public static string GetHTMLWithCookie(
            string url,
            out HttpStatusCode responseCode,
            out CookieContainer cookie
        )
        {
            cookie = new CookieContainer();
            try
            {
                HttpWebRequest myWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
                myWebRequest.Method = "GET";
                myWebRequest.UserAgent =
                    "Mozilla/5.0 (Windows NT 5.2; rv:2.0.1) Gecko/20100101 Firefox/4.0.1";
                myWebRequest.AllowAutoRedirect = true;
                myWebRequest.CookieContainer = cookie;
                myWebRequest.Timeout = 10000; //dat 10s
                // make request for web page
                HttpWebResponse myWebResponse = (HttpWebResponse)myWebRequest.GetResponse();
                responseCode = myWebResponse.StatusCode;
                StreamReader myWebSource = new StreamReader(myWebResponse.GetResponseStream());
                string myPageSource = myWebSource.ReadToEnd();
                myWebResponse.Close();

                return myPageSource;
            }
            catch (Exception ex)
            {
                responseCode = HttpStatusCode.BadRequest;
                return ex.Message;
            }
        }

        public static string PostHTML(
            string url,
            List<object> headers,
            String postString,
            bool isgzip = false
        )
        {
            System.Net.ServicePointManager.Expect100Continue = false;
            HttpWebRequest webRequest;
            webRequest = WebRequest.Create(url) as HttpWebRequest;
            if (isgzip)
            {
                webRequest.AutomaticDecompression =
                    DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }
            webRequest.Method = "POST";
            webRequest.ContentLength = postString.Length;
            if (headers != null)
            {
                foreach (dynamic header in headers)
                {
                    String name = (string)header.name;
                    String value = (String)header.value;
                    if (name.Equals("Content-Type"))
                    {
                        webRequest.ContentType = value;
                    }
                    else if (name.Equals("Host"))
                    {
                        webRequest.Host = value;
                    }
                    else if (name.Equals("Accept"))
                    {
                        webRequest.Accept = value;
                    }
                    else if (name.Equals("Referer"))
                    {
                        webRequest.Referer = value;
                    }
                    else if (name.Equals("User-Agent"))
                    {
                        webRequest.UserAgent = value;
                    }
                    else if (name.Equals("AllowAutoRedirect"))
                    {
                        webRequest.AllowAutoRedirect = bool.Parse(value);
                    }
                    else
                    {
                        webRequest.Headers.Add(name, value);
                    }
                }
            }

            StreamWriter requestWriter = new StreamWriter(webRequest.GetRequestStream());
            requestWriter.Write(postString);
            requestWriter.Close();

            Encoding encode = System.Text.Encoding.GetEncoding("utf-8");

            //StreamReader readStream = new StreamReader(ReceiveStream, encode);
            StreamReader responseReader = new StreamReader(
                webRequest.GetResponse().GetResponseStream()
            );
            string responseData = responseReader.ReadToEnd();

            responseReader.Close();
            webRequest.GetResponse().Close();
            return responseData;
        }

        public static string PostHTML(string url, String postString)
        {
            const string contentType = "application/json";
            System.Net.ServicePointManager.Expect100Continue = false;

            CookieContainer cookies = new CookieContainer();
            HttpWebRequest webRequest = WebRequest.Create(url) as HttpWebRequest;
            webRequest.Method = "POST";
            webRequest.ContentType = contentType;
            webRequest.CookieContainer = cookies;
            webRequest.ContentLength = postString.Length;
            webRequest.UserAgent =
                "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.9.0.1) Gecko/2008070208 Firefox/3.0.1";
            webRequest.Accept =
                "text/html,application/xhtml+xml,application/xml,application/json;q=0.9,*/*;q=0.8";
            //webRequest.Referer = "https://accounts.craigslist.org";

            StreamWriter requestWriter = new StreamWriter(webRequest.GetRequestStream());
            requestWriter.Write(postString);
            requestWriter.Close();

            Encoding encode = System.Text.Encoding.GetEncoding("utf-8");

            //StreamReader readStream = new StreamReader(ReceiveStream, encode);
            StreamReader responseReader = new StreamReader(
                webRequest.GetResponse().GetResponseStream()
            );
            string responseData = responseReader.ReadToEnd();

            responseReader.Close();
            webRequest.GetResponse().Close();
            return responseData;
        }

        public static string PostHTML(string url)
        {
            try
            {
                HttpWebRequest myWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
                myWebRequest.Method = "POST";
                myWebRequest.UserAgent =
                    "Mozilla/5.0 (Windows NT 5.2; rv:2.0.1) Gecko/20100101 Firefox/4.0.1";
                myWebRequest.Timeout = 10000; //dat 10s
                // make request for web page
                HttpWebResponse myWebResponse = (HttpWebResponse)myWebRequest.GetResponse();
                StreamReader myWebSource = new StreamReader(myWebResponse.GetResponseStream());
                string myPageSource = myWebSource.ReadToEnd();
                myWebResponse.Close();

                return myPageSource;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
