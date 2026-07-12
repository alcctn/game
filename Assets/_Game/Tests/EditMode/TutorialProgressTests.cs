using CleanEnergy.Tutorial;
using NUnit.Framework;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class TutorialProgressTests
    {
        [Test]
        public void Starts_AtCamera()
        {
            var service = new TutorialProgressService();
            Assert.AreEqual(TutorialStepId.Camera, service.CurrentStep);
        }

        [Test]
        public void Completes_InOrder()
        {
            var service = new TutorialProgressService();
            Assert.IsTrue(service.TryComplete(TutorialStepId.Camera));
            Assert.AreEqual(TutorialStepId.HireEngineer, service.CurrentStep);
            Assert.IsTrue(service.TryComplete(TutorialStepId.HireEngineer));
            Assert.AreEqual(TutorialStepId.PlaceWaterWheel, service.CurrentStep);
        }

        [Test]
        public void Rejects_OutOfOrder()
        {
            var service = new TutorialProgressService();
            Assert.IsFalse(service.TryComplete(TutorialStepId.PlaceWind));
            Assert.AreEqual(TutorialStepId.Camera, service.CurrentStep);
        }

        [Test]
        public void Restore_AllowsContinue()
        {
            var service = new TutorialProgressService();
            service.Restore(TutorialStepId.HireTechnician);
            Assert.AreEqual(TutorialStepId.HireTechnician, service.CurrentStep);
            Assert.IsTrue(service.TryComplete(TutorialStepId.HireTechnician));
            Assert.AreEqual(TutorialStepId.PlaceWind, service.CurrentStep);
        }

        [Test]
        public void CompletingAll_RaisesCompleted()
        {
            var service = new TutorialProgressService();
            var completed = 0;
            service.Completed += () => completed++;
            for (var i = 0; i < TutorialProgressService.StepCount; i++)
            {
                Assert.IsTrue(service.TryComplete((TutorialStepId)i));
            }

            Assert.IsTrue(service.IsComplete);
            Assert.AreEqual(TutorialStepId.Completed, service.CurrentStep);
            Assert.AreEqual(1, completed);
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
        public void Sequence_WindAfterTechnician()
        {
            Assert.AreEqual(TutorialStepId.HireTechnician + 1, TutorialStepId.PlaceWind);
            var info = TutorialProgressService.GetInfo(TutorialStepId.PlaceWind);
            Assert.AreEqual(TutorialStepId.PlaceWind, info.Id);
        }

        [Test]
        public void PlaceWind_CompletesWhenActive()
        {
            var service = new TutorialProgressService();
            service.Restore(TutorialStepId.PlaceWind);
            Assert.IsTrue(service.TryComplete(TutorialStepId.PlaceWind));
            Assert.AreEqual(TutorialStepId.MeetDemand, service.CurrentStep);
        }

        [Test]
        public void StepCount_IsSix()
        {
            Assert.AreEqual(6, TutorialProgressService.StepCount);
            Assert.AreEqual(TutorialStepId.HireEngineer, TutorialProgressService.GetInfo(TutorialStepId.HireEngineer).Id);
            Assert.AreEqual(TutorialStepId.PlaceWind, TutorialProgressService.GetInfo(TutorialStepId.PlaceWind).Id);
        }
    }
}
