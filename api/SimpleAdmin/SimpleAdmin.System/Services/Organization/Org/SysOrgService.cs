﻿// SimpleAdmin 基于 Apache License Version 2.0 协议发布，可用于商业项目，但必须遵守以下补充条款:
// 1.请不要删除和修改根目录下的LICENSE文件。
// 2.请不要删除和修改SimpleAdmin源码头部的版权声明。
// 3.分发源码时候，请注明软件出处 https://gitee.com/zxzyjs/SimpleAdmin
// 4.基于本软件的作品。，只能使用 SimpleAdmin 作为后台服务，除外情况不可商用且不允许二次分发或开源。
// 5.请不得将本软件应用于危害国家安全、荣誉和利益的行为，不能以任何形式用于非法为目的的行为不要删除和修改作者声明。
// 6.任何基于本软件而产生的一切法律纠纷和责任，均于我司无关。

namespace SimpleAdmin.System;

/// <inheritdoc cref="ISysOrgService"/>
public class SysOrgService : DbRepository<SysOrg>, ISysOrgService
{
    private readonly ISimpleCacheService _simpleCacheService;

    public SysOrgService(ISimpleCacheService simpleCacheService)
    {
        _simpleCacheService = simpleCacheService;
    }

    #region 查询

    /// <summary>
    /// 获取全部
    /// </summary>
    /// <returns></returns>
    public override async Task<List<SysOrg>> GetListAsync()
    {
        //先从Redis拿
        var sysOrgList = _simpleCacheService.Get<List<SysOrg>>(SystemConst.CACHE_SYS_ORG);
        if (sysOrgList == null)
        {
            //redis没有就去数据库拿
            sysOrgList = await base.GetListAsync();
            if (sysOrgList.Count > 0)
            {
                //插入Redis
                _simpleCacheService.Set(SystemConst.CACHE_SYS_ORG, sysOrgList);
            }
        }
        return sysOrgList;
    }

    /// <inheritdoc />
    public async Task<SysOrg> GetSysOrgById(long id)
    {
        var sysOrg = await GetListAsync();
        var result = sysOrg.Where(it => it.Id == id).FirstOrDefault();
        return result;
    }

    /// <inheritdoc />
    public async Task<List<SysOrg>> GetChildListById(long orgId, bool isContainOneself = true, List<SysOrg> sysOrgList = null)
    {
        //获取所有组织
        sysOrgList ??= await GetListAsync();
        //查找下级
        var childList = GetSysOrgChildren(sysOrgList, orgId);
        if (isContainOneself)//如果包含自己
        {
            //获取自己的组织信息
            var self = sysOrgList.Where(it => it.Id == orgId).FirstOrDefault();
            if (self != null) childList.Insert(0, self);//如果组织不为空就插到第一个
        }
        return childList;
    }

    /// <inheritdoc />
    public async Task<List<long>> GetOrgChildIds(long orgId, bool isContainOneself = true, List<SysOrg> sysOrgList = null)
    {
        var orgIds = new List<long>();//组织列表
        if (orgId > 0)//如果orgId有值
        {
            //获取所有子集
            var childList = await GetChildListById(orgId, isContainOneself, sysOrgList);
            orgIds = childList.Select(x => x.Id).ToList();//提取ID列表
        }
        return orgIds;
    }

    /// <inheritdoc/>
    public async Task<SqlSugarPagedList<SysOrg>> Page(SysOrgPageInput input)
    {
        var query = Context.Queryable<SysOrg>()
            .WhereIF(input.ParentId > 0, it => it.ParentId == input.ParentId || it.Id == input.ParentId)//父级
            .WhereIF(input.OrgIds != null, it => input.OrgIds.Contains(it.Id))//机构ID查询
            .WhereIF(!string.IsNullOrEmpty(input.Name), it => it.Name.Contains(input.Name))//根据名称查询
            .WhereIF(!string.IsNullOrEmpty(input.Category), it => it.Category == input.Category)//根据分类查询
            .WhereIF(!string.IsNullOrEmpty(input.Code), it => it.Code.Contains(input.Code))//根据编码查询
            .OrderByIF(!string.IsNullOrEmpty(input.SortField), $"{input.SortField} {input.SortOrder}").OrderBy(it => it.SortCode)
            .OrderBy(it => it.CreateTime);//排序
        var pageInfo = await query.ToPagedListAsync(input.PageNum, input.PageSize);//分页
        return pageInfo;
    }

