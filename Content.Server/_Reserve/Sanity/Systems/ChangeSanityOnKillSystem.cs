using Robust.Server.Player;
using Content.Server.KillTracking;
using Content.Shared.Popups;
using Content.Server.Popups;

public sealed class ChangeSanityOnKillSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<KillReportedEvent>(OnKillReported);
    }

    private void OnKillReported(ref KillReportedEvent ev)
    {
        if(ev.Suicide) return;

        EntityUid killer;
        switch(ev.Primary)
        {
            case KillPlayerSource player:
                killer = _playerManager.GetSessionById(player.PlayerId)?.AttachedEntity ?? EntityUid.Invalid;
                break;
            case KillNpcSource npc:
                killer = npc.NpcEnt;
                break;
            case KillEnvironmentSource _:
                return;
            default:
                return;
        }
        if(killer.IsValid() && HasComp<ImmuneToSanityChangeComponent>(killer)) return;

        _popup.PopupEntity(Loc.GetString("on-kill-other"), killer, killer, PopupType.LargeCaution);
        AddComp<BadSanityComponent>(killer);
    }
}
