<%@ Control Language="c#" AutoEventWireup="true" CodeBehind="View.ascx.cs" Inherits="Upendo.Modules.UpendoPrompt.View" %>
<%@ Import Namespace="DotNetNuke.Services.Localization" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/labelcontrol.ascx" %>

<div class="dnnClear">
	<h1>Upendo Prompt</h1>
	<p><%=Localization.GetString("Welcome") %></p>
	<hr />
	
	<h1>❤️ Enjoying This Extension? Support Its Future!</h1>
	<p>
		This project is built and maintained with passion to help people just like you.  If you're 
		finding value in this extension, consider <a href="https://github.com/sponsors/UpendoVentures" target="_blank">sponsoring 
		us on GitHub Sponsors</a>.
	</p>
	<p>
		Your support helps us keep improving, adding new features, and ensuring ongoing maintenance. Even a small 
		contribution makes a big difference!
	</p>

	<ul>
		<li>
			<a href="https://github.com/sponsors/UpendoVentures" target="_blank">👉🏽 Sponsor Us Today!</a>
		</li>
	</ul>

	<p>Thank you for helping keep open source sustainable! 🚀</p>

</div>
