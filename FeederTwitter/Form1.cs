using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Twitterizer;
using Twitterizer.Streaming;
using System.Diagnostics;
using Newtonsoft.Json;

namespace FeederTwitter
{
    //private delegate void D_RawJsonCallback(string json);


    public partial class mainWindow : Form
    {
        static string consumerKey = "yMUPe213tCB5qcwXEsaD0g";
        static string consumerSecret = "YT3eJZurFpWrOH5OHGwoCQeWMLUWd133ajjV7rcz36Y";
        string accToken;
        string accTokenSec;
        OAuthTokens token = new OAuthTokens
        {
            ConsumerKey = consumerKey,
            ConsumerSecret = consumerSecret,
            //AccessToken = actToken.Token,
            //AccessTokenSecret = actToken.TokenSecret
        };

            OAuthTokenResponse req =
                    OAuthUtility.GetRequestToken(consumerKey, consumerSecret, "oob");

        public mainWindow()
        {
            InitializeComponent();
            InitializeTwitter();
            mainWindow.CheckForIllegalCrossThreadCalls = false;
        }

        public void InitializeTwitter()
        {
            try
            {
                System.Xml.Serialization.XmlSerializer serializer =
                    new System.Xml.Serialization.XmlSerializer(typeof(saveSettings));
                System.IO.FileStream fs = new System.IO.FileStream(
                    @"settings.xml", System.IO.FileMode.Open);
                saveSettings setting = (saveSettings)serializer.Deserialize(fs);
                fs.Close();
                textBox1.Enabled = false;
                //accToken = setting.AccToken;
                token.AccessToken = setting.AccToken;
                //accTokenSec = setting.AccTokenSec;
                token.AccessTokenSecret = setting.AccTokenSec;

                var Stream = new TwitterStream(token, "Feedertter", null);

                StatusCreatedCallback statusCreatedCallback = new StatusCreatedCallback(StatusCreatedCallback);
                RawJsonCallback rawJsonCallback = new RawJsonCallback(RawJsonCallback);

                Stream.StartUserStream(null, null,
                    /*(x) => { label1.Text += x.Text; }*/
                    statusCreatedCallback,
                    null, null, null, null, rawJsonCallback);
            }
            catch
            {
                Process.Start(OAuthUtility.BuildAuthorizationUri(req.Token).ToString());
            }

            //Process.Start(OAuthUtility.BuildAuthorizationUri(req.Token).ToString());
        }

        void StatusCreatedCallback(TwitterStatus status)
        {
            MessageBox.Show("ee");
            label1.Text += status.User.Name + " - " + status.Text;
        }

        void RawJsonCallback(string json)
        {
            Console.WriteLine(json);
            Newtonsoft.Json.Linq.JObject statusjson = new Newtonsoft.Json.Linq.JObject();
            Newtonsoft.Json.Linq.JToken jtoken;
            statusjson = Newtonsoft.Json.Linq.JObject.Parse(json);
            if (statusjson.TryGetValue("text", out jtoken))
            {
                label1.Text += "\n" + (string)statusjson["user"]["name"] + "「" + (string)statusjson["text"] + "」";
            }
            //label1.Text += "\n" + json;
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                OAuthTokenResponse actToken =
                    OAuthUtility.GetAccessToken(consumerKey, consumerSecret, req.Token, textBox1.Text);

                token.AccessToken = actToken.Token;
                token.AccessTokenSecret = actToken.TokenSecret;

                label1.Text += "\n始まったな";

                // start save setting
                saveSettings tokensetting = new saveSettings();
                tokensetting.AccToken = actToken.Token;
                tokensetting.AccTokenSec = actToken.TokenSecret;

                System.Xml.Serialization.XmlSerializer seriarizer =
                    new System.Xml.Serialization.XmlSerializer(typeof(saveSettings));
                System.IO.FileStream fs = new System.IO.FileStream(
                    @"settings.xml", System.IO.FileMode.Create);
                seriarizer.Serialize(fs, tokensetting);
                fs.Close();
                // end save setting

                var Stream = new TwitterStream(token, "Feedertter", null);

                StatusCreatedCallback statusCreatedCallback = new StatusCreatedCallback(StatusCreatedCallback);
                RawJsonCallback rawJsonCallback = new RawJsonCallback(RawJsonCallback);

                Stream.StartUserStream(null, null,
                    /*(x) => { label1.Text += x.Text; }*/
                    statusCreatedCallback,
                    null, null, null, null, rawJsonCallback);
                
            }
        }
    }

    public class saveSettings
    {
        public string AccToken;
        public string AccTokenSec;
    }
}
