using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml;
using SAMLServices;
using System.IO.Compression;
using System.IO;
using System.Net;
using System.Collections.Specialized;
using System.Security.AccessControl;
using System.Security;
using System.Security.Principal;


public class AuthNRequest
{
    String _id, _issuer;
    public String Id
    {
        get
        {
            return _id;
        }
        set
        {
            _id = value;
        }
    }

    public String Issuer
    {
        get
        {
            return _issuer;
        }
        set
        {
            _issuer = value;
        }
    }
}

/// <summary>
/// Summary description for AuthenticationPage
/// </summary>
public class AuthenticationPage : System.Web.UI.Page
{

    /// <summary>
    /// Method to extract the core information from the SAML message,
    /// specifically the artifact and the SAML ID of the request.
    /// </summary>
    /// <returns>Two element String array, the first is the artifact, the second is the request ID</returns>
    public String[] ExtractInfo()
    {
        String[] result = new String[3];
        Common.debug("inside ResolveArt::ExtractInfo");
        // Get the SAML message from the request, in the form of a string
        String req = ReadRequest();
        Common.debug("request from GSA=" + req);

        // Find the artifact node and obtain the InnerText
        XmlNode node = Common.FindOnly(req, Common.ARTIFACT);
        result[0] = node.InnerText;
        Common.debug("Artifiact Id = " + result[0]);

        // Find the SAML request ID in an XML attribute.
        //  This is needed for the response for the GSA
        //  to understand how to match it up with its request
        node = Common.FindOnly(req, Common.ArtifactResolve);
        result[1] = node.Attributes[Common.ID].Value;
        Common.debug("SAML Resolve Request Id = " + result[1]);

        //Find the Issuer details. 
        //These need to be passed back as AudienceRestriction in the Assertion Conditions
        node = Common.FindOnly(req, Common.ISSUER);
        result[2] = node.InnerText;
        Common.debug("Resolve Requester = " + result[2]);

        Common.debug("exit ResolveArt::ExtractInfo");
        return result;
    }

    /// <summary>
    /// Extracts the Authn request ID from the SAML Request parameter
    /// </summary>
    /// <returns></returns>
    public AuthNRequest ExtractAuthNRequest(String samlRequest)
    {
        Common.debug("samlRequest = " + samlRequest);
        samlRequest = Decompress(samlRequest);
        Common.debug("samlRequest decoded = " + samlRequest);
        if (samlRequest == null)
        {
            Common.debug("Decompress failed");
            return null;
        }
        XmlDocument doc = new XmlDocument();
        doc.InnerXml = samlRequest;
        XmlElement root = doc.DocumentElement;
        AuthNRequest req = new AuthNRequest();
        req.Id = root.Attributes["ID"].Value;
        req.Issuer = Common.FindOnly(samlRequest, "Issuer").InnerText;
        return req;
    }

    #region Decompression requires .NET Framework v 2.0

    public static String Decompress(String samlRequest)
        {
            byte[] b = Convert.FromBase64String(samlRequest);

            using (MemoryStream inputStream = new MemoryStream(b))
            {
                using (DeflateStream gzip =
                  new DeflateStream(inputStream, CompressionMode.Decompress))
                {
                    using (StreamReader reader =
                      new StreamReader(gzip, System.Text.Encoding.UTF8))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }
    #endregion

    /// <summary>
    ///  Method to read an HTTP request and convert it into a string.
    /// </summary>
    /// <param name="Request"></param>
    /// <returns></returns>
    public String ReadRequest()
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


    public void printHeader()
    {
        Response.Write("<style><!--					body,td,div,.p,a,.d,.s{font-family:arial,sans-serif}--></style>");
        Response.Write("<html><body><table><tr><td><img src='google.gif'/></td><td><font size=6>Google SAML Bridge for Windows</font></td></tr></table>");
        Response.Write("<br>");
    }
    public void printFooter()
    {
        Response.Write("</body></html>");
    }

    public String handleDeny(HttpWebResponse response)
    {
        //now handle deny 
        String denyAction = Common.DenyAction;
        Common.debug("action is " + denyAction);
        if (Common.DENY_REDIRECT.Equals(denyAction))
        {

            String[] locations = response.Headers.GetValues("Location");
            if (locations != null)
            {
                String location = locations[0].ToLower();
                Common.debug("The new location of the requested resource is at " + location);
                foreach (String key in Common.denyUrls.Keys)
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
            foreach (String key in Common.denyCodes.Keys)
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
        for (int i = 0; i < headers.Count; ++i)
        {
            Common.debug("<td>" + headers.Keys[i] + "</td><td>");
            foreach (String val in headers.GetValues(i))
            {
                Common.debug(val + ",  ");
            }
        }
    }

    public  void dumpResponse()
    {
        Common.debug("response: ");
        Stream responseStream = Response.OutputStream;
        StreamReader reader = new StreamReader(responseStream);
        String res = reader.ReadToEnd();
        responseStream.Close();
        Common.debug(res);
    }

    /// <summary>
    /// Test impersonation directly without GSA involved.
    /// Expect the request having a parameter "subject"
    /// </summary>
    public void Diagnose()
    {
        String samlRequest = Request.Params["SAMLRequest"];
        // Put user code to initialize the page here
        printHeader();
        String subject = Request.Params["subject"];
        if (subject == null)
        {
            Response.Write("Application Pool Identity  = " + WindowsIdentity.GetCurrent().Name);
            Response.Write("<br>");
            Response.Write("Your Windows account  = " + Page.User.Identity.Name);
            Response.Write("<p>");
            Response.Write("<b>Use Login.aspx?subject=user@domain to test impersonation.</b>");
        }
        else
        //Test Impersonation
        {
            WindowsIdentity wi = new WindowsIdentity(subject);
            if (wi != null)
                Response.Write("<br>Obtained Windows identity for user " + subject);
            WindowsImpersonationContext wic = null;
            wic = wi.Impersonate();
            Response.Write("<br>This message was written using the identity of " + subject);
            Response.Write("<br>Impersonation successful!");
            if (wic != null)
                wic.Undo();
            Response.Write("<p><b>Now you can test content authorization using <a href=\"/gsa-simulator/Default.aspx\">GSA Simulator!</a></b>");
        }
        printFooter();
    }

}
