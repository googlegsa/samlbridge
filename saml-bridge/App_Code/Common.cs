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
using System.Collections.Specialized;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Security.Principal;
using System.Configuration;
using System.Collections;

//using System.IO.Compression;

namespace SAMLServices
{
	/// <summary>
	/// Summary description for Common.
	/// </summary>
	public class Common
	{
		// String to hold the SAML namespace declaration
		public static String SAML_NAMESPACE = "urn:oasis:names:tc:SAML:2.0:protocol";
		// String used as prefix for artifacts
		public static String ARTIFACT = "Artifact";
		// SAML element name when resolving artifacts
		public static String ArtifactResolve = "ArtifactResolve";
		public static String ID="ID";
		public static String AuthNResponseTemplate = null;
		public static String AuthZResponseTemplate = null;
        public static String AuthZResponseSopaEnvelopeStart = null;
        public static String AuthZResponseSopaEnvelopeEnd = null;
		public static String LogFile = null;
		public static bool bDebug = false;
		public static String artifactConsumer = null;
		public static Type provider = typeof (SAMLServices.Wia.AuthImpl);
		public static int DEBUG=0, INFO=1, ERROR=2;
		public static int LOG_LEVEL  = INFO;
		public static String SsoSubjectVar= null;
		public static Hashtable denyUrls = new Hashtable();
		public static Hashtable denyCodes = new Hashtable();
		public static String DenyAction = null;
		public static String DENY_REDIRECT = "redirect";
		public static String DENY_RETURN_CODE = "return_code";

		/// <summary>
		/// When log level lower or equal to debug, show debug(most detailed msg)
		/// </summary>
		/// <param name="msg"></param>
	 
		public static void debug(String msg)
		{
			if (LOG_LEVEL <= DEBUG)
				log(msg);
		}

		/// <summary>
		/// If log level is lower than error, then show all error msgs
		/// </summary>
		/// <param name="msg"></param>
		public static void error(String msg)
		{
			if (LOG_LEVEL <= ERROR)
				log(msg);
		}


		/// <summary>
		/// Method for logging debug/trace information to a file.
		///NOTE:  Read/Write privileges must be given to the web service
		/// process owner and domain users if impersonation is performed
		/// </summary>
		/// <param name="msg"></param>
		private static void log(String msg)
		{
			lock(LogFile)
			{
			
				StreamWriter logger = File.AppendText(LogFile);
				System.Diagnostics.StackFrame frame = new System.Diagnostics.StackTrace().GetFrame(2);
				logger.WriteLine(System.DateTime.Now + ", " + frame.GetMethod().Name + ": " +  msg );
				logger.Close();
			}
		}

		 
		/// <summary>
		///Method to determine the URL to which the user is redirected
		/// after login.
		/// Based on whether this is a simulation or not.
		///The simulator is a test utility that simulates
		///the SAML requests that come from a GSA.
		/// 
		/// </summary>
		public static String GSAArtifactConsumer
		{
			get
			{
				return artifactConsumer;
			}
			set
			{
				artifactConsumer = value;
			}
		}

		/// <summary>
		///  Method to obtain the current time, converted
		/// tto a specific format for insertion into responses
		/// </summary>
		/// <param name="time"></param>
		/// <returns>Universal time format</returns>
		public static String FormatInvariantTime(DateTime time)
		{
			return time.ToString("s", DateTimeFormatInfo.CurrentInfo) + "Z";
		}

		/// <summary>
		///  Method that creates a random string that's used
		///as the artifact after login.  The GSA uses the artifact
		/// to then obtain the user ID. The random strings that used by all the different "ID" must start with alphabet, or an underscore. 
		/// That's why I've prefixed "a" here.
		/// </summary>
		/// <returns>Random string</returns>

		public static String GenerateRandomString()
		{
			String id = "a" + System.Guid.NewGuid().ToString("N");
			id = System.Web.HttpUtility.UrlEncode(id);
			return id;
		}

		/// <summary>
		/// Method to obtain an XML element within an XML string,
		///  given the element name.
		/// </summary>
		/// <param name="xml">The xml to be searched</param>
		/// <param name="name">element name</param>
		/// <returns></returns>
		public static XmlNode FindOnly(String xml, String name)
		{
			XmlDocument doc = new XmlDocument();
			doc.InnerXml = xml;
			return FindOnly(doc, name);
		}

		
		/// <summary>
		/// Method to obtain an XML element within an XMLDocument,
		/// given the element name.
		/// </summary>
		/// <param name="doc">Xml Document</param>
		/// <param name="name">element name to be found</param>
		/// <returns></returns>
		public static XmlNode FindOnly(XmlDocument doc, String name)
		{
			XmlNodeList list = FindAllElements(doc, name);
			return list.Item(0);
		}

