﻿namespace SimpleAdmin.Plugin.SignalR;


/// <summary>
/// 即时通讯集线器
/// </summary>
public interface ISimpleHub
{
    /// <summary>
    /// 退出登录
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    Task LoginOut(object context);
}
