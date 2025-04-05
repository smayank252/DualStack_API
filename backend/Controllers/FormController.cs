using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs;
using Azure.Data.Tables;
using FormApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FormApp.Controllers
{
    [Route("api/form")]
    [ApiController]
    public class FormController : ControllerBase
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly TableServiceClient _tableServiceClient;
        private readonly TelemetryClient _telemetryClient;
        private readonly string _tableName = "FormSubmissions";
        private readonly string _containerName = "photos";

        public FormController(BlobServiceClient blobServiceClient, TableServiceClient tableServiceClient, TelemetryClient telemetryClient)
        {
            _blobServiceClient = blobServiceClient;
            _tableServiceClient = tableServiceClient;
            _telemetryClient = telemetryClient;
        }

        [HttpPost]
        public async Task<IActionResult> SubmitForm([FromForm] FormModel model)
        {
            _telemetryClient.TrackEvent("Form Submission Received");

            var tableClient = _tableServiceClient.GetTableClient(_tableName);
            await tableClient.CreateIfNotExistsAsync();

            string blobUrl = null;
            if (model.Photo != null)
            {
                try
                {
                    var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                    await containerClient.CreateIfNotExistsAsync();

                    // Generate a unique blob name and upload the photo
                    var blobClient = containerClient.GetBlobClient(Guid.NewGuid().ToString() + Path.GetExtension(model.Photo.FileName));

                    using (var stream = model.Photo.OpenReadStream())
                    {
                        await blobClient.UploadAsync(stream);
                    }
                    blobUrl = blobClient.Uri.ToString(); // This is the blob URL
                    _telemetryClient.TrackEvent("Photo Uploaded Successfully");
                }
                catch (Exception ex)
                {
                    _telemetryClient.TrackException(ex);
                    return StatusCode(500, "Error uploading photo");
                }
            }

            try
            {
                // Save form data in Azure Table Storage
                var entity = new TableEntity(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
                {
                    { "Name", model.Name },
                    { "Email", model.Email },
                    { "Address", model.Address },
                    { "PhotoUrl", blobUrl }
                };

                await tableClient.AddEntityAsync(entity);
                _telemetryClient.TrackTrace("Form data stored in Table Storage");
            }
            catch (Exception ex)
            {
                _telemetryClient.TrackException(ex);
                return StatusCode(500, "Error storing form data");
            }

            return Ok(new { Message = "Form submitted successfully!" });
        }

        [HttpGet("submissions")]
        public async Task<IActionResult> GetSubmissions()
        {
            _telemetryClient.TrackEvent("Fetching Form Submissions");
            var tableClient = _tableServiceClient.GetTableClient(_tableName);
            var submissions = new List<object>();

            try
            {
                // Fetch all submissions from Azure Table Storage
                await foreach (var entity in tableClient.QueryAsync<TableEntity>())
                {
                    submissions.Add(new
                    {
                        Name = entity.GetString("Name"),
                        Email = entity.GetString("Email"),
                        Address = entity.GetString("Address"),
                        PhotoUrl = entity.GetString("PhotoUrl")
                    });
                }
                _telemetryClient.TrackTrace("Successfully fetched form submissions");
            }
            catch (Exception ex)
            {
                _telemetryClient.TrackException(ex);
                return StatusCode(500, "Error fetching submissions");
            }

            return Ok(submissions);
        }
    }
}