    /// <inheritdoc />
    public async Task<List<SysOrg>> Tree(List<long> orgIds = null, SysOrgTreeInput treeInput = null)
    {
        long parentId = SimpleAdminConst.ZERO;//父级ID
        //获取所有组织
        var sysOrgList = await GetListAsync();
        if (orgIds != null)
            sysOrgList = GetParentListByIds(sysOrgList, orgIds);//如果组织ID不为空则获取组织ID列表的所有父节点
        //如果选择器ID不为空则表示是懒加载,只加载子节点
        if (treeInput != null && treeInput.ParentId != null)
        {
            parentId = treeInput.ParentId.Value;
            sysOrgList = GetSysOrgChildrenLazy(sysOrgList, treeInput.ParentId.Value);//获取懒加载下级
        }
        sysOrgList = sysOrgList.OrderBy(it => it.SortCode).ToList();//排序
        //构建组织树
        var result = ConstructOrgTrees(sysOrgList, parentId);
        return result;
    }

    /// <inheritdoc />
    public async Task<SysOrg> Detail(BaseIdInput input)
    {
        var sysOrgList = await GetListAsync();
        var orgDetail = sysOrgList.Where(it => it.Id == input.Id).FirstOrDefault();
        if (orgDetail.DirectorId != null)
        {
            //获取主管信息
            orgDetail.DirectorInfo = await Tenant.QueryableWithAttr<SysUser>().Where(it => it.Id == orgDetail.DirectorId).Select(it =>
                new UserSelectorOutPut
                {
                    Id = it.Id,
                    Name = it.Name,
                    Account = it.Account
                }).FirstAsync();
        }
        return orgDetail;
    }

    /// <inheritdoc />
    public List<SysOrg> GetOrgParents(List<SysOrg> allOrgList, long orgId, bool includeSelf = true)
    {
        //找到组织
        var sysOrgList = allOrgList.Where(it => it.Id == orgId).FirstOrDefault();
        if (sysOrgList != null)//如果组织不为空
        {
            var data = new List<SysOrg>();
            var parents = GetOrgParents(allOrgList, sysOrgList.ParentId, includeSelf);//递归获取父节点
            data.AddRange(parents);//添加父节点;
            if (includeSelf)
                data.Add(sysOrgList);//添加到列表
            return data;//返回结果
        }
        return new List<SysOrg>();
    }

    /// <inheritdoc />
    public bool IsExistOrgByName(List<SysOrg> sysOrgList, string orgName, long parentId,
        out long orgId)
    {
        orgId = 0;
        var sysOrg = sysOrgList.Where(it => it.ParentId == parentId && it.Name == orgName).FirstOrDefault();
        if (sysOrg != null)
        {
            orgId = sysOrg.Id;
            return true;
        }
        return false;
    }

    /// <inheritdoc />
    public async Task<List<SysOrg>> GetOrgListByIdList(IdListInput input)
    {
        var sysOrgList = await GetListAsync();
        var orgList = sysOrgList.Where(it => input.IdList.Contains(it.Id)).ToList();// 获取指定ID的组织列表
        return orgList;
    }

    #endregion 查询

    #region 新增

    /// <inheritdoc />
    public async Task Add(SysOrgAddInput input, string name = SystemConst.SYS_ORG)
    {
        await CheckInput(input, name);//检查参数
        var sysOrg = input.Adapt<SysOrg>();//实体转换
        if (await InsertAsync(sysOrg))//插入数据
            await RefreshCache();//刷新缓存
    }

