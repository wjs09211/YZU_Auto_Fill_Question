using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web;
using System.Net;
using System.IO;
using System.Web.SessionState;
namespace YZU_Auto_Fill_Question
{
    public partial class Form1 : Form
    {
        CookieContainer cookieContainer = new CookieContainer();
        List<string> classUrl = new List<string>();
        string html; //存放當前網頁內容
        string SessionID;
        bool click = true;
        bool checkLogin = false;
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            listBox2.Items.Add("歡迎使用 YZU自動填寫課程問卷");
        }
        private void HttpGet(string url, bool cookie)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.KeepAlive = true;
                ///UserAgent 使用者的裝置和瀏覽器
                request.UserAgent = "Mozilla/5.0 (MSIE 9.0; Windows NT 6.1; Trident/5.0)";
                request.Accept = "application/x-ms-application, image/jpeg, application/xaml+xml, image/gif, image/pjpeg, application/x-ms-xbap, */*";
                request.Headers.Add("Accept-Encoding", "gzip-deflate");
                request.Headers.Add("Cache-Control", "no-cache");
                request.Headers.Add("Accept-Language", "zh-TW");
                if (cookie)
                    request.CookieContainer = cookieContainer;
                ///傳回的資料
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                    html = sr.ReadToEnd();
                }
            }
            catch
            {
                listBox1.Items.Add("HttpGet error 程式錯誤");
            }
        }
        private void HttpPost(string url, string param)
        {
            byte[] bs = Encoding.ASCII.GetBytes(param);
            //requset
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.KeepAlive = true;
                ///UserAgent 使用者的裝置和瀏覽器
                request.UserAgent = "Mozilla/5.0 (MSIE 9.0; Windows NT 6.1; Trident/5.0)";
                request.Accept = "application/x-ms-application, image/jpeg, application/xaml+xml, image/gif, image/pjpeg, application/x-ms-xbap, */*";
                request.Headers.Add("Accept-Encoding", "gzip-deflate");
                request.Headers.Add("Cache-Control", "no-cache");
                request.Headers.Add("Accept-Language", "zh-TW");
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = bs.Length;
                request.CookieContainer = cookieContainer;
                using (Stream reqStream = request.GetRequestStream())
                {
                    reqStream.Write(bs, 0, bs.Length);
                }
                ///傳回的資料
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    StreamReader sr = new StreamReader(response.GetResponseStream(), System.Text.Encoding.GetEncoding("utf-8"));
                    html = sr.ReadToEnd();                   
                }
            }
            catch
            {
                listBox1.Items.Add("post error 程式錯誤");
            }
        }
        private string getInfo(string s)
        {         
            int i = html.IndexOf(s) + s.Length;
            int j = html.IndexOf("\"", i);
            string info = html.Substring(i, j - i);
            return info;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            login();
            if (checkLogin == true)
            {
                HttpGet("https://portalx.yzu.edu.tw/PortalSocialVB/FMain/DefaultPage.aspx?Menu=Default", true);//portal 首頁
                                                                                                               //模擬封包操作 到填問卷的頁面
                HttpGet("https://portalx.yzu.edu.tw/PortalSocialVB/FMain/ClickMenuLog.aspx?type=App_&SysCode=A08", true);
                HttpGet("https://portalx.yzu.edu.tw/PortalSocialVB/IFrameSub.aspx", true);
                //取得SessionID
                string str = "id='SessionID' name='SessionID' type='hidden' value='";
                int start = html.IndexOf(str) + str.Length;
                int end = html.IndexOf("'", start);
                SessionID = html.Substring(start, end - start);
                //
                string formatString =
                    "Account={0}&SessionID={1}&LangVersion=TW&Y=&M=&CosID=&CosClass=&UseType=STD";
                string param =
                         string.Format(formatString, textBox1.Text.ToString(), SessionID);
                HttpPost("https://portal.yzu.edu.tw/NewSurvey/NewLogin.aspx", param);
                HttpGet(" https://portal.yzu.edu.tw/NewSurvey/std/F01_Questionnaire.aspx", true);
                getClass();
                if (classUrl.Count != 0)
                {
                    listBox2.Items.Add("請選擇問卷評價，確定請按自動填寫");
                }
                else
                {
                    listBox2.Items.Add("無問卷可填寫");
                    click = false;
                }
            }
        }
        private void getClass()
        {
            int start = 0;
            string s = "</a></td><td><a href=\"";
            string s2 = "target=\"_self\">";
            while (true)
            {
                int i = html.IndexOf(s, start);
                if (i == -1) break;
                int j = html.IndexOf("\"", i + s.Length);
                classUrl.Add(html.Substring(i + s.Length, j - i - s.Length).Replace("amp;", ""));
                start = j;

                i = html.IndexOf(s2, start);
                if (i == -1) break;
                j = html.IndexOf("<", i + s2.Length);
                listBox1.Items.Add(html.Substring(i + s2.Length, j - i - s2.Length));
                start = j + 3;
            }

        }
        private void login()
        {
            HttpGet("https://portalx.yzu.edu.tw/PortalSocialVB/Login.aspx", false);
            string viewState = getInfo("id=\"__VIEWSTATE\" value=\"");
            string eventValidation = getInfo("id=\"__EVENTVALIDATION\" value=\"");
            string submitButton = "登入";
            //編碼一下~
            viewState = HttpUtility.UrlEncode(viewState);
            eventValidation = HttpUtility.UrlEncode(eventValidation);
            submitButton = HttpUtility.UrlEncode(submitButton);
            //POST資料
            string formatString =
                "__VIEWSTATE={0}&__EVENTVALIDATION={1}&Txt_UserID={2}&Txt_Password={3}&ibnSubmit={4}";
            string param =
                     string.Format(formatString, viewState, eventValidation, textBox1.Text.ToString(), textBox2.Text.ToString(), submitButton);
            HttpPost("https://portalx.yzu.edu.tw/PortalSocialVB/Login.aspx", param);
            if (html.IndexOf("登入失敗") == -1 && html.IndexOf("請輸入使用者帳號密碼") == -1)
            {
                listBox2.Items.Add("登入成功");
                checkLogin = true;
            }
            else
            {
                listBox2.Items.Add("登入失敗");
                checkLogin = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (click)
            {
                string value = "1";
                if (radioButton1.Checked == true) value = "1";
                else if (radioButton2.Checked == true) value = "2";
                else if (radioButton3.Checked == true) value = "3";
                else if (radioButton4.Checked == true) value = "4";
                else if (radioButton5.Checked == true) value = "5";
                for (int k = 0; k < classUrl.Count; k++)
                {
                    HttpGet("https://portal.yzu.edu.tw/NewSurvey/std/" + classUrl[k], true);
                    string viewState = getInfo("id=\"__VIEWSTATE\" value=\"");
                    string eventValidation = getInfo("id=\"__EVENTVALIDATION\" value=\"");
                    string submitButton = "完成";
                    //編碼一下~
                    viewState = HttpUtility.UrlEncode(viewState);
                    eventValidation = HttpUtility.UrlEncode(eventValidation);
                    submitButton = HttpUtility.UrlEncode(submitButton);
                    //POST資料
                    string formatString =
                        "__VIEWSTATE={0}&__EVENTVALIDATION={1}&btOK={2}";
                    string param =
                             string.Format(formatString, viewState, eventValidation, submitButton);
                    //取得問卷內容
                    string s = "valign=\"top\"><table id=\"";
                    int start = 0;
                    while (true)
                    {
                        int i = html.IndexOf(s, start);
                        if (i == -1) break;
                        int j = html.IndexOf("\"", i + s.Length);
                        param += ("&" + html.Substring(i + s.Length, j - i - s.Length) + "=" + value);
                        start = j;
                    }
                    HttpPost("https://portal.yzu.edu.tw/NewSurvey/std/" + classUrl[k], param);
                }
                listBox2.Items.Add("問卷已填完，請上網確認");
                click = false;
            }
            else
            {
                if (classUrl.Count == 0)               
                    listBox2.Items.Add("無問卷可填寫");                
                else
                    listBox2.Items.Add("已填完，請勿重複點擊");
            }
        }
    }
}
