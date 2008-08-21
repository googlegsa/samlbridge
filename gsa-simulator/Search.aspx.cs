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
using System.Security;

	/// <summary>
	/// Summary description for Search.
	/// </summary>
public partial class Search : System.Web.UI.Page
	{
	
		protected void Page_Load(object sender, System.EventArgs e)
		{
			Common.log("enter page_load");
			// Put user code to initialize the page here
			if (Request.Params["q"] == null || "".Equals(Request.Params["q"]))
			{
				return;
			}
			String query = "SamlRequest=" + BuildSamlRequest();
			query += "&RelayState=" + HttpUtility.UrlEncode(Request.Url.AbsoluteUri);
			Common.log(query);
			Session.Add("URL", Request.Params["Resource"]);
			Common.log("URL to test: " + Request.Params["Resource"]);
			//Response.Write(query);
			Response.Redirect(Common.getAC(Request) + "Login.aspx?" + query);
		}

		String BuildSamlRequest()
		{
			Common.log("inside BuildSamlRequest");
			// Put user code to initialize the page here
			String req = Common.SamlRequestTemplate;
			req = req.Replace("%ID", Common.GenerateRandomString());
			req = req.Replace("%INSTANT", Common.FormatNow());
			Common.log("request before encoding=" + req);
			byte[] decData = new System.Text.UTF8Encoding().GetBytes(req);
			Common.log("before deflate length=" + decData.Length);
			//for simulation, we don't compress because in AC, the data is not used anyway.
            /*byte[] compressData = Common.Compress(decData);
			//encode
			String encoded = Convert.ToBase64String(compressData);
			*/
			String encoded = Convert.ToBase64String(decData);
			Common.log("base64 encoded string: " + encoded);
            return HttpUtility.UrlEncode(encoded);
		}


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