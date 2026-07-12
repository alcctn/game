using CleanEnergy.Core;
using CleanEnergy.Save;
using NUnit.Framework;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class CloudStubTests
    {
        [TearDown]
        public void TearDown()
        {
            GameServices.ResetToDefaults();
        }

        [Test]
        public void LocalCloudSaveStore_PassthroughDirectory()
        {
            var store = new LocalCloudSaveStore();
            Assert.IsFalse(store.IsCloudAvailable);
            Assert.AreEqual(@"C:\saves", store.ResolveSaveDirectory(@"C:\saves"));
            Assert.AreEqual(string.Empty, store.ResolveSaveDirectory(null));
        }

        [Test]
        public void SteamCloudStub_UnavailableWithoutSdk()
        {
            var store = new SteamCloudSaveStoreStub();
            Assert.IsFalse(store.IsCloudAvailable);
            Assert.AreEqual("/tmp/saves", store.ResolveSaveDirectory("/tmp/saves"));
        }

        [Test]
        public void GameServices_RegistersLocalByDefault()
        {
            GameServices.ResetToDefaults();
            Assert.IsNotNull(GameServices.CloudSaveStore);
            Assert.IsInstanceOf<LocalCloudSaveStore>(GameServices.CloudSaveStore);
            Assert.IsFalse(GameServices.CloudSaveStore.IsCloudAvailable);
        }

        [Test]
        public void RegisterCloudSaveStore_AcceptsStub()
        {
            var stub = new SteamCloudSaveStoreStub();
            GameServices.RegisterCloudSaveStore(stub);
            Assert.AreSame(stub, GameServices.CloudSaveStore);

            GameServices.RegisterCloudSaveStore(null);
            Assert.IsInstanceOf<LocalCloudSaveStore>(GameServices.CloudSaveStore);
        }

        [Test]
        public void SaveGameService_UsesResolvedDirectory()
        {
            var resolved = GameServices.CloudSaveStore.ResolveSaveDirectory("cloud-root");
            var service = new SaveGameService(resolved);
            Assert.AreEqual("cloud-root", service.SlotDirectory);
        }
    }
}
