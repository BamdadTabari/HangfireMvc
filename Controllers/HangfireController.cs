using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HangfireMvc.Controllers
{
    public class HangfireController : Controller
    {
        // GET: Hangfire
        // GET: Hangfire
        public ActionResult Index()
        {
            // Schedule a job to execute after 1 minute
            BackgroundJob.Schedule(() => Console.WriteLine("Scheduled Job executed"), TimeSpan.FromMinutes(1));

            // Enqueue a fire-and-forget job
            BackgroundJob.Enqueue(() => Console.WriteLine("Fire-and-forget Job executed"));

            // Schedule a recurring job
            RecurringJob.AddOrUpdate("RecurringJob", () => Console.WriteLine("Recurring Job executed"), Cron.Minutely);

            RecurringJob.AddOrUpdate("RecurringJob", () => Console.WriteLine("Recurring Job executed"), Cron.Daily(hour:01, minute: 01));
            return Content("Hangfire jobs scheduled!");
        }
    }
}