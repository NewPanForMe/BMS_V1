﻿using System.Text.Json;
using BMS_Db.BLL.Sys.File;
using BMS_Db.EfContext;
using BMS_Models.DbModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ys.Tools.Controllers;
using Ys.Tools.Extra;
using Ys.Tools.Response;

namespace BMS.Controllers.Sys
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class FileController : BaseController
    {
        private readonly BmsV1DbContext _dbContext;
        private readonly FileBll _fileBll;
        public FileController(BmsV1DbContext dbContext, FileBll fileBll)
        {
            _dbContext = dbContext;
            _fileBll = fileBll;
        }


        [HttpPost]
        public async Task<ApiResult> Upload(IFormFile file)
        {
            var upload = await _fileBll.Upload(file, CurrentUser.Code, CurrentUser.Name);
            await _dbContext.SaveChangesAsync();
            return upload;
        }


        [HttpGet]
        public async Task<ApiResult> GetList()
        {
            var data = await _fileBll.GetFileUpload();
            var pagination = new Pagination()
            {
                DefaultPageSize = 5,//默认多少条
                DefaultCurrent = 1,
                Total = data.Count
            };
            return ApiResult.True(new { data, pagination });
        }


        [HttpPost]
        public async Task<ApiResult> GetHasUploadList(JsonElement req)
        {
            var jsonString = req.GetJsonString("codes");
            var codeStrings = Array.Empty<string>();
            if (!string.IsNullOrEmpty(jsonString))
            {
                codeStrings = jsonString.Split(',');
            }
            var data = await _fileBll.GetHasUploadList(codeStrings);
            return ApiResult.True(new { data = data.Select(x => new { x.Code, x.FullName, x.Location }) });
        }
        [HttpPost]
        public ApiResult Delete(JsonElement req)
        {
            var jsonString = req.GetJsonString("code");
            _fileBll.Delete(_fileBll.GetFileUploadEntityByCode(jsonString ?? ""));
            _dbContext.SaveChanges();
            return ApiResult.True();
        }
    }
}
