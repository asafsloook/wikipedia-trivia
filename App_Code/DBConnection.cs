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
    public int insert(User user)
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

        string cStr = "insert into UsersP (IMEI, pushKey, LocationPush, PhotoPush, PhotoPushTime, RandomArticlePush, RandomArticleQuantity)";
        cStr += string.Format("values('{0}','{1}','{2}','{3}','{4}','{5}',{6})", user.Imei, user.PushKey, user.LocationPush.ToString(), user.PhotoPush.ToString(), user.PhotoPushTime.ToShortTimeString(), user.ArticlePush.ToString(), user.ArticlesPerDay);

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
        string selectStr2 = "Select CategoryName,CategoryID from userCategoriesView where UserID =" + u.Id;
        SqlDataAdapter da2 = new SqlDataAdapter(selectStr2, con);
        da2.Fill(ds, "userPref1");
        dt = ds.Tables["userPref1"];

       // int i = 0;
        foreach (DataRow item in dt.Rows)
        {
            Category c = new Category();
            c.Name = item.ItemArray[0].ToString();
            c.CategoryId = int.Parse(item.ItemArray[1].ToString());
            u.Categories.Add(c);
         //   i++;
        }

        return u;
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


}