        /// <summary>
        /// Method to obtain a List of XML elements within an XMLDocument,
        /// given the element name.
        /// </summary>
        /// <param name="doc">Xml Document</param>
        /// <param name="name">element name to be found</param>
        /// <returns>List of XML Elements with matcing name</returns>
        public static XmlNodeList FindAllElements(XmlDocument doc, String name)
        {
            XmlNodeList list = doc.GetElementsByTagName(name, Common.SAML_NAMESPACE);
            return list;
        }
		
		/// <summary>
		/// Method to add an XML attribute to an element
		/// </summary>
		/// <param name="node"></param>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public static void AddAttribute(XmlNode node, String name, String value)
		{
			XmlAttribute attr = node.OwnerDocument.CreateAttribute(name);
			attr.Value = value;
			node.Attributes.Append(attr);
		}

		/// <summary>
		///  Method to read an HTTP request and convert it into a string.
		/// </summary>
		/// <param name="Request"></param>
		/// <returns></returns>
		public static String ReadRequest(HttpRequest Request)
		{
			int bufSize = 1024;
			Stream stream = Request.InputStream;
			byte[] input = new byte[bufSize];
			int size = stream.Read(input, 0, bufSize); 
			String req = "";
			while (size > 0)
			{
				req += System.Text.Encoding.UTF8.GetString(input, 0, size);
				size = stream.Read(input, 0, bufSize); 
			}
			stream.Close();
			return req;
		}


		public static void printHeader(HttpResponse Response)
		{
			Response.Write("<style><!--					body,td,div,.p,a,.d,.s{font-family:arial,sans-serif}--></style>");
			Response.Write("<html><body><table><tr><td><img src='google.gif'/></td><td><font size=6>Google SAML Bridge for Windows</font></td></tr></table>");
			Response.Write("<br>");
		}
		public static void printFooter(HttpResponse Response)
		{
			Response.Write("</body></html>");
		}

		public static String handleDeny(HttpWebResponse response)
		{
			//now handle deny 
			String denyAction = Common.DenyAction;
			Common.debug ("action is " + denyAction);			
			if (Common.DENY_REDIRECT.Equals(denyAction))
			{
				
				String[] locations =response.Headers.GetValues("Location");
				if (locations != null)
				{
					String location = locations[0].ToLower();
					Common.debug("The new location of the requested resource is at " + location);
					foreach(String key in Common.denyUrls.Keys)
					{
						if (location.StartsWith(key))
						{
							Common.debug("deny url found");
							return "Deny";
						}
					}
				}
			}
			else //error code
			{
				int status = (int)response.StatusCode;
				Common.debug("status code " + status);
				foreach(String key in Common.denyCodes.Keys)
				{
					Common.debug("deny code " + key);
					if (key.Equals(status))
					{
						Common.debug("deny status found");
						return "Deny";
					}
				}
			}
			return "Permit";
		}

		public static void dumpHeaders(NameValueCollection headers)
		{
			for (int i =0; i< headers.Count; ++i)
			{
				Common.debug("<td>" + headers.Keys[i] + "</td><td>" );
				foreach (String val in headers.GetValues(i))
				{
					Common.debug(val + ",  ");
				}				
			}
		}

		public static void dumpResponse(HttpWebResponse response)
		{
			Common.debug ("response: ");
			Stream responseStream = response.GetResponseStream();
			StreamReader reader = new StreamReader (responseStream);
			String res = reader.ReadToEnd ();
			responseStream.Close();
			Common.debug (res);
		}

		#region Decompression requires .NET Framework v 2.0

		/*public static String Decompress(HttpServerUtility server, String samlRequest)
		{
			//base64 decode
			try
			{
                //Common.log("samlRequest before urldecode: " + samlRequest);
                //samlRequest = server.UrlDecode(samlRequest);
				Common.log("before base64 decode string: " + samlRequest);
				byte[] decData = Convert.FromBase64String(samlRequest);
                System.Text.Encoding enc = System.Text.Encoding.ASCII;
                string decoded = enc.GetString(decData);

                Common.log("after base64 decode: " + decoded );
				//compress inflate
				GZipStream zipStream = new GZipStream(new MemoryStream(decData), CompressionMode.Decompress);
				MemoryStream ms = new MemoryStream();
				byte []writeData = new byte[1096];
				int size = -1;
				while (true)
				{
					size = zipStream.Read(writeData, 0, writeData.Length);
					if (size > 0)
						ms.Write(writeData, 0, size) ;
					else
						break;
				}
				ms.Close();
				return System.Text.Encoding.UTF8.GetString(ms.ToArray()); 
			}
			catch(Exception e)
			{
				Common.log(e.Message);
				return null;
			}
		}
    */
		#endregion
	}
}

