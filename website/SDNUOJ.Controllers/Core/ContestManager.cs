﻿using System;
using System.Collections.Generic;

using SDNUOJ.Caching;
using SDNUOJ.Controllers.Core.Exchange;
using SDNUOJ.Controllers.Exception;
using SDNUOJ.Data;
using SDNUOJ.Entity;
using SDNUOJ.Entity.Complex;
using SDNUOJ.Utilities;
using SDNUOJ.Utilities.Text.RegularExpressions;

namespace SDNUOJ.Controllers.Core
{
    /// <summary>
    /// 竞赛数据管理类
    /// </summary>
    internal static class ContestManager
    {
        #region 常量
        /// <summary>
        /// 竞赛页面页面大小
        /// </summary>
        private const Int32 CONTEST_PAGE_SIZE = 20;
        #endregion

        #region 用户方法
        /// <summary>
        /// 根据ID得到一个竞赛实体
        /// </summary>
        /// <param name="id">竞赛ID</param>
        /// <returns>竞赛实体</returns>
        public static ContestEntity GetContest(Int32 id)
        {
            if (id < ContestRepository.NONECONTEST)
            {
                throw new InvalidRequstException(RequestType.Contest);
            }

            ContestEntity contest = ContestCache.GetContestCache(id);//获取缓存

            if (contest == null)
            {
                contest = ContestRepository.Instance.GetEntity(id);
                ContestCache.SetContestCache(contest);//设置缓存
            }

            if (contest == null)
            {
                throw new NullResponseException(RequestType.Contest);
            }

            if (contest.IsHide && !AdminManager.HasPermission(PermissionType.ContestManage))
            {
                throw new NoPermissionException("You have no privilege to view the contest!");
            }

            return contest;
        }

        /// <summary>
        /// 根据ID得到一个需要注册的竞赛实体
        /// </summary>
        /// <param name="id">竞赛ID</param>
        /// <returns>竞赛实体</returns>
        public static ContestEntity GetRegisterContest(Int32 id)
        {
            ContestEntity contest = ContestManager.GetContest(id);

            if (contest.ContestType != ContestType.RegisterPrivate && contest.ContestType != ContestType.RegisterPublic)
            {
                if (contest.ContestType == ContestType.Public)
                {
                    throw new NoPermissionException("You don't need to register for this contest!");
                }
                else
                {
                    throw new NoPermissionException("You can not register for this contest!");
                }
            }

            if (contest.RegisterStartTime > DateTime.Now)
            {
                throw new NoPermissionException("This contest registration has not been started yet!");
            }

            if (contest.RegisterEndTime < DateTime.Now)
            {
                throw new NoPermissionException("This contest has registration ended!");
            }

            return contest;
        }

        /// <summary>
        /// 获取竞赛列表
        /// </summary>
        /// <param name="pageIndex">页面索引</param>
        /// <param name="passed">是否已过去的竞赛</param>
        /// <returns>竞赛列表</returns>
        public static PagedList<ContestEntity> GetContestList(Int32 pageIndex, Boolean passed)
        {
            Int32 pageSize = AdminManager.ADMIN_LIST_PAGE_SIZE;
            Int32 recordCount = ContestManager.CountContests(passed);

            return ContestRepository.Instance
                .GetEntities(pageIndex, pageSize, recordCount, passed)
                .ToPagedList(pageSize, recordCount);
        }

        /// <summary>
        /// 获取公告总数(有缓存)
        /// </summary>
        /// <param name="passed">是否已过去的竞赛</param>
        /// <returns>竞赛总数</returns>
        public static Int32 CountContests(Boolean passed)
        {
            Int32 recordCount = ContestCache.GetContestListCountCache(passed);//获取缓存

            if (recordCount < 0)
            {
                recordCount = ContestRepository.Instance.CountEntities(passed);
                ContestCache.SetContestListCountCache(passed, recordCount);//设置缓存
            }

            return recordCount;
        }

        /// <summary>
        /// 获取竞赛排行
        /// </summary>
        /// <param name="entity">竞赛实体</param>
        /// <returns>竞赛排行</returns>
        public static List<RankItem> GetContestRanklist(ContestEntity entity)
        {
            List<RankItem> list = ContestCache.GetContestRankCache(entity.ContestID);

            if (list == null)
            {
                list = new List<RankItem>();
                Dictionary<String, RankItem> rank = SolutionRepository.Instance.GetContestRanklist(entity.ContestID, entity.StartTime);

                foreach (RankItem userRank in rank.Values)
                {
                    list.Add(userRank);
                }

                list.Sort();

                ContestCache.SetContestRankCache(entity.ContestID, list);
            }

            return list;
        }
        #endregion

