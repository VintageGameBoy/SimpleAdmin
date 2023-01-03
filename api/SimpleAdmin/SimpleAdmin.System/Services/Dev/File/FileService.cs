﻿using Furion.Extensions;
using Microsoft.AspNetCore.Mvc;
using NewLife;
using System.DrawingCore;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;

namespace SimpleAdmin.System
{
    /// <summary>
    /// <inheritdoc cref="IFileService"/>
    /// </summary>
    public class FileService : DbRepository<DevFile>, IFileService
    {
        private readonly IConfigService _configService;

        public FileService(IConfigService configService)
        {
            this._configService = configService;
        }



        /// <inheritdoc/>
        public async Task<SqlSugarPagedList<DevFile>> Page(FilePageInput input)
        {
            var query = Context.Queryable<DevFile>()
                             .WhereIF(!string.IsNullOrEmpty(input.Engine), it => it.Engine == input.Engine)//根据关键字查询
                             .WhereIF(!string.IsNullOrEmpty(input.SearchKey), it => it.Name.Contains(input.SearchKey))//根据关键字查询
                             .OrderByIF(!string.IsNullOrEmpty(input.SortField), $"{input.SortField} {input.SortOrder}")//排序
                             .OrderBy(it => it.Id);
            var pageInfo = await query.ToPagedListAsync(input.Current, input.Size);//分页
            return pageInfo;
        }


        /// <inheritdoc/>
        public async Task UploadFile(string engine, IFormFile file)
        {
            await StorageFile(engine, file);

        }

        /// <inheritdoc/>
        public async Task Delete(List<BaseIdInput> input)
        {
            var ids = input.Select(it => it.Id).ToList();//获取ID
            await DeleteByIdsAsync(ids.Cast<object>().ToArray());//根据ID删除数据库

        }

        /// <inheritdoc/>
        public async Task<FileStreamResult> Download(BaseIdInput input)
        {
            var devFile = await GetByIdAsync(input.Id);
            if (devFile != null)
            {
                if (devFile.Engine != DevDictConst.FILE_ENGINE_LOCAL)
                    throw Oops.Bah($"非本地文件不支持此方式下载");
                var fileName = HttpUtility.UrlEncode(devFile.Name, Encoding.GetEncoding("UTF-8"));
                var result = new FileStreamResult(new FileStream(devFile.StoragePath, FileMode.Open), "application/octet-stream") { FileDownloadName = fileName };
                return result;
            }
            else
            {
                return null;
            }

        }


        #region 方法
        /// <summary>
        /// 存储文件
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="file"></param>
        private async Task StorageFile(string engine, IFormFile file)
        {

            string bucketName = string.Empty;    // 存储桶名称
            string storageUrl = string.Empty;// 定义存储的url，本地文件返回文件实际路径，其他引擎返回网络地址
            var objectId = YitIdHelper.NextId();//生成id

            switch (engine)
            {
                //存储本地
                case DevDictConst.FILE_ENGINE_LOCAL:
                    bucketName = "defaultBucketName";// 存储桶名称
                    storageUrl = await StorageLocal(objectId, file);
                    break;
                //存储本地
                case DevDictConst.FILE_ENGINE_MINIO:
                    var config = await _configService.GetByConfigKey(CateGoryConst.Config_FILE_MINIO, DevConfigConst.FILE_MINIO_DEFAULT_BUCKET_NAME);
                    if (config != null)
                    {
                        bucketName = config.ConfigValue;// 存储桶名称
                        storageUrl = await StorageMinio(objectId, file);
                    }
                    break;
                default:

                    throw Oops.Bah($"不支持的文件引擎");
            }
            var fileSizeKb = (long)(file.Length / 1024.0); // 文件大小KB
            var fileSuffix = Path.GetExtension(file.FileName).ToLower(); // 文件后缀
            DevFile devFile = new DevFile
            {
                Id = objectId,
                Engine = engine,
                Bucket = bucketName,
                Name = file.FileName,
                Suffix = fileSuffix.Split(".")[1],
                ObjName = $"{objectId}{fileSuffix}",
                SizeKb = fileSizeKb,
                SizeInfo = GetSizeInfo(fileSizeKb),
                StoragePath = storageUrl,

            };
            if (engine != CateGoryConst.Config_FILE_LOCAL)//如果不是本地，设置下载地址
            {
                devFile.DownloadPath = storageUrl;
            }
            //如果是图片,生成缩略图
            if (IsPic(fileSuffix))
            {

                //$"data:image/png;base64," + imgByte;
                var image = Image.FromStream(file.OpenReadStream());//获取图片

                var thubnail = image.GetThumbnailImage(100, 100, () => false, IntPtr.Zero);//压缩图片
                var thubnailBase64 = ImageUtil.ImgToBase64String(thubnail);//转base64
                devFile.Thumbnail = $"data:image/png;base64," + thubnailBase64;
            }
            await InsertAsync(devFile);

        }

