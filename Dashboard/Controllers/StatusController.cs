﻿using Dashboard.Helpers;
using Dashboard.Models;
using Roslyn.Sql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace Dashboard.Controllers
{
    public class StatusController : Controller
    {
        private readonly SqlUtil _sqlUtil;
        private readonly TestCacheStats _testCacheStats;
        private readonly TestResultStorage _testResultStorage;

        public StatusController()
        {
            var connectionString = ConfigurationManager.AppSettings["jenkins-connection-string"];
            _sqlUtil = new SqlUtil(connectionString);
            _testCacheStats = new TestCacheStats(_sqlUtil);
            _testResultStorage = new TestResultStorage(_sqlUtil);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _sqlUtil.Dispose();
            }
        }

        public ActionResult Index()
        {
            return RedirectToAction(nameof(Tests));
        }

        public ActionResult Tests([FromUri] bool all = false)
        {
            var startDate = all
                ? (DateTime?)null
                : DateTime.UtcNow - TimeSpan.FromDays(1);
            return View(_testCacheStats.GetSummary(startDate));
        }

        public ActionResult Result(string id)
        {
            TestResult testResult;
            if (!_testResultStorage.TryGetValue(id, out testResult))
            {
                throw new Exception("Invalid key");
            }

            var contentType = testResult.ResultsFileName.EndsWith("xml")
                ? "application/xml"
                : "text/html";
            return Content(testResult.ResultsFileContent, contentType);
        }

        public ActionResult Results()
        {
            return View(_testResultStorage.Keys);
        }

        public ActionResult Errors()
        {
            return View(StorageLogger.Instance.EntryList);
        }

        public ActionResult TestRuns([FromUri] string startDate = null, [FromUri] string endDate = null)
        {
            var startDateTime = ParameterToDateTime(startDate);
            var endDateTime = ParameterToDateTime(endDate);

            var testRunList = _sqlUtil.GetTestRuns(startDateTime, endDateTime);
            var compList = GetTestRunComparisons(testRunList);
            return View(compList);
        }

        private static Dictionary<DateTime, List<TestRun>> GetTestRunGroupedByDate(List<TestRun> testRunList)
        {
            // First group the data by date
            var map = new Dictionary<DateTime, List<TestRun>>();
            foreach (var cur in testRunList)
            {
                if (!cur.Succeeded || !cur.IsJenkins || cur.Cache == "test" || cur.AssemblyCount < 35 || cur.Elapsed.Ticks == 0)
                {
                    continue;
                }

                var date = cur.RunDate.Date;
                List<TestRun> list;
                if (!map.TryGetValue(date, out list))
                {
                    list = new List<TestRun>();
                    map[date] = list;
                }

                list.Add(cur);
            }

            return map;
        }

        /// <summary>
        /// Break up the test runs into a list of comparisons by the date on which they occured. 
        /// </summary>
        private static List<TestRunComparison> GetTestRunComparisons(List<TestRun> testRunList)
        {
            var map = GetTestRunGroupedByDate(testRunList);
            var compList = new List<TestRunComparison>(map.Count);
            var fullList = new List<TimeSpan>();
            var chunkList = new List<TimeSpan>();
            var legacyList = new List<TimeSpan>();
            foreach (var pair in map.OrderBy(x => x.Key))
            {
                fullList.Clear();
                chunkList.Clear();
                legacyList.Clear();

                var countCached = 0;
                var countHighCached = 0;
                var countNoCached = 0;

                foreach (var item in pair.Value)
                {
                    if (item.CacheCount > 0)
                    {
                        fullList.Add(item.Elapsed);
                    }
                    else if (item.ChunkCount > 0 && item.ChunkCount > item.AssemblyCount)
                    {
                        chunkList.Add(item.Elapsed);
                    }
                    else
                    {
                        legacyList.Add(item.Elapsed);
                    }

                    if (item.CacheCount > 0)
                    {
                        countCached++;
                        if (((double)item.CacheCount / item.ChunkCount) > .5)
                        {
                            countHighCached++;
                        }
                    }
                    else
                    {
                        countNoCached++;
                    }
                }

                compList.Add(new TestRunComparison()
                {
                    Date = pair.Key,
                    TimeCached = Average(fullList),
                    TimeNoCache = Average(chunkList),
                    TimeLegacy = Average(legacyList),
                    Count = pair.Value.Count,
                    CountHighCached = countHighCached,
                    CountCached = countCached,
                    CountNoCached = countNoCached
                });
            }

            return compList;
        }

        private static TimeSpan Average(IEnumerable<TimeSpan> e)
        {
            if (!e.Any())
            {
                return TimeSpan.FromSeconds(0);
            }

            var average = e.Average(x => x.Ticks);
            return TimeSpan.FromTicks((long)average);
        }

        private static DateTime? ParameterToDateTime(string p)
        {
            if (string.IsNullOrEmpty(p))
            {
                return null;
            }

            DateTime dateTime;
            if (DateTime.TryParse(p, out dateTime))
            {
                return dateTime;
            }

            return null;
        }
    }
}