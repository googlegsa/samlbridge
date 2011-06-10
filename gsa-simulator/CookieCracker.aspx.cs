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


public partial class CookieCracker : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        HttpCookie smcookie = Request.Cookies["SM_USER"];
        HttpCookieCollection cookies = Request.Cookies;
        for (int i = 0; i < cookies.Count; i++)
        {
            Response.Write("cookie: " + cookies[i].Name + "=" + cookies[i].Value);
            Common.log("cookie: " + cookies[i].Name + "=" + cookies[i].Value);
        }
        

        if (smcookie == null)
        {
            Response.Status = "401 Unauthorized";
            Response.Write("not authorized");
            Common.log("not authorized");
            return;
        }
        String uid = Request.Cookies["SM_USER"].Value;
        if (uid != null) 
        {
            Common.log("found SM_USER " + smcookie.Value);
            Response.AddHeader("X-Username", smcookie.Value);
            Response.Write("found SM_USER " + smcookie.Value);
            Common.log("found SM_USER");
        }
    }
}
