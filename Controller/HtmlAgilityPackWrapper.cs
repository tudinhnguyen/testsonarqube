using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teechip.Controller
{
    class HtmlAgilityPackWrapper
    {
        public static bool ParseGoogleLoginPage(string strEmail, string strPassword, string strHTMLLoginPage, string strReferer, out string strData, out string strPostUri)
        {
            // Expected strData BEGIN
            // ======================
            // GALX=K2kN1B3RhzI
            // &continue=https%3A%2F%2Faccounts.google.com%2Fo%2Foauth2%2Fauth%3Fscope%3Demail%26response_type%3Dcode%26redirect_uri%3Dhttps%3A%2F%2Fteechip.com%2Fgoogle%26client_id%3D759311199253-liule4h0ij4trokni0clghutesmfrpaq.apps.googleusercontent.com%26hl%3Dvi%26from_login%3D1%26as%3D6c13d32c4c484e0a
            // &service=lso
            // &ltmpl=popup
            // &shdf=Cm4LEhF0aGlyZFBhcnR5TG9nb1VybBoADAsSFXRoaXJkUGFydHlEaXNwbGF5TmFtZRoHVGVlQ2hpcAwLEgZkb21haW4aB1RlZUNoaXAMCxIVdGhpcmRQYXJ0eURpc3BsYXlUeXBlGgdERUZBVUxUDBIDbHNvIhR9f65sYBLeiTyA3ksWmSKfvjjy-CgBMhR69Mfqff5tofmPaXiU63kbTQdAzw
            // &scc=1
            // &sarp=1
            // &_utf8=%E2%98%83
            // &bgresponse=%21qapCVouLleiO7EZE_x4-8E88lh4CAAAAMFIAAAAFKgEfCMGz3oizuMFi9s1elOqqg8JcOP5CRdkNPXH--weicJaEAyf-pqsUcGZzFbdUIPWD6hVYEdNFEz35CvkIrsyHVpQRGe5ksfhAee9ehS3pR6bzwXyNFX7Pap-J7SQaN1swC53RMscX-GgiAB7Bd4pINn3QSjFPup0K0JDhJsMqBdkFQikNK4TCndA3NFztVcnkdr91yQuMEwxpfG6CjcHt3ckSyopOpBsFuHRcRqeEorlpvItyd9ITIvZdWd6LNZJgkDdAVbI2TdP3bPWBv9Lu5ux73a-RC4Oc7ow5uAWWbcMQ2D_4hKd4xT66Ie6pV8U4JxgvxhegMPaylafpK6NrqFNbEtEHjTksqILJKLRInylFLU37UpdQuwqPtUWz240
            // &pstMsg=1
            // &dnConn=
            // &checkConnection=youtube%3A3988%3A1
            // &checkedDomains=youtube
            // &Email=<strEmail>
            // &Passwd=<strPassword>
            // &signIn=Sign+in
            // &PersistentCookie=yes
            // ======================
            // Expected strData END

            strData = "";
            strPostUri = "";

            HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.LoadHtml(strHTMLLoginPage);

            if (htmlDoc.DocumentNode != null)
            {
                HtmlAgilityPack.HtmlNode frmNode = htmlDoc.DocumentNode.SelectSingleNode("//form[@id=\"gaia_loginform\"]");

                if (frmNode != null)
                {
                    strPostUri = frmNode.GetAttributeValue("action", "");

                    BuildDataString(frmNode, "Page", ref strData, true);

                    BuildDataString(frmNode, "GALX", ref strData, string.IsNullOrEmpty(strData));

					BuildDataString(frmNode, "gxf", ref strData, string.IsNullOrEmpty(strData));

                    BuildDataString(GetValueByName(frmNode, "input", "continue"), "continue", ref strData);
                    BuildDataString(GetValueByName(frmNode, "input", "followup"), "followup", ref strData);
                    
                    BuildDataString(frmNode, "service", ref strData);
                    BuildDataString(frmNode, "ProfileInformation", ref strData);
                    BuildDataString(GetValueByName(frmNode, "input", "_utf8"), "_utf8", ref strData);
                    BuildDataString(frmNode, "bgresponse", ref strData);
                    BuildDataString(frmNode, "pstMsg", ref strData);
                    BuildDataString(frmNode, "dnConn", ref strData);
                    BuildDataString(frmNode, "checkConnection", ref strData);
                    BuildDataString(frmNode, "checkedDomains", ref strData);
                    BuildDataString(frmNode, "identifiertoken", ref strData);
                    BuildDataString(frmNode, "identifiertoken_audio", ref strData);

                    BuildDataString(strEmail, "Email", ref strData);
                    BuildDataString(strPassword, "Passwd", ref strData);
                    BuildDataString(frmNode, "rmShown", ref strData);
                }
            }

            return false; ;
        }

        private static string GetValueByName(HtmlAgilityPack.HtmlNode oParentNode, string strNodeName, string strNameAttribute)
        {
            return oParentNode.SelectSingleNode(string.Format("//{0}[@name=\"{1}\"]", strNodeName, strNameAttribute)).GetAttributeValue("value", "");
        }

        private static void BuildDataString(HtmlAgilityPack.HtmlNode oFormNode, string strKey, ref string strData, bool isFirst = false)
        {
            try
            {
                string strValue = GetValueByName(oFormNode, "input", strKey);
                if (!string.IsNullOrEmpty(strValue))
                {
                    //strValue = System.Uri.EscapeDataString(strValue);
                    if (string.IsNullOrEmpty(strData))
                    {
                        strData += string.Format("{0}={1}", strKey, strValue);
                    }
                    else
                    {
                        strData += string.Format("&{0}={1}", strKey, strValue);
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.LogError(ex);
            }
        }

        private static void BuildDataString(string strRawValue, string strKey, ref string strData, bool isFirst = false)
        {
            // TODO: need to encode value
            string strValue = System.Uri.EscapeDataString(strRawValue);
            if (!string.IsNullOrEmpty(strValue))
            {
                if (isFirst)
                {
                    strData += string.Format("{0}={1}", strKey, strValue);
                }
                else
                {
                    strData += string.Format("&{0}={1}", strKey, strValue);
                }
            }
        }

        #region aaa

        #endregion

        internal static string ParseGooGL(string strGoogleLoginPage)
        {
            string ret = "";
            try
            {
                HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
                htmlDoc.LoadHtml(strGoogleLoginPage);

                if (htmlDoc.DocumentNode != null)
                {
                    HtmlAgilityPack.HtmlNode frmNode = htmlDoc.DocumentNode;

                    if (frmNode != null)
                    {
                        BuildDataString(GetValueByName(frmNode, "input", "security_token"), "security_token", ref ret);
                    }
                }
            }
            catch
            {
            }

            return ret;
        }
    }
}
