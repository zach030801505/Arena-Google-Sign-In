# Arena Google Sign-In
<p>This Module allows the use of Google's Sign-In system on the Shelby Arena Members Responsive Webpage</p>

<h2>How It Works</h2>
<p>When a user clickes the sign in with google button this module checks if a that googleID is valid and exists in the database, if it exists it lookes up the ascociated personID and then the persons username, if the person does not have a username it creates one for that user then signes in with the new Credentials. If that googleID is not in the database it then creates a new person including firts name last name email and googleID provided by the googleAPI gives the user a login username then signes the person in, onced signed in the module telles the webpage to either refresh or redirect based on the current context.</p>
<p>A new person created by this module can be merged with an existing person to link the existing person to the googleID</p>

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
