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
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Security.Principal ;
using System.Xml;

namespace SAMLServices
{
	/// <summary>
	/// This is the page that the user is redirected to (from the GSA) for login.
	/// For silent authentication, this page must not have Anonymous Access enabled,
	///  and should only have Integrated Windows Authentication enabled.
	/// </summary>
	public class Login : System.Web.UI.Page
	{
		private void Page_Load(object sender, System.EventArgs e)
		{
			Common.debug("before Login::entering pageload");
			// create an IAutn instance
			IAuthn authn = AAFactory.getAuthn(this);
			String samlRequest = Request.Params["SAMLRequest"];			
			String subject  = Request.Params["subject"];
			if ((samlRequest == null || "".Equals(samlRequest) ) && (subject == null || "".Equals(subject) ))
			{
				// Put user code to initialize the page here
				// Put user code to initialize the page here
				Response.Write("Google SAML Bridge for Windows<br/>");
				Response.Write("<br>");
				Response.Write("Application Pool Identity  = "  + WindowsIdentity.GetCurrent().Name);
				Response.Write("<br>");
				Response.Write("User's Windows Identity = " + Page.User.Identity.Name);
				Response.Write("<br>");
				Response.Write("Use Login.aspx?subject=user@domain to test impersonation.");
				return;
			}

			if (subject != null)
			{
				TestImpersonation();
				return;
			}
			// Decode the request from the GSA.
			//  This isn't used but shows how it could be.
			// Since this requires version 2.0 of the .NET Framework, it's commented out for now
			// DecodeRequest();

			// Generate a random string (artifact) that the GSA
			//  will use later to confirm the user's identity
			String id = Common.GenerateRandomString();

            // Get the user's identity (silently, if properly configured).
			subject = authn.GetUserIdentity();
			if (subject == null || subject.Equals(""))
			{
				Common.error("Couldn't get user name, check your system setup");
				throw new Exception("Failed to get user name");
			}
			Common.debug("The user is: " + subject);

			// Set an application level name/value pair for storing the user ID with the artifact string.
			// This is used later when the GSA asks to verify the artifact and obtain the 
			//  user ID (in ResolveArt.aspx.cs).
			Application[Common.ARTIFACT + "_" + id] = subject;

			// Get the relay state, which is the search URL to which the user
			//  is redirected following authentication and verification
			String relayState = Request.Params["RelayState"];

			// Look up the GSA host name (stored in Web.config)
			String gsa = Common.GSA;

			// Encode the relay state for building the redirection URL (back to the GSA)
			relayState = HttpUtility.UrlEncode(relayState);
			gsa = Common.GSAArtifactConsumer + "?SAMLart=" + id  + "&RelayState=" + relayState;

			Common.debug("before Login::redirect");
			Common.debug(" to: " + gsa);
			// Redirect back to the GSA, which will theb contact the Artifact verifier service
			//  with the artifact, to ensure its validity and obtain the user's ID
			Response.Redirect(gsa);

		}

		/// <summary>
		/// Test impersonation directly without GSA involved.
		/// Expect the request having a parameter "subject"
		/// </summary>
		private void TestImpersonation()
		{
			String subject  = Request.Params["subject"];
			if (subject == null)
				return;
			Response.Write("Google SAML Bridge for Windows<br/>");
			Response.Write("<br>Impersonate for user " + subject);
			WindowsIdentity wi = new WindowsIdentity(subject);
			if (wi != null)
				Response.Write("<br>Obtained Windows identity");
			WindowsImpersonationContext wic = null;
			wic = wi.Impersonate();
			Response.Write("<br>Impersonation successful!");
			if( wic != null)
				wic.Undo();
		}

		#region DecodeRequest requires .NET Framework v 2.0

		/// <summary>
		/// The ID from the first handshake is not really used
		/// Plus this requires System.IO.Compress references which is only available
		/// in .NET Framework 2.0
		/// </summary>
		/// <returns></returns>
/*
		String DecodeRequest()
		{
			// Put user code to initialize the page here
			String samlRequest = Request.Params["SAMLRequest"];
			//samlRequest = Server.UrlDecode(samlRequest);
			Common.log("samlRequest = " + samlRequest);
			samlRequest = Common.Decompress(samlRequest);
			if (samlRequest == null)
			{
				Common.log("Decompress failed");
				return null;
			}
			XmlDocument doc = new XmlDocument();
			doc.InnerXml = samlRequest;
			XmlElement root = doc.DocumentElement; 
			return root.Attributes["ID"].Value;
		}
*/
		#endregion
		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion
	}
}
