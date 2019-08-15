<%@ Control Language="c#" Inherits="ArenaWeb.WebControls.Custom.Luminate.Security.GoogleLogin" CodeFile="gLogin.ascx.cs" %>
<script src="https://apis.google.com/js/platform.js" async defer></script>


<meta name="google-signin-client_id" content="<%= ClientIDSetting %>">

<div id="gSignIn" class="col-4 g-signin2" align="center" data-onsuccess="GSignIn" visible="false"></div>
<asp:Button id="LogInPageBtn" runat="server" class="btn btn-primary col-4"/>

<asp:Literal ID="lcName" runat="server" /><br/>
<a href="#" class="btn btn-primary col-4" id="gSignOut" onclick="GsignOut();" style="display:none;">Click here to log out</a>
<script type="text/javascript">

    function GSignIn(googleUser) {

            //grabs profile information and token
            var profile = googleUser.getBasicProfile();
            var id_token = googleUser.getAuthResponse().id_token;

            //sends the id_token for validation
            var xhr = new XMLHttpRequest();
            xhr.open('POST', 'default.aspx?page=<%= Request["page"] %>&requestUrl=<%= Request["requestUrl"]%>');
            xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
            xhr.onload = function() {
                if(xhr.responseText != "1"){
                    var response = xhr.responseText;

                    if(response.startsWith("http") | response.startsWith("/")) window.location.href = xhr.responseText; //redirect the page
                    else location.reload(); //refreshes the page
                }
            };
            xhr.send('idtoken=' + id_token+'&googleID='+profile.getId() +'&name='+profile.getName()+'&email='+profile.getEmail());

            $("#gSignIn").hide();
            $("#gSignOut").show();

    }

    function GsignOut() {
        var auth2 = gapi.auth2.getAuthInstance();
        auth2.signOut().then(function () {
            console.log("user signed out");

            $("#gSignIn").show();
            $("#gSignOut").hide();

            var xhr = new XMLHttpRequest();
            xhr.open('POST', 'default.aspx?page=<%= Request["page"] %>');
            xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
            xhr.onload = function() {
              location.reload();
            };
            xhr.send('sucessLogOut=true');

        });
    }
</script>
