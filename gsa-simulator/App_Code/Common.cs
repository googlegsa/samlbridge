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
using System.Globalization;
using System.Xml;
using System.Web;
using System.Net;
using System.IO;
using System.Configuration;
using System.IO.Compression;

	/// <summary>
	/// Summary description for Common.
	/// </summary>
	public class Common
	{
		public static String SAML_NAMESPACE = "urn:oasis:names:tc:SAML:2.0:protocol";
		public static String ARTIFACT = "Artifact";
		public static String RelayState = "RelayState";
		public static String SamlRequestTemplate = null;
		public static String AuthzTemplate = null;
		public static String SamlResolveTemplate = null;
		public static String LogFile  = null;
		
		public static String FormatInvariantTime(DateTime time)
		{
			return time.ToUniversalTime().ToString("s", DateTimeFormatInfo.InvariantInfo) + "Z";
		}

		public static String GetSamlResolver(HttpRequest Request)
		{
			return Common.getAC(Request)  + "Resolve.aspx";
		}
		public static String GetAuthorizer(HttpRequest Request)
		{
			return Common.getAC(Request) + "Authz.aspx";
		}

		public static String FormatNow()
		{
			return DateTime.Now.ToUniversalTime().ToString("s", DateTimeFormatInfo.InvariantInfo) + "Z";
		}

		public static String GenerateRandomString()
		{
			String id = System.Guid.NewGuid().ToString("N");
			id = System.Web.HttpUtility.UrlEncode(id);
			return id;
		}

		public static XmlNode FindOnly(String xml, String name)
		{
			XmlDocument doc = new XmlDocument();
			doc.InnerXml = xml;
			XmlNodeList list = doc.GetElementsByTagName(name, Common.SAML_NAMESPACE);
			return list.Item(0);
		}
		public static void AddAttribute(XmlNode node, String name, String value)
		{
			XmlAttribute attr = node.OwnerDocument.CreateAttribute(name);
			attr.Value = value;
			node.Attributes.Append(attr);
		}

		public static String PostToURL(String url, String req)
		{
			Common.log("post to url: " + url);
			Common.log("request = " + req);
			WebRequest web = WebRequest.Create (url);
			//post
			web.Method = "POST";
			//web.Credentials = new NetworkCredential("jeffling", "MSI2800", "google");
			web.ContentType = "text/xml";
			web.Headers.Add("SOAPAction", "http://www.oasis-open.org/committees/security");
			Stream output = web.GetRequestStream();
			byte[] data = new System.Text.UTF8Encoding().GetBytes(req);
			output.Write(data, 0, data.Length);
			output.Close();
			//read
			WebResponse response  = web.GetResponse();
			Stream responseStream = response.GetResponseStream();
			StreamReader reader = new StreamReader (responseStream);
			String res = reader.ReadToEnd ();
			Common.log("response from saml authz is: " + res);
			responseStream.Close();
			return res;
		}

		public static void log(String msg)
		{
			StreamWriter logger = File.AppendText(LogFile);
			logger.WriteLine("gsa: " + msg);
			logger.Close();
		}

		public static String getAC(HttpRequest request)
		{
			String ac = ConfigurationSettings.AppSettings["ac"];
			if (!ac.EndsWith("/"))
				ac = ac + "/";
			if (ac.StartsWith("http"))
                return ac;
            
            ac = "http://" + request.Headers["Host"] + ac;
            Common.log("getAC:: " + ac);
            return ac;
                            
		}
		public static void printHeader(HttpResponse Response)
		{
			Response.Write("<style><!--					body,td,div,.p,a,.d,.s{font-family:arial,sans-serif}--></style>");
			Response.Write("<html><body><table><tr><td><img src='google.gif'/></td><td><font size=6>Google Search Appliance Security SPI Simulator </font></td></tr></table>");
			Response.Write("<br>");
		}
		public static void printFooter(HttpResponse Response)
		{
			Response.Write("</body></html>");
		}

        #region Decompression requires .NET Framework v 2.0

        public static String Compress(String samlRequest)
        {
            byte[] compressedData;
            using (MemoryStream output = new MemoryStream())
            {
                using (DeflateStream gzip = new DeflateStream(output, CompressionMode.Compress))
                {
                    using (StreamWriter writer = new StreamWriter(gzip, System.Text.Encoding.UTF8))
                    {
                        writer.Write(samlRequest);
                    }
                }

                compressedData = output.ToArray();
            }
            return Convert.ToBase64String(compressedData);
        }

        #endregion
	}