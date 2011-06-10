/*
 * Copyright (C) 2006 Google Inc.
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
using System.IO;
using System.Security;
using System.Security.Principal;
using System.Web.UI;
using System.Net;
using System.Security.AccessControl;
using System.Collections;

namespace SAMLServices.Wia
{
	/// <summary>
	/// Implementation for AA interfaces, Windows Integrated Auth and IIS security check
	/// </summary>
    public class AuthImpl : AuthenticationPage, IAuthn, IAuthz
	{
		System.Web.UI.Page page;
		public AuthImpl(System.Web.UI.Page page)
		{
			this.page = page;
		}
		#region IAuthn Members
		/// <summary>
		// Method to obtain a user's acount name
		/// </summary>
		/// <returns>user account user@domain</returns>
		public String GetUserIdentity()
		{
			String principal = page.User.Identity.Name;
			String[] prins = principal.Split('\\');
			//Kerberos accepts this format
			if (prins.Length == 2)
				principal = prins[1] + "@" + prins[0];
			return principal;
		}

		#endregion

		#region IAuthz Members

		/// <summary>
		/// Method to determine whether a user has acces to a given URL
		/// </summary>
		/// <param name="url">Target URL</param>
		/// <param name="subject">account to be tested, in the form of username@domain</param>
		/// <returns>Permit|Deny|Intermediate</returns>

		public String GetPermission(String url, String subject)
		{
			Common.debug("inside AuthImpl::GetPermission");
			Common.debug("url=" + url);
			Common.debug("subject=" + subject);
			// Convert the user name from domainName\userName format to 
			// userName@domainName format if necessary
	
			// The WindowsIdentity(string) constructor uses the new
			// Kerberos S4U extension to get a logon for the user
			// without a password.

			// Default AuthZ decision is set to "Deny"
			String status = "Deny";
			// Attempt to impersonate the user and verify access to the URL
			WindowsImpersonationContext wic = null;
			try
			{
				status = "before impersonation";
				Common.debug("before WindowsIdentity");
				// Create a Windows Identity
				WindowsIdentity wi = new WindowsIdentity(subject);
				if (wi == null)
				{
					Common.error("Couldn't get WindowsIdentity for account " + subject);
					return "Indeterminate";
				}
				Common.debug("name=" + wi.Name + ", authed=" + wi.IsAuthenticated);				
				Common.debug("after WindowsIdentity");
				// Impersonate the user
				wic = wi.Impersonate();
				Common.debug("after impersonate");
				// Attempt to access the network resources as this user
                if (url.StartsWith("http"))
                    status = GetURL(url, CredentialCache.DefaultCredentials);
                else if (url.StartsWith("smb"))
                {
                    String file = url;
                    file = file.Replace("smb://", "\\\\");
                    status = GetFile(file, wi);
                }
                else if (url.StartsWith("\\\\"))
                {
                    status = GetFile(url, wi);
                }
                else
                {
                    status = "Deny";
                }
                // Successfully retrieved URL, so set AuthZ decision to "Permit"
			}
			catch(SecurityException e)
			{
				Common.error("AuthImpl::caught SecurityException");
				// Determine what sort of exception was thrown by checking the response status
				Common.error("e = " + e.ToString());
				Common.error("msg = " + e.Message);
				Common.error("grantedset = " + e.GrantedSet);
				Common.error("innerException= " + e.InnerException);
				Common.error("PermissionState = " + e.PermissionState);
				Common.error("PermissionType = " + e.PermissionType);
				Common.error("RefusedSet = " + e.RefusedSet);
				Common.error("TargetSet = " + e.TargetSite);
				status = "Indeterminate";
				return status;
			}
			catch(WebException e)
			{
				if( wic != null)
				{
					wic.Undo();
					wic = null;
				}
				Common.debug("AuthImpl::caught WebException");
				// Determine what sort of exception was thrown by checking the response status
				Common.debug("e = " + e.ToString());
				HttpWebResponse resp = (HttpWebResponse)((WebException)e).Response;
				
                if (resp != null)
					Common.debug("status = " + resp.StatusCode.ToString());
				else
				{
					Common.debug("response is null");
					status = "Indeterminate";
					return status;
				}
				status = "Deny";
			}
			catch(UnauthorizedAccessException e) //can't write to the log file
			{
                if (wic != null)
				{
					wic.Undo();
					wic = null;
				}
                Common.debug("caught UnauthorizedAccessException");
                Common.debug(e.Message);
                status = "Deny";
			}
			catch(Exception e)
			{
				if( wic != null)
				{
					wic.Undo();
					wic = null;
				}
				// Some undetermined exception occured
				// Setting the AuthZ decision to "Indeterminate" allows the GSA to use other
				// AuthZ methods (i.e. Basic, NTLM, SSO) to determine access
				Common.error("AuthImpl::caught exception");
				Common.error(e.Message);
				status = "Indeterminate, unknown exception, check ac.log";
			}
			finally
			{
				// Make sure to remove the impersonation token
				if( wic != null)
					wic.Undo();
				Common.debug("exit AuthImpl::GetPermission::finally status=" + status);
			}
			Common.debug("exit AuthImpl::GetPermission return status=" + status);
			return status;
		}

		/// <summary>
		/// Method to perform an HTTP GET request for a URL.
		///This is used in combination with user impersonation
		/// to determine whether the user has access to the document
		/// </summary>
		/// <param name="url">target URL</param>
		/// <param name="cred">The credential to be used when accessing the URL</param>
		/// <returns></returns>
		public String GetURL(String url, ICredentials cred)
		{
			Common.debug("inside GetURL internal");
			HttpWebRequest web = (HttpWebRequest) WebRequest.Create(url);
			web.AllowAutoRedirect = false;

			web.Method = "HEAD";
			web.Credentials = cred;
			
			//web.Credentials = CredentialCache.DefaultNetworkCredentials;
			//web.Credentials = new NetworkCredential("username", "password", "myDomain");
			Common.debug("Sending a Head request to target URL");
			//you can set a proxy if you need to
			//web.Proxy = new WebProxy("http://proxyhost.abccorp.com", 3128);
			// Get/Read the response from the remote HTTP server
			HttpWebResponse response  = (HttpWebResponse)web.GetResponse();
			if (Common.LOG_LEVEL == Common.DEBUG)
			{
                Common.debug("let's see what we've got, the response page is: ");
                web.Method = "GET";
                response = (HttpWebResponse) web.GetResponse();
				Stream responseStream = response.GetResponseStream();
				StreamReader reader = new StreamReader (responseStream);
				String res = reader.ReadToEnd ();
                Common.debug(res);
                Common.debug("end of response");
				responseStream.Close();
			}
			return handleDeny();
		}


        public String GetFile(String url, WindowsIdentity wi)
        {
            Common.debug("GetFile: " + url);
            //urldecode, because GSA sends URL for file in encoded format
            url = System.Web.HttpUtility.UrlDecode(url);
            Common.debug("afer : " + url);
            //FileInfo fi = new FileInfo(url);
            FileSecurity security = File.GetAccessControl(url);
            AuthorizationRuleCollection acl = security.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount));
            bool allowRead = false;
            String user = wi.Name;
            //check users directly
            Common.debug(" acl count = " + acl.Count);
            Common.debug("user " + wi.Name);
            bool bAllow = false;

            //check user 
            for (int i = 0; i < acl.Count; i++)
            {
                System.Security.AccessControl.FileSystemAccessRule rule = (System.Security.AccessControl.FileSystemAccessRule)acl[i];
                Common.debug("user listed in acl: '" + rule.IdentityReference.Value + "'");
                Common.debug("current user:'" + user + "'");
                if (user.Equals(rule.IdentityReference.Value))
                {
                    Common.debug("match user " + user);
                    if (System.Security.AccessControl.AccessControlType.Deny.Equals(rule.AccessControlType))
                    {
                        Common.debug("deny");
                        if (contains(FileSystemRights.Read, rule))
                        {
                            Common.debug("read");
                            return "Deny"; //if any deny, it's deny
                        }
                    }
                    if (System.Security.AccessControl.AccessControlType.Allow.Equals(rule.AccessControlType))
                    {
                        Common.debug("allow");
                        if (contains(FileSystemRights.Read, rule))
                        {
                            Common.debug("allow @ user level is set");
                            bAllow = true; 
                        }
                    }
                }
            }
            //check groups

            IdentityReferenceCollection groups = wi.Groups;
            for (int j = 0; j < groups.Count; j++)
            {
                for (int i = 0; i < acl.Count; i++)
                {
                    System.Security.AccessControl.FileSystemAccessRule rule = (System.Security.AccessControl.FileSystemAccessRule)acl[i];
                    IdentityReference group = groups[j].Translate(typeof(System.Security.Principal.NTAccount));
                    //Common.debug("check the group " + group.Value);
                    //Common.debug("rule.IdentityReference.Value = " + rule.IdentityReference.Value);
                    if (group.Value.Equals(rule.IdentityReference.Value))
                    {
                        int iCurrent = -1;
                        Common.debug("found the group!" + group.Value);
                        if (System.Security.AccessControl.AccessControlType.Deny.Equals(rule.AccessControlType))
                        {
                            Common.debug("deny");
                            if (contains(FileSystemRights.Read, rule))
                            {
                                Common.debug("read");
                                return "Deny";
                            }
                        }
                        if (System.Security.AccessControl.AccessControlType.Allow.Equals(rule.AccessControlType))
                        {
                            Common.debug("allow");
                            if (contains(FileSystemRights.Read, rule))
                            {
                                Common.debug("read");
                                bAllow = true;
                            }
                        }
                    }
                }
            }
            if (bAllow) 
                return "Permit"; 
            else
                return "Deny";
        }

        private bool contains(System.Security.AccessControl.FileSystemRights right, System.Security.AccessControl.FileSystemAccessRule rule)
        {
            return (((int)right & (int)rule.FileSystemRights) == (int)right);
        }

        #endregion
    }
}
