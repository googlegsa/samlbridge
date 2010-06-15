/*
 * Copyright (C) 2006-2010 Google Inc.
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
    public partial class Login : System.Web.UI.Page
	{
		private void Page_Load(object sender, System.EventArgs e)
		{

            Common.debug("Login Request is: " + Request.RawUrl);
			Common.debug("before Login::entering pageload");
			// create an IAutn instance
			IAuthn authn = AAFactory.getAuthn(this);

            //Decode request and extract the 
            String AuthNRequestId = ExtractAuthNRequestId();
            if (AuthNRequestId == null || AuthNRequestId.Equals(""))
            {
                Common.error("Couldn't extract AuthN Request Id from SAMLRequest");
                throw new Exception("Failed to extract AuthN Request Id from SAML Request");
            }
            Common.debug("Extracted AuthNRequestId is :" + AuthNRequestId);

            String samlRequest = Request.Params["SAMLRequest"];			
			if (samlRequest == null || "".Equals(samlRequest) ) 
			{
				authn.Diagnose();
				return;
			}
			String subject = authn.GetUserIdentity();
			// Get the user's identity (silently, if properly configured).
			if (subject == null || subject.Equals(""))
			{
				Common.error("Couldn't get user name, check your system setup");
				throw new Exception("Failed to get user name");
			}
			Common.debug("The user is: " + subject);

			// Generate a random string (artifact) that the GSA
			//  will use later to confirm the user's identity
			String artifactId = Common.GenerateRandomString();
			
            // Set an application level name/value pair for storing the user ID 
            // and the AuthN request Id with the artifact string.
			// This is used later when the GSA asks to verify the artifact and obtain the 
			// user ID (in ResolveArt.aspx.cs).
            SamlArtifactCacheEntry samlArtifactCacheEntry = new SamlArtifactCacheEntry(subject, AuthNRequestId);

            Application[Common.ARTIFACT + "_" + artifactId] = samlArtifactCacheEntry;

			// Get the relay state, which is the search URL to which the user
			//  is redirected following authentication and verification
			String relayState = Request.Params["RelayState"];

			// Look up the GSA host name (stored in Web.config)
			String gsa;

			// Encode the relay state for building the redirection URL (back to the GSA)
			relayState = HttpUtility.UrlEncode(relayState);
			gsa = Common.GSAArtifactConsumer + "?SAMLart=" + artifactId  + "&RelayState=" + relayState;
            if (!gsa.StartsWith("http"))
            {
                gsa = "http://" + Request.Headers["Host"] + gsa;
            }

			Common.debug("before Login::redirect");
			Common.debug(" to: " + gsa);
			// Redirect back to the GSA, which will theb contact the Artifact verifier service
			//  with the artifact, to ensure its validity and obtain the user's ID
			Response.Redirect(gsa);

		}


		#region ExtractID requires .NET Framework v 2.0

		/// <summary>
		/// Extracts the Authn request ID from the SAML Request parameter
		/// </summary>
		/// <returns></returns>
        String ExtractAuthNRequestId()
		{
			// Put user code to initialize the page here
			String samlRequest = Request.Params["SAMLRequest"];
			//samlRequest = Server.UrlDecode(samlRequest);
			Common.debug("samlRequest = " + samlRequest);
            samlRequest = Common.Decompress(Server, samlRequest);
            Common.debug("samlRequest decoded = " + samlRequest);
			if (samlRequest == null)
			{
				Common.debug("Decompress failed");
				return null;
			}
			XmlDocument doc = new XmlDocument();
			doc.InnerXml = samlRequest;
			XmlElement root = doc.DocumentElement; 
			return root.Attributes["ID"].Value;
		}

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
