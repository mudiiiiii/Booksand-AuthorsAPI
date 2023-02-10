
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Util;
using Syncfusion.Pdf.Parsing;
using System;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Windows;

namespace _301113658_otojareri__lab2
{
    /// <summary>
    /// Interaction logic for ViewWindow.xaml
    /// </summary>
    public partial class ViewWindow : Window
    {
        public string bookKey { get; set; }
        public string username { get; set; }
        BasicAWSCredentials credentials;
        AmazonS3Client client;
        AmazonDynamoDBClient dynamoDBClient;
        Table bookShelf;
        public ViewWindow()
        {
            InitializeComponent();
        }
        public ViewWindow(string bookName, string userName) : this()
        {
            bookKey = bookName;
            username = userName;
            credentials = new BasicAWSCredentials(ConfigurationManager.AppSettings["accessId"], ConfigurationManager.AppSettings["secretKey"]);
            client = new AmazonS3Client(credentials, RegionEndpoint.USEast1);
            dynamoDBClient = new AmazonDynamoDBClient(credentials, RegionEndpoint.USEast1);
            bookShelf = Table.LoadTable(dynamoDBClient, "BookShelf");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                GetObjectRequest request = new GetObjectRequest();
                request.BucketName = "mudiaga-bookshelf";
                request.Key = bookKey + ".pdf";
                GetObjectResponse response = client.GetObjectAsync(request).Result;
                MemoryStream DocumentStream = new MemoryStream();
                response.ResponseStream.CopyTo(DocumentStream);
                PdfLoadedDocument loadedDocument = new PdfLoadedDocument(DocumentStream);
                pdfViewer.Load(loadedDocument);
            }
            catch (AmazonS3Exception ex)
            {
                MessageBox.Show(ex.ErrorCode + " \n " + ex.Message + " \n " + ex.StackTrace);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            var book = new Document();
            book["BookName"] = bookKey;
            book["username"] = username;
            book["bookmarkTime"] = DateTime.Now.ToString(AWSSDKUtils.ISO8601DateFormat);
            book["bookMarkPage"] = (pdfViewer.CurrentPageIndex).ToString();
            Console.WriteLine("Closed");
            bookShelf.UpdateItemAsync(book);
            BooksWindow bookShelfWindow = new BooksWindow(username);
            bookShelfWindow.Show();
        }

        private void bookMark_btnclick(object sender, RoutedEventArgs e)
        {
            var book = new Document();
            book["BookName"] = bookKey;
            book["username"] = username;
            book["bookmarkTime"] = DateTime.Now.ToString(AWSSDKUtils.ISO8601DateFormat);
            book["bookMarkPage"] = (pdfViewer.CurrentPageIndex).ToString();
            Console.WriteLine("Closed");
            bookShelf.UpdateItemAsync(book);
        }
    }
}
