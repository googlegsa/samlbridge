using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

public partial class CookieRedirect : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Request.Headers["SM_USER"]  == null)
        {
            Response.Redirect("CookieProvider.aspx?return_path=CookieRedirect.aspx");
        }
        //add header Set-Cookie: LLGoogleAuthn=encode(username); Domain=${domain}; Path=/ 
        //redirect to ${returnPath}
    }
}
