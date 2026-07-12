using CleanEnergy.Simulation;
using NUnit.Framework;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class WeatherEventTests
    {
        [Test]
        public void DeterministicRoll_IsXor()
        {
            Assert.AreEqual(120 ^ 42, WeatherEventService.DeterministicRoll(120, 42));
            Assert.AreEqual(WeatherEventService.HashSeed("abc"), WeatherEventService.HashSeed("abc"));
            Assert.AreNotEqual(WeatherEventService.HashSeed("a"), WeatherEventService.HashSeed("b"));
        }

        [Test]
        public void Cloudy_HalvesSolarMultiplier()
        {
            var weather = new WeatherEventService();
            weather.Start(WeatherEventKind.Cloudy);
            Assert.AreEqual(WeatherEventKind.Cloudy, weather.ActiveKind);
            Assert.AreEqual(WeatherEventService.CloudySolarMultiplier, weather.SolarMultiplier, 0.001f);
            Assert.AreEqual(WeatherEventService.CloudyDurationTicks, weather.RemainingTicks);
            Assert.AreEqual(1f, weather.WindMultiplier, 0.001f);
        }

        [Test]
        public void WindGust_BoostsWindMultiplier()
        {
            var weather = new WeatherEventService();
            weather.Start(WeatherEventKind.WindGust);
            Assert.AreEqual(WeatherEventService.WindGustWindMultiplier, weather.WindMultiplier, 0.001f);
            Assert.AreEqual(WeatherEventService.WindGustDurationTicks, weather.RemainingTicks);
        }

        [Test]
        public void Context_AppliesWeatherToDaylightAndWind()
        {
            var context = new SimulationContext(
                1, 0.5f, SimulationSpeed.One, 0.3f, DayPhase.Noon, 0.5f, 1.4f);
            Assert.AreEqual(
                DayCycleService.GetDaylightFactor(DayPhase.Noon) * 0.5f,
                context.DaylightFactor,
                0.001f);
            Assert.AreEqual(
                DayCycleService.GetWindFactor(DayPhase.Noon) * 1.4f,
                context.WindFactor,
                0.001f);
        }

        [Test]
        public void Advance_ExpiresActiveEvent()
        {
            var weather = new WeatherEventService();
            weather.Start(WeatherEventKind.Cloudy);
            for (var i = 0; i < WeatherEventService.CloudyDurationTicks; i++)
            {
                weather.Advance(1, 0);
            }

            Assert.AreEqual(WeatherEventKind.None, weather.ActiveKind);
            Assert.AreEqual(1f, weather.SolarMultiplier, 0.001f);
        }
    }
}
