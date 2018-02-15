using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.IO;
using System.Web.UI.WebControls;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net;
using System.Xml.Linq;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Web.Script.Services;

/// <summary>
/// Summary description for WebService
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[System.Web.Script.Services.ScriptService]
public class WebService : System.Web.Services.WebService
{

    public WebService()
    {

        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string HelloWorld()
    {

        
        Class1 c1 = new Class1();
        var a = c1.RandomPageFromCategory("Sports", "Sports");

        a = a.Replace("  " ," ");

        return a;
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

        //fix for first sentence wiki api function
        //articleTitle = articleTitle.Replace("%C3%A9", "é"); 
        //articleTitle = articleTitle.Replace("%E2%80%93", "-");

        string ResponseText;
        HttpWebRequest myRequest2 =
        (HttpWebRequest)WebRequest.Create("https://en.wikipedia.org/w/api.php?action=query&prop=extracts&exintro=&format=json&titles=" + articleTitle);
        using (HttpWebResponse response = (HttpWebResponse)myRequest2.GetResponse())
        {
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                ResponseText = reader.ReadToEnd();
            }
        }

        JObject root = JObject.Parse(ResponseText);

        var dig = root["query"]["pages"].First.First;

        //if (dig["extract"] == null)
        //{
        //    RandomPageFromCategory(rootCategoryTitle, rootCategoryTitle);
        //    return;
        //}

        string content;
        string id;
        string title;

        try
        {
            content = dig["extract"].ToString();
            id = dig["pageid"].ToString();
            title = dig["title"].ToString();
        }
        catch (Exception)
        {
            RandomPageFromCategory(rootCategoryTitle, rootCategoryTitle);
            return;
        }

        title = title.Replace(' ', '_');

        if (content == null || content == "" || title.StartsWith("Category") || content.StartsWith("<p>This") || content.StartsWith("<p>The following") || title.StartsWith("List") || title.StartsWith("Portal") || title.StartsWith("Index") || title.StartsWith("Template") || title.StartsWith("Timeline") || title.StartsWith("Book:") || title.StartsWith("Draft:") || content.Length < 50)
        {
            if (title.StartsWith("Category"))
            {
                title = title.Replace("Category:", "");
                RandomPageFromCategory(title, rootCategoryTitle);
                return;
            }

            RandomPageFromCategory(rootCategoryTitle, rootCategoryTitle);
            return;
        }


        //cheack for issues with article   
        string ResponseText5;
        HttpWebRequest myRequest5 =
        (HttpWebRequest)WebRequest.Create("https://en.wikipedia.org/w/api.php?action=query&prop=revisions&rvprop=content&format=json&titles=" + title);
        using (HttpWebResponse response5 = (HttpWebResponse)myRequest5.GetResponse())
        {
            using (StreamReader reader5 = new StreamReader(response5.GetResponseStream()))
            {
                ResponseText5 = reader5.ReadToEnd();
            }
        }

        JObject root2 = JObject.Parse(ResponseText5);
        JToken dig2 = null;

        try
        {
            dig2 = root2["query"]["pages"].First.First["revisions"].First["*"];

            var hasIssues = dig2.ToString().Substring(0, 100).Contains("issues");

            if (hasIssues)
            {
                RandomPageFromCategory(rootCategoryTitle, rootCategoryTitle);
                return;
            }
        }
        catch (Exception)
        {

        }

        var views = GetViews(title);
        if (views == -1 || views < 100)
        {
            RandomPageFromCategory(rootCategoryTitle, rootCategoryTitle);
            return;
        }

        if (content.Contains("(<span></span>)"))
        {
            content = content.Replace("(<span></span>)", "");
        }

        if (content.Contains("</dl>"))
        {
            content = content.Substring(content.IndexOf("<p>"));
        }

        //printing the title
        //ph.Text += "title:" + title.ToString() + "<br/><br/>";

        //wikipedia 

        if (content.Contains("<math"))
        {
            //cdn script for translate math formualas to img
            //ph.Text += "<script src='https://cdnjs.cloudflare.com/ajax/libs/mathjax/2.7.2/MathJax.js?config=TeX-MML-AM_CHTML'></script>";
        }

        //testing printing
        //ph.Text += content + "<br/>" + "<br/>";
        //ph.Text += "FirstSentence: " + FirstSentence(title) + "<br/>" + "<br/>";
        //ph.Text += "<br/><br/>Root Category: " + rootCategoryTitle + "<br/>" + "<br/>";


        if (content == title + " may refer to:")
        {
            RandomPageFromCategory(rootCategoryTitle, rootCategoryTitle);
            return;
        }


        #region wikidata
        // var DESCquestion = "";
        //if (hasDescription(title) != null)
        //{
        //    var desc = hasDescription(title);
        //    if (desc.Contains("Wikipedia disambiguation page") || desc.Contains("Wikimedia disambiguation page") || content.Substring(0, 25).Contains("list") || desc.Contains("list") || desc.ToLower().StartsWith("of "))
        //    {
        //        //RandomPageFromCategory(rootCategoryTitle, rootCategoryTitle);
        //        //return;
        //    }

        //    if (desc.ToLower().StartsWith("the ") || desc.ToLower().StartsWith("a ") || desc.ToLower().StartsWith("an "))
        //    {
        //        DESCquestion = "<br/><br/> DESC-Q: Do you know what is " + desc + "?<br/>";
        //    }

        //    else
        //    {
        //        var x = startWithVowel(desc);

        //        DESCquestion = "<br/><br/> DESC-Q: Do you know what is " + x + desc + "?<br/>";
        //    }

        //}
        //ph.Text += DESCquestion;
        #endregion

        string notification = renderNotification(content, title, rootCategoryTitle);
        #region isPerson, isAnimal, isEvent tests
        //if (isPerson(title))
        //{
        //    ph.Text += "<br/><br/> isPerson:true <br/>";

        //    var birth_date = hasBirthDate(title);
        //    if (birth_date != null)
        //    {
        //        ph.Text += "<br/>*BD*:" + birth_date.Substring(0, 11).Replace("+", "");
        //    }

        //    var death_date = hasDeathDate(title);
        //    if (death_date != null)
        //    {
        //        ph.Text += "<br/>*DD*:   " + death_date.Substring(0, 11).Replace("+", "");
        //    }

        //    var cause_death = hasCauseOfDeath(title);
        //    if (cause_death != null)
        //    {
        //        ph.Text += "<br/>*COD*:   " + cause_death;
        //    }

        //    //var x = startWithVowel(desc2);
        //    //personQuestion = "<br/><br/> Q: Do you know who is " + x + desc2 + "?<br/>";
        //}

        //if (isAnimal(title) != null)
        //{
        //    ph.Text += "<br/><br/>Q: Do you know that animal?: " + isAnimal(title);
        //}

        //ph.Text += personQuestion;


        //Unstable// Not for use
        //if (isEvent(title))
        //{
        //    ph.Text += "isEvent: true";
        //}
        #endregion

        //ph.Text = "<h1>Title: " + title + "</h1>"
        //        + "<h1>ID: " + id + "</h1><br/>"
        //        + "<h3>Content :<h3><br/>" + content;
    }
    
