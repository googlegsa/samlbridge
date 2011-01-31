<%@ page language="c#" autoeventwireup="false" inherits="SAMLServices.Dump, App_Web_4xkttqzw" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" > 

<html>
  <head>
    <title>Dump</title>
    <meta name="GENERATOR" Content="Microsoft Visual Studio .NET 7.1">
    <meta name="CODE_LANGUAGE" Content="C#">
    <meta name=vs_defaultClientScript content="JavaScript">
    <meta name=vs_targetSchema content="http://schemas.microsoft.com/intellisense/ie5">
    <style type="text/css">
        .style1
        {
            width: 45%;
        }
        .style2
        {
            width: 312px;
        }
    </style>
    <script>
        function storeSecret()
        {
            var sURL = unescape(window.location.pathname);
            var vForm = document.forms["Form1"];
            window.location.href = sURL + "?secret=" + vForm.secret.value;
        }
        function retrieveSecret()
        {
            var sURL = unescape(window.location.pathname);
            window.location.href = sURL + "?retrieve=secret";
        }        
    </script>
  </head>
  <body onload="init();">

    <%
        if (SAMLServices.Common.AppDataStoreInMemory())
        {
            String secret = Request.Params["secret"];
            if (secret != null && !"".Equals(secret))
            {
                SAMLServices.SamlArtifactCacheEntry samlArtifactCacheEntry = new SAMLServices.SamlArtifactCacheEntry(secret, "efg");
                SAMLServices.Common.storeArtifact("secret", Application, samlArtifactCacheEntry);
            }
    %>	
    <form id="Form1" method="post" runat="server">
        
     <table class="style1">
        <tr><th style="text-align: left">Farm deployment verification, machine name: <%=Server.MachineName %></th><th>&nbsp;</th></tr>
         <tr>
             <td class="style2">
                 <a href="" onclick="storeSecret();return false;">Store screte</a></td>
             <td>
                 <input type="text" id="secret" name="secret" /></td>
         </tr>
         <tr>
             <td class="style2">
                 <a href="" onclick="retrieveSecret();return false;">Retrieve secret</a></td>
             <td>
                 <label id="secret_back">
                 <%
                String retrieve = Request.Params["retrieve"];
                if (retrieve != null && !"".Equals(retrieve))
                {
                    SAMLServices.SamlArtifactCacheEntry entry = SAMLServices.Common.returnArtifact(Application, SAMLServices.Common.ARTIFACT + "_secret");
                    
                    Response.Write("The secret is " + entry.Subject);
                }
                %>
                </label></td>
         </tr>
     </table>
     </form>
     <%} //end if %>
  </body>
</html>
