using SAWSCore8API.Models;
using Microsoft.AspNetCore.Mvc;
using PayFast;

namespace SAWSCore8API.Interfaces
{
    public interface IRawSourceService
    {
        Task<GetRawFile> GetFile(string filePath, string imagefoldername);
        IEnumerable<RawFile> GetSourceFolderFiles(string folderPath, string foldername);
    }
}