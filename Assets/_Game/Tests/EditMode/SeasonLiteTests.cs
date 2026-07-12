using CleanEnergy.Simulation;
using NUnit.Framework;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class SeasonLiteTests
    {
        [Test]
        public void MultiplierTable_MatchesSpec()
        {
            Assert.AreEqual(1f, SeasonService.GetSolarMultiplier(SeasonKind.Spring), 0.001f);
            Assert.AreEqual(1f, SeasonService.GetWindMultiplier(SeasonKind.Spring), 0.001f);
            Assert.AreEqual(1.2f, SeasonService.GetSolarMultiplier(SeasonKind.Summer), 0.001f);
            Assert.AreEqual(0.85f, SeasonService.GetWindMultiplier(SeasonKind.Summer), 0.001f);
            Assert.AreEqual(0.9f, SeasonService.GetSolarMultiplier(SeasonKind.Autumn), 0.001f);
            Assert.AreEqual(1.15f, SeasonService.GetWindMultiplier(SeasonKind.Autumn), 0.001f);
            Assert.AreEqual(0.7f, SeasonService.GetSolarMultiplier(SeasonKind.Winter), 0.001f);
            Assert.AreEqual(1.25f, SeasonService.GetWindMultiplier(SeasonKind.Winter), 0.001f);
        }

        [Test]
        public void ResolveSeason_AdvancesEveryDaysPerSeason()
        {
            var d = SeasonService.DaysPerSeason;
            Assert.AreEqual(SeasonKind.Spring, SeasonService.ResolveSeason(0));
            Assert.AreEqual(SeasonKind.Spring, SeasonService.ResolveSeason(d - 1));
            Assert.AreEqual(SeasonKind.Summer, SeasonService.ResolveSeason(d));
            Assert.AreEqual(SeasonKind.Autumn, SeasonService.ResolveSeason(d * 2));
            Assert.AreEqual(SeasonKind.Winter, SeasonService.ResolveSeason(d * 3));
            Assert.AreEqual(SeasonKind.Spring, SeasonService.ResolveSeason(d * 4));
        }

        [Test]
        public void SyncFromDayIndex_UpdatesCurrentAndMultipliers()
        {
            var seasons = new SeasonService();
            seasons.SyncFromDayIndex(SeasonService.DaysPerSeason + 10);
            Assert.AreEqual(SeasonKind.Summer, seasons.Current);
            Assert.AreEqual(SeasonService.SummerSolar, seasons.SolarMultiplier, 0.001f);
            Assert.AreEqual(SeasonService.SummerWind, seasons.WindMultiplier, 0.001f);
        }

        [Test]
        public void Context_AppliesSeasonThenWeather()
        {
            var context = new SimulationContext(
                1, 0.5f, SimulationSpeed.One, 0.3f, DayPhase.Noon,
                weatherSolarMultiplier: 0.5f,
                weatherWindMultiplier: 1.4f,
                seasonSolarMultiplier: 1.2f,
                seasonWindMultiplier: 0.85f);

            Assert.AreEqual(
                DayCycleService.GetDaylightFactor(DayPhase.Noon) * 1.2f * 0.5f,
                context.DaylightFactor,
                0.001f);
            Assert.AreEqual(
                DayCycleService.GetWindFactor(DayPhase.Noon) * 0.85f * 1.4f,
                context.WindFactor,
                0.001f);
        }

        [Test]
        public void DisplayName_MatchesEnum()
        {
            Assert.AreEqual("Spring", SeasonService.DisplayName(SeasonKind.Spring));
            Assert.AreEqual("Winter", SeasonService.DisplayName(SeasonKind.Winter));
        }
    }
}
