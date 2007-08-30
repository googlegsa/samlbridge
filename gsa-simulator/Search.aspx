<%@ Page language="c#" Inherits="gsa.Search" CodeBehind="Search.aspx.cs" AutoEventWireup="false" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<HTML>
	<HEAD>
		<title>Search</title>
		<meta content="Microsoft Visual Studio .NET 7.1" name="GENERATOR">
		<meta content="C#" name="CODE_LANGUAGE">
		<meta content="JavaScript" name="vs_defaultClientScript">
		<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema">
	</HEAD>
	<body>
		<FONT face="ËÎÌו">
			<P><FONT face="arial" size="5">
					<asp:Image id="Image1" runat="server" ImageUrl="google.gif"></asp:Image>
					<asp:Label id="Label2" runat="server" Width="408px" Height="80px" Font-Names="Arial" Font-Size="Large">Google Search Appliance  Security SPI Simulator</asp:Label></FONT></P>
		</FONT>
		<form id="Form1" action="Search.aspx" method="get" runat="server">
			<P>
				<asp:label id="Label1" runat="server" Width="224px" Height="32px" Font-Names="Arial" Font-Size="Larger">Test Authorization</asp:label></P>
			<P>
				<asp:label id="Label3" runat="server" Width="88px" Height="32px">URL:&nbsp;</asp:label>
				<input type=hidden name="q" value="v"/>
				<asp:textbox id="Resource" Width="472px" Height="26px" size="100" Runat="server"></asp:textbox></P>
			<P>
				<asp:button id="Button1" runat="server" Width="88px" Height="24px" Text="Submit"></asp:button></P>
			<P><FONT face="ËÎÌו"></FONT>&nbsp;</P>
			<P><FONT face=""></FONT>&nbsp;</P>
		</form>
		<P>&nbsp;</P>
		<P><FONT face="ËÎÌו"></FONT>&nbsp;</P>
	</body>
</HTML>