        #region 管理方法
        /// <summary>
        /// 增加一条竞赛
        /// </summary>
        /// <param name="entity">竞赛实体</param>
        /// <returns>是否成功增加</returns>
        public static IMethodResult AdminInsertContest(ContestEntity entity)
        {
            if (!AdminManager.HasPermission(PermissionType.ContestManage))
            {
                throw new NoPermissionException();
            }

            if (String.IsNullOrEmpty(entity.Title))
            {
                return MethodResult.FailedAndLog("Contest title cannot be NULL!");
            }

            if (String.IsNullOrEmpty(entity.Description))
            {
                return MethodResult.FailedAndLog("Contest description cannot be NULL!");
            }

            if (entity.StartTime >= entity.EndTime)
            {
                return MethodResult.FailedAndLog("Start time must be less than end time!");
            }

            if (entity.ContestType == ContestType.RegisterPrivate || entity.ContestType == ContestType.RegisterPublic)
            {
                if (!entity.RegisterStartTime.HasValue || !entity.RegisterEndTime.HasValue)
                {
                    return MethodResult.FailedAndLog("Register time cannot be NULL!");
                }

                if (entity.RegisterStartTime >= entity.RegisterEndTime)
                {
                    return MethodResult.FailedAndLog("Register start time must be less than register end time!");
                }

                if (entity.RegisterEndTime >= entity.StartTime)
                {
                    return MethodResult.FailedAndLog("Register end time must be less than contest start time!");
                }
            }

            entity.IsHide = true;
            entity.LastDate = DateTime.Now;

            Boolean success = ContestRepository.Instance.InsertEntity(entity) > 0;

            if (!success)
            {
                return MethodResult.FailedAndLog("No contest was added!");
            }

            ContestCache.RemoveContestListCountCache();//删除缓存

            return MethodResult.SuccessAndLog("Admin add contest, title = {0}", entity.Title);
        }

        /// <summary>
        /// 更新竞赛信息
        /// </summary>
        /// <param name="entity">对象实体</param>
        /// <returns>是否成功更新</returns>
        public static IMethodResult AdminUpdateContest(ContestEntity entity)
        {
            if (!AdminManager.HasPermission(PermissionType.ContestManage))
            {
                throw new NoPermissionException();
            }

            if (String.IsNullOrEmpty(entity.Title))
            {
                return MethodResult.FailedAndLog("Contest title cannot be NULL!");
            }

            if (String.IsNullOrEmpty(entity.Description))
            {
                return MethodResult.FailedAndLog("Contest description cannot be NULL!");
            }

            if (entity.StartTime >= entity.EndTime)
            {
                return MethodResult.FailedAndLog("Start time must be less than end time!");
            }

            if (entity.ContestType == ContestType.RegisterPrivate || entity.ContestType == ContestType.RegisterPublic)
            {
                if (!entity.RegisterStartTime.HasValue || !entity.RegisterEndTime.HasValue)
                {
                    return MethodResult.FailedAndLog("Register time cannot be NULL!");
                }

                if (entity.RegisterStartTime >= entity.RegisterEndTime)
                {
                    return MethodResult.FailedAndLog("Register start time must be less than register end time!");
                }

                if (entity.RegisterEndTime >= entity.StartTime)
                {
                    return MethodResult.FailedAndLog("Register end time must be less than contest start time!");
                }
            }

            entity.LastDate = DateTime.Now;

            Boolean success = ContestRepository.Instance.UpdateEntity(entity) > 0;

            if (!success)
            {
                return MethodResult.FailedAndLog("No contest was updated!");
            }

            ContestCache.RemoveContestCache(entity.ContestID);//删除缓存
            ContestCache.RemoveContestListCountCache();//删除缓存

            return MethodResult.SuccessAndLog("Admin update contest, id = {0}", entity.ContestID.ToString());
        }

