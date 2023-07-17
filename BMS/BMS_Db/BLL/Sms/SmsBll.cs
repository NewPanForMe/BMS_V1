﻿using BMS_Db.BLL.User;
using BMS_Db.EfContext;
using BMS_Models.DbModels;
using BMS_SMS.Models;
using Microsoft.Extensions.Logging;
using Ys.Tools.Extra;
using Ys.Tools.Interface;
using Ys.Tools.MoreTool;

namespace BMS_Db.BLL.Sms;

public class SmsBll:IBll
{


    private readonly BmsV1DbContext _dbContext;
    private readonly ILogger<SmsBll> _logger;

    public SmsBll(BmsV1DbContext dbContext, ILogger<SmsBll> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }



    /// <summary>
    /// 发送验证码
    /// </summary>
    /// <param name="phoneNumber">手机号</param>
    /// <param name="code">验证码</param>
    public async Task<SmsResult> SendSmsCode(string phoneNumber,string code)
    {
        var param = new Param()
        {
            code = code
        };
        SmsSendConfig.Instance = new SmsSendConfig()
        {
            SignName = "BMS",
            TemplateCode = "SMS_461960505",
            TemplateParam = JsonTools.Serialize(param)
        };
        var smsResult = await BMS_SMS.Base.BaseSms.Instance.Send(phoneNumber);
        return smsResult;
    }
  
    class Param
    {
        public string code { get; set; } = String.Empty;
    }


    /// <summary>
    /// 通过主键获得记录
    /// </summary>
    /// <param name="code"></param>
    public SmsLog GetEntityByCode(string code)
    {
        code.NotNull("传入编号为空");
        var smsLog = _dbContext.SmsLog.FirstOrDefault(x => x.Code.Equals(code)).NotNull("当前数据不存在"); ;
        _logger.LogWarning("获取短信日志{code}：{user}", code, smsLog);
        return smsLog;
    }
}