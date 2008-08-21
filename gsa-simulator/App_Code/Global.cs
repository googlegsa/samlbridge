using System;
using System.Collections;
using System.ComponentModel;
using System.Web;
using System.Web.SessionState;
using System.IO;

namespace gsa 
{
	/// <summary>
	/// Summary description for Global.
	/// </summary>
	public class Global : System.Web.HttpApplication
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		public Global()
		{
			InitializeComponent();
		}	
		
		protected void Application_Start(Object sender, EventArgs e)
		{
			//read template only once
			String template = Server.MapPath("SamlRequest.xml");
			Stream fs = new FileStream(template, FileMode.Open,FileAccess.Read);
			StreamReader reader = new StreamReader(fs);
			Common.SamlRequestTemplate = reader.ReadToEnd();
			fs.Close();
			template = Server.MapPath("SamlResolve.xml");
			fs = new FileStream(template, FileMode.Open,FileAccess.Read);
			reader = new StreamReader(fs);
			Common.SamlResolveTemplate = reader.ReadToEnd();
			fs.Close();
			template = Server.MapPath("Authz.xml");
			fs = new FileStream(template, FileMode.Open,FileAccess.Read);
			reader = new StreamReader(fs);
			Common.AuthzTemplate = reader.ReadToEnd();
			fs.Close();
			Common.LogFile = Server.MapPath("gsa.log");
		}
 
		protected void Session_Start(Object sender, EventArgs e)
		{

		}

		protected void Application_BeginRequest(Object sender, EventArgs e)
		{

		}

		protected void Application_EndRequest(Object sender, EventArgs e)
		{

		}

		protected void Application_AuthenticateRequest(Object sender, EventArgs e)
		{

		}

		protected void Application_Error(Object sender, EventArgs e)
		{

		}

		protected void Session_End(Object sender, EventArgs e)
		{

		}

		protected void Application_End(Object sender, EventArgs e)
		{

		}
			
		#region Web Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
			this.components = new System.ComponentModel.Container();
		}
		#endregion
	}
}

