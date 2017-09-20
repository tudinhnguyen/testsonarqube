using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Teechip.Controller
{
    internal class WBUtils
    {
        internal static bool WaitForComplete(WebBrowser webBrowser1, double dTimeout = 5000)
        {
            DateTime dtTimeout = DateTime.Now.AddMilliseconds(dTimeout);
            bool bCompleted = false;

            while (!bCompleted && dtTimeout.CompareTo(DateTime.Now) > 0)
            {
                if (webBrowser1.ReadyState == WebBrowserReadyState.Complete && webBrowser1.IsBusy == false)
                {
                    bCompleted = true;
                }
                Thread.Sleep(100);
                Application.DoEvents();
            }

            return bCompleted;
        }

        internal static bool IsBrowerComplete(WebBrowser webBrowser1)
        {
            if (webBrowser1.ReadyState == WebBrowserReadyState.Complete && webBrowser1.IsBusy == false)
            {
                return true;
            }

            return false;
        }

        internal static bool ClickBtn(WebBrowser webBrowser1, string strCancelBtn, string strConfirmBtn, string strFormId, double dTimeout = 1)
        {
            bool bFound = false;
            DateTime dtTimeOut = DateTime.Now.AddMilliseconds(dTimeout);
            while (!bFound && dtTimeOut.CompareTo(DateTime.Now) > 0)
            {
                try
                {
                    bool allButtonsExisted = false;
                    foreach (HtmlElement itemitem in webBrowser1.Document.GetElementById(strFormId).All)
                    {
                        Application.DoEvents();
                        if (itemitem.Name == strCancelBtn)
                        {
                            allButtonsExisted = true;
                            break;
                        }
                    }
                    if (allButtonsExisted)
                    {
                        foreach (HtmlElement itemitem in webBrowser1.Document.GetElementById(strFormId).All)
                        {
                            Application.DoEvents();
                            if (itemitem.Name == strConfirmBtn)
                            {
                                itemitem.InvokeMember("Click");
                                bFound = true;
                                break;
                            }
                        }
                    }
                }
                catch
                {
                }
                Thread.Sleep(300);
            }

            return bFound;
        }
    }
}
