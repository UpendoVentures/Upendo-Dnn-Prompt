<%@ Control Language="c#" AutoEventWireup="true" CodeBehind="View.ascx.cs" Inherits="Upendo.Modules.UpendoPrompt.View" %>
<%@ Import Namespace="DotNetNuke.Services.Localization" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/labelcontrol.ascx" %>

<div class="dnnClear">
	<h1>Upendo Prompt</h1>
	<p><%=Localization.GetString("Welcome") %></p>
</div>
