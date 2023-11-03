/**
 * @description 角色管理
 * @license Apache License Version 2.0
 * @remarks
 * SimpleAdmin 基于 Apache License Version 2.0 协议发布，可用于商业项目，但必须遵守以下补充条款:
 * 1.请不要删除和修改根目录下的LICENSE文件。
 * 2.请不要删除和修改SimpleAdmin源码头部的版权声明。
 * 3.分发源码时候，请注明软件出处 https://gitee.com/zxzyjs/SimpleAdmin
 * 4.基于本软件的作品，只能使用 SimpleAdmin 作为后台服务，除外情况不可商用且不允许二次分发或开源。
 * 5.请不得将本软件应用于危害国家安全、荣誉和利益的行为，不能以任何形式用于非法为目的的行为不要删除和修改作者声明。
 * 6.任何基于本软件而产生的一切法律纠纷和责任，均于我司无关
 * @see https://gitee.com/zxzyjs/SimpleAdmin
 */

import { ReqId, ResPage, SysRole } from "@/api";

import { moduleRequest } from "@/api/request";

const http = moduleRequest("/sys/limit/role/");

/** 获取角色分页 */
export const SysRolePageApi = (params: SysRole.Page) => {
  return http.get<ResPage<SysRole.SysRoleInfo>>("page", params);
};

/** 获取角色树 */
export const SysRoleTreeApi = () => {
  return http.get<SysRole.SysRoleTree[]>("tree");
};

/** 获取角色详情 */
export const SysRoleDetailApi = (params: ReqId) => {
  return http.get<SysRole.SysRoleInfo>("detail", params);
};

/**  提交表单 edit为true时为编辑，默认为新增 */
export const SysRoleSubmitFormApi = (params: {}, edit: boolean = false) => {
  return http.post(edit ? "edit" : "add", params);
};

/** 删除角色 */
export const SysRoleDeleteApi = (params: ReqId[]) => {
  return http.post("delete", params);
};
