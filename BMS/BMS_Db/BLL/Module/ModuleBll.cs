﻿using BMS_Db.BLL.User;
using BMS_Db.EfContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ys.Tools.Interface;
using Ys.Tools.Response;
using BMS_Models.DbModels;
using Microsoft.IdentityModel.Tokens;
using Ys.Tools.Extra;
using Consul;

namespace BMS_Db.BLL.Module;

public class ModuleBll : IBll
{
    private readonly BmsV1DbContext _dbContext;
    private readonly ILogger<ModuleBll> _logger;
    public ModuleBll(BmsV1DbContext dbContext, ILogger<ModuleBll> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        _dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }


    /// <summary>
    /// 新增模块
    /// </summary>
    /// <param name="module"></param>
    /// <returns></returns>
    public void Add(BMS_Models.DbModels.Module module)
    {
        _dbContext.Module.Add(module);
        _logger.LogWarning("新增模块：{module}", module);
    }

    /// <summary>
    /// 修改模块
    /// </summary>
    /// <param name="module"></param>
    /// <returns></returns>
    public void Edit(BMS_Models.DbModels.Module module)
    {
        _dbContext.Module.Update(module);
        _logger.LogWarning("修改模块：{module}", module);
    }

    /// <summary>
    /// 删除模块
    /// </summary>
    /// <param name="module"></param>
    /// <returns></returns>
    public void Delete(BMS_Models.DbModels.Module module)
    {
        _dbContext.Module.Remove(module);
        _logger.LogWarning("删除模块：{module}", module);
    }

    /// <summary>
    /// 获取模块
    /// </summary>
    /// <param name="value">值</param>
    /// <returns></returns>
    public async Task<List<BMS_Models.DbModels.Module>> GetModules(string value)
    {
        var listAsync = await _dbContext.Module.AsNoTracking().ToListAsync();
        var newList = string.IsNullOrEmpty(value) ? listAsync : listAsync.Where(x => x.ParentCode == value).ToList();
        _logger.LogWarning("获取模块列表：{newList}", newList.Count);
        return newList;
    }



    /// <summary>
    /// 获取模块
    /// </summary>
    /// <returns></returns>
    public BMS_Models.DbModels.Module GetModuleEntityByCode(string code)
    {
        code.NotNull("传入编号为空");
        var module = _dbContext.Module.AsNoTracking().FirstOrDefault(x => x.Code.Equals(code));
        module = module.NotNull("当前数据不存在");
        _logger.LogWarning("获取模块{code}：{module}", code, module);
        return module;
    }



    /// <summary>
    /// 获取Module的树型图
    /// </summary>
    public async Task<List<Tree>> GetTreeNode()
    {
        var listAsync = await _dbContext.Module.AsNoTracking().ToListAsync();
        var trees = GetTree(listAsync, "father");
        _logger.LogWarning("获取模块树结构：{module}", trees.Count);
        return trees;
    }

    /// <summary>
    /// 节点详细处理
    /// </summary>
    /// <param name="miModules"></param>
    /// <param name="value"></param>
    private List<Tree> GetTree(List<BMS_Models.DbModels.Module> miModules, string value)
    {
        var list = new List<Tree>();
        var modules = miModules.Where(x => x.ParentCode == value).ToList();
        modules.ForEach(module =>
        {
            var tree = new Tree()
            {
                Label = module.Name,
                Value = module.Value,
                Children = new List<Tree>()
            };
            tree.Children.AddRange(GetTree(miModules, module.Value));
            list.Add(tree);
        });
        return list;
    }


    /// <summary>
    /// 生成Menu
    /// </summary>
    public async Task<List<Menu>> GetMenuNode()
    {
        var listAsync = await _dbContext.Module.ToListAsync();
        listAsync = listAsync.Where(x => x.IsShow).ToList();
        var menus = GetMenu(listAsync, "root");
        _logger.LogWarning("获取菜单：{menus}", menus.Count);
        return menus;
    }
    /// <summary>
    /// Menu详细处理
    /// </summary>
    /// <param name="miModules"></param>
    /// <param name="value"></param>
    private List<Menu> GetMenu(List<BMS_Models.DbModels.Module> miModules, string value)
    {
        var list = new List<Menu>();
        var modules = miModules.Where(x => x.ParentCode == value).ToList();
        modules.ForEach(module =>
        {
            var menu = new Menu()
            {
                Name = module.Name,
                Value = module.Value,
                Child = new List<Menu>(),
                Icon = module.Icon,
                Meta = new Meta() { Title = module.Name },
                Path = module.Path,
            };
            menu.Child.AddRange(GetMenu(miModules, module.Value));
            list.Add(menu);
        });
        return list;
    }

}