    /// <inheritdoc />
    public async Task Copy(SysOrgCopyInput input)
    {
        var orgList = await GetListAsync();//获取所有
        var ids = new HashSet<long>();//定义不重复Id集合
        var addOrgList = new List<SysOrg>();//添加机构列表
        var alreadyIds = new HashSet<long>();//定义已经复制过得组织Id
        ids.AddRange(input.Ids);//加到集合
        if (ids.Contains(input.TargetId))
            throw Oops.Bah("不能包含自己");
        //获取目标组织
        var target = orgList.Where(it => it.Id == input.TargetId).FirstOrDefault();
        if (target != null || input.TargetId == SimpleAdminConst.ZERO)
        {
            //需要复制的组织名称列表
            var orgNames = orgList.Where(it => ids.Contains(it.Id)).Select(it => it.Name).ToList();
            //目标组织的一级子组织名称列表
            var targetChildNames = orgList.Where(it => it.ParentId == input.TargetId).Select(it => it.Name).ToList();
            orgNames.ForEach(it =>
            {
                if (targetChildNames.Contains(it)) throw Oops.Bah($"已存在{it}");
            });

            foreach (var id in input.Ids)
            {
                var org = orgList.Where(o => o.Id == id).FirstOrDefault();//获取下级
                if (org != null && !alreadyIds.Contains(id))
                {
                    alreadyIds.Add(id);//添加到已复制列表
                    RedirectOrg(org);//生成新的实体
                    org.ParentId = input.TargetId;//父id为目标Id
                    addOrgList.Add(org);
                    //是否包含下级
                    if (input.ContainsChild)
                    {
                        var childIds = await GetOrgChildIds(id, false);//获取下级id列表
                        alreadyIds.AddRange(childIds);//添加到已复制id
                        var childList = orgList.Where(c => childIds.Contains(c.Id)).ToList();//获取下级
                        var sysOrgChildren = CopySysOrgChildren(childList, id, org.Id);//赋值下级组织
                        addOrgList.AddRange(sysOrgChildren);
                    }
                }
            }
            //遍历机构重新赋值全称
            addOrgList.ForEach(it =>
            {
                it.Names = it.ParentId == SimpleAdminConst.ZERO ? it.Name : GetNames(orgList, it.ParentId, it.Name);
            });

            if (await InsertRangeAsync(addOrgList))//插入数据
                await RefreshCache();//刷新缓存
        }
    }

    #endregion 新增

    #region 编辑

    /// <inheritdoc />
    public async Task Edit(SysOrgEditInput input, string name = SystemConst.SYS_ORG)
    {
        await CheckInput(input, name);//检查参数
        var sysOrg = input.Adapt<SysOrg>();//实体转换
        if (await UpdateAsync(sysOrg))//更新数据
            await RefreshCache();//刷新缓存
    }

    #endregion 编辑

    #region 删除

    /// <inheritdoc />
    public async Task Delete(BaseIdListInput input, string name = SystemConst.SYS_ORG)
    {
        //获取所有ID
        var ids = input.Ids;
        if (ids.Count > 0)
        {
            var sysOrgList = await GetListAsync();//获取所有组织
            var sysDeleteOrgList = new List<long>();//需要删除的组织ID集合
            ids.ForEach(it =>
            {
                var children = GetSysOrgChildren(sysOrgList, it);//查找下级组织
                sysDeleteOrgList.AddRange(children.Select(it => it.Id).ToList());
                sysDeleteOrgList.Add(it);
            });
            //如果组织下有用户则不能删除
            if (await Context.Queryable<SysUser>().AnyAsync(it => sysDeleteOrgList.Contains(it.OrgId)))
            {
                throw Oops.Bah($"请先删除{name}下的用户");
            }
            //获取用户表有兼任组织的信息，oracle要改成Context.Queryable<SysUser>().Where(it => SqlFunc.Length(it.PositionJson) > 0).Select(it => it.PositionJson).ToListAsync();
            var positionJsons = await Context.Queryable<SysUser>().Where(it => !SqlFunc.IsNullOrEmpty(it.PositionJson)).Select(it => it.PositionJson)
                .ToListAsync();
            if (positionJsons.Count > 0)
            {
                //去一次空
                positionJsons.Where(it => it != null).ToList().ForEach(it =>
                {
                    //获取组织列表
                    var orgIds = it.Select(it => it.OrgId).ToList();
                    //获取交集
                    var sameOrgIds = sysDeleteOrgList.Intersect(orgIds).ToList();
                    if (sameOrgIds.Count > 0)
                    {
                        throw Oops.Bah($"请先删除{name}下的兼任用户");
                    }
                });
            }
            //判断组织下是否有角色
            var hasRole = await Context.Queryable<SysRole>().Where(it => sysDeleteOrgList.Contains(it.OrgId.Value)).CountAsync() > 0;
            if (hasRole) throw Oops.Bah($"请先删除{name}下的角色");
            // 判断组织下是否有职位
            var hasPosition = await Context.Queryable<SysPosition>().Where(it => sysDeleteOrgList.Contains(it.OrgId)).CountAsync() > 0;
            if (hasPosition) throw Oops.Bah($"请先删除{name}下的职位");
            //删除组织
            if (await DeleteByIdsAsync(sysDeleteOrgList.Cast<object>().ToArray()))
                await RefreshCache();//刷新缓存
        }
    }