    private string renderNotification(string content, string title, string rootCategoryTitle)
    {
        var isQuestion = "";

        if (content.Contains("<p><span></span></p>"))
        {
            content = content.Replace("<p><span></span></p>", "");
        }
        content = content.Replace("  ", " ");
        content = content.Replace(" . ", ". ");

        //content = content.Replace(".<", ". <"); //(<([^>]+)>)


        content = content.Substring(0, content.IndexOf("</p>"));



        //if (qContent.Length < 100)
        //{
        //    qContent = content.Substring(content.IndexOf("</p>"));
        //}


        content = Regex.Replace(content, @"<[^>]*>", String.Empty);

        //content = Regex.Replace(content, @"\n", " ");


        if (content.StartsWith("There") || content.StartsWith("Among"))
        {
            isQuestion = "sentence";
        }

        if (content.StartsWith("The"))
        {
            if (content.ToLower().StartsWith("the " + title.ToLower().Replace("_", " ")))
            {

            }
            else if (content.ToLower().StartsWith(title.ToLower().Replace("_", " ")))
            {

            }
            else
            {
                isQuestion = "sentence";
            }
        }

        content = content.Replace("&amp;", "&");

        string qContent = content;

        //if (!qContent.StartsWith("The") || !qContent.StartsWith(title))
        //{
        //    ph.Text += "NOT A QUESTION";
        //}

        var startStr = firstOccurence(qContent, new List<string> { " concerns ", " gained ", " occurs ", " as it is ", " were ", " crashed ", " is ", " involved ", " refer(s) ", " establishes ", " gives ", " states ", " premiered ", " began ", " represent ", " are ", " was ", " started ", " appears ", " became ", " contains ", " encompasses ", " attracted ", " accounts ", " presents ", " involves ", " shows ", " describes ", " consists ", " refer ", " refers ", " has ", " have ", " had ", " provides ", " exist ", " exists ", " includes ", " include ", " included " });

        int startIndex = qContent.IndexOf(startStr);

        if (startStr == " were " || startStr == " as it is ")
        {
            isQuestion = "sentence";
        }

        string tempContent = qContent.Substring(startIndex);


        var qRegex = FirstSentenceByRegex(tempContent);

        tempContent = qRegex[0];


        //"((^.*?[a-z,0-9,A-Z,\",.)]{2,}[.])\\s+\\W*[A-Z,\"])|((^.*?[a-z,0-9,A-Z,\",)]{2,}[;])\\s+\\W*[A-Z,a-z,\"])|((^.*?[a-z,0-9,A-Z,)]{2,}[.\"]{2,})\\s+\\W*[A-Z,\"])");       //   /.*?[.;](?= +[A-Z,a-z,\"]|$)      (^.*?[a-z]{2,}[.;])\\s+\\W*[A-Z,\"]



        //tempContent = firstSentence.Substring(0, firstSentence.Length-1);

        //Regex rx = new Regex(@"(\S.+?[.!?])(?=\s+|$)");

        //var sentences = rx.Matches(tempContent);

        //tempContent = sentences[0].Value;


        var endStr = firstOccurence(tempContent, new List<string> { " which has, ", ", though ", " in order to ", ", the most ", ", e.g. ", " in which ", ", whether ", ", consistent with ", ", meaning ", ", originally ", ", in other words ", ", either ", ", including ", ", especially ", ", usually ", ", typically ", ", often ", ", such as ", ", particularly ", " and in which ", ", which ", " which, ", ", in which", ", and in particular:" }); // " whose "   " such as "

        int endIndex = tempContent.IndexOf(endStr);

        if (endIndex != -1) //&& endIndex > 35) ///length
        {
            tempContent = tempContent.Substring(0, endIndex);
        }


        //if (tempContent.Length > 100)
        //{
        //     endStr = firstOccurence(tempContent, new List<string> { " which " , " regarding " }); //, " and "  ,  " that "

        //    endIndex = tempContent.LastIndexOf(endStr);

        //    if (endIndex != -1)
        //    {
        //        tempContent = tempContent.Substring(0, endIndex);
        //    }
        //}

        List<string> wList = new List<string>() { " include ", " represent ", " refer ", " exist " };

        foreach (var item in wList)
        {
            if (tempContent.StartsWith(item))
            {
                tempContent = tempContent.Replace(item, item.TrimEnd(' ') + "s ");
            }
            else if (tempContent.StartsWith(" refer(s) "))
            {
                tempContent = tempContent.Replace(" refer(s) ", " refers ");
            }
        }


        string qWord = "";
        string born = "";
        if (isPerson(title))
        {
            qWord = " Who ";
            born = hasBirthDate(title);

            Regex rxYear = new Regex(@"\b\d{4}\b");

            var year = rxYear.Matches(tempContent);

            string yearStr = "";

            try
            {
                yearStr = year[0].Value;
            }
            catch (Exception)
            {

            }


            if (born != null && yearStr == "" && tempContent.Length < 100)
            {
                born = " that was born in " + born.Substring(0, 5).Replace("+", "");
            }
            else
            {
                born = "";
            }
        }
        else
        {
            qWord = " What ";

        }


        if (qRegex[1] == ".\"")
        {
            tempContent = tempContent + "\"";
        }


        //
        Regex rxParenthesis = new Regex(@"(?<=\()(?:[^()]+|\([^)]+\))+(?=\))");

        var parenthesis = rxParenthesis.Matches(tempContent);

        string parenthesisStr = "";

        try
        {
            for (int i = 0; i < parenthesis.Count; i++)
            {
                parenthesisStr = parenthesis[i].Value;
                tempContent = tempContent.Remove(tempContent.IndexOf(parenthesisStr), parenthesisStr.Length);
            }

        }
        catch (Exception)
        {

        }



        parenthesis = rxParenthesis.Matches(content);
        parenthesisStr = "";

        try
        {
            for (int i = 0; i < parenthesis.Count; i++)
            {
                parenthesisStr = parenthesis[i].Value;
                content = content.Remove(content.IndexOf(parenthesisStr), parenthesisStr.Length);
            }

        }
        catch (Exception)
        {

        }




        while (tempContent.IndexOf("(") != -1)
        {
            tempContent = tempContent.Remove(tempContent.IndexOf("(") - 1, (tempContent.IndexOf(")")) - (tempContent.IndexOf("(") - 2));
        }


        while (content.IndexOf("(") != -1)
        {
            content = content.Remove(content.IndexOf("(") - 1, (content.IndexOf(")")) - (content.IndexOf("(") - 2));
        }


        if (isQuestion != "sentence")
        {
            isQuestion = qWord + tempContent + born + "?";

            if (isQuestion.Length > 150)
            {

            }
            else
            {
                return isQuestion;
            }

        }

        var ls = FirstSentenceByRegex(content);

        string sentence = "";

        if (ls[1] == ".\"")
        {
            sentence = ls[0] + "\".";
        }
        else if (ls[1] == ";" || ls[1] == ".")
        {
            sentence = ls[0] + ".";
        }

        return sentence;



    }

