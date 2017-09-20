using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace Teechip.Controller
{
    enum eAccountType
    {
        Google = 0,
        Facebook = 1,
        TCAccount = 2,
        Unknown = -1
    }

    class Constants
    {
        // product name; color name; color value
        //public static Dictionary<string, Dictionary<string, string>> m_dicColors = null;
    }
    class TeechipControl : RequestManager
    {
        eAccountType m_oAccountType;
        string m_strCurrentAccount;
        string m_strPassword;
        bool m_isLogined;

        static TeechipControl instance = null;

        public static TeechipControl Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new TeechipControl();
                }

                return instance;
            }
        }

        TeechipControl()
            : base()
        {
            m_isLogined = false;
            m_oAccountType = eAccountType.Unknown;
            m_strCurrentAccount = "";
            m_strPassword = "";
        }

        ~TeechipControl()
        {

        }

        public void StopUploadThread()
        {
        }

        #region Login functions

        string _loginInforFile2 = "";
        internal string LoginInforFile2
        {
            get
            {
                if (string.IsNullOrEmpty(_loginInforFile2))
                {
                    string exePath = Path.GetDirectoryName(Application.ExecutablePath);
                    _loginInforFile2 = Path.Combine(exePath, @"urlshortener.dat");
                }
                return _loginInforFile2;
            }
        }
        public bool Login(eAccountType oAccountType, string strUser, string strPassword, bool bSavePassword, bool bServiceMode = false)
        {
            if (m_isLogined
                && m_oAccountType == oAccountType
                && strUser.Trim().Equals(m_strCurrentAccount, StringComparison.InvariantCultureIgnoreCase)
                && strPassword == m_strPassword)
            {
                return m_isLogined;
            }
            try
            {
                switch (oAccountType)
                {
                    case eAccountType.Google:
                        m_isLogined = LoginGoogle(strUser.Trim(), strPassword);
                        break;
                    case eAccountType.Facebook:

                        break;
                    case eAccountType.TCAccount:
                        m_isLogined = LoginTC(strUser.Trim(), strPassword);
                        break;
                    default:
                        break;
                }

                if (m_isLogined)
                {
                    m_oAccountType = oAccountType;
                    m_strCurrentAccount = strUser.Trim();
                    m_strPassword = strPassword;


                    if (bSavePassword)
                    {
                        Utility.SaveAccount(LoginInforFile2, strUser.Trim(), strPassword, (int)m_oAccountType);
                    }
                    else
                    {
                        File.Delete(LoginInforFile2);
                    }
                }

                return m_isLogined;
            }
            catch (Exception ex)
            {
                Utility.LogError(ex);
            }
            return false;
        }

        internal void LogUploadProgress(string prefix, bool done = true)
        {

        }

        private bool LoginTC(string strUser, string strPassword)
        {

            return (LastResponseStatus == HttpStatusCode.OK);
        }
        public string security_token = "";
        private bool LoginGoogle(string strUser, string strPasswod)
        {
            string strResponseUri = "";
            string strRequestUri = "https://accounts.google.com/ServiceLogin?service=urlshortener&continue=https%3A%2F%2Fgoo.gl%2F%3Fauthed%3D1&followup=https%3A%2F%2Fgoo.gl%2F%3Fauthed%3D1&passive=1209600";


            string strData = "";
            string strGoogleLoginPage;

            // Get google login page
            strGoogleLoginPage = SendGetRequest(ref strRequestUri, ref strResponseUri, true);

            Utility.LogInfo(strGoogleLoginPage, @"log\strGoogleLoginPage.log", false, false);

            // Parse login page to get hidden fields value
            HtmlAgilityPackWrapper.ParseGoogleLoginPage(strUser, strPasswod, strGoogleLoginPage, m_strReferer, out strData, out strRequestUri);

            Utility.LogInfo(strData, @"log\ParseGoogleLoginPage.log", false, false);

            //strRequestUri = "https://accounts.google.com/ServiceLoginAuth";
            // Send login post request to google
            strGoogleLoginPage = SendPostRequest(ref strRequestUri, strData, true, ref strResponseUri);

			if (string.IsNullOrEmpty(security_token))
			{
				strRequestUri = "https://accounts.google.com/ServiceLoginAuth";

				strGoogleLoginPage = SendPostRequest(ref strRequestUri, strData, true, ref strResponseUri);

				security_token = HtmlAgilityPackWrapper.ParseGooGL(strGoogleLoginPage);
			}

            try
            {
                if (!strResponseUri.ToLower().Contains("goo.gl"))
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }

            return (LastResponseStatus == HttpStatusCode.OK);
        }

        internal string Shortener(string url)
        {
			if (url.StartsWith("http://goo.gl") || url.StartsWith("https://goo.gl"))
			{
				return url;
			}
            string ret = url;
            try
            {
                url = System.Uri.EscapeDataString(url);
                //&botguard_response=!GRpCDGctCU9kYu1Ed_3LnLWRdLsCAAAAMlIAAAAFKgEfuSpfOQiIAStMsOyiEMN-7_1NFgXw61ttuUnwPquGRH84YWopoqnJid_VG8G5bc7x9zkg1-8keD0Q39VMy84KoYUZCxzbObEqrPAgKhTYl_2qbSSIQvjcXL1Z6-Gixp-qkn8EJrTNHOFpzw-zthMrhU_GkZP2-2RfJaOWLp_NV0w8CqO0zPAMbmLb00gX_mpTLay4x0t27ji1StlDyGsretyWj0C0e9S1mA0x7lmK8ozLZ1R35y897OMrIzu6kw5WRiMgYxXQ2Irwfc4g9VbNB2iMKUMWymqwrJ3awnYvWKqYZdxiujCIFYBOH1-VT6Zf5OvhjW_9YTAJJsf_EuOdf0Nej7PCisq0mSdw5vXeKN75YYKqV4Jeh75YEJIZEW4
                string strData = string.Format("url={0}&captcha_response=&{1}&typed_url={2}", url, security_token, url);
                string strResponseUri = "";
				string strRequestUri = "https://goo.gl/api/shorten";

                string strGoogleLoginPage = SendPostRequest(ref strRequestUri, strData, true, ref strResponseUri);

                ret = Newtonsoft.Json.Linq.JObject.Parse(strGoogleLoginPage)["short_url"].ToString();

                //return jRoot.First["short_url"].ToString();
            }
            catch (Exception ex)
            {
				MessageBox.Show(ex.Message);
            }
            return ret;
        }

        #endregion // Login functions

        public string SendGetRequest(ref string strRequestUri, ref string strResponseUri, bool allowAutoRedirect)
        {
            string strOriginalRequestUri = strRequestUri;
            HttpWebResponse oResponse = null;
            string strResponse = "";

            oResponse = SendGETRequest(strRequestUri, "", "", allowAutoRedirect);

            if (null != oResponse)
            {
                if (null != oResponse.Headers["Location"] && !string.IsNullOrEmpty(oResponse.Headers["Location"]))
                {
                    strRequestUri = oResponse.Headers["Location"];
                }
                else
                {
                    strRequestUri = "";
                }

                strResponse = GetResponseContent(oResponse);
                strResponseUri = oResponse.ResponseUri.ToString();
            }

            Utility.LogInfo(strOriginalRequestUri, "log\\GET_response.html", true, false);
            Utility.LogInfo(strResponse, "log\\GET_response.html", true, false);

            return strResponse;
        }

        public string SendPostRequest(ref string strRequestUri, string strData, bool allowAutoRedirect, ref string strResponseUri)
        {
            string strOriginalRequestUri = strRequestUri;
            HttpWebResponse oResponse = null;
            string strResponse = "";

            oResponse = SendPOSTRequest(strRequestUri, strData, "", "", allowAutoRedirect);

            if (null != oResponse)
            {
                if (null != oResponse.Headers["Location"] && !string.IsNullOrEmpty(oResponse.Headers["Location"]))
                {
                    strRequestUri = oResponse.Headers["Location"];
                }
                else
                {
                    strRequestUri = "";
                }

                strResponse = GetResponseContent(oResponse);
                strResponseUri = oResponse.ResponseUri.ToString();
            }

            Utility.LogInfo(strOriginalRequestUri, "log\\POST_response.html", true, false);
            Utility.LogInfo(strResponse, "log\\POST_response.html", true, false);

            return strResponse;
        }

        public string SendDeleteRequestJson(ref string strRequestUri, string strData, bool allowAutoRedirect, ref string strResponseUri)
        {
            return SendRequestJson("DELETE", ref strRequestUri, strData, allowAutoRedirect, ref strResponseUri);
        }

        public string SendPATCHRequestJson(ref string strRequestUri, string strData, bool allowAutoRedirect, ref string strResponseUri)
        {
            return SendRequestJson("PATCH", ref strRequestUri, strData, allowAutoRedirect, ref strResponseUri);
        }

        public string SendPostRequestJson(ref string strRequestUri, string strData, bool allowAutoRedirect, ref string strResponseUri)
        {
            return SendRequestJson("POST", ref strRequestUri, strData, allowAutoRedirect, ref strResponseUri);
        }

        public string SendRequestJson(string strMethod, ref string strRequestUri, string strData, bool allowAutoRedirect, ref string strResponseUri)
        {
            string strOriginalRequestUri = strRequestUri;
            HttpWebResponse oResponse = null;
            string strResponse = "";

            if (strRequestUri == null)
            {
                throw new ArgumentNullException("uri");
            }
            // Create a request using a URL that can receive a post. 
            HttpWebRequest oRequest = (HttpWebRequest)HttpWebRequest.Create(strRequestUri);
            // Set the Method property of the request to POST.
            oRequest.Method = strMethod;
            // Set cookie container to maintain cookies
            oRequest.CookieContainer = cookies;
            oRequest.AllowAutoRedirect = allowAutoRedirect;

            oRequest.Credentials = CredentialCache.DefaultNetworkCredentials;
            oRequest.Referer = m_strReferer;

            // Add special header for Teechip
            foreach (Cookie iterCookie in cookies.GetCookies(new Uri("http://teechip.com")))
            {
                if (iterCookie.Name.Contains("XSRF-TOKEN"))
                {
                    oRequest.Headers.Add(iterCookie.Name, iterCookie.Value);
                    break;
                }
            }
            oRequest.KeepAlive = true;
            // Convert POST data to a byte array.
            byte[] byteArray = Encoding.UTF8.GetBytes(strData);
            // Set the ContentType property of the WebRequest.
            oRequest.ContentType = "application/json;charset=UTF-8";
            oRequest.Accept = "application/json, text/plain, */*";
            // Set the ContentLength property of the WebRequest.
            oRequest.ContentLength = byteArray.Length;
            // Get the request stream.
            Stream dataStream = oRequest.GetRequestStream();
            // Write the data to the request stream.
            dataStream.Write(byteArray, 0, byteArray.Length);
            // Close the Stream object.
            dataStream.Close();

            oResponse = GetResponse(oRequest);
            if (null != oResponse)
            {
                if (null != oResponse.Headers["Location"] && !string.IsNullOrEmpty(oResponse.Headers["Location"]))
                {
                    strRequestUri = oResponse.Headers["Location"];
                }
                else
                {
                    strRequestUri = "";
                }

                strResponse = GetResponseContent(oResponse);
                strResponseUri = oResponse.ResponseUri.ToString();
            }

            Utility.LogInfo(strOriginalRequestUri, "log\\POSTJSON_resonse.html", true, false);
            Utility.LogInfo(strResponse, "log\\POSTJSON_resonse.html", true, false);

            return strResponse;
        }
    }
}