    #endregion 删除

    #region 其他

    /// <inheritdoc />
    public async Task RefreshCache()
    {
        _simpleCacheService.Remove(SystemConst.CACHE_SYS_ORG);//从redis删除
        _simpleCacheService.Remove(SystemConst.CACHE_SYS_USER);//清空redis所有的用户信息
        await GetListAsync();//刷新缓存
    }

    /// <inheritdoc />
    public List<SysOrg> ConstructOrgTrees(List<SysOrg> orgList, long parentId = 0)
    {
        //找下级字典ID列表
        var orgInfos = orgList.Where(it => it.ParentId == parentId).OrderBy(it => it.SortCode).ToList();
        if (orgInfos.Count > 0)//如果数量大于0
        {
            var data = new List<SysOrg>();
            foreach (var item in orgInfos)//遍历字典
            {
                item.Children = ConstructOrgTrees(orgList, item.Id);//添加子节点
                data.Add(item);//添加到列表
            }
            return data;//返回结果
        }
        return new List<SysOrg>();
    }

    #endregion 其他

    #region 方法

    /// <summary>
    /// 检查输入参数
    /// </summary>
    /// <param name="sysOrg"></param>
    /// <param name="name"></param>
    private async Task CheckInput(SysOrg sysOrg, string name)
    {
        //判断分类是否正确
        if (sysOrg.Category != CateGoryConst.ORG_COMPANY && sysOrg.Category != CateGoryConst.ORG_DEPT)
            throw Oops.Bah($"{name}所属分类错误:{sysOrg.Category}");

        var sysOrgList = await GetListAsync();//获取全部
        if (sysOrgList.Any(it => it.ParentId == sysOrg.ParentId && it.Name == sysOrg.Name && it.Id != sysOrg.Id))//判断同级是否有名称重复的
            throw Oops.Bah($"存在重复的同级{name}:{sysOrg.Name}");
        sysOrg.Names = sysOrg.Name;//全称默认自己
        if (sysOrg.ParentId != 0)
        {
            //获取父级,判断父级ID正不正确
            var parent = sysOrgList.Where(it => it.Id == sysOrg.ParentId).FirstOrDefault();
            if (parent != null)
            {
                if (parent.Id == sysOrg.Id)
                    throw Oops.Bah($"上级{name}不能选择自己");
            }
            else
            {
                throw Oops.Bah($"上级{name}不存在:{sysOrg.ParentId}");
            }
            sysOrg.Names = GetNames(sysOrgList, sysOrg.ParentId, sysOrg.Name);
        }
        //如果code没填
        if (string.IsNullOrEmpty(sysOrg.Code))
        {
            sysOrg.Code = RandomHelper.CreateRandomString(10);//赋值Code
        }
    }

    /// <summary>
    /// 根据组织Id列表获取所有父级组织
    /// </summary>
    /// <param name="allOrgList"></param>
    /// <param name="orgIds"></param>
    /// <returns></returns>
    public List<SysOrg> GetParentListByIds(List<SysOrg> allOrgList, List<long> orgIds)
    {
        var sysOrgList = new HashSet<SysOrg>();//结果列表
        //遍历组织ID
        orgIds.ForEach(it =>
        {
            //获取该组织ID的所有父级
            var parents = GetOrgParents(allOrgList, it);
            sysOrgList.AddRange(parents);//添加到结果
        });
        return sysOrgList.ToList();
    }

