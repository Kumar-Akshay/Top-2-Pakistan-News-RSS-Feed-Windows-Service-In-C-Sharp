using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Linq;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Configuration;
using System.Timers;

namespace K163994_Q4
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        private System.Threading.Thread _thread;

        private Timer timer;

        protected override void OnStart(string[] args)
        {


            //RSS feeds every 5 minutes from Two Website and Overwrite on file
            this.timer = new Timer((300000D));  // 300000 milliseconds =  5 mints
            this.timer.AutoReset = true;
            this.timer.Elapsed += new ElapsedEventHandler(this.timer_Elapsed);
            this.timer.Start();

          
            
        }


        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {

            try
            {


                // Create the thread object that will do the service's work.
                _thread = new System.Threading.Thread(RssFeedNews);

                // Start the thread.
                _thread.Start();

                // Log an event to indicate successful start.
                EventLog.WriteEntry("Successful start.", EventLogEntryType.Information);


            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ex.Message, EventLogEntryType.Error);
            }
            }




        private void RssFeedNews()
        {
            XmlSerializer xml = new XmlSerializer(typeof(List<NewsItem>));
            List<NewsItem> news = new List<NewsItem>();
            FileStream file = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "RssFeedNews.xml", FileMode.Append, FileAccess.Write,FileShare.None);
            String[] urls = { ConfigurationManager.AppSettings["url1"] , ConfigurationManager.AppSettings["url2"] };

            foreach (var url in urls)
            {
                XDocument doc = XDocument.Load(url);
                var NewsFeeds = (from feed in doc.Descendants("item")
                             from ch in doc.Descendants("channel")  //NewsChannel Fetching
                             select new NewsItem
                             {
                                 Title = feed.Element("title").Value,
                                 Description = feed.Element("description").Value,
                                 PublishedDate = DateTime.Parse(feed.Element("pubDate").Value),
                                 NewsChannel = ch.Element("title").Value, ////NewsChannel Title Fetching 

                             }).ToList();  // fetch 3 news .Take(3)  restrict the fetching limit

                news.AddRange(NewsFeeds);
            }
            
            //Sort orderby descending Publishdate NewsItem List
            List<NewsItem> SortedNewsList = news.OrderByDescending(order => order.PublishedDate).ToList();
            //Xml Serizile the List Obj 
            xml.Serialize(file, SortedNewsList);

            file.Close();
            }

    



       


        protected override void OnStop()
        {
        }




    }
}
