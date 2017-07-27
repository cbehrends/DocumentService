using DocumentService.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using DocumentService.Helpers;
using System.IO;
using log4net;
using System;

namespace DocumentService.Controllers
{
    public class DocumentController : ApiController
    {
        protected readonly ILog Logger;

        public DocumentController()
        {
            Logger = LogManager.GetLogger(GetType().Name);
        }

        public IHttpActionResult GetDocuments(int id)
        {
            Logger.Info($"GetDocuments for InvoiceID: {id}");
            List<InvoiceAttachment> attachments;
            using (var ctx = new LMSNetDocumentsEntities())
            {
                attachments = ctx.NewDocumentManagements
                    .Where(s => s.FileNumber == id.ToString())
                    .Where(s => s.PrintWithInvoices == true)
                            .Select(s => new InvoiceAttachment()
                            {
                                DocumentPath = s.DocPath
                            }).ToList();
            }

            if (attachments.Count == 0)
            {
                return NotFound();
            }
            try
            {
                var imageAttachments = ProcessAttachments(id, attachments);
                return Ok(imageAttachments);
            }
            catch(Exception ex)
            {
                Logger.Error(ex);
                return InternalServerError();
            }
        }

        private static List<InvoiceAttachment> ProcessAttachments(int id, List<InvoiceAttachment> attachments)
        {
            var folderPath = ImageFunctions.CreateFolder(id);

            //list to store new file names
            var attachmentImages = new List<InvoiceAttachment>();
            foreach (var remoteFilePath in attachments)
            {
                attachmentImages.AddRange(ConvertAttachments(remoteFilePath.DocumentPath, folderPath));
            }
            return attachmentImages;
        }

        private static List<InvoiceAttachment> ConvertAttachments(string filePath, string folderPath)
        {
            var extension = Path.GetExtension(filePath);
            var attachmentImages = new List<InvoiceAttachment>();

            switch (extension.ToLower())
            {
                case @".msg":
                    attachmentImages = ImageFunctions.ConvertMsgToPng(filePath, folderPath);
                    break;
                case @".pdf":
                    attachmentImages = ImageFunctions.ConvertPdfToPng(filePath, folderPath);
                    break;
                case @".tif":
                case @".tiff":
                case @".jpg":
                case @".jpeg":
                case @".png":
                case @".bmp":
                    attachmentImages = ImageFunctions.ConvertToPng(filePath, folderPath);
                    break;
            }
            return attachmentImages;
        }   
    }
}
