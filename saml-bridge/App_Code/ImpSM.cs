using System.Configuration;
using System;
using System.Web.UI;
using System.Web;
using System.Net;
using System.Text;
using System.IO;

namespace SAMLServices
{
	/// <summary>
	/// Summary description for SiteMinderImpersonation.
	/// </summary>
    public class ImpSM : AuthenticationPage, IImpersonation
	{
		public static string IMP_TOR = null;
		public static string IMP_PWD = null;
		public static string IMP_START = null;
		public static string IMP_END = null;
		public static string AUTH_REALM = null;
		HttpWebRequest web;
		CookieCollection mCookies;
		public ImpSM()
		{
			IMP_TOR = ConfigurationSettings.AppSettings["sm_impersonator"];
			Common.debug("impersonator: " + IMP_TOR);
			IMP_PWD = ConfigurationSettings.AppSettings["sm_impersonator_pwd"];
			Common.debug("password: " + IMP_PWD);
			IMP_START = ConfigurationSettings.AppSettings["sm_start_imp_fcc"];
			IMP_END = ConfigurationSettings.AppSettings["sm_end_imp_fcc"];
			AUTH_REALM = ConfigurationSettings.AppSettings["sm_impersonator_realm"];
		}

		void Login()
		{
			Common.debug("impersonator login: " + AUTH_REALM);
			web = (HttpWebRequest) WebRequest.Create(AUTH_REALM);
			web.Credentials = new NetworkCredential(IMP_TOR, IMP_PWD);
			web.KeepAlive = true;
			web.CookieContainer = new CookieContainer();
			HttpWebResponse resp = (HttpWebResponse) web.GetResponse();
			mCookies = resp.Cookies;
			dumpHeaders(resp.Headers);
			dumpResponse();
		}

		public String GetPermission(Page page, String url, String subject)
		{
			try
			{
				//check whether there is a valid session
				Login();
				//start impersonation
				Common.debug("start impersonation");
				web = (HttpWebRequest) WebRequest.Create(IMP_START);
				web.CookieContainer = new CookieContainer();
				web.CookieContainer.Add(mCookies);
				//web.AllowAutoRedirect = true;
				ASCIIEncoding encoding=new ASCIIEncoding();
				string postData="USER="+subject + "&TARGET="+ url + "&submit=impersonate_now";
				//byte[]  data = encoding.GetBytes(postData);
				web.Method = "POST";
				web.ContentType="application/x-www-form-urlencoded";
				web.ContentLength = postData.Length;
				web.Referer = IMP_START;
				StreamWriter writer = new StreamWriter(web.GetRequestStream());
				//newStream.Write(data,0,data.Length);
				writer.Write(postData);
				writer.Close();
				HttpWebResponse resp = (HttpWebResponse) web.GetResponse();
				resp = (HttpWebResponse) web.GetResponse();
				CookieCollection cookies = resp.Cookies;
				dumpHeaders(resp.Headers);
				dumpResponse();
				//test URL
				Common.debug("test URL");
				web = (HttpWebRequest) WebRequest.Create(url);
				web.Method = "HEAD";
				web.CookieContainer = new CookieContainer();
				web.CookieContainer.Add(cookies);
				resp = (HttpWebResponse) web.GetResponse();
				dumpHeaders(resp.Headers);
				dumpResponse();
				cookies = resp.Cookies;
				//end impersonation
				Common.debug("end impersonation");
				web = (HttpWebRequest) WebRequest.Create(IMP_END);
				web.Method = "GET";
				web.CookieContainer = new CookieContainer();
				web.CookieContainer.Add(cookies);
				resp = (HttpWebResponse) web.GetResponse();
				dumpResponse();
				return "Permit";
			}
			catch(Exception e)
			{
				Common.debug(e.Message);
				return "Deny";
			}
		}
	}
}
