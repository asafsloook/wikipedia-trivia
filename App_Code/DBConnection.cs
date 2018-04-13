using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Data;
using System.Text;

/// <summary>
/// DBServices is a class created by me to provides some DataBase Services
/// </summary>
public class DBConnection
{
    public SqlDataAdapter da;
    public DataTable dt;

    public DBConnection()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    internal void insertReading(string userID, string articleId, string date, string categoryname)
    {
        SqlConnection con;
        SqlCommand cmd;

        try
        {
            con = connect("GraspDBConnectionString"); // create the connection
        }
        catch (Exception ex)
        {
            // write to log
            throw (ex);
        }

        int categoryID = getCategoryId(categoryname);

        string cStr = " insert into ReadingP (userId, ArticleId,ReadingTime,CategoryId) values( " + userID + " , '" + articleId + "' , '" + date + "' , " + categoryID +" ) ";

        cmd = new SqlCommand(cStr, con);             // create the command

        try
        {
            int numEffected = cmd.ExecuteNonQuery(); // execute the command
            //return numEffected;
        }
        catch (Exception ex)
        {
            //return 0;
            // write to log
            throw (ex);
        }

        finally
        {
            if (con != null)
            {
                // close the db connection
                con.Close();
            }
        }
    }


    public bool isUserRead(string articleId, string userId)
    {
        SqlConnection con = connect("GraspDBConnectionString"); // open the connection to the database/

        String selectStr = "Select * from ReadingP where UserID = " + userId + " and ArticleId='" + articleId + "'"; // create the select that will be used by the adapter to select data from the DB

        SqlDataAdapter da = new SqlDataAdapter(selectStr, con); // create the data adapter

        DataSet ds = new DataSet("DS"); // create a DataSet and give it a name (not mandatory) as defualt it will be the same name as the DB

        da.Fill(ds);       // Fill the datatable (in the dataset), using the Select command

        dt = ds.Tables[0]; // point to the cars table , which is the only table in this case


        if (dt.Rows.Count == 0)
        {
            return false;
        }


        return true;
    }


    //--------------------------------------------------------------------------------------------------
    // This method creates a connection to the database according to the connectionString name in the web.config 
    //--------------------------------------------------------------------------------------------------
    public SqlConnection connect(String conString)
    {

        // read the connection string from the configuration file
        string cStr = WebConfigurationManager.ConnectionStrings[conString].ConnectionString;
        SqlConnection con = new SqlConnection(cStr);
        con.Open();
        return con;
    }


    //--------------------------------------------------------------------------------------------------
    public void updateUserCategories(List<int> catIDS, string IMEI)
    {

        int userID = getUserIdByIMEI(IMEI);


        SqlConnection con = connect("GraspDBConnectionString"); // open the connection to the database/

        String selectStr = "select * from userCategoriesP where UserID=" + userID; // create the select that will be used by the adapter to select data from the DB

        SqlDataAdapter da = new SqlDataAdapter(selectStr, con); // create the data adapter

        DataSet ds = new DataSet("DS"); // create a DataSet and give it a name (not mandatory) as defualt it will be the same name as the DB

        da.Fill(ds, "userCat");       // Fill the datatable (in the dataset), using the Select command

        dt = ds.Tables["userCat"]; // point to the cars table , which is the only table in this case

        if (dt.Rows.Count != 0)
        {
            foreach (DataRow row in dt.Rows) //for each row in DB 
            {
                if ((bool)row.ItemArray[3]) //if category is active
                {
                    bool visit = true;
                    //update
                    foreach (var catID in catIDS)
                    {
                        if (row.ItemArray[2].ToString() == catID.ToString())
                        {
                            catIDS.Remove(catID);
                            visit = false;
                            break;
                        }
                    }

                    if (visit)
                    {
                        ////update to not active
                        int tempID = int.Parse(row.ItemArray[2].ToString());
                        updateCategoryStatus(tempID, userID, 0);
                    }


                }
                else //if category is NOT active
                {
                    foreach (var catID in catIDS)
                    {
                        if (row.ItemArray[2].ToString() == catID.ToString())
                        {
                            //update to active
                            updateCategoryStatus(catID, userID, 1);
                            catIDS.Remove(catID);
                            break;
                        }

                    }
                }
            }
        }

        if (catIDS.Count > 0)
        {
            foreach (var catID in catIDS)
            {
                insertCategoryForUser(catID, userID);
            }
        }



    }

