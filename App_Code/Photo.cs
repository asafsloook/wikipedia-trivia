using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;

/// <summary>
/// Summary description for Photo
/// </summary>
public class Photo
{
    public Photo()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    string url;
    public string Url { get; set; }

    string title;
    public string Title { get; set; }

    string description;
    public string Description { get; set; }

    DateTime date;
    public DateTime Date { get; set; }



    /// <summary>
    /// Return awesome picture of the day
    /// </summary>
    /// 
    public List<Photo> AllPhotoOfTheDay()
    {
        string ResponseText;
        HttpWebRequest myRequest =
        (HttpWebRequest)WebRequest.Create("https://commons.wikimedia.org/w/api.php?format=json&action=featuredfeed&feed=potd");
        using (HttpWebResponse response = (HttpWebResponse)myRequest.GetResponse())
        {
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                ResponseText = reader.ReadToEnd();
            }
        }

        XDocument doc = XDocument.Parse(ResponseText);

        var x1 = doc.Root.FirstNode;

        var x2 = ((XContainer)x1).Elements("item");

        List<Photo> ls = new List<Photo>();

        foreach (var item in x2)
        {
            Photo p1 = new Photo();

            var b = item.Elements("pubDate").ElementAt(0).FirstNode;

            p1.date = DateTime.Parse(b.ToString());

            var html = item.Elements("description").ElementAt(0).Value;

            p1.url = Regex.Match(html, "<img.+?src=[\"'](.+?)[\"'].*?>", RegexOptions.IgnoreCase).Groups[1].Value;

            var text = Regex.Replace(html, @"<[^>]*>", String.Empty);

            p1.description = text.Replace("Picture of the day", "").Trim();
            
             ls.Add(p1);

        }

        return ls;
    }

}