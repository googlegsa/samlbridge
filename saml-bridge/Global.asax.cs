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
using System.Web;
using System.Web.SessionState;
using System.IO;
using System.Configuration;

namespace SAMLServices 
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
			String template = Server.MapPath("AuthnResponse.xml");
			Stream fs = new FileStream(template, FileMode.Open,FileAccess.Read);
			StreamReader reader = new StreamReader(fs);
			Common.AuthNResponseTemplate = reader.ReadToEnd();
			fs.Close();
			template = Server.MapPath("AuthzResponse.xml");
			fs = new FileStream(template, FileMode.Open,FileAccess.Read);
			reader = new StreamReader(fs);
			Common.AuthZResponseTemplate = reader.ReadToEnd();
			fs.Close();
			String level = ConfigurationSettings.AppSettings["log_level"];
			Common.GSAArtifactConsumer = ConfigurationSettings.AppSettings["artifact_consumer"];
			if (level!= null )
			{
				if (level.ToLower().Equals("debug"))
					Common.LOG_LEVEL = Common.DEBUG;
				else if (level.ToLower().Equals("info"))
					Common.LOG_LEVEL = Common.INFO;
				else if (level.ToLower().Equals("error"))
					Common.LOG_LEVEL = Common.ERROR;
			}
			String sProvider = ConfigurationSettings.AppSettings["provider"];
			if (sProvider != null && !sProvider.Equals(""))
				Common.provider = Type.GetType(sProvider);
			//log 
			Common.LogFile = Server.MapPath("ac.log");
			
			String alias = ConfigurationSettings.AppSettings["host_alias"];
			if (alias != null) 
			{
				String[] hosts = alias.Split(new char[]{';'});
				for (int i =0; i < hosts.Length; ++i)
				{
					if (hosts[i] == null || hosts[i].Equals(""))
						continue;
					String [] host = hosts[i].Split(new char[]{'='});
					if (host != null && host.Length == 2)
						Common.alias.Add(host[0], host[1]);
				}
			}
			
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

