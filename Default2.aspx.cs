using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Default2 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        myPushNot x = new myPushNot("test","test","1",1,"default");
        List<User> ul = new User().getUsers();

        x.RunPushNotificationAll(ul,x);
    }
}