﻿using StreamMasterDomain.Filtering;
using StreamMasterDomain.Pagination;
using StreamMasterDomain.Repository.EPG;

using System.Text.Json;

namespace StreamMasterApplication.Programmes.Queries;

public record GetProgrammeNameSelections(ProgrammeParameters Parameters) : IRequest<PagedResponse<ProgrammeNameDto>>;

internal class GetProgrammeNameSelectionsHandler(ILogger<GetProgrammeNameSelections> logger, IRepositoryWrapper repository, IMapper mapper, IPublisher publisher, ISender sender, IHubContext<StreamMasterHub, IStreamMasterHub> hubContext, IMemoryCache memoryCache) : BaseMemoryRequestHandler(logger, repository, mapper, publisher, sender, hubContext, memoryCache), IRequestHandler<GetProgrammeNameSelections, PagedResponse<ProgrammeNameDto>>
{
    public async Task<PagedResponse<ProgrammeNameDto>> Handle(GetProgrammeNameSelections request, CancellationToken cancellationToken)
    {
        if (request.Parameters.PageSize == 0)
        {
            PagedResponse<ProgrammeNameDto> emptyResponse = new()
            {
                TotalItemCount = MemoryCache.Programmes().Count
            };
            return emptyResponse;
        }

        IQueryable<Programme> programmes = MemoryCache.Programmes().Where(a => !string.IsNullOrEmpty(a.Channel)).AsQueryable();

        if (!string.IsNullOrEmpty(request.Parameters.JSONFiltersString))
        {
            List<DataTableFilterMetaData>? filters = JsonSerializer.Deserialize<List<DataTableFilterMetaData>>(request.Parameters.JSONFiltersString);
            if (filters != null)
            {
                DataTableFilterMetaData? nameFilter = filters.FirstOrDefault(a => a.FieldName == "name");
                if (nameFilter != null)
                {
                    nameFilter.FieldName = "DisplayName";
                }
                programmes = FilterHelper<Programme>.ApplyFiltersAndSort(programmes, filters, "DisplayName asc");
            }
        }

        // Get distinct channel names directly
        List<string> distinctChannels = programmes.Select(a => a.Channel).Distinct().ToList();

        // Map to DTO
        List<ProgrammeNameDto> mappedProgrammes = distinctChannels.Select(channel =>
        {
            Programme? programme = programmes.FirstOrDefault(a => a.Channel == channel);
            return programme != null ? Mapper.Map<ProgrammeNameDto>(programme) : null;
        }).Where(dto => dto != null).ToList();

        IPagedList<ProgrammeNameDto> pagedList = await mappedProgrammes.OrderBy(a => a.DisplayName)
            .ToPagedListAsync(request.Parameters.PageNumber, request.Parameters.PageSize)
            .ConfigureAwait(false);

        PagedResponse<ProgrammeNameDto> pagedResponse = pagedList.ToPagedResponse(pagedList.TotalItemCount);
        return pagedResponse;
    }


}
