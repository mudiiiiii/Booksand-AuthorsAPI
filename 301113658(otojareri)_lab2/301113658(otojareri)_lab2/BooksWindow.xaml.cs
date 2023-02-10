using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace _301113658_otojareri__lab2
{
    /// <summary>
    /// Interaction logic for BooksWindow.xaml
    /// </summary>
    /// 

    public class BookShelf
    {
        public string bookname { get; set; }
        public string author { get; set; }
        public string username { get; set; }
        public string bookmarkTime { get; set; }
        public string bookmarkPage { get; set; }

        public BookShelf(string bookName, string authorName)
        {
            bookname = bookName;
            author = authorName;
        }
        public BookShelf(string bookName, string authorName, string bookmarkTimeName) : this(bookName, authorName)
        {
            bookname = bookName;
            author = authorName;
            bookmarkTime = bookmarkTimeName;
        }
        public override string ToString()
        {
            return bookname;
        }
    }
    public partial class BooksWindow : Window
    {
        BasicAWSCredentials credentials;
        AmazonDynamoDBClient client;
        List<BookShelf> bookshelfItems = new List<BookShelf>();
        string tableName = "BookShelf";
        string username;
        public BooksWindow()
        {
            InitializeComponent();
            try
            {
                credentials = new BasicAWSCredentials(
                     ConfigurationManager.AppSettings["accessId"],
                     ConfigurationManager.AppSettings["secretKey"]);
                client = new AmazonDynamoDBClient(credentials, RegionEndpoint.USEast1);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + '\n' + e.StackTrace);
            }
        }
        public BooksWindow(string name) : this()
        {
            usernameLabel.Content = "Welcome " + name;
            username = name;
            LoadBooks();
        }

        private async void LoadBooks()
        {
            try
            {
                var bookshelfScanRequest = new ScanRequest
                {
                    TableName = "BookShelf",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                        { ":v_username", new AttributeValue { S = username } }
                    },
                    FilterExpression = "username = :v_username",
                    ProjectionExpression = "BookName, author, bookmarkTime, bookMarkPage"
                };
                var response = await client.ScanAsync(bookshelfScanRequest);
                string booktitle = string.Empty, author = string.Empty, bookMarkPage = string.Empty, bookmarkTime = string.Empty;
                var items = response.Items;
                foreach (var currentItem in items)
                {
                    foreach (string attr in currentItem.Keys)
                    {
                        Console.Write(attr + "---> ");
                        if (attr == "BookName")
                        {
                            booktitle = currentItem[attr].S;


                        }
                        else if (attr == "author")
                        {
                            author = currentItem[attr].S;

                        }
                        else if (attr == "bookmarkTime")
                        {
                            bookmarkTime = currentItem[attr].S;

                        }
                    }
                    bookshelfItems.Add(new BookShelf(booktitle, author, bookmarkTime));

                }
                //ordering the book by time
                bookshelfItems = bookshelfItems.OrderByDescending(x => x.bookmarkTime).ToList();

                bookList.ItemsSource = bookshelfItems;
            }
            catch (AmazonDynamoDBException adbe)
            {
                MessageBox.Show(adbe.ErrorCode + "\n " + adbe.Message + " \n " + adbe.StackTrace);
            }
        }

        private void booklistClick(object sender, MouseButtonEventArgs e)
        {
            var dataContexxt = ((FrameworkElement)e.OriginalSource).DataContext;
            if (dataContexxt is BookShelf)
            {
                Hide();
                ViewWindow viewWindow = new ViewWindow(dataContexxt.ToString(), username);
                viewWindow.Show();

            }
        }
        private void PrintItem(
       Dictionary<string, AttributeValue> attributeList)
        {
            int count = 0;
            foreach (KeyValuePair<string, AttributeValue> kvp in attributeList)
            {
                string attributeName = kvp.Key;
                AttributeValue value = kvp.Value;

                /*                MessageBox.Show(
                                    attributeName + " " +
                                    (value.S == null ? "" : "S=[" + value.S + "]") +
                                    (value.S == null ? "" : "S=[" + value.S + "]") +
                                    (value.S == null ? "" : "S=[" + value.S + "]") +
                                    (value.S == null ? "" : "S=[" + value.S + "]")
                                    );*/

                count++;
            }

            Console.WriteLine("************************************************");
        }

        private void signoutClick(object sender, RoutedEventArgs e)
        {
            Hide();
            MainWindow mainwindow = new MainWindow();
            mainwindow.Show();
            MessageBox.Show("You have been successfully signed out");
        }
    }
}
