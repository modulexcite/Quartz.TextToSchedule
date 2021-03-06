﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz.Impl.Calendar;
using Quartz;

namespace Quartz.TextToSchedule.Test
{
    [TestClass]
    public class GoogleAppEngineSyntaxTests
    {
        //syntax:
        //every N (hours|mins|minutes) ["from" (time) "to" (time)]
        //("every"|ordinal) (days) ["of" (monthspec)] (time)

        //every 12 hours
        //every 5 minutes from 10:00 to 14:00
        //2nd,third mon,wed,thu of march 17:00
        //every monday 09:00
        //1st monday of sep,oct,nov 17:00
        //every day 00:00
        //every 2 hours from 10:00 to 14:00
        //every 2 hours synchronized

        public ITextToSchedule tts { get; private set; }

        public GoogleAppEngineSyntaxTests()
        {
            tts = new TextToScheduleFactory().CreateEnglishParser();
        }

        [TestMethod]
        public void Every12Hours()
        {
            string text = "every 12 hours";
            var results = tts.Parse(text);

            Assert.AreEqual(1, results.RegisterGroups.Count);
            var group = results.RegisterGroups[0];
            var trigger = group.TriggerBuilder.Build();

            TestHelper.AssertHasTimeIntervalOf(trigger, 12, IntervalUnit.Hour);
        }

        [TestMethod]
        public void Every5MinutesFrom10to14()
        {
            string text = "every 5 minutes from 10:00 to 14:00";
            var results = tts.Parse(text);

            Assert.AreEqual(1, results.RegisterGroups.Count);
            var group = results.RegisterGroups[0];

            TestHelper.AssertDailyCalendarIsTimeIncluded(group, 12, 0, 0);
            TestHelper.AssertDailyCalendarIsTimeIncluded(group, 10, 0, 0);
            TestHelper.AssertDailyCalendarIsTimeIncluded(group, 14, 0, 0);

            TestHelper.AssertDailyCalendarIsTimeExcluded(group, 9, 0, 0);
            TestHelper.AssertDailyCalendarIsTimeExcluded(group, 15, 0, 0);
            TestHelper.AssertDailyCalendarIsTimeExcluded(group, 16, 0, 1);
        }

        [TestMethod]
        public void SecondThirdMonWedThuOfMarch1700()
        {
            string text = "2nd,third mon,wed,thu of march 17:00";
            var results = tts.Parse(text);

            //there should be a group for each 
            //ordinal and day of week combination
            Assert.AreEqual(6, results.RegisterGroups.Count);

            TestHelper.AssertHasCronExpression(results, "0 0 17 ? MAR MON#2");
            TestHelper.AssertHasCronExpression(results, "0 0 17 ? MAR WED#2");
            TestHelper.AssertHasCronExpression(results, "0 0 17 ? MAR THU#2");
            TestHelper.AssertHasCronExpression(results, "0 0 17 ? MAR MON#3");
            TestHelper.AssertHasCronExpression(results, "0 0 17 ? MAR WED#3");
            TestHelper.AssertHasCronExpression(results, "0 0 17 ? MAR THU#3");
        }

        [TestMethod]
        public void EveryMonday900()
        {
            string text = "every Monday 9:00";
            var results = tts.Parse(text);

            Assert.AreEqual(1, results.RegisterGroups.Count);

            TestHelper.AssertHasCronExpression(results, "0 0 9 ? * MON");
        }

        [TestMethod]
        public void FirstMondayOfSepOctNov1700()
        {
            string text = "1st Monday of sep,oct,nov 17:00";
            var results = tts.Parse(text);

            Assert.AreEqual(1, results.RegisterGroups.Count);

            TestHelper.AssertHasCronExpression(results, "0 0 17 ? SEP,OCT,NOV MON#1");
        }

        [TestMethod]
        public void EveryDay0000()
        {
            string text = "every day 00:00";
            var results = tts.Parse(text);

            Assert.AreEqual(1, results.RegisterGroups.Count);

            TestHelper.AssertHasCronExpression(results, "0 0 0 ? * *");
        }

        [TestMethod]
        public void Every2HoursFrom10to14()
        {
            string text = "every 2 hours from 10:00 to 14:00";
            var results = tts.Parse(text);

            Assert.AreEqual(1, results.RegisterGroups.Count);
            var group = results.RegisterGroups[0];
            var trigger = group.TriggerBuilder.Build();

            TestHelper.AssertHasTimeIntervalOf(trigger, 2, IntervalUnit.Hour);

            TestHelper.AssertDailyCalendarIsTimeIncluded(group, 12, 0, 0);
            TestHelper.AssertDailyCalendarIsTimeIncluded(group, 10, 0, 0);
            TestHelper.AssertDailyCalendarIsTimeIncluded(group, 14, 0, 0);

            TestHelper.AssertDailyCalendarIsTimeExcluded(group, 9, 0, 0);
            TestHelper.AssertDailyCalendarIsTimeExcluded(group, 15, 0, 0);
            TestHelper.AssertDailyCalendarIsTimeExcluded(group, 16, 0, 1);
        }

        [TestMethod]
        public void Every2HoursSynchronized()
        {
            string text = "every 2 hours synchronized";
            var results = tts.Parse(text);

            Assert.AreEqual(1, results.RegisterGroups.Count);
            var group = results.RegisterGroups[0];
            var trigger = group.TriggerBuilder.Build();

            TestHelper.AssertHasTimeIntervalOf(trigger, 2, IntervalUnit.Hour);
            TestHelper.AssertHasStartTimeOfDayOf(trigger, 0, 0, 0);
        }
    }
}
