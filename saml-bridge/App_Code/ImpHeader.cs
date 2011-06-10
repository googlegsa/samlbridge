using System;
using System.Collections.Specialized;
using System.Web;
using System.Net;
using System.Web.UI;
using System.IO;

namespace SAMLServices
{
	/// <summary>
	/// Summary description for ImpHeader.
	/// </summary>
    public class ImpHeader : AuthenticationPage, IImpersonation
	{
		public static String COOKIE_DOMAIN = "";
		public ImpHeader()
		{
			//
			// TODO: Add constructor logic here
			//
		}
		public String GetPermission(Page page, String url, String subject)
		{
			Common.debug("inside SsoAuthImpl:GetPermission, subject = " + subject);
			HttpRequest Request = (HttpRequest ) page.Application[subject];
			NameValueCollection headers = Request.Headers;
			Common.debug("headers = " + headers);
			HttpWebRequest web = (HttpWebRequest) WebRequest.Create(url);
			web.Method = "GET";
			if (Common.LOG_LEVEL == Common.DEBUG)
			{
				foreach(String key in headers.Keys)
				{
					if (key.StartsWith("SM"))
					{
						web.Headers.Add(key, headers.GetValues(key).ToString());
						Common.debug("add:::::" + key + "::::::" );
						foreach (String val in headers.GetValues(key))
						{
							Common.debug("value=" + val + "::::::" );
						}
					}
					else
					{
						Common.debug("bypass:::::" + key + "::::::" + headers.GetValues(key).ToString());
						foreach (String val in headers.GetValues(key))
						{
							Common.debug("value=" + val + "::::::" );
						}
					}
				}
			}
			Common.debug("Add cookies");
			try
			{
				if (web.CookieContainer == null)
				{
					web.CookieContainer  = new CookieContainer();
				}
				String domain = COOKIE_DOMAIN;
				Common.debug("set cookie domain to " + domain);
				foreach (String key in Request.Cookies)
				{
					Common.debug("add :::::" + key + ":::" + Request.Cookies[key].Value);
					web.CookieContainer.Add(new Cookie(key, Request.Cookies[key].Value,  "/", domain));
				}
				//add smsession
				//web.CookieContainer.Add(new Cookie("SMSESSION", Request.Cookies["SMSESSION"].Value,  "/", "google.com"));
			}
			catch(Exception e)
			{
				Common.debug(e.Message);
			}
			Common.debug("before getResponse");			
			
			try
			{
				web.AllowAutoRedirect = false;
				HttpWebResponse response  = (HttpWebResponse)web.GetResponse();
				Common.debug("response code  "  + response.StatusCode);
				if (Common.LOG_LEVEL == Common.DEBUG)
				{
					Common.debug("response header");
					dumpHeaders(response.Headers);
					dumpResponse();
				}
				return handleDeny();
			}
			catch(Exception e)
			{
				Common.debug(e.Message);
				return "Deny";
			}
		}

	}
}