    private int insertCategoryForUser(int catID, int userID)
    {

        SqlConnection con;
        SqlCommand cmd;

        try
        {
            con = connect("GraspDBConnectionString"); // create the connection
        }
        catch (Exception ex)
        {
            // write to log
            throw (ex);
        }

        string cStr = " insert into userCategoriesP values( " + userID + " , " + catID + " , 1 ) ";

        cmd = new SqlCommand(cStr, con);             // create the command

        try
        {
            int numEffected = cmd.ExecuteNonQuery(); // execute the command
            return numEffected;
        }
        catch (Exception ex)
        {
            return 0;
            // write to log
            throw (ex);
        }

        finally
        {
            if (con != null)
            {
                // close the db connection
                con.Close();
            }
        }
    }

    private int updateCategoryStatus(int catID, int userID, int v)
    {


        SqlConnection con;
        SqlCommand cmd;

        try
        {
            con = connect("GraspDBConnectionString"); // create the connection
        }
        catch (Exception ex)
        {
            // write to log
            throw (ex);
        }

        string cStr = " update userCategoriesP "
                       + " set Active = " + v
                       + " where CategoryID = " + catID + " and UserID = " + userID;

        cmd = new SqlCommand(cStr, con);             // create the command

        try
        {
            int numEffected = cmd.ExecuteNonQuery(); // execute the command
            return numEffected;
        }
        catch (Exception ex)
        {
            return 0;
            // write to log
            throw (ex);
        }

        finally
        {
            if (con != null)
            {
                // close the db connection
                con.Close();
            }
        }



    }

    internal List<int> checkCategories(string[] arr) // gets array of category names - returns list of category IDs
    {
        List<int> catIDS = new List<int>();

        foreach (var item in arr)
        {
            int id = getCategoryId(item);

            if (id == -1)
            {
                insertCategory(item);
                id = getCategoryId(item);
            }
            catIDS.Add(id);
        }
        return catIDS;
    }

    private int insertCategory(string CategoryName)
    {
        SqlConnection con;
        SqlCommand cmd;

        try
        {
            con = connect("GraspDBConnectionString"); // create the connection
        }
        catch (Exception ex)
        {
            // write to log
            throw (ex);
        }

        string cStr = "insert into CategoriesP (CategoryName)";
        cStr += string.Format("values('{0}')", CategoryName);

        //String cStr = "INSERT INTO UsersP values({0},{1}      // helper method to build the insert string

        cmd = new SqlCommand(cStr, con);             // create the command

        try
        {
            int numEffected = cmd.ExecuteNonQuery(); // execute the command
            return numEffected;
        }
        catch (Exception ex)
        {
            return 0;
            // write to log
            throw (ex);
        }

        finally
        {
            if (con != null)
            {
                // close the db connection
                con.Close();
            }
        }
    }

    private int getCategoryId(string categoryName)
    {
        SqlConnection con = connect("GraspDBConnectionString"); // open the connection to the database/

        string selectStr = "select CategoryId from CategoriesP where CategoryName='" + categoryName + "' ";

        SqlDataAdapter da = new SqlDataAdapter(selectStr, con); // create the data adapter

        DataSet ds = new DataSet("DS"); // create a DataSet and give it a name (not mandatory) as defualt it will be the same name as the DB

        da.Fill(ds, "catID");       // Fill the datatable (in the dataset), using the Select command

        dt = ds.Tables["catID"]; // point to the cars table , which is the only table in this case

        if (dt.Rows.Count == 0)
        {
            return -1;
        }

        return int.Parse(dt.Rows[0].ItemArray[0].ToString());
    }