        /// <summary>
        /// 存储本地文件
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="file"></param>
        private async Task<string> StorageLocal(long fileId, IFormFile file)
        {
            string uploadFileFolder;
            string configKey = string.Empty;
            //判断是windos还是linux
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                configKey = DevConfigConst.FILE_LOCAL_FOLDER_FOR_UNIX; //Linux

            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                configKey = DevConfigConst.FILE_LOCAL_FOLDER_FOR_WINDOWS;  //Windows
            }
            //获取路径配置
            var config = await _configService.GetByConfigKey(CateGoryConst.Config_FILE_LOCAL, configKey);
            if (config != null)
            {
                uploadFileFolder = config.ConfigValue;//赋值路径
                var now = DateTime.Now.ToString("d");
                var filePath = Path.Combine(uploadFileFolder, now);
                if (!Directory.Exists(filePath))//如果不存在就创建文件夹
                    Directory.CreateDirectory(filePath);
                var fileSuffix = Path.GetExtension(file.FileName).ToLower(); // 文件后缀
                var fileObjectName = $"{fileId}{fileSuffix}";//存储后的文件名
                var fileName = Path.Combine(filePath, fileObjectName);//获取文件全路局
                fileName = fileName.Replace("\\", "/");//格式化一系
                //存储文件
                using (var stream = File.Create(Path.Combine(filePath, fileObjectName)))
                {
                    await file.CopyToAsync(stream);
                }
                return fileName;

            }
            else
            {
                throw Oops.Oh($"文件存储路径未配置");
            }
        }

        /// <summary>
        /// 存储到minio
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="file"></param>
        /// <returns></returns>

        private async Task<string> StorageMinio(long fileId, IFormFile file)
        {
            var minioService = App.GetService<MinioUtils>();
            var now = DateTime.Now.ToString("d");
            var fileSuffix = Path.GetExtension(file.FileName).ToLower(); // 文件后缀
            var fileObjectName = $"{now}/{fileId}{fileSuffix}";//存储后的文件名
            return await minioService.PutObjectAsync(fileObjectName, file.OpenReadStream(), file.Length);


        }

        /// <summary>
        /// 获取文件大小
        /// </summary>
        /// <param name="fileSizeKb"></param>
        /// <returns></returns>
        private string GetSizeInfo(long fileSizeKb)
        {

            var b = fileSizeKb * 1024;
            const int MB = 1024 * 1024;
            const int KB = 1024;
            if (b / MB >= 1)
            {
                return Math.Round(b / (float)MB, 2) + "MB";
            }

            if (b / KB >= 1)
            {
                return Math.Round(b / (float)KB, 2) + "KB";
            }
            if (b == 0)
            {
                return "0B";
            }
            return null;
        }

        /// <summary>
        /// 判断是否是图片
        /// </summary>
        /// <param name="suffix">后缀名</param>
        /// <returns></returns>
        private bool IsPic(string suffix)
        {
            //图片后缀名列表
            var pics = new string[]
            {
                ".png", ".bmp", ".gif", ".jpg", ".jpeg",".psd"
            };
            if (pics.Contains(suffix))
                return true;
            else
                return false;
        }

        #endregion
    }
}