# SPRINT 60 — NOTIFICATION SFX

## 1. Sprint Amacı

Bildirim ve yerleştirme olaylarına kısa SFX eklemek; mute PlayerPrefs ile kalıcı.

## 2. Kapsam

Dahil:

- `SfxService.Play(SfxId)` + `ce_sfx_mute`
- Hook: place success, demolish, shortage, research unlock, battery full
- Null clip = no-op; runtime sine stub (`AudioClip.Create`)
- `NotificationHudUI` Mute SFX toggle

Dahil değil:

- Müzik / ambience
- Spatial 3D audio

## 3. Definition of Done

- Mute açıkken Play sayacı artmaz
- Yerleştirme / demolish / bildirim hook’ları çalışır
- Testler geçer
