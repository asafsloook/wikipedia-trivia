using System;
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net;
using System.Xml.Linq;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Xml;
using System.Xml.Serialization;

public partial class _Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

        //FirstSentence("Mary Ball Washington");
        //OnlyIntro("Mary Ball Washington");
        //AllText("Kenneth Burke");

        RandomPageFromCategory("People");
        
        //GetInfoNearBy("31.771959", "35.217018", "1000");
        //GetInfoNearByWithImgs("32.4613", "35.0067", "100"); // "31.771959", "35.217018", "1000"

        //RandomPhotoOfTheDay();

        //--Beta--
        //MoreLike("Technology", "Tennis");

        //var a = GetViews("Fernando Pasarin");
        //var b = GetViews("Paris");
        //Response.Write(a + "<br/> <br/>" + b);
    }

    /// <summary>
    /// ////////////////////////////////////////////////////////////
    /// Main feature, random article from desired root category. 
    /// Important: recursing and may be slow sometimes. (10-20secs)
    /// ////////////////////////////////////////////////////////////
    /// </summary>
    /// 
    private void RandomPageFromCategory(string categoryTitle)
    {
        string ResponseURI;
        HttpWebRequest myRequest =
        (HttpWebRequest)WebRequest.Create("https://en.wikipedia.org/wiki/Special:RandomInCategory/" + categoryTitle);
        using (HttpWebResponse response = (HttpWebResponse)myRequest.GetResponse())
        {
            ResponseURI = response.ResponseUri.ToString();
        }
        ResponseURI = ResponseURI.Replace("https://en.wikipedia.org/wiki/", "");

        string ResponseText;
        myRequest =
        (HttpWebRequest)WebRequest.Create("https://en.wikipedia.org/w/api.php?action=query&prop=extracts&exintro=1&format=json&titles=" + ResponseURI);
        using (HttpWebResponse response = (HttpWebResponse)myRequest.GetResponse())
        {
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                ResponseText = reader.ReadToEnd();
            }
        }

        JObject root = JObject.Parse(ResponseText);

        var dig = root["query"]["pages"].First.First;

        var content = dig["extract"];
        var id = dig["pageid"];
        var title = dig["title"];

        if (content.ToString() == "" || title.ToString().StartsWith("Category") || title.ToString().StartsWith("List")) //((string)content).ToArray().Length < 100 ||
        {
            if (title.ToString().StartsWith("Category"))
            {
                title = title.ToString().Replace("Category:", "");
                RandomPageFromCategory(title.ToString());
                return;
            }

            if (title.ToString().StartsWith("List"))
            {
                RandomPageFromCategory("people");
                return;
            }

            RandomPageFromCategory(categoryTitle);
            return;
        }

        OnlyIntro(title.ToString());

        //var a = hasBorn(content.ToString());
        var birth_date = hasBirthDate(title.ToString());
        if (birth_date != null)
        {
            ph.Text += "<br/>BIRTHDAY:   " + birth_date.ToString().Substring(0, 11).Replace("+", "");
        }

        var death_date = hasDeathDate(title.ToString());
        if (death_date != null)
        {
            ph.Text += "<br/>DEATHDAY:   " + death_date.ToString().Substring(0, 11).Replace("+", "");
        }

        //ph.Text = "<h1>Title: " + title + "</h1>"
        //        + "<h1>ID: " + id + "</h1><br/>"
        //        + "<h3>Content :<h3><br/>" + content;
    }


    /// <summary>
    /// ////////////////////////////////////////////////////
    /// Return awesome picture of the day
    /// ////////////////////////////////////////////////////
    /// </summary>
    /// 
    private void RandomPhotoOfTheDay()
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

        ph.Text = x2.ElementAt(9).Value;
    }


    /// <summary>
    /// ////////////////////////////////////////////////////
    /// Get number of views of yesterday (today is not compatible)
    /// ////////////////////////////////////////////////////
    /// </summary>
    /// 
    private void GetInfoNearByWithImgs(string lat, string lng, string radius)
    {

        string ResponseText;
        HttpWebRequest myRequest =
        (HttpWebRequest)WebRequest.Create("https://en.wikipedia.org/w/api.php?action=query&format=json&prop=coordinates%7Cpageimages%7Cpageterms&colimit=50&piprop=thumbnail&pithumbsize=144&pilimit=50&wbptterms=description&generator=geosearch&ggscoord=" + lat + "%7C" + lng + "&ggsradius=" + radius + "&ggslimit=1");
        using (HttpWebResponse response = (HttpWebResponse)myRequest.GetResponse())
        {
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                ResponseText = reader.ReadToEnd();
            }
        }

        JObject root = JObject.Parse(ResponseText);

        try
        {
            var digTest = root["query"].First.First.First.First;
        }
        catch (Exception)
        {
            return;
        }

        var dig = root["query"].First.First.First.First;

        var articleID = dig["pageid"];
        var img = dig["thumbnail"]["source"];


        myRequest =
       (HttpWebRequest)WebRequest.Create("https://en.wikipedia.org/w/api.php?action=query&prop=extracts&exintro=1&format=json&pageids=" + articleID);
        using (HttpWebResponse response = (HttpWebResponse)myRequest.GetResponse())
        {
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                ResponseText = reader.ReadToEnd();
            }
        }

        root = JObject.Parse(ResponseText);

        var article = root["query"]["pages"].First.First;

        var content = article["extract"];
        var id = article["pageid"];
        var title = article["title"];

        ph.Text = "<h1>Title: " + title + "</h1>" + "<h1>ID: " + id + "</h1><br/> <img src='" + img + "'/> <br/>" + "<h3>Content :<h3><br/>" + content;
    }
    /// 
    private void GetInfoNearBy(string lat, string lng, string radius)
    {

        string ResponseText;
        HttpWebRequest myRequest =
        (HttpWebRequest)WebRequest.Create("https://en.wikipedia.org/w/api.php?action=query&format=json&list=geosearch&gscoord=" + lat + "%7C" + lng + "&gsradius=" + radius + "&gslimit=1");
        using (HttpWebResponse response = (HttpWebResponse)myRequest.GetResponse())
        {
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                ResponseText = reader.ReadToEnd();
            }
        }

        JObject root = JObject.Parse(ResponseText);
        var dig = new object();

        try
        {
            dig = root["query"]["geosearch"].First.First.First;
        }
        catch (Exception)
        {
            return;
        }

        var articleID = JsonConvert.SerializeObject(dig);


        myRequest =
       (HttpWebRequest)WebRequest.Create("https://en.wikipedia.org/w/api.php?action=query&prop=extracts&exintro=1&format=json&pageids=" + articleID);
        using (HttpWebResponse response = (HttpWebResponse)myRequest.GetResponse())
        {
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                ResponseText = reader.ReadToEnd();
            }
        }

        root = JObject.Parse(ResponseText);

        var article = root["query"]["pages"].First.First;

        var content = article["extract"];
        var id = article["pageid"];
        var title = article["title"];

        ph.Text = "<h1>Title: " + title + "</h1>" + "<h1>ID: " + id + "</h1><br/>" + "<h3>Content :<h3><br/>" + content;
    }


    /// <summary>
    /// ////////////////////////////////////////////////////
    /// Get number of views of yesterday (today is not compatible)
    /// ////////////////////////////////////////////////////
    /// </summary>
    /// 
    private int GetViews(string articleTitle)
    {
        string yearStr = DateTime.Now.Year.ToString();
        string dayStr = DateTime.Now.AddDays(-1).Day.ToString();

        int monthInt = DateTime.Now.Month;
        string monthStr = "";

        if (monthInt < 10)
        {
            monthStr = "0" + monthInt;
        }

        string timeStamp = yearStr + monthStr + dayStr;
        string url = "https://wikimedia.org/api/rest_v1/metrics/pageviews/per-article/en.wikipedia/all-access/all-agents/" + articleTitle + "/daily/" + timeStamp + "/" + timeStamp;

        string ResponseText;
        HttpWebRequest myRequest =
        (HttpWebRequest)WebRequest.Create(url);
        using (HttpWebResponse response = (HttpWebResponse)myRequest.GetResponse())
        {
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                ResponseText = reader.ReadToEnd();
            }
        }

        JObject root = JObject.Parse(ResponseText);

        var dig = root["items"].First;

        int views = int.Parse(dig["views"].ToString());

        return views;
    }

    
    /// <summary>
    /// ////////////////////////////////////////////////////
    /// Text extracts from wiki
    /// ////////////////////////////////////////////////////
    /// </summary>
    /// 
    private void FirstSentence(string articleTitle)
    {
        string ResponseText;
        HttpWebRequest myRequest =
        (HttpWebRequest)WebRequest.Create("https://www.wikidata.org/w/api.php?action=query&prop=extracts&exintro=true&exsentences=1&format=json&titles=" + articleTitle);
        using (HttpWebResponse response = (HttpWebResponse)myRequest.GetResponse())
        {
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                ResponseText = reader.ReadToEnd();
            }
        }

        JObject root = JObject.Parse(ResponseText);

        var article = root["query"]["pages"].First.First;

        var content = article["extract"];
        var id = article["pageid"];
        var title = article["title"];

        ph.Text = "<h1>Title: " + title + "</h1>" + "<h1>ID: " + id + "</h1><br/>" + "<h3>Content :<h3><br/>" + content;
    }
    /// 
    private void OnlyIntro(string articleTitle)
    {
        string ResponseText;
        HttpWebRequest myRequest =
        (HttpWebRequest)WebRequest.Create("https://en.wikipedia.org/w/api.php?action=query&prop=extracts&exintro=1&format=json&titles=" + articleTitle);
        using (HttpWebResponse response = (HttpWebResponse)myRequest.GetResponse())
        {
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                ResponseText = reader.ReadToEnd();
            }
        }

        JObject root = JObject.Parse(ResponseText);

        var article = root["query"]["pages"].First.First;

        var content = article["extract"];
        var id = article["pageid"];
        var title = article["title"];

        ph.Text = "<h1>Title: " + title + "</h1>" + "<h1>ID: " + id + "</h1><br/>" + "<h3>Content :<h3><br/>" + content;
    }
    /// 
    private void AllText(string articleTitle)
    {
        string ResponseText;
        HttpWebRequest myRequest =
        (HttpWebRequest)WebRequest.Create("https://en.wikipedia.org/w/api.php?action=query&prop=extracts&format=json&titles=" + articleTitle);
        using (HttpWebResponse response = (HttpWebResponse)myRequest.GetResponse())
        {
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                ResponseText = reader.ReadToEnd();
            }
        }

        JObject root = JObject.Parse(ResponseText);

        var article = root["query"]["pages"].First.First;

        var content = article["extract"];
        var id = article["pageid"];
        var title = article["title"];

        ph.Text = "<h1>Title: " + title + "</h1>" + "<h1>ID: " + id + "</h1><br/>" + "<h3>Content :<h3><br/>" + content;
    }

    
    /// <summary>
    /// ////////////////////////////////////////////////////
    /// Person events check
    /// ////////////////////////////////////////////////////
    /// </summary>
    /// 
    private string hasBirthDate(string articleTitle)
    {
        string ResponseText;
        HttpWebRequest myRequest =
        (HttpWebRequest)WebRequest.Create("https://www.wikidata.org/w/api.php?format=json&action=wbgetentities&sites=enwiki&props=claims&titles=" + articleTitle);
        using (HttpWebResponse response = (HttpWebResponse)myRequest.GetResponse())
        {
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                ResponseText = reader.ReadToEnd();
            }
        }

        string timeOfBirthStr="";

        try
        {
            JObject root = JObject.Parse(ResponseText);
            timeOfBirthStr = root["entities"].First.First["claims"]["P569"].First["mainsnak"]["datavalue"]["value"]["time"].ToString();

        }
        catch (Exception)
        {
            return null;
        }
        
        
        return timeOfBirthStr;
    }
    /// 
    private string hasDeathDate(string articleTitle)
    {
        string ResponseText;
        HttpWebRequest myRequest =
        (HttpWebRequest)WebRequest.Create("https://www.wikidata.org/w/api.php?format=json&action=wbgetentities&sites=enwiki&props=claims&titles=" + articleTitle);
        using (HttpWebResponse response = (HttpWebResponse)myRequest.GetResponse())
        {
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                ResponseText = reader.ReadToEnd();
            }
        }

        string timeOfDeathStr = "";

        try
        {
            JObject root = JObject.Parse(ResponseText);
            timeOfDeathStr = root["entities"].First.First["claims"]["P570"].First["mainsnak"]["datavalue"]["value"]["time"].ToString();

        }
        catch (Exception)
        {
            return null;
        }


        return timeOfDeathStr;
    }

    
    /// <summary>
    /// ////////////////////////////////////////////////////
    /// Beta, unknown behaviar
    /// ////////////////////////////////////////////////////
    /// </summary>
    /// 
    private void MoreLike(string var1, string var2)
    {
        string ResponseText;
        HttpWebRequest myRequest =
        (HttpWebRequest)WebRequest.Create("https://en.wikipedia.org/w/api.php?action=query&format=json&list=search&srsearch=morelike:" + var1 + "%7C" + var2 + "&srlimit=100&srprop=size&formatversion=2");
        using (HttpWebResponse response = (HttpWebResponse)myRequest.GetResponse())
        {
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                ResponseText = reader.ReadToEnd();
            }
        }

        JObject root = JObject.Parse(ResponseText);

        Random rnd = new Random();
        int randomNum = rnd.Next(0, 99);

        var article = root["query"]["search"].ElementAt(randomNum);

        var articleID = article["pageid"];

        myRequest =
       (HttpWebRequest)WebRequest.Create("https://en.wikipedia.org/w/api.php?action=query&prop=extracts&exintro=1&format=json&pageids=" + articleID);
        using (HttpWebResponse response = (HttpWebResponse)myRequest.GetResponse())
        {
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                ResponseText = reader.ReadToEnd();
            }
        }

        root = JObject.Parse(ResponseText);

        article = root["query"]["pages"].First.First;

        var content = article["extract"];
        var id = article["pageid"];
        var title = article["title"];

        ph.Text = "<h1>Title: " + title + "</h1>" + "<h1>ID: " + id + "</h1><br/>" + "<h3>Content :<h3><br/>" + content;

    }

    
    /// <summary>
    /// ////////////////////////////////////////////////////
    /// Functions tests, not for use
    /// ////////////////////////////////////////////////////
    /// </summary>
    /// 
    private string hasBorn(string content)
    {
        foreach (var item in content.Split())
        {
            if (item.StartsWith("born") || item.EndsWith("born") || item.Contains("born"))
            {
                if (GetYears(content) != null && GetYears(content) != "")
                {
                    return isYear(GetYears(content).Split())[0].ToString();
                }
            }
        }

        return null;
    }
    /// 
    private string GetYears(string content)
    {
        string[] sentenceArr = (content.ToString().Split());

        var yearArr = isYear(sentenceArr);

        string yearStr = "";

        if (yearArr.Count > 0)
        {
            foreach (var year in yearArr)
            {
                yearStr += "<br/>    " + year.ToString() + " ";
            }
        }

        return yearStr;
    }
    /// 
    private List<int> isYear(string[] sentenceArr)
    {
        List<int> YearArr = new List<int>();
        foreach (var word in sentenceArr)
        {
            try
            {
                var lettersArr = word.ToList();
                var counter = 0;
                var year = "";

                foreach (var letter in lettersArr)
                {
                    var checkNum = int.Parse(letter.ToString());
                    year += letter.ToString();
                    counter++;

                    if (counter == 4)
                    {
                        YearArr.Add(int.Parse(year));
                    }
                }
            }
            catch (Exception)
            {
                continue;
            }
        }
        return YearArr;
    }
    /// 
    private string checkContent(string content)
    {
        var updatedContent = content;
        int cutFrom = updatedContent.Length;

        foreach (var item in content.Split())
        {
            if (item.StartsWith("<h2>") || item.EndsWith("<h2>"))
            {
                cutFrom = updatedContent.IndexOf("<h2>");
                return updatedContent.Substring(0, cutFrom);
            }
        }
        return updatedContent.Substring(0, cutFrom);
    }

}