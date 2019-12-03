using MahApps.Metro.Controls;
using System;
using System.Net.Http;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.IO;
using System.Web;
using System.Threading;
using System.Windows.Threading;
using System.Web.UI;
using MahApps.Metro.Controls.Dialogs;

namespace BingSearch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        ObservableCollection<CompanyInfo> c = new ObservableCollection<CompanyInfo>();
        List<string> Cnms = new List<string>();
        OpenFileDialog dlg;
        string filename;
        string line;
        string var1;
        bool status;
        Thread th;
        HttpWebRequest myRequest;

        public MainWindow()
        {
            InitializeComponent();
            //getPage("http://www.ask.com/web?q=diveo+twitter&o=0&qo=homepageSearchBox");
        }

        private void getPage()
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate()
            {
                //var1 = variable_tb.Text;
                c.Clear();
                searchresults.ItemsSource = c;

                //c = new ObservableCollection<CompanyInfo>();
            });

            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate()
            {
                progressbar.Visibility = Visibility.Visible;
                progressbar.IsIndeterminate = true;
                statusTb.Visibility = Visibility.Visible;
                statusTb.Text = "Please hold on the search is running......";
            });
            string responseString;
            FileStream f = new FileStream(filename, FileMode.Open);
            StreamReader sr = new StreamReader(f);
            Cnms.Clear();
            line = sr.ReadLine().Trim();
            while (line != null)
            {
                Cnms.Add(line);
                line = sr.ReadLine();
            }
            sr.Close();
            f.Close();
            status = true;
            foreach (string cnm in Cnms)
            {
                cnm.Replace(" ", "+");
                string urlString = "http://www.ask.com/web?q=+" + cnm + "+" + var1 + "&o=0&qo=homepageSearchBox";
                try
                {
                    using (var client = new WebClient())
                    {

                        responseString = client.DownloadString(@urlString);

                        //myRequest = (HttpWebRequest)WebRequest.Create(@urlString);
                        //myRequest.Method = "GET";
                        //WebResponse myresponse = myRequest.GetResponse();
                        //StreamReader strm = new StreamReader(myresponse.GetResponseStream(), System.Text.Encoding.UTF8);
                        //responseString = strm.ReadToEnd();
                        //strm.Close();
                        //myresponse.Close();

                        MatchCollection m1 = Regex.Matches(responseString, "target=\"_blank\" href='(.*?)'", RegexOptions.Singleline);
                        CompanyInfo comInfo = new CompanyInfo();
                        comInfo.Company = cnm;
                        for (int i = 0; i < 3; i++)
                        {
                            string s = m1[i].Groups[1].Value;
                            if (s.Contains(var1 + ".com"))
                            {
                                if (i == 0)
                                {
                                    comInfo.Url = s;
                                }
                                else if (i == 1)
                                {
                                    comInfo.Url2 = s;
                                }
                                else if (i == 2)
                                {
                                    comInfo.Url3 = s;
                                }
                            }
                        }
                        this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate()
                        {
                            c.Add(comInfo);
                        });
                    }
                }
                catch (ThreadAbortException thAbortex)
                {
                    //Cnms.Clear();
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate()
                    {
                        progressbar.Visibility = Visibility.Hidden;
                        progressbar.IsIndeterminate = false;
                        searchresults.ItemsSource = c;
                        searchbtn.IsEnabled = true;
                        statusTb.Visibility = Visibility.Hidden;
                        stopbtn.IsEnabled = false;
                    });
                    status = false;

                    break;
                }
                catch (Exception ex)
                {
                    //Cnms.Clear();
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate()
                    {
                        //MessageBox.Show(ex.Message);
                        //progressbar.Visibility = Visibility.Hidden;
                        //progressbar.IsIndeterminate = false;
                        searchresults.ItemsSource = c;
                        searchbtn.IsEnabled = true;
                        statusTb.Visibility = Visibility.Hidden;
                        //stopbtn.IsEnabled = false;
                    });
                    status = false;

                    //break;

                }

            }

            if (status)
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate()
                {
                    statusTb.Text = "Loading Html file......";
                });
                Random rd = new Random();
                string nwfilename = "SearchResult" + rd.Next(900) + ".html";
                string newfile = @"..\..\..\..\..\..\..\Documents\" + nwfilename;
                //string newfile = @"..\..\..\" + nwfilename;
                FileStream myfile = new FileStream(newfile, FileMode.Create);
                using (StreamWriter sw = new StreamWriter(myfile))
                {
                    StringWriter streamw = new StringWriter();
                    using (HtmlTextWriter h = new HtmlTextWriter(streamw))
                    {
                        h.RenderBeginTag(HtmlTextWriterTag.Html);
                        h.RenderBeginTag(HtmlTextWriterTag.Head);
                        h.RenderBeginTag(HtmlTextWriterTag.Title);
                        h.Write("BingSearch For Companies");
                        h.RenderEndTag();
                        h.RenderEndTag();
                        h.RenderBeginTag(HtmlTextWriterTag.Body);
                        h.RenderBeginTag(HtmlTextWriterTag.H1);
                        h.Write("BingSearch For Companies");
                        h.RenderEndTag();
                        foreach (CompanyInfo m in c)
                        {
                            h.RenderBeginTag(HtmlTextWriterTag.Div);
                            h.RenderBeginTag(HtmlTextWriterTag.H3);
                            h.Write(m.Company);
                            h.RenderEndTag();
                            h.AddAttribute(HtmlTextWriterAttribute.Target, "_blank");
                            h.AddAttribute(HtmlTextWriterAttribute.Href, m.Url);
                            h.RenderBeginTag(HtmlTextWriterTag.A);
                            h.Write(m.Url);
                            h.RenderEndTag();
                            h.RenderBeginTag(HtmlTextWriterTag.Br);
                            h.AddAttribute(HtmlTextWriterAttribute.Target, "_blank");
                            h.AddAttribute(HtmlTextWriterAttribute.Href, m.Url2);
                            h.RenderBeginTag(HtmlTextWriterTag.A);
                            h.Write(m.Url2);
                            h.RenderEndTag();
                            h.RenderBeginTag(HtmlTextWriterTag.Br);
                            h.AddAttribute(HtmlTextWriterAttribute.Target, "_blank");
                            h.AddAttribute(HtmlTextWriterAttribute.Href, m.Url3);
                            h.RenderBeginTag(HtmlTextWriterTag.A);
                            h.Write(m.Url3);
                            h.RenderEndTag();
                            h.RenderEndTag();
                        }
                        h.RenderBeginTag(HtmlTextWriterTag.P);
                        h.Write("Thank you for the order.");
                        h.RenderEndTag();
                        h.RenderEndTag();
                        h.RenderEndTag();
                    }
                    sw.Write(streamw.ToString());
                }
                FileInfo f_info = new FileInfo(newfile);
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate()
                {
                    this.ShowMessageAsync("Saved File Info", "The file name is: " + nwfilename + "\n File path is:" + f_info.DirectoryName);
                    System.Diagnostics.Process.Start(f_info.FullName);
                });


                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate()
                {
                    progressbar.Visibility = Visibility.Hidden;
                    progressbar.IsIndeterminate = false;
                    searchresults.ItemsSource = c;
                    searchbtn.IsEnabled = true;
                    statusTb.Visibility = Visibility.Hidden;
                    stopbtn.IsEnabled = false;
                });



            }
        }

        private void searchbtn_Click(object sender, RoutedEventArgs e)
        {
            searchbtn.IsEnabled = false;
            stopbtn.IsEnabled = true;
            if (string.IsNullOrEmpty(filename))
            {
                
                MessageBox.Show("Browse a file to import the company names");
                searchbtn.IsEnabled = true;
                stopbtn.IsEnabled = false;
                 

            }
            else if (string.IsNullOrEmpty(var1))
            {

                    MessageBox.Show("Enter a search criteria");
                    searchbtn.IsEnabled = true;
                    stopbtn.IsEnabled = false;
            }
            else
            {
                th = new Thread(getPage);
                th.Start();
            }
            
        }

        private void browsebtn_Click(object sender, RoutedEventArgs e)
        {
            //OpenFileDialog initialization
            dlg = new OpenFileDialog();
            dlg.Filter = "Text Files|*.txt";
            dlg.Title = "Text files only containing company names";
            dlg.ShowDialog();
            //Start the appending to file chosen from the file dialog
            filename = dlg.FileName;
            cnamefile_tb.Text = filename;
        }

        private void variable_tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            var1 = variable_tb.Text;
        }

        private void stopbtn_Click(object sender, RoutedEventArgs e)
        {
            th.Abort();

            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate()
            {
                statusTb.Text = "Loading Html file......";
            });
            Random rd = new Random();
            string nwfilename = "SearchResult" + rd.Next(900) + ".html";
            string newfile = @"..\..\..\..\..\..\..\Documents\" + nwfilename;
            //string newfile = @"..\..\..\" + nwfilename;
            FileStream myfile = new FileStream(newfile, FileMode.Create);
            using (StreamWriter sw = new StreamWriter(myfile))
            {
                StringWriter streamw = new StringWriter();
                using (HtmlTextWriter h = new HtmlTextWriter(streamw))
                {
                    h.RenderBeginTag(HtmlTextWriterTag.Html);
                    h.RenderBeginTag(HtmlTextWriterTag.Head);
                    h.RenderBeginTag(HtmlTextWriterTag.Title);
                    h.Write("BingSearch For Companies");
                    h.RenderEndTag();
                    h.RenderEndTag();
                    h.RenderBeginTag(HtmlTextWriterTag.Body);
                    h.RenderBeginTag(HtmlTextWriterTag.H1);
                    h.Write("BingSearch For Companies");
                    h.RenderEndTag();
                    foreach (CompanyInfo m in c)
                    {
                        h.RenderBeginTag(HtmlTextWriterTag.Div);
                        h.RenderBeginTag(HtmlTextWriterTag.H3);
                        h.Write(m.Company);
                        h.RenderEndTag();
                        h.AddAttribute(HtmlTextWriterAttribute.Target, "_blank");
                        h.AddAttribute(HtmlTextWriterAttribute.Href, m.Url);
                        h.RenderBeginTag(HtmlTextWriterTag.A);
                        h.Write(m.Url);
                        h.RenderEndTag();
                        h.RenderBeginTag(HtmlTextWriterTag.Br);
                        h.AddAttribute(HtmlTextWriterAttribute.Target, "_blank");
                        h.AddAttribute(HtmlTextWriterAttribute.Href, m.Url2);
                        h.RenderBeginTag(HtmlTextWriterTag.A);
                        h.Write(m.Url2);
                        h.RenderEndTag();
                        h.RenderBeginTag(HtmlTextWriterTag.Br);
                        h.AddAttribute(HtmlTextWriterAttribute.Target, "_blank");
                        h.AddAttribute(HtmlTextWriterAttribute.Href, m.Url3);
                        h.RenderBeginTag(HtmlTextWriterTag.A);
                        h.Write(m.Url3);
                        h.RenderEndTag();
                        h.RenderEndTag();
                    }
                    h.RenderBeginTag(HtmlTextWriterTag.P);
                    h.Write("Thank you for the order.");
                    h.RenderEndTag();
                    h.RenderEndTag();
                    h.RenderEndTag();
                }
                sw.Write(streamw.ToString());
            }
            FileInfo f_info = new FileInfo(newfile);
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate()
            {
                this.ShowMessageAsync("Saved File Info", "The file name is: " + nwfilename + "\n File path is:" + f_info.DirectoryName);
                System.Diagnostics.Process.Start(f_info.FullName);
            });


            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate()
            {
                progressbar.Visibility = Visibility.Hidden;
                progressbar.IsIndeterminate = false;
                searchresults.ItemsSource = c;
                searchbtn.IsEnabled = true;
                statusTb.Visibility = Visibility.Hidden;
                stopbtn.IsEnabled = false;
            });

            statusTb.Visibility = Visibility.Hidden;
            progressbar.IsIndeterminate = false;
            progressbar.Visibility = Visibility.Hidden;
            searchbtn.IsEnabled = true;
            stopbtn.IsEnabled = false;
        }
    }
}
