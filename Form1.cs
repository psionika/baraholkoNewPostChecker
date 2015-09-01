using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.ServiceModel.Syndication;
using System.Net;
using System.IO;
using System.Globalization;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace baraholkoNewPostChecker
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public ObservableCollection<RssFeed> RssFeedList { get; set; }        

        public class RssFeed
        {
            public string CategoryTitle { get; set; }
            public string Author { get; set; }
            public string Category { get; set; }
            public string Text { get; set; }
            public string Date { get; set; }
            public string Link { get; set; }
        }

        public DateTime PostLastDT = DateTime.Now;

        void getBaraholkoRSS()
        {
            var uri = "http://baraholko.ru/rss.php";

            var request = WebRequest.Create(uri);

            using (WebResponse response = request.GetResponse())
            using (XmlReader reader = XmlReader.Create(response.GetResponseStream()))
            {
                var feed = SyndicationFeed.Load(reader);

                RssFeedList = new ObservableCollection<RssFeed>();

                if (feed != null)
                {
                    foreach (var item in feed.Items)
                    {
                        if (item.Title.Text.ToString() == "Статистика конференции") continue;
                        var rf = new RssFeed();

                        rf.CategoryTitle = (item.Title != null) ? item.Title.Text : String.Empty;

                        if (item.Links.Count != 0)
                            rf.Link = item.Links[0].Uri.ToString();

                        if (item.Categories.Count != 0)
                            rf.Category = item.Categories[0].Name;

                        rf.Date = item.PublishDate.ToString();

                        StringBuilder sb = new StringBuilder();
                        foreach (SyndicationElementExtension extension in item.ElementExtensions)
                        {
                            XElement ele = extension.GetObject<XElement>();
                            if (ele.Name.LocalName == "encoded" && ele.Name.Namespace.ToString().Contains("content"))
                            {
                                sb.Append(ele.Value + "<br/>");
                            }
                        }

                        rf.Text = sb.ToString();

                        if (item.Authors != null && item.Authors.Any())
                        {
                            rf.Author = item.Authors.First().Email;
                        }


                        RssFeedList.Add(rf);
                    }
                }
            }

            dataGridView1.DataSource = RssFeedList;
        }

        void button1_Click(object sender, EventArgs e)
        {
            getBaraholkoRSS();

            PostLastDT = DateTime.Parse(dataGridView1.Rows[0].Cells[4].Value.ToString());
           
            label1.Text = PostLastDT.ToString();
        }        

        void Form1_Load(object sender, EventArgs e)
        {
            PostLastDT = DateTime.Now;
        }

        void button2_Click(object sender, EventArgs e)
        {
            getBaraholkoRSS();

            if(PostLastDT != DateTime.Parse(dataGridView1.Rows[0].Cells[4].Value.ToString()))
            {
                MessageBox.Show("Есть новые сообщения!");
                notifyIcon1.ShowBalloonTip(10000, "Baraholko", "На форуме есть новое сообщение - " + Environment.NewLine 
                    + dataGridView1.Rows[0].Cells[0].Value.ToString() + Environment.NewLine
                    + "Автор - " + dataGridView1.Rows[0].Cells[1].Value.ToString() 
                    , ToolTipIcon.Info);
                PostLastDT = DateTime.Parse(dataGridView1.Rows[0].Cells[4].Value.ToString());

            }
        }
    }

 
}
