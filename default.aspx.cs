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
        //OnlyIntro("Ariel Sharon");
        //AllText("Kenneth Burke");

        RandomPageFromCategory("Animals‎", "Animals");

        //GetInfoNearBy("31.771959", "35.217018", "1000");
        //GetInfoNearByWithImgs("32.4613", "35.0067", "100"); // "31.771959", "35.217018", "1000"

        //RandomPhotoOfTheDay();

        //var a = GetViews("Gymnastics at the 1961 Summer Universiade");
        //var b = GetViews("Front of Shamyl");
        //Response.Write(a + "<br/> <br/>" + b);

        //--Beta--
        //MoreLike("Technology", "Tennis");

        //getMainCategories();
    }


    /// <summary>
    /// Get the main/root categories from wikipedia
    /// </summary>
    /// 
    private List<string> getMainCategories()
    {
        string ResponseText;
        HttpWebRequest myRequest =
        (HttpWebRequest)WebRequest.Create("https://en.wikipedia.org/w/api.php?format=json&action=query&list=categorymembers&cmtitle=Category:Main_topic_classifications&cmlimit=100");
        using (HttpWebResponse response = (HttpWebResponse)myRequest.GetResponse())
        {
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                ResponseText = reader.ReadToEnd();
            }
        }

        JObject root = JObject.Parse(ResponseText);
        var dig = root["query"]["categorymembers"];

        List<string> mainCategories = new List<string>();

        foreach (var item in dig)
        {
            mainCategories.Add(item["title"].ToString().Replace("Category:", ""));
        }

        return mainCategories;
    }


    /// <summary>
    /// Main feature, random article from desired root category. 
    /// Important: recursing and may be slow sometimes. (10-20secs)
    /// </summary>
    /// 
    private void RandomPageFromCategory(string categoryTitle, string rootCategoryTitle)
    {
        string ResponseURI;
        HttpWebRequest myRequest =
        (HttpWebRequest)WebRequest.Create("https://en.wikipedia.org/wiki/Special:RandomInCategory/" + categoryTitle);
        using (HttpWebResponse response = (HttpWebResponse)myRequest.GetResponse())
        {
            ResponseURI = response.ResponseUri.ToString();
        }
        string articleTitle = ResponseURI.Replace("https://en.wikipedia.org/wiki/", "");

        string ResponseText;
        myRequest =
        (HttpWebRequest)WebRequest.Create("https://en.wikipedia.org/w/api.php?action=query&prop=extracts&exintro=1&format=json&titles=" + articleTitle);
        using (HttpWebResponse response = (HttpWebResponse)myRequest.GetResponse())
        {
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                ResponseText = reader.ReadToEnd();
            }
        }

        JObject root = JObject.Parse(ResponseText);

        var dig = root["query"]["pages"].First.First;

        var content = dig["extract"].ToString();
        var id = dig["pageid"].ToString();
        var title = dig["title"].ToString();

        if (content == null || content == "" || title.StartsWith("Category") || title.StartsWith("List") || title.StartsWith("Portal") || title.StartsWith("Index") || title.StartsWith("Template")) //((string)content).ToArray().Length < 100 ||
        {
            if (title.StartsWith("Category"))
            {
                title = title.Replace("Category:", "");
                RandomPageFromCategory(title, rootCategoryTitle);
                return;
            }

            //if (title.StartsWith("List"))
            //{
            //    RandomPageFromCategory(rootCategoryTitle, rootCategoryTitle);
            //    return;
            //}

            RandomPageFromCategory(rootCategoryTitle, rootCategoryTitle);
            return;
        }

        //var views = GetViews(title);
        //if (views == -1 || views < 10)
        //{
        //    RandomPageFromCategory(rootCategoryTitle, rootCategoryTitle);
        //    return;
        //}

        OnlyIntro(title);

        ph.Text += "<br/> Description: " + hasDescription(title) + "<br/>";

        if (isPerson(title))
        {
            ph.Text += "<br/> isPerson:true <br/>";

            var birth_date = hasBirthDate(title);
            if (birth_date != null)
            {
                ph.Text += "<br/>Who is the *BD*:" + birth_date.Substring(0, 11).Replace("+", "");
            }

            var death_date = hasDeathDate(title);
            if (death_date != null)
            {
                ph.Text += "<br/>Who is the *DD*:   " + death_date.Substring(0, 11).Replace("+", "");
            }

            var cause_death = hasCauseOfDeath(title);
            if (cause_death != null)
            {
                ph.Text += "<br/>Who is the *COH*:   " + cause_death;
            }
        }

        if (isAnimal(title) != null)
        {
            ph.Text += "<br/>Do you know that animal?:" + isAnimal(title);
        }

        //ph.Text = "<h1>Title: " + title + "</h1>"
        //        + "<h1>ID: " + id + "</h1><br/>"
        //        + "<h3>Content :<h3><br/>" + content;
    }


    /// <summary>
    /// Return awesome picture of the day
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
    /// Geolocation based queries
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
    /// Get number of views of yesterday (today is not compatible)
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

        try
        {

            HttpWebRequest myRequest =
            (HttpWebRequest)WebRequest.Create(url);
            using (HttpWebResponse response = (HttpWebResponse)myRequest.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    ResponseText = reader.ReadToEnd();
                }
            }
        }
        catch (Exception)
        {
            return -1;
        }


        JObject root = JObject.Parse(ResponseText);

        var dig = root["items"].First;

        int views = int.Parse(dig["views"].ToString());

        return views;
    }


    /// <summary>
    /// Text extracts from wiki
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
    /// Person functions
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

        string timeOfBirthStr = "";

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
    ///
    private string hasCauseOfDeath(string articleTitle)
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

        string propID = "";

        try
        {
            JObject root = JObject.Parse(ResponseText);
            propID = root["entities"].First.First["claims"]["P509"].First["mainsnak"]["datavalue"]["value"]["id"].ToString();
        }
        catch (Exception)
        {
            return null;
        }

        var propVal = propertyValue(propID);

        return propVal;
    }
    ///
    private bool isPerson(string articleTitle)
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

        string propVal = "";

        try
        {
            JObject root = JObject.Parse(ResponseText);
            propVal = root["entities"].First.First["claims"]["P31"].First["mainsnak"]["datavalue"]["value"]["id"].ToString();
            if (propVal == "Q5")
            {
                return true;
            }
        }
        catch (Exception)
        {
            return false;
        }
        return false;
    }



    /// <summary>
    /// If the article is about animel, function returns emoji of the animal
    /// </summary>
    /// 
    private string isAnimal(string articleTitle)
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

        string propVal = "";

        try
        {
            JObject root = JObject.Parse(ResponseText);
            propVal = root["entities"].First.First["claims"]["P1417"].First["mainsnak"]["datavalue"]["value"].ToString();
            if (propVal.StartsWith("animal"))
            {
                propVal = root["entities"].First.First["claims"]["P487"].First["mainsnak"]["datavalue"]["value"].ToString();
                return propVal;
            }
        }
        catch (Exception)
        {
            return null;
        }
        return null;
    }


    /// <summary>
    /// get article short description for notification
    /// </summary>
    /// 
    private string hasDescription(string articleTitle)
    {
        string ResponseText;
        HttpWebRequest myRequest =
        (HttpWebRequest)WebRequest.Create("https://www.wikidata.org/w/api.php?format=json&action=wbgetentities&sites=enwiki&props=descriptions&titles=" + articleTitle);
        using (HttpWebResponse response = (HttpWebResponse)myRequest.GetResponse())
        {
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                ResponseText = reader.ReadToEnd();
            }
        }

        try
        {
            JObject root = JObject.Parse(ResponseText);

            var dig = root["entities"].First.First["descriptions"]["en"]["value"];

            return dig.ToString();
        }
        catch (Exception)
        {
            return null;
        }
    }


    /// <summary>
    /// extract property description
    /// </summary>
    /// 
    private string propertyValue(string propertyID)
    {
        string ResponseText;
        HttpWebRequest myRequest =
        (HttpWebRequest)WebRequest.Create("https://www.wikidata.org/w/api.php?format=json&action=wbgetentities&ids=" + propertyID);
        using (HttpWebResponse response = (HttpWebResponse)myRequest.GetResponse())
        {
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                ResponseText = reader.ReadToEnd();
            }
        }

        JObject root = JObject.Parse(ResponseText);


        var dig = root["entities"].First.First["labels"]["en"];

        //random alias of the property
        ////////////////////////////////////////
        //List<string> ls = new List<string>();
        //for (int i = 0; i < dig.Count(); i++)
        //{
        //    if (dig[i]["value"].ToString().Length > 5)
        //    {
        //        ls.Add(dig[i]["value"].ToString());
        //    }
        //}
        //ls.Sort((x, y) => x.Length.CompareTo(y.Length));
        //ls.ElementAt((ls.Count / 2)).ToString()
        //Random rnd = new Random();
        //int randomNum = rnd.Next(0, ls.Count());
        //return ls.ElementAt(randomNum).ToString();

        return dig["value"].ToString();
    }


    /// <summary>
    /// Beta, unknown behaviar
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
    /// Functions tests, not for use
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