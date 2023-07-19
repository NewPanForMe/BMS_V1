﻿using BMS_Base.Config;
using BMS_Db.EfContext;
using BMS_Models.DbModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Ys.Tools.Extra;
using Ys.Tools.Interface;

namespace BMS_Db.BLL.File;

public class FileBll:IBll
{
    private readonly BmsV1DbContext _dbContext;
    public FileBll(BmsV1DbContext dbContext)
    {
        _dbContext = dbContext;
    }


    /// <summary>
    /// 上传
    /// </summary>
    /// <param name="file"></param>
    /// <param name="userCode"></param>
    /// <param name="userName"></param>
    public async Task<string> Upload(IFormFile file,string userCode,string userName)
    {
        var fileFullPath = Path.Combine(SystemConfig.Instance.UploadFileFolder, DateTime.Now.ToString("yyyy-MM-dd"));
        if (!Directory.Exists(fileFullPath)) Directory.CreateDirectory(fileFullPath);
        Console.WriteLine($"fileFullPath={fileFullPath}");
        var filePath = fileFullPath + "\\" + file.FileName;
        await file.CopyToAsync(System.IO.File.Create(filePath));
        Console.WriteLine($"filePath={filePath}");
        var files = new FileUpload()
        {
            UserCode = userCode,
            UserName = userName,
            Code = Guid.NewGuid().ToString(),
            CreateDate = DateTime.Now,
            FullName = file.FileName,
            Location = filePath
        };
        _dbContext.FileUpload.Add(files);
        return files.Code;
    }


    /// <summary>
    /// 获取列表
    /// </summary>
    /// <returns></returns>
    public async Task<List<FileUpload>> GetFileUpload()
    {
        var listAsync = await _dbContext.FileUpload.OrderByDescending(x => x.CreateDate).AsNoTracking().ToListAsync();
        return listAsync;
    }

    /// <summary>
    /// 获取列表
    /// </summary>
    /// <returns></returns>
    public async Task<List<FileUpload>> GetHasUploadList(string[] args)
    {
        var listAsync = await _dbContext.FileUpload.Where(x => args.Contains(x.Code))
            .OrderByDescending(x => x.CreateDate).AsNoTracking().ToListAsync();
        return listAsync;
    }
    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public void Delete(FileUpload file)
    {
        _dbContext.FileUpload.Remove(file);
    }

    /// <summary>
    /// 获取模块
    /// </summary>
    /// <returns></returns>
    public FileUpload GetFileUploadEntityByCode(string code)
    {
        code.NotNull("传入编号为空");
        var module = _dbContext.FileUpload.AsNoTracking().FirstOrDefault(x => x.Code.Equals(code));
        module = module.NotNull("当前数据不存在");
        return module;
    }

}