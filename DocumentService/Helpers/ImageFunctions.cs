using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web;
using Aspose.Email.Mail;
using Aspose.Words;
using DocumentService.Models;
using WebSupergoo.ABCpdf8;
using System.Configuration;

namespace DocumentService.Helpers
{
    public static class ImageFunctions
    {
        public static List<InvoiceAttachment> ConvertMsgToPng(string pathToDocument, string folderPath)
        {
            SetAsposeLicense();

            pathToDocument = LocalizePath(pathToDocument);
            var convertedAttachments = new List<InvoiceAttachment>();

            var convertedFileName = $"{folderPath}\\{Path.GetFileNameWithoutExtension(pathToDocument)}.png";

            var msg = MailMessage.Load(pathToDocument);

            var msgStream = new MemoryStream();
            msg.Save(msgStream, MailMessageSaveType.MHtmlFormat);

            msgStream.Position = 0;
            var msgDocument = new Document(msgStream);
            msgDocument.Save(convertedFileName, SaveFormat.Png);

            var newDoc = new InvoiceAttachment
            {
                DocumentPath = convertedFileName
            };
            convertedAttachments.Add(newDoc);
            return convertedAttachments;
        }       

        public static List<InvoiceAttachment> ConvertPdfToPng(string pathToDocument, string folderPath)
        {
            XSettings.InstallLicense(ConfigurationManager.AppSettings["ABCPdfKey"]);
            pathToDocument = LocalizePath(pathToDocument);

            var convertedFileName = string.Empty;
            var convertedAttachments = new List<InvoiceAttachment>();
            var pdfToConvert = new Doc();

            pdfToConvert.Read(pathToDocument);

            // loop through the pages

            var n = pdfToConvert.PageCount;

            for (var i = 1; i <= n; i++)
            {
                pdfToConvert.PageNumber = i;
                pdfToConvert.Rect.String = pdfToConvert.CropBox.String;
                convertedFileName = $"{folderPath}\\{Path.GetFileNameWithoutExtension(pathToDocument)}{i}.png";
                pdfToConvert.Rendering.Save(convertedFileName);
                var newDoc = new InvoiceAttachment
                {
                    DocumentPath = convertedFileName
                };
                convertedAttachments.Add(newDoc);
                newDoc = null;
            }
            pdfToConvert.Clear();
            return convertedAttachments;
        }

        public static List<InvoiceAttachment> ConvertToPng(string pathToDocument, string folderPath)
        {
            pathToDocument = LocalizePath(pathToDocument);          
            var convertedFileName = $"{folderPath}\\{Path.GetFileNameWithoutExtension(pathToDocument)}.png";
            var convertedAttachments = new List<InvoiceAttachment>();

            var oldImage = Image.FromFile(pathToDocument);
            oldImage.Save(convertedFileName, ImageFormat.Png);
            var newDoc = new InvoiceAttachment
            {
                DocumentPath = convertedFileName
            };
            convertedAttachments.Add(newDoc);
            return convertedAttachments;
        }         

        public static string CreateFolder(int fileNumber)
        {
            var folderPath = Path.Combine(HttpContext.Current.Server.MapPath("~"), fileNumber.ToString());
            if (Directory.Exists(folderPath))
                Directory.Delete(folderPath, true);
            Directory.CreateDirectory(folderPath);
            return folderPath;
        }

        private static string LocalizePath(string pathToDocument)
        {
            pathToDocument = pathToDocument.Replace("C:\\", ConfigurationManager.AppSettings["DocumentDrive"]);
            return pathToDocument;
        }

        private static void SetAsposeLicense()
        {
            var W = new License();
            W.SetLicense(ConfigurationManager.AppSettings["AsposeWords"]);

            var E = new Aspose.Email.License();
            E.SetLicense(ConfigurationManager.AppSettings["AsposeEmail"]);
        }
    }
}