﻿using BMS_Base.Interface;
using BMS_Db.EfContext;
using BMS_Models.DbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ys.Base.Tools.xTool;
using Ys.Tools.Extra;
using Ys.Tools.Response;

namespace BMS_Db.BLL;

/// <summary>
/// User基础操作类-增删改查
/// </summary>
public class UserBaseBll:IBll
{
    private readonly BmsV1DbContext _dbContext;
    private readonly ILogger<UserBaseBll> _logger;
    public UserBaseBll(BmsV1DbContext dbContext, ILogger<UserBaseBll> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// 新增用户
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public void Add(User user)
    {
        user.JwtVersion = 1;
        _dbContext.User.Add(user);
    }

    /// <summary>
    /// 修改用户
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public void Edit(User user)
    {
        _dbContext.User.Update(user);
    }

    /// <summary>
    /// 删除用户
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public void Delete(User user)
    {
        user.IsDelete = true;
        _dbContext.User.Update(user);
    }

    /// <summary>
    /// 获取用户
    /// </summary>
    /// <returns></returns>
    public async Task<ApiResult> GetUser()
    {
        var listAsync = await _dbContext.User.ToListAsync();
        return ApiResult.True(listAsync);
    }
}