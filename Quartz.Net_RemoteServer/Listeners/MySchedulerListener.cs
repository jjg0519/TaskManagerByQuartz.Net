﻿using System;
using Quartz;

using System.Collections.Generic;

using Quartz.Net_RemoteServer.Events;
using Quartz.Net_Infrastructure.LogUtil;
using Quartz.Net_RemoteServer.Models;

namespace Quartz.Net_RemoteServer.Listeners
{
    //TODOThink:是否应该添加调度器表来完成服务属于调度器的状态
    //TODOThink:是否开始异步 减少监听器本身运行的时间
    //TODOThink:log4netkey是否可以动态得到？
    //TODOThink:构造通知类型
    internal class MySchedulerListener : SubjectBase, ISchedulerListener
    {

        //private static string _localIP = string.Empty;
        //private HttpClientHelper _httpClient;
        //const string log4netSchedulerInfoKey = "logSchedulerInfo";
        //const string log4netSchedulerErrorKey = "logSchedulerError";
        //const string log4netJobInfoKey = "logJobInfo";
        //const string log4netJobErrorKey = "logJobError";
        //readonly string quartzServerName = "Bh_Crm_QuartzServer";
        public MySchedulerListener()
        {
            new Observer(this);
            //_httpClient = new HttpClientHelper();
            //_localIP = IPHelper.IpAddress;
        }

        /// <summary>
        /// 任务被部署时执行
        /// </summary>
        /// <param name="jobDetail"></param>
        public void JobAdded(IJobDetail jobDetail)
        {

            Console.WriteLine("任务被部署");
            var jobState = -2;
            var operateType = "部署";
            _postAsync(jobDetail.Key.Name, jobState, operateType);
        }
        /// <summary>
        /// 任务删除时被执行
        /// </summary>
        /// <param name="jobKey"></param>
        public void JobDeleted(JobKey jobKey)
        {
            Console.WriteLine("任务被删除,删除时间为：" + DateTime.Now);
            var jobState = 5;
            var operateType = "删除";
            var isDelete = true;
            _postAsync(jobKey.Name, jobState, operateType, isDelete);
        }
        /// <summary>
        /// 任务暂停时被执行
        /// </summary>
        /// <param name="jobKey"></param>
        public void JobPaused(JobKey jobKey)
        {
            Console.WriteLine("任务被暂停");
            var jobState = 1;
            var operateType = "暂停";
            _postAsync(jobKey.Name, jobState, operateType);
        }
        /// <summary>
        /// 任务恢复时执行
        /// </summary>
        /// <param name="jobKey"></param>
        public void JobResumed(JobKey jobKey)
        {
            Console.WriteLine("任务被恢复");

            var jobState = 6;
            var operateType = "运行";
            _postAsync(jobKey.Name, jobState, operateType);
        }
        /// <summary>
        /// 任务完成使命不在被调用时被执行
        /// </summary>
        /// <param name="trigger"></param>
        public void TriggerFinalized(ITrigger trigger)
        {
            Console.WriteLine("任务完成使命，不在被执行");
        }
        /// <summary>
        /// 调度器发生错误时执行
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="cause"></param>
        public void SchedulerError(string msg, SchedulerException cause)

