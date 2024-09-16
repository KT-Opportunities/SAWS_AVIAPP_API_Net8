using SAWSCore8API.Models;
using SAWSCore8API.DbContexts;
using SAWSCore8API.Interfaces;

namespace SAWSCore8API.Services
{
    public class RawSourceService : IRawSourceService
    {
        private readonly SAWSDbContext _context;
        private readonly IUriService _uriService;
        private ILogger<RawSourceService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public RawSourceService(
            SAWSDbContext context,
            IUriService uriService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<RawSourceService> logger,
            IConfiguration configuration,
            HttpClient httpClient
            )
        {
            _context = context;
            _uriService = uriService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClient ?? new HttpClient();
        }

        public async Task<GetRawFile> GetFile(string filePath, string imagefoldername)
        {
            List<GetRawFile> textFiles = new List<GetRawFile>();

            FileInfo fileInfo = new FileInfo(filePath);
            DateTime fileModDateTime = fileInfo.LastWriteTime;

            string base64String = "";
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    await fileStream.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;
                    base64String = Convert.ToBase64String(memoryStream.ToArray());
                }
            }

            return GetRawFile.Result(fileInfo.Name, imagefoldername, fileModDateTime, base64String);
        }

        public IEnumerable<RawFile> GetSourceFolderFiles(string folderPath, string foldername)
        {
            List<RawFile> rawFiles = new List<RawFile>();
            DateTime fileAfterThisDateTime = DateTime.Now.AddHours(-12);

            var files = Directory.GetFiles(folderPath);

            foreach (string filePath in files)
            {
                FileInfo fileInfo = new FileInfo(filePath);
                DateTime fileModDateTime = fileInfo.LastWriteTime;

                // Filter files based on modification time
                // NB: Add this condition if the api is slow
                // if (fileModDateTime > fileAfterThisDateTime)
                //  {
                    RawFile rawFile = new RawFile
                    {
                        filename = fileInfo.Name,
                        lastmodified = fileModDateTime,
                        foldername = foldername
                    };
                    rawFiles.Add(rawFile);

                //  }

            }

            rawFiles = rawFiles.OrderByDescending(d => d.lastmodified).ToList();

            return rawFiles;
        }

        public IEnumerable<RawTextFile> GetTextSourceFolderFiles(string folderPath, string foldername)
        {
            List<RawTextFile> textFiles = new List<RawTextFile>();
            DateTime fileAfterThisDateTime = DateTime.Now.AddHours(-12);

            var files = Directory.GetFiles(folderPath);

            foreach (string filePath in files)
            {
                FileInfo fileInfo = new FileInfo(filePath);
                DateTime fileModDateTime = fileInfo.LastWriteTime;

                string textContents = "";
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        textContents = reader.ReadToEnd();
                    }
                }

                // if (fileModDateTime > fileAfterThisDateTime)
                // {
                    RawTextFile textFile = new RawTextFile
                    {
                        filename = fileInfo.Name,
                        lastmodified = fileModDateTime,
                        foldername = foldername,
                        filecontent = textContents
                    };
                    textFiles.Add(textFile);
                // }

            }

            textFiles = textFiles.OrderByDescending(d => d.lastmodified).ToList();

            return textFiles;
        }
    }
}
