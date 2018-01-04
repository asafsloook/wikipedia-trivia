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

public partial class _Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        //firstSentence();
        //onlyIntro();
        //allText();
    }

    private void firstSentence()
    {
        string ResponseText;
        HttpWebRequest myRequest =
        (HttpWebRequest)WebRequest.Create("https://en.wikipedia.org/w/api.php?action=query&prop=extracts&exsentences=1&format=json&titles=Earth");
        using (HttpWebResponse response = (HttpWebResponse)myRequest.GetResponse())
        {
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                ResponseText = reader.ReadToEnd();
            }
        }

        JavaScriptSerializer js = new JavaScriptSerializer();

        var a1 = js.DeserializeObject(ResponseText);

        var a2 = ((Dictionary<string, object>)a1).Values.ElementAt(2);

        var a3 = ((Dictionary<string, object>)a2).Values.ElementAt(0);

        var a4 = ((Dictionary<string, object>)a3).Values.ElementAt(0);

        var id = ((Dictionary<string, object>)a4).Values.ElementAt(0);

        var title = ((Dictionary<string, object>)a4).Values.ElementAt(2);

        var content = ((Dictionary<string, object>)a4).Values.ElementAt(3);


        ph.Text = "<h1>Title: " + title + "<br/> ID: " + id + "</h1><br/>" +
         "<h3>Content :<h3><br>" + content + "<br/>";
    }

    private void onlyIntro()
    {
        string ResponseText;
        HttpWebRequest myRequest =
        (HttpWebRequest)WebRequest.Create("https://en.wikipedia.org/w/api.php?action=query&prop=extracts&exintro=1&format=json&titles=Earth");
        using (HttpWebResponse response = (HttpWebResponse)myRequest.GetResponse())
        {
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                ResponseText = reader.ReadToEnd();
            }
        }

        JavaScriptSerializer js = new JavaScriptSerializer();

        var a1 = js.DeserializeObject(ResponseText);

        var a2 = ((Dictionary<string, object>)a1).Values.ElementAt(1);

        var a3 = ((Dictionary<string, object>)a2).Values.ElementAt(0);

        var a4 = ((Dictionary<string, object>)a3).Values.ElementAt(0);

        var id = ((Dictionary<string, object>)a4).Values.ElementAt(0);

        var title = ((Dictionary<string, object>)a4).Values.ElementAt(2);

        var content = ((Dictionary<string, object>)a4).Values.ElementAt(3);


        ph.Text = "<h1>Title: " + title + "<br/> ID: " + id + "</h1><br/>" +
         "<h3>Content :<h3><br>" + content + "<br/>";
    }

    private void allText()
    {
        string ResponseText;
        HttpWebRequest myRequest =
        (HttpWebRequest)WebRequest.Create("https://en.wikipedia.org/w/api.php?action=query&prop=extracts&format=json&titles=Earth");
        using (HttpWebResponse response = (HttpWebResponse)myRequest.GetResponse())
        {
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                ResponseText = reader.ReadToEnd();
            }
        }

        JavaScriptSerializer js = new JavaScriptSerializer();

        var a1 = js.DeserializeObject(ResponseText);

        var a2 = ((Dictionary<string, object>)a1).Values.ElementAt(2);

        var a3 = ((Dictionary<string, object>)a2).Values.ElementAt(0);

        var a4 = ((Dictionary<string, object>)a3).Values.ElementAt(0);

        var id = ((Dictionary<string, object>)a4).Values.ElementAt(0);

        var title = ((Dictionary<string, object>)a4).Values.ElementAt(2);

        var content = ((Dictionary<string, object>)a4).Values.ElementAt(3);


        ph.Text = "<h1>Title: " + title + "<br/> ID: " + id + "</h1><br/>" +
         "<h3>Content :<h3><br>" + content + "<br/>";
    }
}