    //--------------------------------------------------------------------------------------------------
    public int updateUserPref(User user)
    {

        SqlConnection con;
        SqlCommand cmd;

        try
        {
            con = connect("GraspDBConnectionString"); // create the connection
        }
        catch (Exception ex)
        {
            // write to log
            throw (ex);
        }

        string cStr = "update UsersP "
            + " set "
            + " LocationPush = '" + user.LocationPush.ToString() + "', "
            + " PhotoPush = '" + user.PhotoPush.ToString() + "',"
            + " PhotoPushTime = '" + user.PhotoPushTime.ToShortTimeString() + "', "
            + " RandomArticlePush = '" + user.ArticlePush.ToString() + "', "
            + " RandomArticleQuantity = '" + user.ArticlesPerDay + "' "
            + " where userId = " + user.Id;

        //String cStr = "INSERT INTO UsersP values({0},{1}      // helper method to build the insert string

        cmd = new SqlCommand(cStr, con);             // create the command

        try
        {
            int numEffected = cmd.ExecuteNonQuery(); // execute the command
            return numEffected;
        }
        catch (Exception ex)
        {
            return 0;
            // write to log
            throw (ex);
        }

        finally
        {
            if (con != null)
            {
                // close the db connection
                con.Close();
            }
        }

    }


    //--------------------------------------------------------------------------------------------------
    public int insertUser(string IMEI)
    {

        SqlConnection con;
        SqlCommand cmd;

        try
        {
            con = connect("GraspDBConnectionString"); // create the connection
        }
        catch (Exception ex)
        {
            // write to log
            throw (ex);
        }

        //Default preferences for a new user
        string cStr = "insert into UsersP (IMEI,pushKey,LocationPush,PhotoPush,PhotoPushTime,RandomArticlePush,RandomArticleQuantity) ";
        cStr += "values('" + IMEI + "','www',1,1,'12:00',1,5)";

        //String cStr = "INSERT INTO UsersP values({0},{1}      // helper method to build the insert string

        cmd = new SqlCommand(cStr, con);             // create the command

        try
        {
            int numEffected = cmd.ExecuteNonQuery(); // execute the command
            return numEffected;
        }
        catch (Exception ex)
        {
            return 0;
            // write to log
            throw (ex);
        }

        finally
        {
            if (con != null)
            {
                // close the db connection
                con.Close();
            }
        }

    }


    //--------------------------------------------------------------------------------------------------
    public User checkUser(string IMEI)
    {
        SqlConnection con = connect("GraspDBConnectionString"); // open the connection to the database/

        String selectStr = "select * from UsersP where IMEI='" + IMEI + "'"; // create the select that will be used by the adapter to select data from the DB

        SqlDataAdapter da = new SqlDataAdapter(selectStr, con); // create the data adapter

        DataSet ds = new DataSet("DS"); // create a DataSet and give it a name (not mandatory) as defualt it will be the same name as the DB

        da.Fill(ds, "userPref2");       // Fill the datatable (in the dataset), using the Select command

        dt = ds.Tables["userPref2"]; // point to the cars table , which is the only table in this case
        if (dt.Rows.Count == 0)
        {
            insertUser(IMEI);

            User u = new User();
            u.Imei = IMEI;


            return u;
        }
        else
        {
            User u = new User();

            u.Id = (int)dt.Rows[0].ItemArray[0];
            u.Imei = (string)dt.Rows[0].ItemArray[1];
            u.PushKey = (string)dt.Rows[0].ItemArray[2];
            u.LocationPush = (bool)dt.Rows[0].ItemArray[3];
            u.PhotoPush = (bool)dt.Rows[0].ItemArray[4];
            u.PhotoPushTime = Convert.ToDateTime(dt.Rows[0].ItemArray[5].ToString());
            u.ArticlePush = (bool)dt.Rows[0].ItemArray[6];
            u.ArticlesPerDay = int.Parse(dt.Rows[0].ItemArray[7].ToString());
            u.Categories = new List<Category>();

            string selectStr2 = "Select CategoryName,CategoryID,Active from userCategoriesView where UserID =" + u.Id;
            SqlDataAdapter da2 = new SqlDataAdapter(selectStr2, con);

            da2.Fill(ds, "userPref1");
            dt = ds.Tables["userPref1"];

            foreach (DataRow item in dt.Rows)
            {
                if ((bool)item.ItemArray[2])
                {
                    Category c = new Category();
                    c.Name = item.ItemArray[0].ToString();
                    c.CategoryId = int.Parse(item.ItemArray[1].ToString());
                    u.Categories.Add(c);
                }

            }

            return u;
        }
    }


