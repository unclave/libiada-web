﻿namespace LibiadaWeb.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using LibiadaWeb.Models.Account;
    using System.Data.Entity.Validation;
    using System.Data.Entity;
    using Newtonsoft.Json;

    /// <summary>
    /// The task manager.
    /// </summary>
    public static class TaskManager
    {
        /// <summary>
        /// Machine core count.
        /// </summary>
        private static readonly int CoreCount = Environment.ProcessorCount;

        /// <summary>
        /// Gets the tasks.
        /// </summary>
        private static readonly List<Task> Tasks = new List<Task>();

        /// <summary>
        /// The created tasks counter.
        /// </summary>
        private static int taskCounter;

        /// <summary>
        /// The add task.
        /// </summary>
        /// <param name="task">
        /// The task.
        /// </param>
        public static void AddTask(Task task)
        {
            lock (Tasks)
            {
                Tasks.Add(task);
                using (var db = new LibiadaWebEntities())
                {
                    var dbTask = new CalculationTask()
                    {
                        created = DateTime.Now,
                        started = task.TaskData.Started,
                        description = task.TaskData.DisplayName,
                        status = (short)task.TaskData.TaskState,
                        user_id = Convert.ToInt32(task.TaskData.UserId),
                        task_type = (short)task.taskType
                    };

                    db.CalculationTask.Add(dbTask);
                    db.SaveChanges();
                    task.TaskData.Id = (int)dbTask.id;
                }
                TasksManagerHub.Send(TaskEvent.addTask, task.TaskData);
            }


            ManageTasks();
        }

        /// <summary>
        /// The get id.
        /// </summary>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public static int GetId()
        {
            return taskCounter++;
        }

        /// <summary>
        /// The clear tasks.
        /// </summary>
        public static void DeleteAllTasks()
        {
            lock (Tasks)
            {
                var tasks = Tasks;

                if (!UserHelper.IsAdmin())
                {
                    tasks = tasks.Where(t => t.TaskData.UserId == UserHelper.GetUserId()).ToList();
                }

                while (tasks.Count > 0)
                {
                    var task = tasks.Last();
                    lock (task)
                    {
                        if (task.Thread != null && task.Thread.IsAlive)
                        {
                            task.Thread.Abort();
                        }

                        tasks.Remove(task);
                        Tasks.Remove(task);

                        TasksManagerHub.Send(TaskEvent.deleteTask, task.TaskData);
                    }
                }
            }
        }

        /// <summary>
        /// The delete task.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        public static void DeleteTask(int id)
        {
            lock (Tasks)
            {
                var task = Tasks.Single(t => t.TaskData.Id == id);
                if (task.TaskData.UserId == UserHelper.GetUserId() || UserHelper.IsAdmin())
                {
                    lock (task)
                    {
                        if (task.Thread != null && task.Thread.IsAlive)
                        {
                            task.Thread.Abort();
                        }

                        Tasks.Remove(task);
                        TasksManagerHub.Send(TaskEvent.deleteTask, task.TaskData);
                    }
                }
            }
        }

        /// <summary>
        /// The get tasks data.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{TaskData}"/>.
        /// </returns>
        public static IEnumerable<TaskData> GetTasksData()
        {
            lock (Tasks)
            {
                var tasks = Tasks;

                if (!UserHelper.IsAdmin())
                {
                    tasks = tasks.Where(t => t.TaskData.UserId == UserHelper.GetUserId()).ToList();
                }

                return tasks.Select(t => t.TaskData.Clone());
            }
        }

        /// <summary>
        /// The get task.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public static Task GetTask(int id)
        {
            Task result;
            lock (Tasks)
            {
                var task = Tasks.Single(t => t.TaskData.Id == id);

                lock (task)
                {
                    if (!UserHelper.IsAdmin() && task.TaskData.UserId != UserHelper.GetUserId())
                    {
                        throw new AccessViolationException("You do not have access to current task");
                    }

                    result = task.Clone();
                }
            }

            return result;
        }

        /// <summary>
        /// The manage tasks.
        /// </summary>
        private static void ManageTasks()
        {
            lock (Tasks)
            {
                int activeTasks = 0;
                foreach (var task in Tasks)
                {
                    lock (task)
                    {
                        if (task.TaskData.TaskState == TaskState.InProgress)
                        {
                            activeTasks++;
                        }
                    }
                }

                while (activeTasks < CoreCount)
                {
                    activeTasks++;
                    var taskToStart = Tasks.FirstOrDefault(t => t.TaskData.TaskState == TaskState.InQueue);
                    if (taskToStart != null)
                    {
                        lock (taskToStart)
                        {
                            taskToStart.TaskData.TaskState = TaskState.InProgress;
                            StartTask(taskToStart.TaskData.Id);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// The create action.
        /// </summary>
        /// <param name="task">
        /// The task.
        /// </param>
        private static void Action(Task task)
        {
            try
            {
                Func<Dictionary<string, object>> method;
                lock (task)
                {
                    method = task.Action;
                    task.TaskData.Started = DateTime.Now;
                    ///////////
                    using (var db = new LibiadaWebEntities())
                    {
                        var dbTask = db.CalculationTask.Single(t => (t.id == task.TaskData.Id));

                        dbTask.started = DateTime.Now;
                        db.Entry(dbTask).State = EntityState.Modified;
                        db.SaveChanges();
                      
                    }
                    ///////////
                    TasksManagerHub.Send(TaskEvent.changeStatus, task.TaskData);
                }

                var result = method();
                lock (task)
                {
                    task.TaskData.Completed = DateTime.Now;
                    task.TaskData.ExecutionTime = task.TaskData.Completed - task.TaskData.Started;
                    task.Result = result;
                    task.TaskData.TaskState = TaskState.Completed;
                    using (var db = new LibiadaWebEntities())
                    {
                        var dbTask = db.CalculationTask.Single(t => (t.id == task.TaskData.Id));

                        dbTask.completed = DateTime.Now;
                        dbTask.result = JsonConvert.SerializeObject(result["data"]);
                        db.Entry(dbTask).State = EntityState.Modified;
                        db.SaveChanges();

                    }
                    TasksManagerHub.Send(TaskEvent.changeStatus, task.TaskData);
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message;
                string stackTrace = e.StackTrace;

                while (e.InnerException != null)
                {
                    e = e.InnerException;
                    errorMessage += "<br/>" + e.Message;
                }

                lock (task)
                {
                    task.TaskData.Completed = DateTime.Now;
                    task.TaskData.ExecutionTime = task.TaskData.Completed - task.TaskData.Started;
                    task.TaskData.TaskState = TaskState.Error;
                    task.Result = new Dictionary<string, object>
                                      {
                                          { "Error", true },
                                          { "ErrorMessage", errorMessage },
                                          { "StackTrace", stackTrace }
                                      };
                    TasksManagerHub.Send(TaskEvent.changeStatus, task.TaskData);
                }
            }

            ManageTasks();
        }

        /// <summary>
        /// The start task.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        private static void StartTask(int id)
        {
            Task taskToStart;
            lock (Tasks)
            {
                taskToStart = Tasks.Single(t => t.TaskData.Id == id);
            }

            Action<Task> action = Action;

            var thread = new Thread(() => action(taskToStart));
            taskToStart.Thread = thread;
            thread.Start();
        }
    }
}