    /// <summary>
    /// 获取组织所有下级
    /// </summary>
    /// <param name="orgList"></param>
    /// <param name="parentId"></param>
    /// <returns></returns>
    public List<SysOrg> GetSysOrgChildren(List<SysOrg> orgList, long parentId)
    {
        //找下级组织ID列表
        var orgInfos = orgList.Where(it => it.ParentId == parentId).ToList();
        if (orgInfos.Count > 0)//如果数量大于0
        {
            var data = new List<SysOrg>();
            foreach (var item in orgInfos)//遍历组织
            {
                var children = GetSysOrgChildren(orgList, item.Id);//获取子节点
                data.AddRange(children);//添加子节点);
                data.Add(item);//添加到列表
            }
            return data;//返回结果
        }
        return new List<SysOrg>();
    }

    /// <summary>
    /// 获取组织下级(懒加载)
    /// </summary>
    /// <param name="orgList"></param>
    /// <param name="parentId"></param>
    /// <returns></returns>
    public List<SysOrg> GetSysOrgChildrenLazy(List<SysOrg> orgList, long parentId)
    {
        //找下级组织ID列表
        var orgInfos = orgList.Where(it => it.ParentId == parentId).ToList();
        if (orgInfos.Count > 0)//如果数量大于0
        {
            var data = new List<SysOrg>();
            foreach (var item in orgInfos)//遍历组织
            {
                var children = orgList.Where(it => it.ParentId == item.Id).ToList();//获取子节点
                //遍历子节点
                children.ForEach(it =>
                {
                    if (!orgList.Any(org => org.ParentId == it.Id))
                        it.Leaf = true;//如果没有下级,则设置为叶子节点
                });
                data.AddRange(children);//添加子节点);
                data.Add(item);//添加到列表
            }
            return data;//返回结果
        }
        return new List<SysOrg>();
    }

    /// <summary>
    /// 赋值组织的所有下级
    /// </summary>
    /// <param orgName="orgList">组织列表</param>
    /// <param orgName="parentId">父Id</param>
    /// <param orgName="newParentId">新父Id</param>
    /// <returns></returns>
    public List<SysOrg> CopySysOrgChildren(List<SysOrg> orgList, long parentId, long newParentId)
    {
        //找下级组织列表
        var orgInfos = orgList.Where(it => it.ParentId == parentId).ToList();
        if (orgInfos.Count > 0)//如果数量大于0
        {
            var data = new List<SysOrg>();
            var newId = CommonUtils.GetSingleId();
            foreach (var item in orgInfos)//遍历组织
            {
                var children = CopySysOrgChildren(orgList, item.Id, newId);//获取子节点
                data.AddRange(children);//添加子节点);
                RedirectOrg(item);//实体重新赋值
                item.ParentId = newParentId;//赋值父Id
                data.Add(item);//添加到列表
            }
            return data;//返回结果
        }
        return new List<SysOrg>();
    }

    /// <summary>
    /// 重新生成组织实体
    /// </summary>
    /// <param orgName="org"></param>
    private void RedirectOrg(SysOrg org)
    {
        //重新生成ID并赋值
        var newId = CommonUtils.GetSingleId();
        org.Id = newId;
        org.Code = RandomHelper.CreateRandomString(10);
        org.CreateTime = DateTime.Now;
        org.CreateUser = UserManager.UserAccount;
        org.CreateUserId = UserManager.UserId;
    }

    /// <summary>
    /// 获取全称
    /// </summary>
    /// <param name="sysOrgList">组织列表</param>
    /// <param name="parentId">父Id</param>
    /// <param name="orgName">组织名称</param>
    public string GetNames(List<SysOrg> sysOrgList, long parentId, string orgName)
    {
        var names = "";
        //获取父级菜单
        var parents = GetOrgParents(sysOrgList, parentId);
        parents.ForEach(it => names += $"{it.Name}/");//循环加上名称
        names = names + orgName;//赋值全称
        return names;
    }

    #endregion 方法
}
