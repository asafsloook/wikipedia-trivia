using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Notification
/// </summary>
public class Notification
{
    public Notification()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    string articleId;
    public string ArticleId { get; set; }

    string title;
    public string Title { get; set; }

    string description;
    public string Description { get; set; }

    User user;
    public User User{ get; set; }

    
}