using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Reading
/// </summary>
public class Reading
{
    public Reading()
    {
        //
        // TODO: Add constructor logic here
        //
    }


    public int UserId { get; set; }

    public string ArticleId { get; set; }

    public string CategoryId { get; set; }

    public bool Favorite { get; set; }

    public TimeSpan ReadingDuration { get; set; }

    public DateTime ReadingTime { get; set; }

    public void insert(int userID, string articleId, DateTime date, string categoryname)
    {
        DBConnection db = new DBConnection();
        db.insertReading(userID, articleId, date, categoryname);
    }
}