    private List<string> FirstSentenceByRegex(string tempContent)
    {
        List<string> endSentenceMarkStr = new List<string>() { ".", ".\"", ";" };

        List<int> endSentenceMarkLen = new List<int>();

        foreach (var item in endSentenceMarkStr)
        {
            //if (tempContent.IndexOf(item) != -1)
            //{
            if (item == ".")
            {
                Regex rx1 = new Regex("((^.*?[a-z,0-9,A-Z,\")]{2,}[.])\\s+\\W*[A-Z,\"])");

                var sentences1 = rx1.Matches(tempContent);

                string firstSentence1;

                try
                {
                    firstSentence1 = sentences1[0].Value;
                }
                catch (Exception)
                {
                    firstSentence1 = tempContent;
                }
                endSentenceMarkLen.Add(firstSentence1.Length);


                rx1 = new Regex("((^.*?[0-9)]{1,}[.])\\s+\\W*[A-Z,\"])");

                sentences1 = rx1.Matches(firstSentence1);

                try
                {
                    firstSentence1 = sentences1[0].Value;
                }
                catch (Exception)
                {
                    //firstSentence1 = tempContent;
                }
                endSentenceMarkLen[0] = (firstSentence1.Length);

            }
            else if (item == ".\"")
            {
                Regex rx1 = new Regex("((^.*?[a-z,0-9,A-Z,)]{2,}[.\"]{2,})\\s+\\W*[A-Z,\"])");

                var sentences1 = rx1.Matches(tempContent);

                string firstSentence1;

                try
                {
                    firstSentence1 = sentences1[0].Value;
                }
                catch (Exception)
                {
                    firstSentence1 = tempContent;
                }
                endSentenceMarkLen.Add(firstSentence1.Length);


                rx1 = new Regex("((^.*?[0-9)]{1,}[.])\\s+\\W*[A-Z,\"])");

                sentences1 = rx1.Matches(firstSentence1);

                try
                {
                    firstSentence1 = sentences1[0].Value;
                }
                catch (Exception)
                {
                    //firstSentence1 = tempContent;
                }
                endSentenceMarkLen[1] = (firstSentence1.Length);

            }
            else if (item == ";")
            {
                Regex rx1 = new Regex("((^.*?[a-z,0-9,A-Z,\",)]{2,}[;])\\s+\\W*[A-Z,a-z,\"])");

                var sentences1 = rx1.Matches(tempContent);

                string firstSentence1;

                try
                {
                    firstSentence1 = sentences1[0].Value;
                }
                catch (Exception)
                {
                    firstSentence1 = tempContent;
                }
                endSentenceMarkLen.Add(firstSentence1.Length);


                rx1 = new Regex("((^.*?[0-9)]{1,}[.])\\s+\\W*[A-Z,\"])");

                sentences1 = rx1.Matches(firstSentence1);

                try
                {
                    firstSentence1 = sentences1[0].Value;
                }
                catch (Exception)
                {
                    //firstSentence1 = tempContent;
                }
                endSentenceMarkLen[2] = (firstSentence1.Length);
            }
            //}
        }

        tempContent = tempContent.Substring(0, endSentenceMarkLen.Min());

        var tempInt = endSentenceMarkLen.IndexOf(endSentenceMarkLen.Min());

        try
        {
            tempContent = tempContent.Substring(0, tempContent.LastIndexOf(endSentenceMarkStr[tempInt]));
        }
        catch (Exception)
        {
            tempContent = tempContent.Substring(0, tempContent.Length - 1);
        }

        List<string> ls = new List<string>() { tempContent, endSentenceMarkStr[tempInt] };

        return ls;
    }

