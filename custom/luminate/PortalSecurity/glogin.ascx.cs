namespace ArenaWeb.WebControls.Custom.Luminate.Security{
    using System;
    using System.ComponentModel;
    using System.Collections.Generic;
    using System.Text;
	using System.Web;
    using System.Net.Http;
    using System.Web.Security;
    using System.Web.UI;
    using System.Web.Script.Serialization;
    using System.Data;
    using System.Collections;
    using System.Data.SqlClient;

    using Arena.Portal;
    using Arena.Security;

    using Arena.DataLib;
    using Arena.DataLayer;
    using Arena.DataLayer.Security;

	/// <summary>
	///	this module loges in users via google
	/// </summary>

    public partial class GoogleLogin : PortalControl{
        #region Module Settings

        [Category("Google Settings")]
        [TextSetting("ClientID", "This is the google Client ID", true)]
        public string ClientIDSetting { get { return Setting("ClientID", "", false); } }

        [Category("New Member")]
        [LookupSetting("Member Status", "The Member Status Lookup value to set a new User.", true, "0B4532DB-3188-40F5-B188-E7E6E4448C85")]
        public string MemberStatusSetting { get { return Setting("MemberStatus", "", false); } }

        [Category("New Member")]
        [NumericSetting("Title", "this is the default title ID of new people added by this module", true)]
        public string TitleSetting {get {return Setting("Title", "", true);}}

        [Category("New Member")]
        [NumericSetting("DefaultCampus", "The campus to assign a user.", true)]
        public string DefaultCampusSetting { get { return Setting("DefaultCampus", "", false); } }

        [Category("User Propmpts")]
        [TextSetting("Log In Text", "Log In Button Text", false)]
        public string LogInTextSetting { get { return Setting("LogInText", "", false); } }

        [Category("User Propmpts")]
        [TextSetting("Welcome Text", "welcomes the user when logged in. ##firstname##, ##nickname##, ##lastname## are available", false)]
        public string WelcomeTextSetting { get { return Setting("WelcomeText", "", false); } }

        #endregion



        protected void Page_Load(object sender, System.EventArgs e){
            if(!Request.IsAuthenticated){
                if(Request["page"] != CurrentPortal.LoginPageID.ToString()){

                    if((String)LogInTextSetting != String.Empty) LogInPageBtn.Text = (String)LogInTextSetting;
                    else LogInPageBtn.Text = "Sign in with credentials";
                    LogInPageBtn.Visible = true;

                } else{
                    LogInPageBtn.Visible = false;
                }
                lcName.Visible = false;
            }
            else{

                String WelcomeText;
                if((String)WelcomeTextSetting != String.Empty) WelcomeText = (String)WelcomeTextSetting;
                else WelcomeText = "Welcome ##nickname## ##lastname##!";

                LogInPageBtn.Visible = false;
                lcName.Text = WelcomeTextSetting
                                .Replace("##nickname##", (string.IsNullOrEmpty(CurrentPerson.NickName) ? CurrentPerson.FirstName : CurrentPerson.NickName))
                                .Replace("##lastname##", CurrentPerson.LastName);
                lcName.Visible = true;
            }

            if (!Page.IsPostBack){
                Session["googleID"] = string.Empty;
                Session["RedirectValue"] = string.Empty;
                if (Request["requestUrl"] != null){
                    Session["RedirectValue"] = Request["requestUrl"];
                }

                if(Request.Form["sucessLogOut"] == "true"){ //this signes out the current user (for now)
                    gSignOut();
                }

                if(Request.Form["idtoken"] != null && Request.Form["googleID"] != null){ //if the user signes in via google
                    if(Request.IsAuthenticated){ //if already lodded in
                        if(Session["RedirectValue"] != string.Empty){ //if redirect paramater exists
                            //Response.Write((String)Session["RedirectValue"]);
                            Response.Write(Session["RedirectValue"]);
                            Response.End();
                        }
                        else{ //prevents reload loop
                            Response.Write("1");
                            Response.End();
                        }
                    }
                    else{
                        Session["googleID"] =  googleValidation(Request.Form["idtoken"], Request.Form["googleID"]);
                        if(Session["googleID"] != "-1"){ //if validation returned a value

                            googleDataLayer arenaData = new googleDataLayer();

                            int personID;
                            personID = arenaData.getUserID((String)Session["googleID"]); //lookes up userID from googleID
                            if(personID < 0){
                                if (personID == -2){ //sql error
                                    Response.Write("something went wrong");
                                    Response.End();
                                }
                                else{ //no user found

                                    String personName = (String)Request.Form["name"];
                                    String[] splitName = personName.Split(' ');
                                    String fname = splitName[0];
                                    String lname = splitName[1];
                                    String email = (String)Request.Form["email"];

                                    int memberStatus;
                                    int title;
                                    int orgID = CurrentOrganization.OrganizationID;
                                    int campusID;

                                    try { memberStatus = Int32.Parse(MemberStatusSetting); }
                                    catch { throw new ModuleException(CurrentPortalPage, CurrentModule, "Default Member Status ID must be numeric.: "); }
                                    try { title = Int32.Parse(TitleSetting); }
                                    catch { throw new ModuleException(CurrentPortalPage, CurrentModule, "Default TitleSetting ID must be numeric.: "); }
                                    try { campusID = Int32.Parse(DefaultCampusSetting); }
                                    catch { throw new ModuleException(CurrentPortalPage, CurrentModule, "Default Campus ID must be numeric.: "); }

                                    //makes sure the names are in proper format
                                    if(fname == String.Empty){
                                        fname = "";
                                    }
                                    if(lname == String.Empty){
                                        lname = "";
                                    }

                                    //make a new person
                                    personID = arenaData.newUser(splitName[0], splitName[1], email, memberStatus, title, orgID, campusID);
                                    //add personID to google list
                                    if(personID > 0){
                                        int asscociateGid = arenaData.SaveGoogleID(personID, (String)Session["googleID"]);
                                    }

                                }
                            }
                            if(personID == -1 || personID == -2){
                                Response.Write("something went wrong");
                                Response.End();
                            }
                            else{ //match found

                                //sign in function
                                String username;
                                username = arenaData.getUserName(personID);
                                if(username == null || username == "-1" || username == ""){
                                    //make a username
                                    username = arenaData.newUserName(personID, CurrentOrganization.OrganizationID);
                                }

                                if(username == "-2"){
                                    Response.Write("error something went wrong");
                                    Response.End();

                                }
                                else{
                                    //sign in the user
                                    Arena.Security.Login login = new Arena.Security.Login(username);

                                    FormsAuthentication.SetAuthCookie(username, true);
                                    Response.Cookies["portalroles"].Value = string.Empty;
                                    Response.Redirect(Request.ApplicationPath);

                                    if(Session["RedirectValue"] != string.Empty){
                                        //Response.Write(Session["RedirectValue"]);
                                        Response.Write("yeeee");
                                        Response.End();
                                    }
                                    else{
                                        Response.Write("yeee");
                                        //Response.Write(Request.ApplicationPath);
                                        Response.End();
                                    }


                                }
                            }

                        }
                        else{
                            Response.Write("error something went wrong");
                            Response.End();
                        }
                    }
                }
            }
        } //end page load

        //this validates the google ID and returns userID
        public String googleValidation(String key, String id){
            using(var apiClient = new System.Net.Http.HttpClient()){
                apiClient.BaseAddress = new Uri("https://oauth2.googleapis.com/tokeninfo?");
                apiClient.DefaultRequestHeaders.Accept.Clear();
                apiClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                var ApiResult = apiClient.GetAsync("https://oauth2.googleapis.com/tokeninfo?id_token="+key).Result;
                if(ApiResult.IsSuccessStatusCode){
                    String response = ApiResult.Content.ReadAsStringAsync().Result;
                    var dict = new JavaScriptSerializer().Deserialize<dynamic>(response);

                    if(dict["aud"] == ClientIDSetting && dict["sub"] == id){ //check if the client id and provided google ID is valid
                        return dict["sub"];
                    }
                    else return (String)"-1";
                }
                else return (string)"-1";
            }
        }

        public void gSignOut(){ //sign out method
            //signOut
            FormsAuthentication.SignOut();
            // Invalidate roles token
            Response.Cookies["portalroles"].Value = null;
            Response.Cookies["portalroles"].Path = "/";
            Response.Redirect("default.aspx?page="+Request["page"]);
        }

        protected void LogInPageBtn_click(object sender, EventArgs e)
		{
            string redirect = "&requestUrl=" + Page.Server.UrlEncode(Request.RawUrl.ToString());
			Response.Redirect(string.Format("~/Default.aspx?page={0}{1}", CurrentPortal.LoginPageID.ToString(), redirect));
		}


        #region Button Event Listiners
        override protected void OnInit(EventArgs e)
        {
            InitializeComponent();
            base.OnInit(e);
        }

        /// <summary>
        ///		event listiners for buttons
        /// </summary>
        private void InitializeComponent()
        {
            this.LogInPageBtn.Click += new EventHandler(LogInPageBtn_click);
            //this.ibLog.Click += new System.Web.UI.ImageClickEventHandler(ibLog_Click);
        }
        #endregion




    } //end main class

    //this is the database connection class
    public class googleDataLayer : SqlData{

        //constructor
        public googleDataLayer(){}

        //gets user ID
        public int getUserID(String id){
            ArrayList paramList = new ArrayList(); //input paramaters
            paramList.Add(new SqlParameter("@google_id", id));

            SqlParameter paramOut = new SqlParameter(); //output paramaters
            paramOut.ParameterName = "@person";
            paramOut.Direction = ParameterDirection.Output;
            paramOut.SqlDbType = SqlDbType.Int;
            paramList.Add(paramOut);

            try
            {
                this.ExecuteNonQuery("cust_luminate_googleLogIn_sp_get_userID", paramList);
                return (int)((SqlParameter)(paramList[paramList.Count - 1])).Value;
            }
            catch (SqlException ex)
            {
                return -2;
            }
            finally
            {
                paramList = null;
            }
        } //end getUserID function

        //gets a username
        public String getUserName(int id){
            ArrayList paramList = new ArrayList();
            paramList.Add(new SqlParameter("@person_id", id));

            SqlParameter paramOut = new SqlParameter(); //output paramaters
            paramOut.ParameterName = "@person_login";
            paramOut.Direction = ParameterDirection.Output;
            paramOut.Size = 200;
            paramList.Add(paramOut);

            try
            {
                this.ExecuteNonQuery("cust_luminate_googleLogIn_sp_get_userLogin", paramList);
                return (String)((SqlParameter)(paramList[paramList.Count - 1])).Value;
            }
            catch (SqlException ex)
            {
                return "-2";
            }
            finally
            {
                paramList = null;
            }
        }

        //adeds a new person and email
        public int newUser(String fName, String lName, String email, int memberStatus, int title, int orgID, int campus){

            ArrayList paramList = new ArrayList();
            paramList.Add(new SqlParameter("@fname", fName));
            paramList.Add(new SqlParameter("@lname", lName));
            paramList.Add(new SqlParameter("@email", email));
            paramList.Add(new SqlParameter("@defaultMemberStatus", memberStatus));
            paramList.Add(new SqlParameter("@title_luid", title));
            paramList.Add(new SqlParameter("@orgID", orgID));
            paramList.Add(new SqlParameter("@defaultCampusID", campus));

            SqlParameter paramOut = new SqlParameter(); //output paramaters
            paramOut.ParameterName = "@newID";
            paramOut.Direction = ParameterDirection.Output;
            paramOut.SqlDbType = SqlDbType.Int;
            paramList.Add(paramOut);


            try
            {
                this.ExecuteNonQuery("cust_luminate_googleLogIn_sp_save_person", paramList);
                return (int)((SqlParameter)(paramList[paramList.Count - 1])).Value;
            }
            catch (SqlException ex)
            {
                return -2;
            }
            finally
            {
                paramList = null;
            }


        }
        public String newUserName(int id, int orgID){
            //make a new username based on personID
            //secu_sp_create_new_login

            ArrayList paramList = new ArrayList();
            paramList.Add(new SqlParameter("@personID", id));
            paramList.Add(new SqlParameter("@userID", "GoogleSignIn"));
            paramList.Add(new SqlParameter("@orgID", orgID));

            SqlParameter paramOut = new SqlParameter(); //output paramaters
            paramOut.ParameterName = "@loginID";
            paramOut.Direction = ParameterDirection.Output;
            paramOut.Size = 200;
            paramList.Add(paramOut);

            try
            {
                this.ExecuteNonQuery("secu_sp_create_new_login", paramList);
                return (String)((SqlParameter)(paramList[paramList.Count - 1])).Value;
            }
            catch (SqlException ex)
            {
                return "-2";
            }
            finally
            {
                paramList = null;
            }

        }

        public int SaveGoogleID(int id, String gID){
            //cust_luminate_googleLogin_sp_add_googleID

            ArrayList paramList = new ArrayList();
            paramList.Add(new SqlParameter("@google_id", gID));
            paramList.Add(new SqlParameter("@person_id", id));
            try
            {
                this.ExecuteNonQuery("cust_luminate_googleLogin_sp_add_googleID", paramList);
                return 1;
            }
            catch (SqlException ex)
            {
                return -2;
            }
            finally
            {
                paramList = null;
            }
        }

    } //end data class


} //end namespace
