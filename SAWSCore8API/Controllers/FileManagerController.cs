using Microsoft.AspNetCore.Mvc;
using SAWSCore8API.Models;
using SAWSCore8API.DbContexts;
using SAWSCore8API.Interfaces;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SAWSCore8API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class FileManagerController : ControllerBase
    {

        #region Fields

        private readonly SAWSDbContext _context;
        private readonly IPagedService _pagedService;
        private readonly IFileManagerService _fileManagerService;
        private ILogger<FileManagerController> _logger;
        private IWebHostEnvironment _environment;

        public IConfiguration _configuration { get; }

        #endregion

        #region Constructors

        public FileManagerController(
            SAWSDbContext context,
            IPagedService pagedService,
            IFileManagerService fileManagerService,
            ILogger<FileManagerController> logger,
            IConfiguration configuration,
            IWebHostEnvironment environment
        )
        {
            _context = context;
            _pagedService = pagedService;
            _fileManagerService = fileManagerService;
            _logger = logger;
            _configuration = configuration;
            _environment = environment;
        }

        #endregion

        #region File Manager

        [HttpPost("PostDocsForAdvert")]
        public async Task<IActionResult> PostDocsForAdvert([FromForm] IList<DocAdvert> files)
        {
            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).AsEnumerable()
                );

                return BadRequest(new CreateResult
                {
                    Success = false,
                    ErrorMessages = errorMessages
                });
            }

            var toReturn = new List<DocAdvert>();

            foreach (var file in files)
            {
                var dbItem = new DocAdvert();

                try
                {
                    var folderId = Convert.ToString(file.advertId);
                    var root = _configuration["rootPath"];
                    var rootPath = Path.Combine(root, "Uploads");
                    string path = Path.Combine(rootPath, rootPath + "\\Advert\\" + folderId + "\\");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    // Fetch the File
                    IFormFile postedFile = file.file;

                    // Extract file datails.
                    string fileName1 = postedFile.FileName;
                    string fileName = fileName1;
                    string fileUrl = Path.Combine(path, fileName);
                    string fileExtension = Path.GetExtension(postedFile.FileName);
                    long filesize = postedFile.Length;
                    string mimeType = postedFile.ContentType;

                    // Save the File.
                    using (FileStream stream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                    {
                        postedFile.CopyTo(stream);
                    }

                    // Populate dbFileObject less the raw file
                    dbItem.Id = file.Id;
                    dbItem.advertId = file.advertId;
                    dbItem.DocTypeName = file.DocTypeName;
                    dbItem.file_origname = fileName;
                    dbItem.file_url = fileUrl;
                    dbItem.file_size = filesize;
                    dbItem.file_mimetype = mimeType;
                    dbItem.file_extention = fileExtension;

                    if (file.Id == 0)
                    {
                        // Creating new advert document

                        dbItem.created_at = DateTime.Now;
                        dbItem.updated_at = DateTime.Now;
                        dbItem.isdeleted = false;

                        _fileManagerService.AddAdvertDoc(dbItem);
                        toReturn.Add(dbItem);
                    }
                    else
                    {
                        // Updating existing advert document
                        dbItem.updated_at = DateTime.Now;
                        dbItem.isdeleted = file.isdeleted;

                        _fileManagerService.UpdateAdvertDoc(dbItem);
                        toReturn.Add(dbItem);

                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled exception from FileManagerController.PostDocsForAdvert");
                    return Problem("Unable to process the post of advert documents.");
                }
            }

            return Ok(toReturn);

        }

        [HttpPost("PostDocsForFeedback")]
        public async Task<IActionResult> PostDocsForFeedback([FromForm] IList<DocFeedback> files)
        {
            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).AsEnumerable()
                );

                return BadRequest(new CreateResult
                {
                    Success = false,
                    ErrorMessages = errorMessages
                });
            }

            var toReturn = new List<DocFeedback>();

            foreach (var file in files)
            {
                var dbItem = new DocFeedback();

                try
                {
                    var folderId = Convert.ToString(file.feedbackMessageId);
                    var root = _configuration["rootPath"];
                    var rootPath = Path.Combine(root, "Uploads");
                    string path = Path.Combine(rootPath, rootPath + "\\Feedback\\" + folderId + "\\");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    // Fetch the File
                    IFormFile postedFile = file.file;

                    // Extract file datails.
                    string fileName1 = postedFile.FileName;
                    string fileName = fileName1;
                    string fileUrl = Path.Combine(path, fileName);
                    string fileExtension = Path.GetExtension(postedFile.FileName);
                    long filesize = postedFile.Length;
                    string mimeType = postedFile.ContentType;

                    // Save the File.
                    using (FileStream stream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                    {
                        postedFile.CopyTo(stream);
                    }

                    // Populate dbFileObject less the raw file
                    dbItem.Id = file.Id;
                    dbItem.feedbackMessageId = file.feedbackMessageId;
                    dbItem.DocTypeName = file.DocTypeName;
                    dbItem.file_origname = fileName;
                    dbItem.file_url = fileUrl;
                    dbItem.file_size = filesize;
                    dbItem.file_mimetype = mimeType;
                    dbItem.file_extention = fileExtension;

                    if (file.Id == 0)
                    {
                        // Creating new feedback document

                        dbItem.created_at = DateTime.Now;
                        dbItem.updated_at = DateTime.Now;
                        dbItem.isdeleted = false;

                        _fileManagerService.AddFeedbackDoc(dbItem);
                        toReturn.Add(dbItem);
                    }
                    else
                    {
                        // Updating existing feedback document
                        dbItem.updated_at = DateTime.Now;
                        dbItem.isdeleted = file.isdeleted;

                        _fileManagerService.UpdateFeedbackDoc(dbItem);
                        toReturn.Add(dbItem);

                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled exception from FileManagerController.PostDocsForFeedback");
                    return Problem("Unable to process the post of feedback documents.");
                }
            }

            return Ok(toReturn);

        }

        #endregion
    }
}