    //--------------------------------------------------------------------------------------------------
    public List<Category> getUserCategories(int id)
    {
        SqlConnection con = connect("GraspDBConnectionString"); // open the connection to the database/

        String selectStr = "Select CategoryName,CategoryID from userCategoriesView where UserID = " + id; // create the select that will be used by the adapter to select data from the DB

        SqlDataAdapter da = new SqlDataAdapter(selectStr, con); // create the data adapter

        DataSet ds = new DataSet("DS"); // create a DataSet and give it a name (not mandatory) as defualt it will be the same name as the DB

        da.Fill(ds);       // Fill the datatable (in the dataset), using the Select command

        dt = ds.Tables[0]; // point to the cars table , which is the only table in this case

        List<Category> lc = new List<Category>();

        foreach (DataRow item in dt.Rows)
        {
            Category c = new Category();
            c.CategoryId = int.Parse(item.ItemArray[1].ToString());
            c.Name = item.ItemArray[0].ToString();
            lc.Add(c);
        }


        return lc;
    }

    public List<string> getUserCategoriesByImei(string userId)
    {
        SqlConnection con = connect("GraspDBConnectionString"); // open the connection to the database/

        String selectStr = "Select CategoryName from userCategoriesView where UserID = " + userId + " and Active='true'"; // create the select that will be used by the adapter to select data from the DB

        SqlDataAdapter da = new SqlDataAdapter(selectStr, con); // create the data adapter

        DataSet ds = new DataSet("DS"); // create a DataSet and give it a name (not mandatory) as defualt it will be the same name as the DB

        da.Fill(ds);       // Fill the datatable (in the dataset), using the Select command

        dt = ds.Tables[0]; // point to the cars table , which is the only table in this case

        List<string> lc = new List<string>();

        foreach (DataRow item in dt.Rows)
        {
            lc.Add(item.ItemArray[0].ToString());
        }


        return lc;
    }

    //--------------------------------------------------------------------
    // Read from the DB into a table
    //--------------------------------------------------------------------
    public void readTable(string tableName)
    {

        SqlConnection con = connect("GraspDBConnectionString"); // open the connection to the database/

        String selectStr = "SELECT * FROM " + tableName; // create the select that will be used by the adapter to select data from the DB

        SqlDataAdapter da = new SqlDataAdapter(selectStr, con); // create the data adapter

        DataSet ds = new DataSet(tableName + "DS"); // create a DataSet and give it a name (not mandatory) as defualt it will be the same name as the DB

        da.Fill(ds, tableName);       // Fill the datatable (in the dataset), using the Select command

        dt = ds.Tables[tableName]; // point to the cars table , which is the only table in this case
    }

    //--------------------------------------------------------------------
    public int getUserIdByIMEI(string IMEI)
    {

        SqlConnection con = connect("GraspDBConnectionString"); // open the connection to the database/

        String selectStr = "SELECT userId FROM UsersP where IMEI ='" + IMEI + "'"; // create the select that will be used by the adapter to select data from the DB

        SqlDataAdapter da = new SqlDataAdapter(selectStr, con); // create the data adapter

        DataSet ds = new DataSet("DS"); // create a DataSet and give it a name (not mandatory) as defualt it will be the same name as the DB

        da.Fill(ds, "UserIDS");       // Fill the datatable (in the dataset), using the Select command

        dt = ds.Tables["UserIDS"]; // point to the cars table , which is the only table in this case

        return int.Parse(dt.Rows[0].ItemArray[0].ToString());
    }
}
