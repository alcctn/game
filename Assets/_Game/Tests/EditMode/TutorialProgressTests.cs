using CleanEnergy.Tutorial;
using NUnit.Framework;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class TutorialProgressTests
    {
        [Test]
        public void StartsAtCamera()
        {
            var service = new TutorialProgressService();
            Assert.AreEqual(TutorialStepId.Camera, service.CurrentStep);
            Assert.IsFalse(service.IsComplete);
        }

        [Test]
        public void CompletingCurrent_AdvancesInOrder()
        {
            var service = new TutorialProgressService();
            Assert.IsTrue(service.TryComplete(TutorialStepId.Camera));
            Assert.AreEqual(TutorialStepId.OpenWaterLayer, service.CurrentStep);
            Assert.IsTrue(service.TryComplete(TutorialStepId.OpenWaterLayer));
            Assert.AreEqual(TutorialStepId.PlaceWaterWheel, service.CurrentStep);
        }

        [Test]
        public void SkipAhead_IsRejected()
        {
            var service = new TutorialProgressService();
            Assert.IsFalse(service.TryComplete(TutorialStepId.PlaceBattery));
            Assert.AreEqual(TutorialStepId.Camera, service.CurrentStep);
        }

        [Test]
        public void Restore_SetsCurrentStep()
        {
            var service = new TutorialProgressService();
            service.Restore(TutorialStepId.UnlockSolar);
            Assert.AreEqual(TutorialStepId.UnlockSolar, service.CurrentStep);
            Assert.IsTrue(service.TryComplete(TutorialStepId.UnlockSolar));
            Assert.AreEqual(TutorialStepId.PlaceSolar, service.CurrentStep);
        }

        [Test]
        public void CompletingAll_ReachesCompleted()
        {
            var service = new TutorialProgressService();
            var completed = false;
            service.Completed += () => completed = true;

            for (var i = 0; i < TutorialProgressService.StepCount; i++)
            {
                Assert.IsTrue(service.TryComplete((TutorialStepId)i));
            }

            Assert.IsTrue(service.IsComplete);
            Assert.AreEqual(TutorialStepId.Completed, service.CurrentStep);
            Assert.IsTrue(completed);
            Assert.IsFalse(service.TryComplete(TutorialStepId.MeetDemand));
        }

        [Test]
        public void Reset_ReturnsToCamera()
        {
            var service = new TutorialProgressService();
            service.TryComplete(TutorialStepId.Camera);
            service.Reset();
            Assert.AreEqual(TutorialStepId.Camera, service.CurrentStep);
        }

        [Test]
        public void Sequence_PlaceBatteryAfterPlaceSolar_NoBatteryResearchStep()
        {
            Assert.Greater((int)TutorialStepId.PlaceBattery, (int)TutorialStepId.PlaceSolar);
            Assert.AreEqual(TutorialStepId.PlaceSolar + 1, TutorialStepId.PlaceBattery);

            var names = System.Enum.GetNames(typeof(TutorialStepId));
            Assert.That(names, Does.Not.Contain("UnlockBattery"));

            var info = TutorialProgressService.GetInfo(TutorialStepId.PlaceBattery);
            Assert.That(info.Title, Does.Not.Contain("Research").IgnoreCase);
            Assert.That(info.Hint, Does.Contain("unlocked").IgnoreCase);
        }

        [Test]
        public void PlaceBattery_CompletesWhenActive()
        {
            var service = new TutorialProgressService();
            service.Restore(TutorialStepId.PlaceBattery);
            Assert.IsTrue(service.TryComplete(TutorialStepId.PlaceBattery));
            Assert.AreEqual(TutorialStepId.MeetDemand, service.CurrentStep);
        }

        [Test]
        public void OrderedInfos_MatchEnumThroughMeetDemand()
        {
            Assert.AreEqual(9, TutorialProgressService.StepCount);
            Assert.AreEqual(TutorialStepId.UnlockSolar, TutorialProgressService.GetInfo(TutorialStepId.UnlockSolar).Id);
            Assert.AreEqual(TutorialStepId.PlaceSolar, TutorialProgressService.GetInfo(TutorialStepId.PlaceSolar).Id);
            Assert.AreEqual(TutorialStepId.PlaceBattery, TutorialProgressService.GetInfo(TutorialStepId.PlaceBattery).Id);
        }
    }
}
