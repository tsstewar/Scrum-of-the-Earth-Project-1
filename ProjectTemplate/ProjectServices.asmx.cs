﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data;
namespace ProjectTemplate
{
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [System.Web.Script.Services.ScriptService]
    public class ProjectServices : System.Web.Services.WebService
    {

        public string userID = "";
        ////////////////////////////////////////////////////////////////////////
        ///replace the values of these variables with your database credentials
        ////////////////////////////////////////////////////////////////////////
        private string dbID = "scrumoftheearth";
        private string dbPass = "!!Scrumoftheearth";
        private string dbName = "scrumoftheearth";
        ////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////
        ///call this method anywhere that you need the connection string!
        ////////////////////////////////////////////////////////////////////////
        private string getConString()
        {
            return "SERVER=107.180.1.16; PORT=3306; DATABASE=" + dbName + "; UID=" + dbID + "; PASSWORD=" + dbPass;
        }
        ////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////
        //don't forget to include this decoration above each method that you want
        //to be exposed as a web service!
        [WebMethod(EnableSession = true)]
        /////////////////////////////////////////////////////////////////////////
        public string TestConnection()
        {
            try
            {
                string testQuery = "select * from test";
                ////////////////////////////////////////////////////////////////////////
                ///here's an example of using the getConString method!
                ////////////////////////////////////////////////////////////////////////
                MySqlConnection con = new MySqlConnection(getConString());
                ////////////////////////////////////////////////////////////////////////
                MySqlCommand cmd = new MySqlCommand(testQuery, con);
                MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                DataTable table = new DataTable();
                adapter.Fill(table);
                return "Success!";
            }
            catch (Exception e)
            {
                return "Something went wrong, please check your credentials and db name and try again.  Error: " + e.Message;
            }
        }
        [WebMethod(EnableSession = true)] //NOTICE: gotta enable session on each individual method
        public bool LogOn(string uid, string pass)
        {
            //we return this flag to tell them if they logged in or not
            bool success = false;

            //our connection string comes from our web.config file like we talked about earlier
            string sqlConnectString = getConString();
            //here's our query.  A basic select with nothing fancy.  Note the parameters that begin with @
            //NOTICE: we added admin to what we pull, so that we can store it along with the id in the session
            string sqlSelect = "SELECT id FROM users WHERE userid=@idValue and pass=@passValue";
            //set up our connection object to be ready to use our connection string
            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            //set up our command object to use our connection, and our query
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);
            //tell our command to replace the @parameters with real values
            //we decode them because they came to us via the web so they were encoded
            //for transmission (funky characters escaped, mostly)
            sqlCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(uid));
            sqlCommand.Parameters.AddWithValue("@passValue", HttpUtility.UrlDecode(pass));
            //a data adapter acts like a bridge between our command object and 
            //the data we are trying to get back and put in a table object
            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            //here's the table we want to fill with the results from our query
            DataTable sqlDt = new DataTable();
            //here we go filling it!
            sqlDa.Fill(sqlDt);
            //check to see if any rows were returned.  If they were, it means it's 
            //a legit account, if function to determine if user or admin
            if (sqlDt.Rows.Count > 0)
            {
                //if we found an account, store the id and admin status in the session
                //so we can check those values later on other method calls to see if they 
                //are 1) logged in at all, and 2) and admin or not
                Session["id"] = sqlDt.Rows[0]["id"];
                success = true;
            }
            //return the result!
            return success;
        }
        //EXAMPLE OF AN INSERT QUERY WITH PARAMS PASSED IN.  BONUS GETTING THE INSERTED ID FROM THE DB!
        [WebMethod(EnableSession = true)]
        public void RequestAccount(string userid, string pass, string firstname, string lastname, string mentorshiptype)
        {
            //string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            string sqlConnectString = getConString();
            //the only thing fancy about this query is SELECT LAST_INSERT_ID() at the end.  All that
            //does is tell mySql server to return the primary key of the last inserted row.
            string sqlSelect = "insert into users (userid, pass, firstname, lastname, mentorshiptype) " +
                "values(@userIdValue, @passValue, @firstnameValue, @lastnameValue, @mentorshipValue); SELECT LAST_INSERT_ID();";
            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);
            //sqlCommand.Parameters.AddWithValue("@idNumValue", HttpUtility.UrlDecode(id));
            sqlCommand.Parameters.AddWithValue("@userIdValue", HttpUtility.UrlDecode(userid));
            sqlCommand.Parameters.AddWithValue("@passValue", HttpUtility.UrlDecode(pass));
            sqlCommand.Parameters.AddWithValue("@firstnameValue", HttpUtility.UrlDecode(firstname));
            sqlCommand.Parameters.AddWithValue("@lastnameValue", HttpUtility.UrlDecode(lastname));
            sqlCommand.Parameters.AddWithValue("@mentorshipValue", HttpUtility.UrlDecode(mentorshiptype));
            //this time, we're not using a data adapter to fill a data table.  We're just
            //opening the connection, telling our command to "executescalar" which says basically
            //execute the query and just hand me back the number the query returns (the ID, remember?).
            //don't forget to close the connection!
            sqlConnection.Open();
            //we're using a try/catch so that if the query errors out we can handle it gracefully
            //by closing the connection and moving on
            try
            {
                int accountID = Convert.ToInt32(sqlCommand.ExecuteScalar());
                //here, you could use this accountID for additional queries regarding
                //the requested account.  Really this is just an example to show you
                //a query where you get the primary key of the inserted row back from
                //the database!
            }
            catch (Exception e)
            {
            }
            sqlConnection.Close();
        }

        [WebMethod(EnableSession = true)]
        public bool LogOff()
        {
            //if they log off, then we remove the session.  That way, if they access
            //again later they have to log back on in order for their ID to be back
            //in the session!
            Session.Abandon();
            return true;
        }


        [WebMethod(EnableSession = true)]
        public string GetUsers()
        {
            try
            {
                string testQuery = "select * from users";
                ////////////////////////////////////////////////////////////////////////
                ///here's an example of using the getConString method!
                ////////////////////////////////////////////////////////////////////////
                MySqlConnection con = new MySqlConnection(getConString());
                ////////////////////////////////////////////////////////////////////////
                MySqlCommand cmd = new MySqlCommand(testQuery, con);
                MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                DataTable table = new DataTable();
                adapter.Fill(table);

                System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
                Dictionary<string, object> row;
                foreach (DataRow dr in table.Rows)
                {
                    row = new Dictionary<string, object>();
                    foreach (DataColumn col in table.Columns)
                    {
                        row.Add(col.ColumnName, dr[col]);
                    }
                    rows.Add(row);
                }
                return serializer.Serialize(rows);
            }
            catch (Exception e)
            {
                return "Something went wrong, please check your credentials and db name and try again.  Error: " + e.Message;
            }
        }


        [WebMethod(EnableSession = true)]
        public void CreateNewGoal(string goal)
        {
            //string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            string sqlConnectString = getConString();
            //the only thing fancy about this query is SELECT LAST_INSERT_ID() at the end.  All that
            //does is tell mySql server to return the primary key of the last inserted row.
            string sqlSelect = "insert into Goals (goal) " +
                "values(@goal_Value); SELECT LAST_INSERT_ID();";
            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);
            //sqlCommand.Parameters.AddWithValue("@idNumValue", HttpUtility.UrlDecode(id));
            sqlCommand.Parameters.AddWithValue("@goal_Value", HttpUtility.UrlDecode(goal));
            //this time, we're not using a data adapter to fill a data table.  We're just
            //opening the connection, telling our command to "executescalar" which says basically
            //execute the query and just hand me back the number the query returns (the ID, remember?).
            //don't forget to close the connection!
            sqlConnection.Open();
            //we're using a try/catch so that if the query errors out we can handle it gracefully
            //by closing the connection and moving on
            try
            {
                int accountID = Convert.ToInt32(sqlCommand.ExecuteScalar());
                //here, you could use this accountID for additional queries regarding
                //the requested account.  Really this is just an example to show you
                //a query where you get the primary key of the inserted row back from
                //the database!
            }
            catch (Exception e)
            {
            }
            sqlConnection.Close();
        }


        [WebMethod(EnableSession = true)]
        public string GetInProgressGoals()
        {
            try
            {
                string testQuery = "select goal From Goals Where completed=0";
                ////////////////////////////////////////////////////////////////////////
                ///here's an example of using the getConString method!
                ////////////////////////////////////////////////////////////////////////
                MySqlConnection con = new MySqlConnection(getConString());
                ////////////////////////////////////////////////////////////////////////
                MySqlCommand cmd = new MySqlCommand(testQuery, con);
                MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                DataTable table = new DataTable();
                adapter.Fill(table);

                System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
                Dictionary<string, object> row;
                foreach (DataRow dr in table.Rows)
                {
                    row = new Dictionary<string, object>();
                    foreach (DataColumn col in table.Columns)
                    {
                        row.Add(col.ColumnName, dr[col]);
                    }
                    rows.Add(row);
                }
                return serializer.Serialize(rows);
            }
            catch (Exception e)
            {
                return "Something went wrong, please check your credentials and db name and try again.  Error: " + e.Message;
            }
        }

        [WebMethod(EnableSession = true)]
        public string GetCompletedGoals()
        {
            try
            {
                string testQuery = "select goal From Goals Where completed=1";
                ////////////////////////////////////////////////////////////////////////
                ///here's an example of using the getConString method!
                ////////////////////////////////////////////////////////////////////////
                MySqlConnection con = new MySqlConnection(getConString());
                ////////////////////////////////////////////////////////////////////////
                MySqlCommand cmd = new MySqlCommand(testQuery, con);
                MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                DataTable table = new DataTable();
                adapter.Fill(table);

                System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
                Dictionary<string, object> row;
                foreach (DataRow dr in table.Rows)
                {
                    row = new Dictionary<string, object>();
                    foreach (DataColumn col in table.Columns)
                    {
                        row.Add(col.ColumnName, dr[col]);
                    }
                    rows.Add(row);
                }
                return serializer.Serialize(rows);
            }
            catch (Exception e)
            {
                return "Something went wrong, please check your credentials and db name and try again.  Error: " + e.Message;
            }
        }
        
        //
        //Non-working search function, commented out for search page.
        //
        //[WebMethod(EnableSession = true)]
        //public Account[] searchResults(string searchInput)
        //{

        //    string sqlQuery = "select userid, firstname, lastname, mentorshiptype from users where name=@searchInputValue";

        //    MySqlConnection con = new MySqlConnection(getConString());
        //    MySqlCommand cmd = new MySqlCommand(sqlQuery, con);

        //    //use this to fill a data table
        //    MySqlDataAdapter sqlDa = new MySqlDataAdapter(cmd);
        //    sqlDa.Fill(sqlDt);

        //    //creates a list of accounts that fit the search criteria
        //    List<Account> accounts = new List<Account>();
        //    for (int i = 0; i < sqlDt.Rows.Count; i++)
        //    {
        //        accounts.Add(new Account
        //        {
        //            userid = sqlDt.Rows[i]["userid"].ToString(),
        //            firstname = sqlDt.Rows[i]["firstname"].ToString(),
        //            lastname = sqlDt.Rows[i]["lastname"].ToString(),
        //            mentorshiptype = sqlDt.Rows[i]["mentorshiptype"].ToString()
        //        });
        //    }
        //    //convert the list of accounts searched to an array and return it
        //    return accounts.ToArray();
        //}
    }
}
