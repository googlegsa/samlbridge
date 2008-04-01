/*
 * Copyright (C) 2006-2008 Google Inc.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not
 * use this file except in compliance with the License. You may obtain a copy of
 * the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations under
 * the License.
 */

using System;
using System.Web;
using System.Net;
using System.IO;
using System.Configuration;

namespace SAMLServices
{
	/// <summary>
	/// Summary description for SsoAuthImpl.
	/// </summary>
	public class SsoAuthImpl: IAuthn, IAuthz
	{
		System.Web.UI.Page page;
		public static string IMP_METHOD = null;
		IImpersonation imp = null;

		public SsoAuthImpl(System.Web.UI.Page page)
		{
			IMP_METHOD  = ConfigurationSettings.AppSettings["impersonation_method"];
			this.page = page;
			if (IMP_METHOD.Equals("header_playback"))
			{
				Common.debug("instantiate ImpHeader");
				imp = new ImpHeader();
				return;
			}
			if (IMP_METHOD.Equals("siteminder"))
			{
				Common.debug("instantiate ImpSiteMinder");
				imp = new ImpSiteMinder();
				return;
			}
			Common.debug("missing impersonation provider");
		}

		#region IAuthn Members
		/// <summary>
		// Method to obtain a user's acount name
		/// </summary>
		/// <returns>user account user@domain</returns>
		public String GetUserIdentity()
		{
			Common.debug("SsoAuthImpl:GetUserIdentity");
			HttpRequest Request = page.Request;
			String currentUser = null;
			if (Common.SsoSubjectVar.StartsWith("HTTP_"))
			{
				String subjectVar = Common.SsoSubjectVar.Substring(5);
				Common.debug(" header name is " + subjectVar);
				String[] headers = Request.Headers.GetValues(subjectVar);
				if (headers != null)
					currentUser = headers[0];
			}
			else
			{
				if (Request.Cookies.Get(Common.SsoSubjectVar) != null)
					currentUser = Request.Cookies.Get(Common.SsoSubjectVar).Value;
			}
			Common.debug("user = " + currentUser);
			storeHeaders(currentUser);
			return currentUser;
		}

		public void Diagnose()
		{
			HttpRequest Request = page.Request;
			HttpResponse Response = page.Response;

			Common.printHeader(Response);
			Response.Write("Request Headers<br>");
			Response.Write("<table><tr span=2>");
			for (int i =0; i< Request.Headers.Count; ++i)
			{
				Response.Write("<tr>");
				Response.Write("<td>" + Request.Headers.Keys[i] + "</td><td>" );
				foreach (String val in Request.Headers.GetValues(i))
				{
					Response.Write(val + ",  ");
				}
				Response.Write("</td>");
			}
			Response.Write("</tr></table>");
			Common.printFooter(Response);
		}

		private void storeHeaders(String subject)
		{
			HttpRequest Request = page.Request;
			page.Application.Add(subject, Request);

		}
		#endregion
		#region IAuthz Members

		public String GetPermission(String url, String subject)
		{
				return imp.GetPermission(page, url, subject);
		}
		/// <summary>
		/// Method to determine whether a user has acces to a given URL
		/// </summary>
		/// <param name="url">Target URL</param>
		/// <param name="subject">account to be tested, in the form of username@domain</param>
		/// <returns>Permit|Deny|Intermediate</returns>

		#endregion
	}
}

