using Microsoft.Win32.TaskScheduler;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace winaudits
{
    public class RunningTasks
    {
        [JsonProperty("taskname")]
        public string TaskName { get; set; }
        [JsonProperty("nextruntime")]
        public DateTime NextRunTime { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
    }

    public class TaskAuditor
    {
        public static List<RunningTasks> StartAudit()
        {
            List<RunningTasks> lstTask = new List<RunningTasks>();
            try
            {
                using (TaskService ts = new TaskService())
                {
                    //Gets List Of Tasks in a folder. For Root Folder use backslash

                    TaskFolder taskFolder = ts.GetFolder(@"\");
                    TaskCollection col = taskFolder.Tasks;
                    foreach (Task task in col)
                    {
                        RunningTasks tss = new RunningTasks();
                        tss.TaskName = task.Name;
                        tss.NextRunTime = task.NextRunTime;
                        tss.Status = task.State.ToString();

                        lstTask.Add(tss);
                    }
                }
            }
            catch(Exception)
            {

            }
            return lstTask;
        }
    }
}
