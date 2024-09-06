using Microsoft.AspNetCore.Mvc;
using SAWSCore8API.Models;
using SAWSCore8API.DbContexts;
using System.IO;
using SAWSCore8API.Interfaces;

namespace SAWSCore8API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class RawSourceController : ControllerBase
    {
        #region Fields
        private readonly SAWSDbContext _context;
        private readonly IRawSourceService _rawSourceService;
        private ILogger<RawSourceController> _logger;
        private readonly IConfiguration _configuration;

        private const int LASTHOURS = 48;
        private const string FOLDERNAME = "";

        #endregion

        #region Constructors

        public RawSourceController(
            SAWSDbContext context,
            IRawSourceService rawSourceService,
            ILogger<RawSourceController> logger,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _rawSourceService = rawSourceService;
            _configuration = configuration;
        }

        #endregion


        #region RawSource

        [HttpGet("GetSourceTextFolderFiles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSourceTextFolderFiles(string foldername = FOLDERNAME, int lasthours = LASTHOURS)
        {
            string folder = @"text\";
            var rootFolder = _configuration["RootFolder"];

            string folderPath = Path.Combine(rootFolder, folder, foldername);

            if (!Directory.Exists(folderPath))
            {
                return new NotFoundObjectResult($"Directory '{folderPath}' not found.");
            }

            try
            {
                var textFiles = _rawSourceService.GetSourceFolderFiles(folderPath, foldername, lasthours);

                return new OkObjectResult(textFiles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception from RawSourceController.GetSourceTextFolderFiles");
                return Problem("Unable to process read text folder files.");
            }
        }

        [HttpGet("GetSourceChartFolderFilesList")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSourceChartFolderFilesList(string foldername = FOLDERNAME, int lasthours = LASTHOURS)
        {
            string folder = @"charts\";
            var rootFolder = @"C:\Users\manq2\Desktop\KTO\Other\AviationData";

            string folderPath = Path.Combine(rootFolder, folder, foldername);

            if (!Directory.Exists(folderPath))
            {
                return new NotFoundObjectResult($"Directory '{folderPath}' not found.");
            }

            try
            {
                var textFiles = _rawSourceService.GetSourceFolderFiles(folderPath, foldername, lasthours);

                return new OkObjectResult(textFiles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception from RawSourceController.GetSourceChartFolderFilesList");
                return Problem("Unable to process read charts folder files.");
            }
        }

        [HttpGet("GetSourceAviationFolderFilesList")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSourceAviationFolderFilesList(string foldername = FOLDERNAME, int lasthours = LASTHOURS)
        {
            string folder = @"aviation\";
            var rootFolder = _configuration["RootFolder"];

            string folderPath = Path.Combine(rootFolder, folder, foldername);

            if (!Directory.Exists(folderPath))
            {
                return new NotFoundObjectResult($"Directory '{folderPath}' not found.");
            }

            try
            {
                var textFiles = _rawSourceService.GetSourceFolderFiles(folderPath, foldername, lasthours);

                return new OkObjectResult(textFiles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception from RawSourceController.GetSourceAviationFolderFilesList");
                return Problem("Unable to process read aviation folder files.");
            }
        }

        [HttpGet("GetTextFile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetTextFile(string imagefilename, string imagefoldername = FOLDERNAME )
        {

            string folder = "text";
            var rootFolder = _configuration["RootFolder"];
            string filePath = Path.Combine(rootFolder, folder, imagefoldername, imagefilename);

            if (!System.IO.File.Exists(filePath))
            {
                return new NotFoundObjectResult($"File -'{imagefilename}'- not found.");
            }

            try
            {
                var rawFile = await _rawSourceService.GetFile(filePath, imagefoldername);
                return new OkObjectResult(rawFile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception from RawSourceController.GetTextFile");
                return Problem("Unable to process read text folder files.");
            }
        }

        [HttpGet("GetChartFile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetChartFile(string imagefilename, string imagefoldername = FOLDERNAME)
        {
            string folder = "charts";
            var rootFolder = _configuration["RootFolder"];
            string filePath = Path.Combine(rootFolder, folder, imagefoldername, imagefilename);

            if (!System.IO.File.Exists(filePath))
            {
                return new NotFoundObjectResult($"File -'{imagefilename}'- not found.");
            }

            try
            {
                var rawFile = await _rawSourceService.GetFile(filePath, imagefoldername);
                return new OkObjectResult(rawFile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception from RawSourceController.GetChartFile");
                return Problem("Unable to process read chart folder files.");
            }
        }

        [HttpGet("GetAviationFile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAviationFile(string imagefilename, string imagefoldername = FOLDERNAME)
        {

            string folder = "aviation";
            var rootFolder = _configuration["RootFolder"];
            string filePath = Path.Combine(rootFolder, folder, imagefoldername, imagefilename);

            if (!System.IO.File.Exists(filePath))
            {
                return new NotFoundObjectResult($"File -'{imagefilename}'- not found.");
            }
            
            try
            {
                var rawFile = await _rawSourceService.GetFile(filePath, imagefoldername);
                return new OkObjectResult(rawFile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception from RawSourceController.GetAviationFile");
                return Problem("Unable to process read aviation folder files.");
            }
        }


    #endregion
    }

}
