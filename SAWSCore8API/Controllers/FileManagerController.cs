using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
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
        private readonly ISawsService _sawsService;
        private readonly IFileManagerService _fileManagerService;
        private ILogger<FileManagerController> _logger;
        private IWebHostEnvironment _environment;

        public IConfiguration _configuration { get; }

        #endregion

        #region Constructors

        public FileManagerController(
            SAWSDbContext context,
            ISawsService sawsService,
            IFileManagerService fileManagerService,
            ILogger<FileManagerController> logger,
            IConfiguration configuration,
            IWebHostEnvironment environment
        )
        {
            _context = context;
            _sawsService = sawsService;
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
                    var rootPath = Path.Combine(_environment.ContentRootPath, "Uploads");
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
                        // Updating existing advert
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


        #endregion

        // GET: api/<FileManagerController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<FileManagerController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<FileManagerController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<FileManagerController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<FileManagerController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
