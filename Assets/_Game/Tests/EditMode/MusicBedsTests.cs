using CleanEnergy.Audio;
using CleanEnergy.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class MusicBedsTests
    {
        [Test]
        public void ResolveClip_NightUsesNightElseLoop()
        {
            var day = AudioClip.Create("day", 8, 1, 22050, false);
            var night = AudioClip.Create("night", 8, 1, 22050, false);
            var loop = AudioClip.Create("loop", 8, 1, 22050, false);

            Assert.AreSame(night, MusicService.ResolveClipForPhase(DayPhase.Night, day, night, loop));
            Assert.AreSame(loop, MusicService.ResolveClipForPhase(DayPhase.Night, day, null, loop));
            Assert.IsNull(MusicService.ResolveClipForPhase(DayPhase.Night, day, null, null));
        }

        [Test]
        public void ResolveClip_DayPhasesPreferDayElseLoop()
        {
            var day = AudioClip.Create("day2", 8, 1, 22050, false);
            var night = AudioClip.Create("night2", 8, 1, 22050, false);
            var loop = AudioClip.Create("loop2", 8, 1, 22050, false);

            Assert.AreSame(day, MusicService.ResolveClipForPhase(DayPhase.Noon, day, night, loop));
            Assert.AreSame(day, MusicService.ResolveClipForPhase(DayPhase.Morning, day, night, loop));
            Assert.AreSame(day, MusicService.ResolveClipForPhase(DayPhase.Evening, day, night, loop));
            Assert.AreSame(loop, MusicService.ResolveClipForPhase(DayPhase.Noon, null, night, loop));
        }
    }
}
