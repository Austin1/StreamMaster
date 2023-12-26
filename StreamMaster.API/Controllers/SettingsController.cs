﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using StreamMaster.Domain.Common;
using StreamMaster.Domain.Dto;

using StreamMaster.Application.Common.Models;
using StreamMaster.Application.General.Queries;
using StreamMaster.Application.Settings;
using StreamMaster.Application.Settings.Commands;
using StreamMaster.Application.Settings.Queries;

namespace StreamMasterAPI.Controllers;

public class SettingsController : ApiControllerBase, ISettingController
{

    [HttpGet]
    [Route("[action]")]
    public async Task<ActionResult<bool>> GetIsSystemReady()
    {
        return await Mediator.Send(new GetIsSystemReadyRequest()).ConfigureAwait(false);
    }

    [HttpGet]
    [Route("[action]")]
    public async Task<ActionResult<List<TaskQueueStatusDto>>> GetQueueStatus()
    {
        return await Mediator.Send(new GetQueueStatus()).ConfigureAwait(false);
    }

    [HttpGet]
    [Route("[action]")]
    public async Task<ActionResult<SettingDto>> GetSetting()
    {
        return await Mediator.Send(new GetSettings()).ConfigureAwait(false);
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("[action]")]
    public async Task<ActionResult<SDSystemStatus>> GetSystemStatus()
    {
        return await Mediator.Send(new GetSystemStatus()).ConfigureAwait(false);
    }

    [HttpGet]
    [Route("[action]")]
    public async Task<ActionResult<bool>> LogIn(LogInRequest logInRequest)
    {
        Setting setting = await SettingsService.GetSettingsAsync();
        return setting.AdminUserName == logInRequest.UserName && setting.AdminPassword == logInRequest.Password;
    }

    [HttpPatch]
    [Route("[action]")]
    public async Task<IActionResult> UpdateSetting(UpdateSettingRequest command)
    {
        UpdateSettingRequestHandler.UpdateSettingResponse updateSettingResponse = await Mediator.Send(command).ConfigureAwait(false);

        if (updateSettingResponse.NeedsLogOut)
        {
            return Redirect("/logout");
        }

        return updateSettingResponse == null ? NotFound() : NoContent();
    }
}