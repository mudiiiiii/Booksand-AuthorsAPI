using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Syncfusion.Pdf.Parsing;
using System.IO;
using System.Windows;

namespace _301113658_Otojareri__Lab2
{
    public partial class PdfClass : Window
    {
        BasicAWSCredentials credentials;
        AmazonS3Client client;
        private string userName;

        public PdfClass(string bookName, string user)
        {
            userName = user;

            credentials = new BasicAWSCredentials(
            System.Configuration.ConfigurationManager.AppSettings["accessId"],
            System.Configuration.ConfigurationManager.AppSettings["secretKey"]);

            client = new AmazonS3Client(credentials, Amazon.RegionEndpoint.USEast1);
            PdfLoadedDocument loadedDocument = new PdfLoadedDocument(GetBook1(bookName));
        }

        private MemoryStream GetBook1(string book)
        {
            try
            {
                GetObjectRequest request = new GetObjectRequest();
                request.BucketName = "mudiaga_bookshelf";
                request.Key = book;
                GetObjectResponse response = client.GetObjectAsync(request).Result;

                MemoryStream docStream = new MemoryStream();
                response.ResponseStream.CopyTo(docStream);
                return docStream;

            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                throw amazonS3Exception;
            }
        }
    }

}

