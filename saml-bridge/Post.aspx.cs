/*
 Locate the certificate:
Start menu -> Run -> mmc
File->Add/Remove Snap-in->Add->Certificates->Add->Computer Account->Next->Local Computer->Finish, its usually under "Personal".
Double click on the certificate->Details, find "Subject", identify a keyword 

Show the access privileges using winhttpcertcfg tool:

C:\Program Files\Windows Resource Kits\Tools>winhttpcertcfg -l -c LOCAL_MACHINE\My -s keyword_identified_above

Grant to account "Network Service"

C:\Program Files\Windows Resource Kits\Tools>winhttpcertcfg -g -c LOCAL_MACHINE\My -s saml -a "Network Service"

 */

using System;
using System.Security.Cryptography.X509Certificates;
using System.Web.UI;
using System.Xml;
using SAMLServices;
using System.Security;
using System.Security.Principal;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;


public partial class _Default : AuthenticationPage
{
    public X509Certificate2Collection Certificates;
    protected void Page_Load(object sender, EventArgs e)
    {
        Common.debug("Login Request is: " + Request.RawUrl);
        Common.debug("before Login::entering pageload");
        // create an IAutn instance
        IAuthn authn = AAFactory.getAuthn(this);

        String samlRequest = Request.Params["SAMLRequest"];
        if (samlRequest == null || "".Equals(samlRequest))
        {
            Diagnose();
            return;
        }

        //Decode request and extract the AuthNRequestId
        AuthNRequest authNRequest = ExtractAuthNRequest(samlRequest);
        if (authNRequest.Id == null || authNRequest.Equals(""))
        {
            Common.error("Couldn't extract AuthN Request Id from SAMLRequest");
            throw new Exception("Failed to extract AuthN Request Id from SAML Request");
        }
        Common.debug("Extracted AuthNRequestId is :" + authNRequest.Id);


        String subject = authn.GetUserIdentity();
        // Get the user's identity (silently, if properly configured).
        if (subject == null || subject.Equals(""))
        {
            Common.error("Couldn't get user name, check your system setup");
            throw new Exception("Failed to get user name");
        }
        Common.debug("The user is: " + subject);
        String SamlAssession = BuildAssertion(subject, authNRequest);
        Response.Write(GenerateResponse(SamlAssession));
    }

    String GenerateResponse(String SamlAssession)
    {
        String postForm = String.Copy(Common.postForm);
        //Base64 encoding
        String encoded = Common.EncodeTo64(SamlAssession);
        //return the html
        postForm = postForm.Replace("%ASSERTION_CONSUMER", Common.assertionConsumer);
        postForm = postForm.Replace("%SAML_RESPONSE", encoded);
        Common.debug(postForm);
        return postForm;
    }

    String BuildAssertion(String subject, AuthNRequest authNRequest)
    {
        Common.debug("inside BuildAssertion");
        String recipientGsa = Common.GSAAssertionConsumer;
        XmlDocument respDoc = (XmlDocument)Common.postResponse.CloneNode(true);
        Common.debug("before replacement: " + respDoc.InnerXml);
        if (!recipientGsa.StartsWith("http"))
        {
            recipientGsa = "http://" + Request.Headers["Host"] + recipientGsa;
        }

        String req = respDoc.InnerXml;
        req = req.Replace("%REQID", authNRequest.Id);
        DateTime currentTimeStamp = DateTime.Now;
        req = req.Replace("%INSTANT", Common.FormatInvariantTime(currentTimeStamp.AddMinutes(-1)));
        req = req.Replace("%NOT_ON_OR_AFTER", Common.FormatInvariantTime(currentTimeStamp.AddSeconds(Common.iTrustDuration)));
        String idpEntityId;
        if (Common.IDPEntityId == null || "".Equals(Common.IDPEntityId))
        {
            throw new Exception("IDP Entity ID is not set in config. Using machine name as default");
        }
        req = req.Replace("%ISSUER", Common.IDPEntityId);
        String MessageId = Common.GenerateRandomString();
        req = req.Replace("%MESSAGE_ID", MessageId);
        req = req.Replace("%RESPONSE_ID", Common.GenerateRandomString());
        req = req.Replace("%ASSERTION_ID", Common.GenerateRandomString());
        req = req.Replace("%SUBJECT", SecurityElement.Escape(subject));
        req = req.Replace("%RECIPIENT", recipientGsa);
        req = req.Replace("%AUTHN_REQUEST_ID", SecurityElement.Escape(authNRequest.Id));
        req = req.Replace("%AUDIENCE", authNRequest.Issuer);        

        respDoc.InnerXml = req;
        // Sign the XML document. 
        SignXml(respDoc, MessageId);
        Common.debug("exit BuildAssession");
        return respDoc.InnerXml;
    }

    // Sign an XML file. 
    // This document cannot be verified unless the verifying 
    public class TestCert
    {
        public static string HasPublicKeyAccess(X509Certificate2 cert)
        {
            try
            {
                AsymmetricAlgorithm algorithm = cert.PublicKey.Key;
            }
            catch (Exception ex)
            {
                return "No";
            }
            return "Yes";
        }
        public static string HasPrivateKeyAccess(X509Certificate2 cert)
        {
            try
            {
                string algorithm = cert.PrivateKey.KeyExchangeAlgorithm;
            }
            catch (Exception ex)
            {
                return "No";
            }
            return "Yes";
        }
    }
    // code has the key with which it was signed.
    public void SignXml(XmlDocument xmlDoc, String MessageId)
    {
        RSACryptoServiceProvider Key = Common.certificate.PrivateKey as RSACryptoServiceProvider;
        // Check arguments.
        if (xmlDoc == null)
            throw new ArgumentException("xmlDoc is null");
        if (Key == null)
            throw new ArgumentException("Key");

        // Create a SignedXml object.
        SignedXml signedXml = new SignedXml(xmlDoc);

        // Add the key to the SignedXml document.
        signedXml.SigningKey = Key;

        // Create a reference to be signed.
        Reference reference = new Reference();
        reference.Uri = "#" + MessageId;

        // Add an enveloped transformation to the reference.
        Transform env = new XmlDsigEnvelopedSignatureTransform();
        //env.PropagatedNamespaces.Add("ds", "http://www.w3.org/2000/09/xmldsig#");
        reference.AddTransform(env);


        // Add the reference to the SignedXml object.
        signedXml.AddReference(reference);


        // Compute the signature.
        signedXml.ComputeSignature();

        // Get the XML representation of the signature and save
        // it to an XmlElement object.
        XmlElement xmlDigitalSignature = signedXml.GetXml();

        // Append the element to the XML document.
        xmlDoc.DocumentElement.AppendChild(xmlDoc.ImportNode(xmlDigitalSignature, true));
        Common.debug(xmlDoc.InnerXml);
    }

}