    //Not for use, sababa?
    private string finalOccurence(string content, List<string> qList, int startIndex)
    {
        content = content.Substring(startIndex);

        List<int> iqList = new List<int>();

        for (int i = 0; i < qList.Count; i++)
        {
            if (content.IndexOf(qList[i]) == -1)
            {
                iqList.Add(999999);
            }
            else
            {
                if (content.IndexOf(qList[i]) < startIndex)
                {
                    while (content.IndexOf(qList[i]) < startIndex)
                    {
                        content = content.Substring(content.IndexOf(qList[i]) + 1);
                    }
                    iqList.Add(content.IndexOf(qList[i]));
                }
                else
                {
                    if (content.IndexOf("(") < content.IndexOf(")") && content.IndexOf(qList[i]) < content.IndexOf(")") && content.IndexOf(qList[i]) > content.IndexOf("(") && content.IndexOf("(") != -1 && content.IndexOf(")") != -1)
                    {
                        iqList.Add(999999);
                    }
                    else
                    {
                        iqList.Add(content.IndexOf(qList[i]));
                    }
                }

            }
        }

        var a = qList[iqList.IndexOf(iqList.Min())];

        return a;
    }

    private string firstOccurence(string content, List<string> qList)
    {
        List<int> iqList = new List<int>();

        for (int i = 0; i < qList.Count; i++)
        {
            //if (qList[i] == " as it is " && content.IndexOf(qList[i]) != -1)
            //{
            //    //content = content.Substring(content.IndexOf(qList[i]) + qList[i].Length);
            //    iqList.Add(999999);
            //    continue;
            //}
            if (content.IndexOf(qList[i]) == -1)
            {
                iqList.Add(999999);
            }
            else
            {
                if (content.IndexOf("(") < content.IndexOf(")") && content.IndexOf(qList[i]) < content.IndexOf(")") && content.IndexOf(qList[i]) > content.IndexOf("(") && content.IndexOf("(") != -1 && content.IndexOf(")") != -1)
                {
                    iqList.Add(999999);
                }
                else
                {
                    iqList.Add(content.IndexOf(qList[i]));
                }
            }
        }

        var a = qList[iqList.IndexOf(iqList.Min())];

        return a;
    }

