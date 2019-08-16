/** This Script makes all tables and stored procedures for the google sign in module **/
/** Developed by Zachary Justus **/


IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'cust_luminate_google_id')
BEGIN
	CREATE TABLE dbo.cust_luminate_google_id (
	person_id int,
	google_id varchar(50) primary key
	)
END
GO


/** this addes a google id user pair to the database **/
CREATE or ALTER PROCEDURE dbo.cust_luminate_googleLogin_sp_add_googleID(
@google_id varchar(50),
@person_id int)
as
BEGIN
	DECLARE @valid int = (SELECT person_id FROM dbo.cust_luminate_google_id where google_id = @google_id)
	IF @valid is null
	BEGIN
		insert into dbo.cust_luminate_google_id(person_id, google_id)
		values(@person_id, @google_id)
	END
END
GO

/** this alters the google id table to update person id's **/
CREATE or ALTER PROCEDURE dbo.cust_luminate_googleLogin_sp_save_userID(
@google_id varchar(50),
@person_id int
) as 
BEGIN
	UPDATE dbo.cust_luminate_google_id
	set person_id = @person_id where google_id = @google_id;
END
GO

/** this checks if a google ID user Pair exists in the database **/
CREATE or ALTER PROCEDURE dbo.cust_luminate_googleLogIn_sp_get_userID(
@google_id varchar(50),
@person int OUTPUT)
as
BEGIN
	
	DECLARE @person_id int = (SELECT top(1) person_id from dbo.cust_luminate_google_id where google_id = @google_id)
	
	if @person_id is not null
	BEGIN --if person is in google table
		DECLARE @valid int =(select top(1) person_id from dbo.core_person where person_id = @person_id)
		
		if @valid is not null --if person id is in person table
		BEGIN
			SET @person = @valid 
			return
		END
		ELSE --if person was not in person table
		BEGIN 
			DECLARE @merged int = (SELECT top 1 old_person_id from dbo.core_person_merged where old_person_id = @person_id)
			IF(@merged is not null) --this checks if the person was merged
			BEGIN
				WHILE @valid is NUll --loops till most current record is found
				BEGIN
					DECLARE @new_id int = (select top(1) new_person_id from dbo.core_person_merged where old_person_id = @person_id)
					SET @valid = (select top(1) person_id from dbo.core_person where person_id = @new_id)
				END
				--update the google record
				UPDATE dbo.cust_luminate_google_id set person_id = @valid where google_id = @google_id
				SET @person = @valid
				RETURN
			END
			ELSE --if no merged member was found
			BEGIN
				DELETE from dbo.cust_luminate_google_id where google_id = @google_id
				SET @person = -1
				Return
			END
		END
	END
	ELSE --if no record matches
	BEGIN
		SET @person = -1
		Return
	END
	
END
GO

--select login_id from dbo.secu_login where person_id = 24211
CREATE or ALTER PROCEDURE dbo.cust_luminate_googleLogIn_sp_get_userLogin(
@person_id int,
@person_login varchar(50) OUTPUT
) as
BEGIN
	SET @person_login = (select top(1) login_id from dbo.secu_login where person_id = @person_id)
	if(@person_login is not null)
	BEGIN
		SET @person_login = (select top(1) login_id from dbo.secu_login where person_id = @person_id)
		RETURN
	END
	ELSE
	BEGIN
		SET @person_login = '-1'
		RETURN
	END
END
GO

--creates a new user and returns userID
CREATE or ALTER PROCEDURE dbo.cust_luminate_googleLogIn_sp_save_person(
@fname varchar(50),
@lname varchar(50),
@email varchar(100),
@defaultMemberStatus int,
@orgID int,
@defaultCampusID int,
@newID int OUTPUT
) as 
BEGIN
	DECLARE
	-- new person variables
	@PersonID int = -1,
	@UserId varchar(50) = 'GoogleSignIn',
	@OrganizationId int = @orgID,
	@CampusId int = @defaultCampusID,
	@Title int = null,
	@NickName nvarchar(50) = @fname,
	@FirstName nvarchar(50) = @fname,
	@MiddleName nvarchar(50) = '',
	@LastName nvarchar(50) = @lname,
	@Suffix int = null,
	@BirthDate datetime = '1-1-1900',
	@Gender int = 0,
	@Notes varchar(255) = 'created by Google SignIn',
	@MedicalInformation varchar(1000) = '',
	@Ssn varchar(12) = NULL,
	@Pin varchar(10) = NULL,
	@MaritalStatus int = null,
	@AnniversaryDate datetime = '1-1-1900',
	@MemberStatus int = @defaultMemberStatus,
	@RecordStatus int = 0,
	@BlobID int = null,
	@InactiveReason int = null,
	@ActiveMeter int = 0,
	@ContributeIndividually bit = 1,
	@PrintStatement bit = 1,
	@EmailStatement bit = 0,
	@IncludeOnEnvelope bit = 0,
	@StaffMember bit = 0,
	@LastAttended datetime = '1-1-1900',
	@GraduationDate datetime = '1-1-1900',
	@Business bit = 0,
	@Restricted bit = 0,
	@ContributionNote varchar(max) = NULL,
	@SubDonorId int = null,
	@Sync bit = 0,
	@GenerateEnvelopeNumber bit = 0,
	@EnvelopeNumberPersonId INT = -1,
	@ID int,
	--add email variables
	@emailID int = -1

	--make a person
	exec dbo.core_sp_save_person 
		@PersonID, @UserId, @OrganizationId, @CampusId, @Title, @NickName, @FirstName, @MiddleName, @LastName, @Suffix,
		@BirthDate, @Gender, @Notes, @MedicalInformation, @Ssn, @Pin, @MaritalStatus, @AnniversaryDate, @MemberStatus,
		@RecordStatus, @BlobID,	@InactiveReason, @ActiveMeter, @ContributeIndividually,	@PrintStatement, @EmailStatement,
		@IncludeOnEnvelope,	@StaffMember, @LastAttended, @GraduationDate, @Business, @Restricted, @ContributionNote, @SubDonorId,
		@Sync, @GenerateEnvelopeNumber,	@EnvelopeNumberPersonId, @ID OUTPUT
	
	--set there email
	exec dbo.core_sp_save_personEmail @emailID, @ID, 1, 1, @email, 'added by google', 'GoogleSignIn', 1, @orgID, @EmailID OUTPUT
	
	--add them to a family
	DECLARE @famID int,
		@famName varchar(100) = 'The '+@lname+' Family'
	exec dbo.core_sp_save_family -1, 'GoogelSignIn', @orgID, @famName, @famID OUTPUT
	exec dbo.core_sp_save_family_member @famID, @ID, 'GoogleSignIn', 29, @orgID

	--assign username
	DECLARE @username varchar(50)
	exec dbo.secu_sp_create_new_login @ID, 'googleSignIn', 1, @username OUTPUT

	SET @newID = @ID
	RETURN

END