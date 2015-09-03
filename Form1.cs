using System;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.ServiceModel.Syndication;
using System.Net;
using System.Collections.ObjectModel;


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
                        if (item.Title.Text.ToString() == "Перейти на страницу…") continue;

                        var rf = new RssFeed();

                        rf.CategoryTitle = (item.Title != null) ? item.Title.Text : String.Empty;

                        if (item.Links.Count != 0)
                            rf.Link = item.Links[0].Uri.ToString();

                        rf.Date = item.PublishDate.ToString();

                        if (item.Authors != null && item.Authors.Any())
                        {
                            var s = item.Authors.First().Email;
                            s = s.Substring(s.IndexOf('(') + 1, s.IndexOf(')') - s.IndexOf('(')-1);

                            rf.Author = s;
                        }

                        RssFeedList.Add(rf);
                    }
                }
            }

            dataGridView1.DataSource = RssFeedList;
        }

        void Form1_Load(object sender, EventArgs e)
        {
            getBaraholkoRSS();

            PostLastDT = DateTime.Parse(dataGridView1.Rows[0].Cells[2].Value.ToString());

            timer1.Start();
        }

        void notifyIcon1_BalloonTipClicked(object sender, EventArgs e)
        {
            if(!string.IsNullOrEmpty(dataGridView1.Rows[0].Cells[3].Value.ToString()))
            {
                System.Diagnostics.Process.Start(dataGridView1.Rows[0].Cells[3].Value.ToString());
            }
        }

        void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                getBaraholkoRSS();

                if (PostLastDT != DateTime.Parse(dataGridView1.Rows[0].Cells[2].Value.ToString()))
                {

                    notifyIcon1.ShowBalloonTip(15000, "Baraholko", "На форуме есть новое сообщение - " + Environment.NewLine
                        + dataGridView1.Rows[0].Cells[0].Value.ToString() + Environment.NewLine
                        + "Автор - " + dataGridView1.Rows[0].Cells[1].Value.ToString()
                        , ToolTipIcon.Info);

                    PostLastDT = DateTime.Parse(dataGridView1.Rows[0].Cells[2].Value.ToString());
                }
            }
            catch (Exception ex)
            {
                notifyIcon1.ShowBalloonTip(5000, "Ошибка", ex.Message, ToolTipIcon.Error);
            }
        }

        void перейтиКПоследнемуПостуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(dataGridView1.Rows[0].Cells[3].Value.ToString()))
            {
                System.Diagnostics.Process.Start(dataGridView1.Rows[0].Cells[3].Value.ToString());
            }
        }

        void перейтиНаГлавнуюToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://baraholko.ru/index.php");
        }

        void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
            }
        }

        void Form1_Deactivate(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                notifyIcon1.Visible = true;
            }
        }

        void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            notifyIcon1.Visible = true;
        }

        void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            //проверяем что не заголовок
            if (e.RowIndex != -1)
            {
                var s = dataGridView1.CurrentRow.Cells[3].Value.ToString();

                System.Diagnostics.Process.Start(s);
            }
        }
    }
}