    private string startWithVowel(string desc)
    {
        List<string> vowels = new List<string> { "a", "e", "i", "o", "u" };

        var x = "a ";

        foreach (var item in vowels)
        {
            if (desc.ToLower().StartsWith(item))
            {
                x = "an ";
            }
        }

        return x;
    }


    /// <summary>
    /// Return awesome picture of the day
    /// </summary>
    /// 
    private string RandomPhotoOfTheDay()
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

        return x2.ElementAt(9).Value;
    }


    /// <summary>
    /// Geolocation based queries
    /// </summary>
    /// 
    private string GetInfoNearByWithImgs(string lat, string lng, string radius)
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
            return "";
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

        return "<h1>Title: " + title + "</h1>" + "<h1>ID: " + id + "</h1><br/> <img src='" + img + "'/> <br/>" + "<h3>Content :<h3><br/>" + content;
    }
    /// 
    private string GetInfoNearBy(string lat, string lng, string radius)
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
            return "";
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

        return "<h1>Title: " + title + "</h1>" + "<h1>ID: " + id + "</h1><br/>" + "<h3>Content :<h3><br/>" + content;
    }


    /// <summary>
    /// Get number of views of yesterday (today is not compatible)
    /// </summary>
    /// 
    private int GetViews(string articleTitle)
    {
        var dateYesterday = DateTime.Now.AddDays(-1);

        string yearStr = dateYesterday.Year.ToString();
        int dayInt = dateYesterday.Day;
        int monthInt = dateYesterday.Month;

        string monthStr = "";
        string dayStr = "";

        if (dayInt < 10)
        {
            dayStr = "0" + dayInt;
        }
        else
        {
            dayStr = dayInt.ToString();
        }

        if (monthInt < 10)
        {
            monthStr = "0" + monthInt;
        }
        else
        {
            monthStr = monthInt.ToString();

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
    private string FirstSentence(string articleTitle)
    {
        string ResponseText;
        HttpWebRequest myRequest =
        (HttpWebRequest)WebRequest.Create("https://www.wikipedia.org/w/api.php?action=query&prop=extracts&exintro=true&exsentences=1&format=json&titles=" + articleTitle);
        using (HttpWebResponse response = (HttpWebResponse)myRequest.GetResponse())
        {
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                ResponseText = reader.ReadToEnd();
            }
        }

        JObject root = JObject.Parse(ResponseText);

        var article = root["query"]["pages"].First.First;

        var content = article["extract"].ToString();
        //var id = article["pageid"];
        //var title = article["title"];

        return content;
    }
    /// 
    private string OnlyIntro(string articleTitle)
    {
        string ResponseText;
        HttpWebRequest myRequest =
        (HttpWebRequest)WebRequest.Create("https://en.wikipedia.org/w/api.php?action=query&prop=extracts&exintro=1&format=json&titles=" + articleTitle); //&explaintext= for clean text
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

        return content.ToString();
    }
    /// 
    private string AllText(string articleTitle)
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

        return "<h1>Title: " + title + "</h1>" + "<h1>ID: " + id + "</h1><br/>" + "<h3>Content :<h3><br/>" + content;
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
            if (propVal == "Q5")  // Q5 = wikidata id for human
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
    /// Events functions//warning// not for use
    /// </summary>
    ///
    private bool isEvent(string articleTitle)
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
        JObject root = new JObject();
        string propID = "P279";

        while (propVal == "")
        {
            try
            {
                root = JObject.Parse(ResponseText);
                propVal = root["entities"].First.First["claims"][propID].First["mainsnak"]["datavalue"]["value"]["id"].ToString();

                if (propVal == "Q1190554")
                {
                    return true;
                }
            }
            catch (Exception)
            {
                if (propID == "P31")
                {
                    return false;
                }
                propID = "P31";
            }
        }

        root = new JObject();
        int count = 0;
        propID = "P279";
        while (propVal != "Q1190554") // Q1656682 = event id in wikidata
        {

            myRequest =
        (HttpWebRequest)WebRequest.Create("https://www.wikidata.org/w/api.php?format=json&action=wbgetentities&sites=enwiki&props=claims&ids=" + propVal);
            using (HttpWebResponse response = (HttpWebResponse)myRequest.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    ResponseText = reader.ReadToEnd();
                }
            }

            try
            {
                root = JObject.Parse(ResponseText);
                propVal = root["entities"].First.First["claims"][propID].First["mainsnak"]["datavalue"]["value"]["id"].ToString();

                if (propVal == "Q1190554")
                {
                    return true;
                }
            }
            catch (Exception)
            {

            }
            if (propID == "P31")
            {
                propID = "P279";
            }
            else
            {
                propID = "P31";
            }

            count++;
            if (count > 10)
            {
                return false;
            }
        }

        if (propVal == "Q1190554")
        {
            return true;
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
    private string MoreLike(string var1, string var2)
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

        return "<h1>Title: " + title + "</h1>" + "<h1>ID: " + id + "</h1><br/>" + "<h3>Content :<h3><br/>" + content;

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