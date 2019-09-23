<%@ Control Language="c#" Inherits="ArenaWeb.WebControls.Custom.Luminate.Security.GoogleLogin" CodeFile="gLogin.ascx.cs" %>
<script src="https://apis.google.com/js/platform.js" async defer></script>


<meta name="google-signin-client_id" content="<%= ClientIDSetting %>">

<asp:Panel ID="signInPnl" Runat="server">
<div id="gSignIn" class="col-4 g-signin2" align="center" data-onsuccess="GSignIn" visible="false"></div>
<asp:Button id="LogInPageBtn" runat="server" class="btn btn-primary col-4"/>
</asp:Panel>

<asp:Literal ID="lcName" runat="server" /><br/>
<asp:Panel ID="logOut" Runat="server" Visible="False">
<a href="#" class="btn btn-primary col-4" id="SignOut" onclick="signOut();">Click here to log out</a>
</asp:Panel>
<script type="text/javascript">

    function GSignIn(googleUser) {

            //grabs profile information and token
            var profile = googleUser.getBasicProfile();
            var id_token = googleUser.getAuthResponse().id_token;

            //sends the id_token for validation
            var xhr = new XMLHttpRequest();
            xhr.open('POST', 'default.aspx'+window.location.search);
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
    function signOut() {
        try{
            var auth2 = gapi.auth2.getAuthInstance();
            auth2.signOut().then(function () {});
        }
        catch(err){}
        $("#gSignIn").show();
        $("#gSignOut").hide();

        var xhr = new XMLHttpRequest();
        xhr.open('POST', 'default.aspx?page=<%= Request["page"] %>');
        xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
        xhr.onload = function() {
          location.reload();
        };
        xhr.send('sucessLogOut=true');
    }
</script>