        {
            var operateType = "运行";
            var operateState = "异常";
            var subject = "调度器运行发生异常";
            _sendMail(subject, cause.GetBaseException(), Log4NetKeys.Log4netSchedulerErrorKey, CustomerLogUtil.Error, operateState, operateType);


            //var schedulerError = CustomerLogFormatUtil.LogSchedulerMsgFormat(_localIP, quartzServerName, operateType, operateState);
            //CustomerLog.Error(log4netSchedulerErrorKey, schedulerError, cause.GetBaseException());
            //_sendMail(subject, schedulerError);
        }
        /// <summary>
        ///调度器启动时被执行
        /// </summary>
        public void SchedulerStarted()
        {
            var operateType = "启动";
            var operateState = "正常";
            var subject = "调度器被启动";
            _sendMail(subject, null, Log4NetKeys.Log4netSchedulerInfoKey, CustomerLogUtil.Info, operateState, operateType);


            //var schedulerInfo = CustomerLogFormatUtil.LogSchedulerMsgFormat(_localIP, quartzServerName, operateType, operateState);
            //CustomerLog.Info(log4netSchedulerInfoKey, schedulerInfo);
            //_sendMail(subject, schedulerInfo);
            Console.WriteLine("调度器被启动");
        }
        /// <summary>
        ///调度器关闭时执行
        /// </summary>
        public void SchedulerShutdown()
        {
            var operateType = "关闭";
            var operateState = "正常";
            var subject = "调度器被关闭";
            _sendMail(subject, null, Log4NetKeys.Log4netSchedulerInfoKey, CustomerLogUtil.Info, operateState, operateType);
            //var schedulerInfo = CustomerLogFormatUtil.LogSchedulerMsgFormat(_localIP, quartzServerName, operateType, operateState);
            //CustomerLog.Info(log4netSchedulerInfoKey, schedulerInfo);
            //_sendMail(subject, schedulerInfo);
            Console.WriteLine("调度器被关闭");
        }

        private void _postAsync(string jobName, int jobState, string operateType, bool IsDelete = false)
        {
            this.NotifyAsync(new JobExcutedCallBackModel { Log4NetKey_JobInfo = Log4NetKeys.Log4netJobInfoKey, IsJobDeleted = IsDelete, JobName = jobName, JobState = jobState, Log4NetKey_JobError = Log4NetKeys.Log4netJobErrorKey, OperateType = operateType, RequestUrl = "", RequestBody = new { JobName = jobName, JobState = jobState, Deleted = IsDelete } });
            //try
            //{
            //    var result = _httpClient.PostAsync(new { JobName = jobName, JobState = jobState, Deleted = IsDelete }).Result;
            //    CustomerLog.Info(log4netJobInfoKey, CustomerLogFormatUtil.LogJobMsgFormat(jobName, jobState, operateType));
            //}
            //catch (Exception ex)
            //{
            //    CustomerLog.Error(log4netJobErrorKey, CustomerLogFormatUtil.LogJobMsgFormat(jobName, jobState, operateType), ex);
            //}
        }

        private void _sendMail(string subject, Exception ex, string log4NetKey_scheduler, Action<string, string, Exception> logSchedulerAction, string operateState, string operateType)
        {
            this.NotifyAsync(new SchedulerExecutedCallBackModel { CCMailAddressList = new List<string>(), Exception = ex, Log4NetKey_Scheduler = log4NetKey_scheduler, LogSchedulerAction = logSchedulerAction, OperateState = operateState, OperateType = operateType, Subject = subject, ToMailAddressList = new List<string> { "chenlong@91bihu.com" } });
        }
        //private void _sendMail(string subject, string body)
        //{
        //    var mailMessageConfigurationInfo = new MailMessageConfigurationInfo { Body = body, ToMailAddressList = new List<string> { "chenlong@91bihu.com" }, Subject = subject };
        //    var mailClient = MailClient.GetInstance;
        //    mailClient.SendMail(mailMessageConfigurationInfo);

        //}

        public void JobScheduled(ITrigger trigger)
        {
            throw new NotImplementedException();
        }

        public void JobUnscheduled(TriggerKey triggerKey)
        {
            throw new NotImplementedException();
        }
        public void SchedulerShuttingdown()
        {

            throw new NotImplementedException();
        }

        public void TriggerPaused(TriggerKey triggerKey)
        {
            throw new NotImplementedException();
        }

        public void TriggersPaused(string triggerGroup)
        {
            throw new NotImplementedException();
        }

        public void TriggerResumed(TriggerKey triggerKey)
        {
            throw new NotImplementedException();
        }

        public void TriggersResumed(string triggerGroup)
        {
            throw new NotImplementedException();
        }

        public void JobsPaused(string jobGroup)
        {
            throw new NotImplementedException();
        }

        public void JobsResumed(string jobGroup)
        {
            throw new NotImplementedException();
        }

        public void SchedulerInStandbyMode()
        {
            throw new NotImplementedException();
        }

        public void SchedulerStarting()
        {
            throw new NotImplementedException();
        }



        public void SchedulingDataCleared()
        {
            throw new NotImplementedException();
        }
    }
}
