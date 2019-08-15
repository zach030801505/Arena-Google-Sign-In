# Arena Google Sign-In
<p>This Module allows the use of Google's Sign-In system on the Shelby Arena Members Responsive Webpage</p>
<h2>Installation</h2>
<ol>
    <li>start by going to this link <a href="https://developers.google.com/identity/sign-in/web/sign-in?refresh=1">here</a> and configure a new project in the google API Console</li>
    <li>Run the google_signIn_setup.sql script on the arena database</li>
    <li>Drag n Drop the custom folder into Arena/WebControls
        <ul>
            <li>this may not be the appropreate folder to place this but its all I really know</li>
        </ul></LI>
    <li>In Arena add a new module and point it to the files you just installed</li>
    <li>add the module to any page you want to allow Google Sign in to be accepted
        <ul>
            <li>I added a special clause to hide the "Sign In with Credentials" button while on the credential sign in page</li>
        </ul></li>

</ol>
