﻿using Microsoft.AspNetCore.Mvc;

using StreamMasterDomain.Dto;
using StreamMasterDomain.Pagination;
using StreamMasterDomain.Repository.EPG;

namespace StreamMasterApplication.Programmes;

public interface IProgrammeChannelDB
{
}

public interface IProgrammeChannelHub
{
    Task<IEnumerable<Programme>?> GetProgramme(string Channel);

    Task<IEnumerable<ProgrammeChannel>> GetProgrammeChannels();

    Task<PagedResponse<ProgrammeNameDto>> GetProgrammeNameSelections(ProgrammeParameters Parameters);

    Task<ProgrammeNameDto?> GetProgrammeFromDisplayName(string Tvg_ID);


    Task<List<ProgrammeNameDto>> GetProgrammsSimpleQuery(ProgrammeParameters Parameters);
    Task<IEnumerable<string>> GetProgrammeNames();

    Task<IEnumerable<Programme>> GetProgrammes();
}

public interface IProgrammeChannelScoped
{
}

public interface IProgrammeChannelTasks
{
    ValueTask AddProgrammesFromSDRequest(CancellationToken cancellationToken = default);
}

public interface IProgrammeChannelController
{
    Task<ActionResult<IEnumerable<Programme>?>> GetProgramme(string Channel);

    Task<ActionResult<IEnumerable<ProgrammeChannel>>> GetProgrammeChannels();

    Task<ActionResult<List<ProgrammeNameDto>>> GetProgrammsSimpleQuery(ProgrammeParameters Parameters);
    Task<ActionResult<PagedResponse<ProgrammeNameDto>>> GetProgrammeNameSelections(ProgrammeParameters Parameters);
    Task<ActionResult<IEnumerable<string>>> GetProgrammeNames();

    Task<ActionResult<IEnumerable<Programme>>> GetProgrammes();

    Task<ActionResult<ProgrammeNameDto?>> GetProgrammeFromDisplayName(string Tvg_ID);
}
