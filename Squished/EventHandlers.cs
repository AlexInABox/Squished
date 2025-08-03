using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using PlayerStatsSystem;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

namespace Squished;

public static class EventHandlers
{
    public static void RegisterEvents()
    {
        AudioClipStorage.LoadClip(Plugin.Instance.Config.BonkSoundEffectPath, "squished_sound_effect");
        PlayerEvents.Hurting += OnHurting;
    }

    public static void UnregisterEvents()
    {
        PlayerEvents.Hurting -= OnHurting;
    }

    private static void OnHurting(PlayerHurtingEventArgs ev)
    {
        float fallDamageDealt = FallDamage(ev.DamageHandler);
        if (fallDamageDealt <= 0f) return;

        int bonkCount = 0;
        Collider[] hitColliders = Physics.OverlapSphere(ev.Player.Position, 1f);
        foreach (Collider collider in hitColliders)
            if (Player.TryGet(collider.gameObject, out Player player))
            {
                if (player.PlayerId == ev.Player.PlayerId || player.IsGodModeEnabled) continue;
                DamageHandlerBase damageHandler =
                    new CustomReasonDamageHandler("Zerquetscht von " + ev.Player.Nickname + "!", fallDamageDealt);
                player.Damage(damageHandler);
                bonkCount++;
            }

        PlaySoundEffect(ev.Player.Position, bonkCount);
    }

    private static float FallDamage(DamageHandlerBase dHandler)
    {
        if (dHandler is UniversalDamageHandler uHandler)
        {
            Logger.Debug(uHandler.ServerLogsText);
            if (uHandler.ServerLogsText.Contains("Fall")) return uHandler.Damage;
        }

        return 0f;
    }

    private static void PlaySoundEffect(Vector3 pos, int bonkCount)
    {
        if (bonkCount <= 0) return;
        AudioPlayer audioPlayer = AudioPlayer.CreateOrGet("squished_audioplayer" + pos.GetHashCode());
        audioPlayer.AddSpeaker("squished_speaker" + pos.GetHashCode(), pos, 5F + bonkCount, true, 5F, 1000F);
        audioPlayer.DestroyWhenAllClipsPlayed = true;
        audioPlayer.AddClip("squished_sound_effect", 5F + bonkCount);

        Logger.Debug("Playing sound effect at position: " + pos);
    }
}