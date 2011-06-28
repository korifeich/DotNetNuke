<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.UI.Skins.Controls.ModuleMessage" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<asp:Panel id="dnnSkinMessage" runat="server" CssClass="dnnModuleMessage">
    <asp:label id="lblHeading" runat="server" visible="False" enableviewstate="False" CssClass="dnnModMessageHeading" />
    <asp:label id="lblMessage" runat="server" enableviewstate="False" />
</asp:Panel>
<dnn:DnnScriptBlock ID="scrollScript" runat="server">
	<script type="text/javascript">
		$(document).ready(function () {
			$body = window.opera ? (document.compatMode == "CSS1Compat" ? $('html') : $('body')) : $('html,body');
			$body.animate({ scrollTop: $('#<%=dnnSkinMessage.ClientID %>').offset().top }, 'fast');
		});
	</script>
</dnn:DnnScriptBlock>