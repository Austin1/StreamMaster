﻿using StreamMasterDomain.Dto;
using StreamMasterDomain.Pagination;

namespace StreamMasterDomain.Repository
{
    public interface IEPGFileRepository : IRepositoryBase<EPGFile>
    {
        Task<IEnumerable<EPGFile>> GetAllEPGFilesAsync();

        Task<PagedResponse<EPGFilesDto>> GetEPGFilesAsync(EPGFileParameters EPGFileParameters);

        Task<EPGFile> GetEPGFileByIdAsync(int Id);

        Task<EPGFile> GetEPGFileBySourceAsync(string source);

        void CreateEPGFile(EPGFile EPGFile);

        void UpdateEPGFile(EPGFile EPGFile);

        void DeleteEPGFile(EPGFile EPGFile);
    }
}