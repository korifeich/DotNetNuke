<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.SQL.SQL" CodeFile="SQL.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<div class="dnnForm dnnSQLModule dnnClear" id="dnnSQLModule">
    <fieldset>
        <div class="dnnFormItem">
            <dnn:Label ID="plSqlScript" runat="server" ControlName="uplSqlScript" />
            <asp:FileUpload ID="uplSqlScript" runat="server" />
            <asp:LinkButton ID="cmdUpload" resourcekey="cmdUpload" EnableViewState="False" runat="server" CssClass="dnnPrimaryAction" />
        </div>
        <div class="dnnFormItem">
            <dnn:Label ID="plConnection" runat="server" ControlName="cboConnection" />
            <dnn:DnnComboBox ID="cboConnection" runat="server" />
        </div>
        <div class="dnnFormItem">
            <dnn:Label ID="scriptLabel" runat="server" ControlName="txtQuery" />
            <asp:TextBox ID="txtQuery" runat="server" TextMode="MultiLine" Columns="75" Rows="15" EnableViewState="False" />
        </div>
        <div class="dnnFormItem">
            <dnn:Label ID="runasLabel" runat="server" ControlName="chkRunAsScript" />
            <asp:CheckBox ID="chkRunAsScript" runat="server" />
        </div>
    </fieldset>
    <ul class="dnnActions dnnClear">
    	<li><asp:LinkButton id="cmdExecute" runat="server" CssClass="dnnPrimaryAction" resourcekey="cmdExecute" /></li>
    </ul>
    <div class="dnnFormItem dnnResults">
     <%--   <asp:GridView ID="gvResults" runat="server" AutoGenerateColumns="True" CellSpacing="4" 
            EnableViewState="False" CssClass="dnnGrid" gridlines="None">
            <headerstyle CssClass="dnnGridHeader" />
            <rowstyle CssClass="dnnGridItem" />
            <alternatingrowstyle CssClass="dnnGridAltItem" />
            <EmptyDataTemplate>
                <div class="dnnFormItem"><asp:Label ID="Label1" runat="server" resourcekey="NoDataReturned" CssClass="dnnFormMessage dnnFormWarning" /></div>
            </EmptyDataTemplate>
        </asp:GridView>--%>
        
        <dnn:DnnGrid ID="gvResults" runat="server" AutoGenerateColumns="true" EnableViewState="False">
        </dnn:DnnGrid>
  
    </div>
</div>
<script type="text/javascript">
    $(function () {
        $('.dnnResults').jScrollPane();
    })
</script>