﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using PushSharp.Core;
using PushSharp.Google;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using Newtonsoft.Json.Linq;

/// <summary>
/// Summary description for myPushNot
/// </summary>
public class myPushNot
{
    public myPushNot()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    private string Message;

    public string message
    {
        get { return Message; }
        set { Message = value; }
    }

    private string payloadString;

    public string PayloadString
    {
        get { return payloadString; }
        set { payloadString = value; }
    }

    private string Title;

    public string title
    {
        get { return Title; }
        set { Title = value; }
    }

    private string Msgcnt;

    public string msgcnt
    {
        get { return Msgcnt; }
        set { Msgcnt = value; }
    }

    private int Badge;

    public int badge
    {
        get { return Badge; }
        set { Badge = value; }
    }

    private string Sound;

    public string sound
    {
        get { return Sound; }
        set { Sound = "default"; }
    }

    private JObject data;

    public JObject Data
    {
        get { return data; }
        set { data = value; }
    }

    public myPushNot(string _message, string _title, string _msgcnt, int _badge, string _sound)
    {
        message = _message;
        title = _title;
        msgcnt = _msgcnt;
        badge = _badge;
        sound = _sound;
    }

    public void RunPushNotificationAll(List<User> userList, myPushNot pushNot)
    {
        List<string> registrationIDs = new List<string>();

        foreach (var item in userList)
        {

            //ignore nulls
            if (item.PushKey != "" && item.PushKey != "no-reg-id")
            {
                registrationIDs.Add(item.PushKey);
            }

        }


        // Configuration
        var config = new GcmConfiguration("AIzaSyALPWklqgv9OjE5KcZTG-yFi5UznpXD7fE");
        config.GcmUrl = "https://fcm.googleapis.com/fcm/send";

        // Create a new broker
        var gcmBroker = new GcmServiceBroker(config);

        // Wire up events
        gcmBroker.OnNotificationFailed += (notification, aggregateEx) =>
        {
            Console.WriteLine("GCM Notification Failed!");
        };

        gcmBroker.OnNotificationSucceeded += (notification) =>
        {
            Console.WriteLine("GCM Notification Sent!");
        };

        // Start the broker
        gcmBroker.Start();

        foreach (var regId in registrationIDs)
        {

            // Queue a notification to send
            gcmBroker.QueueNotification(new GcmNotification
            {

                RegistrationIds = new List<string> {
            regId
        },
                Data = JObject.Parse(
                        "{" +
                               "\"title\" : \"" + pushNot.Title + "\"," +
                               "\"message\" : \"" + pushNot.Message + "\"," +
                                "\"info\" : \" Optional \"," +
                            "\"content-available\" : \"" + "1" + "\"" +
                        "}")
            });
        }


        // Stop the broker, wait for it to finish   
        // This isn't done after every message, but after you're
        // done with the broker
        gcmBroker.Stop();
    }

    public void RunPushNotificationOne(User user, JObject data)
    {

        // Configuration
        var config = new GcmConfiguration("AIzaSyALPWklqgv9OjE5KcZTG-yFi5UznpXD7fE");
        config.GcmUrl = "https://fcm.googleapis.com/fcm/send";

        // Create a new broker
        var gcmBroker = new GcmServiceBroker(config);

        // Wire up events
        gcmBroker.OnNotificationFailed += (notification, aggregateEx) =>
        {
            //Console.WriteLine("GCM Notification Failed!");
        };

        gcmBroker.OnNotificationSucceeded += (notification) =>
        {
            //Console.WriteLine("GCM Notification Sent!");
        };

        // Start the broker
        gcmBroker.Start();

        // Queue a notification to send
        gcmBroker.QueueNotification(new GcmNotification
        {

            RegistrationIds = new List<string> {
            user.PushKey
        },
            Data = data
        });



        // Stop the broker, wait for it to finish   
        // This isn't done after every message, but after you're
        // done with the broker
        gcmBroker.Stop();
    }
    
    public void cancelRide(int rideID, User user)
    {
        var x = new JObject();
        
        x.Add("title","נסיעה בוטלה");

        //modify by import ride from db by rideID
        x.Add("message", "ביום 26.8 מהדסה לירושלים");

        x.Add("rideID",rideID);
        x.Add("info", "Canceled");
        x.Add("content-available", 1);
      
        RunPushNotificationOne(user, x);
    }

    public void photoPush(List<User> lu)
    {
       
        for (int i = 0; i < lu.Count; i++)
        {
            //if (lu[i].PhotoPush)
            {
                //if (lu[i].PhotoPushTime.Hour >= DateTime.Now.Hour )
                {
                    var x = new JObject();

                    x.Add("title", "Photo of the Day is HERE!");

                    //modify by import photo description?
                    x.Add("message", "Tap here to see it :)");

                    x.Add("info", "PhotoPush");
                    x.Add("content-available", 1);

                    RunPushNotificationOne(lu[i], x);
                    
                }
            }
        }
    }

    public void articlePush(List<User> lu)
    {
        for (int i = 0; i < lu.Count; i++)
        {
            //if (lu[i].ArticlePush)
            {
                double hours = 24;
                double articlePerDay = lu[i].ArticlesPerDay;
                double min = DateTime.Now.Minute;
                double minutes = 60;
                double minFronHour = min / minutes;

                //every 6 minutes
                //2.68  0.01 * 60

                double hour = Math.Round(DateTime.Now.Hour + minFronHour, 2);
                double articleEvery = Math.Round(hours / articlePerDay, 2);

                //if (hour % articleEvery == 0)
                {
                    var x = new JObject();
                    
                    x.Add("title", "A new question is waiting for you");


                    Article a1 = new Article();
                    var categoriesList = a1.getCatByImei(lu[i].Id.ToString());
                    
                    Random rnd = new Random();

                    int randomNum = rnd.Next(0, categoriesList.Count());
                    var a = categoriesList[randomNum].ToString();

                    a1 = a1.RandomPageFromCategory(a, a, lu[i].Id.ToString());
                    

                    //modify by import article (getArticle)
                    x.Add("message", a1.NotificationContent);

                    x.Add("info", "ArticlePush");
                    x.Add("content-available", 1);
                    
                    RunPushNotificationOne(lu[i], x);
                }
            }
        }
    }
}