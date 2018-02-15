using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for User
/// </summary>
public class User
{
    public User()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    int id;
    public int Id { get; set; }

    string imei;
    public string Imei { get; set; }

    string pushKey;
    public string PushKey { get; set; }

    bool locationPush;
    public bool LocationPush { get; set; }

    bool photoPush;
    public bool PhotoPush { get; set; }

    DateTime photoPushTime;
    public DateTime PhotoPushTime { get; set; }

    bool articlePush;
    public bool ArticlePush { get; set; }

    int articlesPerDay;
    public int ArticlesPerDay { get; set; }

    Category[] categories;
    public Category[] Categories { get; set; }
}