using System;
using System.Web.UI;

namespace SAMLServices
{
	/// <summary>
	/// Summary description for IImpersonation.
	/// </summary>
	public interface IImpersonation
	{
		string GetPermission(Page page, String url, String subject);
	}
}
