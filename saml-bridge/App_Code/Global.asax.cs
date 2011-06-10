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
using System.Xml;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security;


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
            Stream fs = new FileStream(template, FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(fs);
            Common.AuthNResponseTemplate = reader.ReadToEnd();
            fs.Close();
            Common.postResponse = new XmlDocument();
            template = Server.MapPath("PostResponse.xml");
            Common.postResponse.Load(template);
            
            template = Server.MapPath("PostForm.html");
            fs = new FileStream(template, FileMode.Open, FileAccess.Read);
            reader = new StreamReader(fs);
            Common.postForm = reader.ReadToEnd();
            fs.Close();
            
            template = Server.MapPath("AuthzResponseNode.xml");
            fs = new FileStream(template, FileMode.Open, FileAccess.Read);
            reader = new StreamReader(fs);
            Common.AuthZResponseTemplate = reader.ReadToEnd();
            fs.Close();
            template = Server.MapPath("AuthzResponseSoapEnvelopeStart.xml");
            fs = new FileStream(template, FileMode.Open, FileAccess.Read);
            reader = new StreamReader(fs);
            Common.AuthZResponseSopaEnvelopeStart = reader.ReadToEnd();
            fs.Close();
            template = Server.MapPath("AuthzResponseSoapEnvelopeEnd.xml");
            fs = new FileStream(template, FileMode.Open, FileAccess.Read);
            reader = new StreamReader(fs);
            Common.AuthZResponseSopaEnvelopeEnd = reader.ReadToEnd();
            fs.Close();

            String level = ConfigurationSettings.AppSettings["log_level"];
            Common.GSAAssertionConsumer = ConfigurationSettings.AppSettings["assertion_consumer"];
            Common.IDPEntityId = ConfigurationSettings.AppSettings["idp_entity_id"];
            if (level != null)
            {
                if (level.ToLower().Equals("debug"))
                    Common.LOG_LEVEL = Common.DEBUG;
                else if (level.ToLower().Equals("info"))
                    Common.LOG_LEVEL = Common.INFO;
                else if (level.ToLower().Equals("error"))
                    Common.LOG_LEVEL = Common.ERROR;
            }
            Common.iTrustDuration = int.Parse(ConfigurationSettings.AppSettings["trust_duration"]);
            GetCertificate();
            String sProvider = ConfigurationSettings.AppSettings["provider"];
            if (sProvider != null && !sProvider.Equals(""))
                Common.provider = Type.GetType(sProvider);
            //log 
            Common.LogFile = Server.MapPath("ac.log");
            Common.SsoSubjectVar = ConfigurationSettings.AppSettings["sso_user_var"];
            Common.DenyAction = ConfigurationSettings.AppSettings["deny_action"];
            String alias = ConfigurationSettings.AppSettings["deny_urls"];
            if (alias != null)
            {
                String[] hosts = alias.Split(new char[] { ';' });
                for (int i = 0; i < hosts.Length; ++i)
                {
                    if (hosts[i] == null || hosts[i].Equals(""))
                        continue;
                    Common.denyUrls.Add(hosts[i].ToLower().Trim(), "1");
                }
            }
            String codes = ConfigurationSettings.AppSettings["deny_codes"];
            if (codes != null)
            {
                String[] aCodes = codes.Split(new char[] { ';' });
                for (int i = 0; i < aCodes.Length; ++i)
                {
                    if (aCodes[i] == null || aCodes[i].Equals(""))
                        continue;
                    Common.denyCodes.Add(aCodes[i].Trim(), "1");
                }
            }
            ImpHeader.COOKIE_DOMAIN = ConfigurationSettings.AppSettings["imp_cookie_domain"];
        }

        protected void GetCertificate()
        {
            String certName = ConfigurationSettings.AppSettings["certificate_friendly_name"];
            if (null == certName || "".Equals(certName))
                return;

            // Local Computer\Personal
            X509Store store = new X509Store(StoreLocation.LocalMachine);
            // create and open store for read-only access
            store.Open(OpenFlags.ReadOnly);
            if (store.Certificates.Count == 0)
            {
                Common.error("Certificate not found!");
                store.Close();
                return;
            }
            for (int i = 0; i < store.Certificates.Count; ++i)
            {
                if (store.Certificates[i].FriendlyName.Equals(ConfigurationSettings.AppSettings["certificate_friendly_name"]))
                    Common.certificate = store.Certificates[0];
            }
            store.Close();
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

