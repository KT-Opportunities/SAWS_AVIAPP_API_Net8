using Microsoft.AspNetCore.Mvc;
using SAWSCore8API.Models;
using SAWSCore8API.DbContexts;
using System.IO;

namespace SAWSCore8API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class RawSourceController : ControllerBase
    {
        #region Fields
        private readonly SAWSDbContext _context;
        private ILogger<RawSourceController> _logger;

        private const int LASTHOURS = 48;

        #endregion

        #region Constructors

        public RawSourceController(
            SAWSDbContext context, 
            ILogger<RawSourceController> logger)
        {
            _context = context;
            _logger = logger;
        }

        #endregion


        #region RawSource

        [HttpGet("GetSourceTextFolderFiles")]
        public async Task<IActionResult> GetSourceTextFolderFiles(string textfoldername, int lasthours = LASTHOURS)
        {
            // Define the root folder where the files are stored on the local machine
            string rootFolder = @"C:\Users\manq2\Desktop\KTO\Other\AviationData\charts\";
            string folderPath = Path.Combine(rootFolder, textfoldername);

            // Check if the directory exists
            if (!Directory.Exists(folderPath))
            {
                return new NotFoundObjectResult($"Directory '{folderPath}' not found.");
            }

            List<TextFile> textFiles = new List<TextFile>();
            DateTime fileAfterThisDateTime = DateTime.Now.AddHours(-lasthours);

            try
            {
                var files = Directory.GetFiles(folderPath);

                foreach (string filePath in files)
                {
                    FileInfo fileInfo = new FileInfo(filePath);
                    DateTime fileModDateTime = fileInfo.LastWriteTime;

                    // Filter files based on modification time
                    if (fileModDateTime < fileAfterThisDateTime)
                    {
                    /* string base64String = "";
                        using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                        {
                            using (MemoryStream memoryStream = new MemoryStream())
                            {
                                await fileStream.CopyToAsync(memoryStream);
                                memoryStream.Position = 0;

                                base64String = Convert.ToBase64String(memoryStream.ToArray());
                            }
                        }*/

                        TextFile textFile = new TextFile
                        {
                            filename = fileInfo.Name,
                            foldername = textfoldername,
                            lastmodified = fileModDateTime,
                            // filetextcontent = base64String
                        };
                        textFiles.Add(textFile);
                    }
                }

                textFiles = textFiles.OrderByDescending(d => d.lastmodified).ToList();

                return Ok(textFiles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception from RawSourceController.GetSourceTextFolderFiles");
                return Problem("Unable to process read text file.");
            }
        }

        #endregion
    }
}
