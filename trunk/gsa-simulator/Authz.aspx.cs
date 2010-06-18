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
using System.Net;
using System.IO;
using System.Xml;

	/// <summary>
	/// Summary description for Authz.
	/// </summary>
public partial class Authz : System.Web.UI.Page
	{
		private void Page_Load(object sender, System.EventArgs e)
		{
			// Put user code to initialize the page here

			if (Request.Form["file"] == null)
			{
				return;
			}

			String file = Request.Form["file"];
			String template = Server.MapPath(file);
			Stream fs = new FileStream(template, FileMode.Open,FileAccess.Read);
			StreamReader reader = new StreamReader(fs);
			String req = reader.ReadToEnd();
			fs.Close();

			try
			{
				String res = Common.PostToURL(Common.GetAuthorizer(Request), req);
				this.txtResponse.Text = res;
			}catch(Exception err)
			{
				Common.log(err.Message);
			}

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
