using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Amazon.Util;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Toolkit.Uwp.Notifications;

namespace _301113658_otojareri__lab2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BasicAWSCredentials credentials;
        AmazonDynamoDBClient client;
        public MainWindow()
        {
            InitializeComponent();
            Task table = TableCreate();
        }
        private async Task TableCreate()
        {
            credentials = new BasicAWSCredentials(
                                 ConfigurationManager.AppSettings["accessId"],
                                 ConfigurationManager.AppSettings["secretKey"]);
            client = new AmazonDynamoDBClient(credentials, RegionEndpoint.USEast1);
            Console.WriteLine("Getting list of tables");
            ListTablesResponse tables = await client.ListTablesAsync();
            Console.WriteLine("Number of tables: " + tables.TableNames.Count);
            if (!tables.TableNames.Contains("listofusers"))
            {
                CreateTableRequest request = new CreateTableRequest
                {
                    TableName = "listofusers",
                    AttributeDefinitions = new List<AttributeDefinition>
                    {
                        new AttributeDefinition
                    {
                      AttributeName = "username",
                      AttributeType = "S"
                    },
                        new AttributeDefinition
                    {
                      AttributeName = "password",
                      AttributeType = "S"
                    },
                    },
                    //"HASH" = hask key
                    //"RANGE" = range key
                    KeySchema = new List<KeySchemaElement>
                        {
                            new KeySchemaElement
                            {
                                AttributeName = "username",
                                KeyType = "HASH"
                            },
                            new KeySchemaElement
                            {
                                AttributeName = "password",
                                KeyType = "RANGE"
                            }
                        },
                    BillingMode = BillingMode.PROVISIONED,
                    ProvisionedThroughput = new ProvisionedThroughput { ReadCapacityUnits = 3, WriteCapacityUnits = 1 },
                };
                try
                {
                    var response = await client.CreateTableAsync(request);
                    MessageBox.Show("Table created with request ID: " + response.ResponseMetadata.RequestId, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    {

                        new ToastContentBuilder()
                            .AddText("Table has been created Successfully").Show();

                    }
                    Trace.Write("Takes time for the table to be active, don't worry");

                    var describeRequest = new DescribeTableRequest
                    {
                        TableName = response.TableDescription.TableName,
                    };

                    TableStatus status;

                    int sleepDuration = 2000;

                    do
                    {
                        System.Threading.Thread.Sleep(sleepDuration);

                        var describeTableResponse = await client.DescribeTableAsync(describeRequest);
                        status = describeTableResponse.Table.TableStatus;

                        Trace.Write(".");
                    }
                    while (status != "ACTIVE");
                    CreateItem(client);
                }
                catch (AmazonDynamoDBException ade)
                {
                    MessageBox.Show("An error has occured on the server - " + ade.Message + "        " + ade.StackTrace);
                }

            }
        }

        private void CreateItem(AmazonDynamoDBClient client)
        {
            var batchRequest = new BatchWriteItemRequest
            {
                ReturnConsumedCapacity = "TOTAL",
                RequestItems = new Dictionary<string, List<WriteRequest>>
        {

              {
                "listofusers", new List<WriteRequest>
                {
                  new WriteRequest
                  {
                    PutRequest = new PutRequest
                    {
                       Item = new Dictionary<string,AttributeValue>
                       {
                         { "username", new AttributeValue { S = "Mudiaga" } },
                         { "password", new AttributeValue { S = "sunderland4147" } 
                          }
                       }
                    }
                  },
                  new WriteRequest
                  {
                    PutRequest = new PutRequest
                    {
                       Item = new Dictionary<string,AttributeValue>
                       {
                         { "username", new AttributeValue { S = "Gladys" } },
                         { "password", new AttributeValue { S = "password" }
                           }
                         
                       }
                    }
                  },
                  new WriteRequest
                  {
                    PutRequest = new PutRequest
                    {
                       Item = new Dictionary<string,AttributeValue>
                       {
                         { "username", new AttributeValue { S = "Wu" } },
                         { "password", new AttributeValue { S = "password" } 
                          }
                       }
                    }
                  },
                  new WriteRequest
                  {
                    PutRequest = new PutRequest
                    {
                       Item = new Dictionary<string,AttributeValue>
                       {
                         { "username", new AttributeValue { S = "Ji Sung" } },
                         { "password", new AttributeValue { S = "password" } 
                         }
                       }
                    }
                  }
                }
              }
            }
            };
            CallBatchWriteTillCompletion(batchRequest);
        }

        private async void CallBatchWriteTillCompletion(BatchWriteItemRequest batchRequest)
        {
            BatchWriteItemResponse response;

            int callCount = 0;

            try
            {
                do
                {
                    Trace.WriteLine("Creating request");
                    response = await client.BatchWriteItemAsync(batchRequest);
                    callCount++;

                    //for checking the request response
                    var tableConsumedCapacities = response.ConsumedCapacity;
                    var unprocessed = response.UnprocessedItems;


                    foreach (var tableConsumedCapacity in tableConsumedCapacities)
                    {
                        Trace.WriteLine(tableConsumedCapacity.TableName + " - " + tableConsumedCapacity.CapacityUnits);
                    }


                    foreach (var up in unprocessed)
                    {
                        Trace.WriteLine(up.Key + " - " + up.Value.Count);
                    }
                    Console.WriteLine();

                    // For if the request will have unprocessed items.
                    batchRequest.RequestItems = unprocessed;
                } while (response.UnprocessedItems.Count > 0);
            }
            catch (AmazonDynamoDBException e)
            {
                MessageBox.Show("An error has occured on the server - " + e.ErrorCode + "   " + e.StackTrace);
            }
            Trace.WriteLine("batch write API calls made: {0}", callCount.ToString());
            new ToastContentBuilder()
                            .AddText("User Credentials have been added succesfully").Show();

        }

        private async void loginBtn_click(object sender, RoutedEventArgs e)
        {
            string password = passBox.Password.ToString();
            string username = userNameBox.Text;

            if (String.IsNullOrEmpty(username) || String.IsNullOrEmpty(password))
            {
                MessageBox.Show("Username and password cannot be empty!", "Empty error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                try
                {
                    // Check the response.     
                    var request = new QueryRequest
                    {
                        TableName = "listofusers",
                        KeyConditionExpression = "username = :v_username and password = :v_password",
                        ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                    {":v_username", new AttributeValue { S =  username }},
                    {":v_password", new AttributeValue{S = password }}
                    },
                        ConsistentRead = true
                    };

                    var response = await client.QueryAsync(request);

                    //if the response.Items.Count has 0 object, it means the query does not find any matching username or password
                    //which means the credentials is wrong
                    if (response.Items.Count <= 0)
                    {
                        MessageBox.Show("User not exist. Please check your username or password and try again.", "Login error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        foreach (Dictionary<string, AttributeValue> item in response.Items)
                        {
                            // Process the result.
                            // MessageBox.Show(item.Values.ToString());
                            //PrintItem(item);
                            if (item.Count > 0)
                            {
                                MessageBox.Show("Login Success");
                                BooksWindow bookShelfWindow = new BooksWindow(username);
                                bookShelfWindow.Show();
                                this.Close();
                            }
                        }
                    }
                    Trace.WriteLine("\nPrinting item after retrieving it ............");
                }
                catch (AmazonDynamoDBException db)
                {
                    MessageBox.Show(db.ErrorCode + "\n " + db.Message + " \n " + db.StackTrace);
                }
            }
        }
        private static void PrintItem(
    Dictionary<string, AttributeValue> attributeList)
        {
            int count = 0;
            foreach (KeyValuePair<string, AttributeValue> kvp in attributeList)
            {
                string attributeName = kvp.Key;
                AttributeValue value = kvp.Value;

                MessageBox.Show(
                    attributeName + " " +
                    (value.S == null ? "" : "S=[" + value.S + "]") +
                    (value.S == null ? "" : "N=[" + value.S + "]") +
                    (value.M == null ? "" : "M=[" + string.Join(",", value.M.ToArray()) + "]") +
                    (value.S == null ? "" : "S=[" + value.S + "]")
                    );
                if (attributeName.Equals("password"))
                {
                    string dynamoPassowrd = value.S;
                }
                count++;
            }
            MessageBox.Show("No. of iterations - " + count);
            Console.WriteLine("************************************************");
        }
    }
}