        /// <summary>
        /// 更新竞赛隐藏状态
        /// </summary>
        /// <param name="ids">竞赛ID列表</param>
        /// <param name="isHide">隐藏状态</param>
        /// <returns>是否成功更新</returns>
        public static IMethodResult AdminUpdateContestIsHide(String ids, Boolean isHide)
        {
            if (!AdminManager.HasPermission(PermissionType.ContestManage))
            {
                throw new NoPermissionException();
            }

            if (!RegexVerify.IsNumericIDs(ids))
            {
                return MethodResult.InvalidRequest(RequestType.Contest);
            }

            Boolean success = ContestRepository.Instance.UpdateEntityIsHide(ids, isHide) > 0;

            if (!success)
            {
                return MethodResult.FailedAndLog("No contest was {0}!", isHide ? "hided" : "unhided");
            }

            ids.ForEachInIDs(',', id =>
            {
                ContestCache.RemoveContestCache(id);//删除缓存
            });

            ContestCache.RemoveContestListCountCache();//删除缓存

            return MethodResult.SuccessAndLog("Admin {1} contest, id = {0}", ids, isHide ? "hide" : "unhide");
        }

        /// <summary>
        /// 获取竞赛排行Excel文件
        /// </summary>
        /// <param name="cid">竞赛ID</param>
        /// <param name="userrealnames">用户姓名对照表</param>
        /// <returns>竞赛排行</returns>
        public static IMethodResult AdminGetExportRanklist(Int32 cid, String userrealnames)
        {
            if (!AdminManager.HasPermission(PermissionType.ContestManage))
            {
                throw new NoPermissionException();
            }

            ContestEntity contest = ContestManager.GetContest(cid);
            Dictionary<String, String> userdict = null;
            Dictionary<String, RankItem> rank = SolutionRepository.Instance.GetContestRanklist(contest.ContestID, contest.StartTime);
            List<ContestProblemEntity> problemlist = ContestProblemManager.GetContestProblemList(contest.ContestID);
            List<RankItem> list = new List<RankItem>();

            foreach (RankItem userRank in rank.Values)
            {
                list.Add(userRank);
            }

            list.Sort();

            if (!String.IsNullOrEmpty(userrealnames))
            {
                userdict = new Dictionary<String, String>();
                String[] nametable = userrealnames.Lines();

                for (Int32 i = 0; i < nametable.Length; i++)
                {
                    if (String.IsNullOrEmpty(nametable[i]))
                    {
                        continue;
                    }

                    String[] namepair = nametable[i].Replace('\t', ' ').Split(' ');

                    if (namepair.Length == 2 && !String.IsNullOrEmpty(namepair[0]) && !String.IsNullOrEmpty(namepair[1]))
                    {
                        userdict.Add(namepair[0], namepair[1]);
                    }
                }
            }

            Byte[] data = ContestResultExport.ExportResultToExcel(contest, problemlist, list, userdict);

            return MethodResult.SuccessAndLog<Byte[]>(data, "Admin export contest result, id = {0}", cid.ToString());
        }

        /// <summary>
        /// 根据ID得到一个竞赛实体
        /// </summary>
        /// <param name="id">竞赛ID</param>
        /// <returns>竞赛实体</returns>
        public static IMethodResult AdminGetContest(Int32 id)
        {
            if (!AdminManager.HasPermission(PermissionType.ContestManage))
            {
                throw new NoPermissionException();
            }

            if (id < ContestRepository.NONECONTEST)
            {
                return MethodResult.InvalidRequest(RequestType.Contest);
            }

            ContestEntity entity = ContestRepository.Instance.GetEntity(id);

            if (entity == null)
            {
                return MethodResult.NotExist(RequestType.Contest);
            }

            return MethodResult.Success(entity);
        }

        /// <summary>
        /// 获取竞赛列表
        /// </summary>
        /// <param name="pageIndex">页面索引</param>
        /// <returns>竞赛列表</returns>
        public static PagedList<ContestEntity> AdminGetContestList(Int32 pageIndex)
        {
            if (!AdminManager.HasPermission(PermissionType.ContestManage))
            {
                throw new NoPermissionException();
            }

            Int32 pageSize = AdminManager.ADMIN_LIST_PAGE_SIZE;
            Int32 recordCount = ContestManager.AdminCountContests();

            return ContestRepository.Instance
                .GetEntities(pageIndex, pageSize, recordCount)
                .ToPagedList(pageSize, recordCount);
        }

        /// <summary>
        /// 获取竞赛总数
        /// </summary>
        /// <returns>竞赛总数</returns>
        private static Int32 AdminCountContests()
        {
            return ContestRepository.Instance.CountEntities();
        }
        #endregion

        #region 内部方法
        /// <summary>
        /// 判断给定ID的竞赛是否存在
        /// </summary>
        /// <param name="id">竞赛ID</param>
        /// <returns>竞赛是否存在</returns>
        internal static Boolean InternalExistsContest(Int32 id)
        {
            return (ContestManager.GetContest(id) != null);
        }
        #endregion
    }
}