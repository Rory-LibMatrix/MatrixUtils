// See https://aka.ms/new-console-template for more information

using System.Collections.Frozen;
using System.Collections.Immutable;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Running;

[SimpleJob(RunStrategy.ColdStart, launchCount: 1, warmupCount: 5, iterationCount: 5, id: "FastAndDirtyJob")]
[InProcess]
[ProcessCount(4)]
public class Program {
    public static void Main(string[] args) {
        BenchmarkRunner.Run<Program>(args: args);
    }

    [Params(true, false)]
    public bool DoDisambiguate { get; set; } = true;

    [Params(true, false)]
    public bool DisambiguateProfileUpdates {
        get => field && DoDisambiguate;
        set;
    } = true;

    [Params(true, false)]
    public bool DisambiguateKicks {
        get => field && DoDisambiguate;
        set;
    } = true;

    [Params(true, false)]
    public bool DisambiguateUnbans {
        get => field && DoDisambiguate;
        set;
    } = true;

    [Params(true, false)]
    public bool DisambiguateInviteAccepted {
        get => field && DoDisambiguate && DisambiguateInviteActions;
        set;
    } = true;

    [Params(true, false)]
    public bool DisambiguateInviteRejected {
        get => field && DoDisambiguate && DisambiguateInviteActions;
        set;
    } = true;

    [Params(true, false)]
    public bool DisambiguateInviteRetracted {
        get => field && DoDisambiguate && DisambiguateInviteActions;
        set;
    } = true;

    [Params(true, false)]
    public bool DisambiguateKnockAccepted {
        get => field && DoDisambiguate && DisambiguateKnockActions;
        set;
    } = true;

    [Params(true, false)]
    public bool DisambiguateKnockRejected {
        get => field && DoDisambiguate && DisambiguateKnockActions;
        set;
    } = true;

    [Params(true, false)]
    public bool DisambiguateKnockRetracted {
        get => field && DoDisambiguate && DisambiguateKnockActions;
        set;
    } = true;

    [Params(true, false)]
    public bool DisambiguateKnockActions {
        get => field && DoDisambiguate;
        set;
    } = true;

    [Params(true, false)]
    public bool DisambiguateInviteActions {
        get => field && DoDisambiguate;
        set;
    } = true;

    public enum MembershipTransition : uint {
        None,
        Join = 0b0001,
        Leave = 0b0010,
        Knock = 0b0100,
        Invite = 0b1000,
        Ban = 0b1001,

        // disambiguated
        ProfileUpdate = 0b0000_0001_0001,
        Kick = 0b0000_0001_0010,
        Unban = 0b0000_0010_0010,
        InviteAccepted = 0b0000_0100_0001,
        InviteRejected = 0b0000_1000_0010,
        InviteRetracted = 0b0001_0000_0010,
        KnockAccepted = 0b0010_0000_1000,
        KnockRejected = 0b0100_0000_0010,
        KnockRetracted = 0b1000_0000_0010
    }

    public readonly struct MembershipEntry {
        public required MembershipTransition State { get; init; }
        public string Aba { get; init; }
        public string Abb { get; init; }
        public string Abc { get; init; }
        public string Abd { get; init; }
    }

    [Params(100, 10_000, 1_000_000)] public int N;

    [GlobalSetup]
    public void Setup() {
        entries = Enumerable.Range(0, N).Select(_ => new MembershipEntry() {
            State = (MembershipTransition)new Random().Next(1, 16),
            Aba = Guid.NewGuid().ToString(),
            Abb = Guid.NewGuid().ToString(),
            Abc = Guid.NewGuid().ToString(),
            Abd = Guid.NewGuid().ToString()
        }).ToImmutableList();
    }

    public ImmutableList<MembershipEntry> entries = ImmutableList<MembershipEntry>.Empty;

    [Benchmark]
    public void TestTruthyness() {
        var @switch = AmbiguateMembershipsSwitch().GetEnumerator();
        var @switchpm = AmbiguateMembershipsSwitchPatternMatching().GetEnumerator();
        var @if = AmbiguateMembershipsIf().GetEnumerator();
        var @map = AmbiguateMembershipsStaticMap().GetEnumerator();
        var @binmask = AmbiguateMembershipsBinMask().GetEnumerator();

        while (@switch.MoveNext() && @map.MoveNext() && @if.MoveNext() && @switchpm.MoveNext() && @binmask.MoveNext()) {
            if (@switch.Current.State != @map.Current.State || @switch.Current.State != @if.Current.State || @switch.Current.State != @switchpm.Current.State ||
                @switch.Current.State != @binmask.Current.State) {
                throw new InvalidOperationException("Results do not match!");
            }
        }

        @switch.Dispose();
        @switchpm.Dispose();
        @if.Dispose();
        @map.Dispose();
        @binmask.Dispose();
    }

    [Benchmark]
    public void TestAmbiguateMembershipsSwitchPatternMatching() => AmbiguateMembershipsSwitchPatternMatching().Consume(new Consumer());

    public IEnumerable<MembershipEntry> AmbiguateMembershipsSwitchPatternMatching() {
        foreach (var entry in entries) {
            var newState = entry.State switch {
                MembershipTransition.ProfileUpdate when !DoDisambiguate || !DisambiguateProfileUpdates => MembershipTransition.Join,
                MembershipTransition.Kick when !DoDisambiguate || !DisambiguateKicks => MembershipTransition.Leave,
                MembershipTransition.Unban when !DoDisambiguate || !DisambiguateUnbans => MembershipTransition.Leave,
                MembershipTransition.InviteAccepted when !DoDisambiguate || !DisambiguateInviteActions || !DisambiguateInviteAccepted => MembershipTransition.Join,
                MembershipTransition.InviteRejected when !DoDisambiguate || !DisambiguateInviteActions || !DisambiguateInviteRejected => MembershipTransition.Leave,
                MembershipTransition.InviteRetracted when !DoDisambiguate || !DisambiguateInviteActions || !DisambiguateInviteRetracted => MembershipTransition.Leave,
                MembershipTransition.KnockAccepted when !DoDisambiguate || !DisambiguateKnockActions || !DisambiguateKnockAccepted => MembershipTransition.Invite,
                MembershipTransition.KnockRejected when !DoDisambiguate || !DisambiguateKnockActions || !DisambiguateKnockRejected => MembershipTransition.Leave,
                MembershipTransition.KnockRetracted when !DoDisambiguate || !DisambiguateKnockActions || !DisambiguateKnockRetracted => MembershipTransition.Leave,
                _ => entry.State
            };
            yield return newState == entry.State ? entry : entry with { State = newState };
        }
    }

    [Benchmark]
    public void TestAmbiguateMembershipsSwitch() => AmbiguateMembershipsSwitch().Consume(new Consumer());

    public IEnumerable<MembershipEntry> AmbiguateMembershipsSwitch() {
        foreach (var entry in entries) {
            if (!DoDisambiguate) {
                yield return entry;
                continue;
            }

            MembershipTransition newState;
            switch (entry.State) {
                case MembershipTransition.ProfileUpdate:
                    newState = !DisambiguateProfileUpdates ? MembershipTransition.Join : entry.State;
                    break;
                case MembershipTransition.Kick:
                    newState = !DisambiguateKicks ? MembershipTransition.Leave : entry.State;
                    break;
                case MembershipTransition.Unban when !DisambiguateUnbans:
                    newState = MembershipTransition.Leave;
                    break;
                case MembershipTransition.InviteAccepted when !DisambiguateInviteActions || !DisambiguateInviteAccepted:
                    newState = MembershipTransition.Join;
                    break;
                case MembershipTransition.InviteRejected when !DisambiguateInviteActions || !DisambiguateInviteRejected:

                    newState = MembershipTransition.Leave;
                    break;
                case MembershipTransition.InviteRetracted when !DisambiguateInviteActions || !DisambiguateInviteRetracted:
                    newState = MembershipTransition.Leave;
                    break;
                case MembershipTransition.KnockAccepted when !DisambiguateKnockActions || !DisambiguateKnockAccepted:
                    newState = MembershipTransition.Invite;
                    break;
                case MembershipTransition.KnockRejected when !DisambiguateKnockActions || !DisambiguateKnockRejected:
                    newState = MembershipTransition.Leave;
                    break;
                case MembershipTransition.KnockRetracted when !DisambiguateKnockActions || !DisambiguateKnockRetracted:
                    newState = MembershipTransition.Leave;
                    break;
                default:
                    newState = entry.State;
                    break;
            }

            yield return newState == entry.State ? entry : entry with { State = newState };
        }
    }

    [Benchmark]
    public void TestAmbiguateMembershipsIf() => AmbiguateMembershipsIf().Consume(new Consumer());

    public IEnumerable<MembershipEntry> AmbiguateMembershipsIf() {
        foreach (var entry in entries) {
            MembershipTransition newState;
            if (entry.State == MembershipTransition.ProfileUpdate && (!DoDisambiguate || !DisambiguateProfileUpdates))
                newState = MembershipTransition.Join;
            else if ((entry.State == MembershipTransition.Kick && (!DoDisambiguate || !DisambiguateKicks)) ||
                     (entry.State == MembershipTransition.Unban && (!DoDisambiguate || !DisambiguateUnbans)))
                newState = MembershipTransition.Leave;
            else if (entry.State == MembershipTransition.InviteAccepted && (!DoDisambiguate || !DisambiguateInviteActions || !DisambiguateInviteAccepted))
                newState = MembershipTransition.Join;
            else if ((entry.State == MembershipTransition.InviteRejected && (!DoDisambiguate || !DisambiguateInviteActions || !DisambiguateInviteRejected)) ||
                     (entry.State == MembershipTransition.InviteRetracted && (!DoDisambiguate || !DisambiguateInviteActions || !DisambiguateInviteRetracted)))
                newState = MembershipTransition.Leave;
            else if (entry.State == MembershipTransition.KnockAccepted && (!DoDisambiguate || !DisambiguateKnockActions || !DisambiguateKnockAccepted))
                newState = MembershipTransition.Invite;
            else if ((entry.State == MembershipTransition.KnockRejected && (!DoDisambiguate || !DisambiguateKnockActions || !DisambiguateKnockRejected)) ||
                     (entry.State == MembershipTransition.KnockRetracted && (!DoDisambiguate || !DisambiguateKnockActions || !DisambiguateKnockRetracted)))
                newState = MembershipTransition.Leave;
            else
                newState = entry.State;

            yield return newState == entry.State ? entry : entry with { State = newState };
        }
    }

    [Benchmark]
    public void TestAmbiguateMembershipsStaticMap() => AmbiguateMembershipsStaticMap().Consume(new Consumer());

    public IEnumerable<MembershipEntry> AmbiguateMembershipsStaticMap() {
        Dictionary<MembershipTransition, MembershipTransition> _map = [];
        if (!DoDisambiguate || !DisambiguateProfileUpdates) _map[MembershipTransition.ProfileUpdate] = MembershipTransition.Join;
        if (!DoDisambiguate || !DisambiguateKicks) _map[MembershipTransition.Kick] = MembershipTransition.Leave;
        if (!DoDisambiguate || !DisambiguateUnbans) _map[MembershipTransition.Unban] = MembershipTransition.Leave;
        if (!DoDisambiguate || !DisambiguateInviteActions || !DisambiguateInviteAccepted) _map[MembershipTransition.InviteAccepted] = MembershipTransition.Join;
        if (!DoDisambiguate || !DisambiguateInviteActions || !DisambiguateInviteRejected) _map[MembershipTransition.InviteRejected] = MembershipTransition.Leave;
        if (!DoDisambiguate || !DisambiguateInviteActions || !DisambiguateInviteRetracted) _map[MembershipTransition.InviteRetracted] = MembershipTransition.Leave;
        if (!DoDisambiguate || !DisambiguateKnockActions || !DisambiguateKnockAccepted) _map[MembershipTransition.KnockAccepted] = MembershipTransition.Invite;
        if (!DoDisambiguate || !DisambiguateKnockActions || !DisambiguateKnockRejected) _map[MembershipTransition.KnockRejected] = MembershipTransition.Leave;
        if (!DoDisambiguate || !DisambiguateKnockActions || !DisambiguateKnockRetracted) _map[MembershipTransition.KnockRetracted] = MembershipTransition.Leave;
        FrozenDictionary<MembershipTransition, MembershipTransition> map = _map.ToFrozenDictionary();
        _map = null!;
        foreach (var entry in entries) {
            var newState = map.TryGetValue(entry.State, out var value) ? value : entry.State;
            yield return newState == entry.State ? entry : entry with { State = newState };
        }
    }

    [Benchmark]
    public void TestAmbiguateMembershipsBinMask() => AmbiguateMembershipsBinMask().Consume(new Consumer());

    public IEnumerable<MembershipEntry> AmbiguateMembershipsBinMask() {
        uint mask = 0;
        // dont mask last 4 bits
        if (!DoDisambiguate || !DisambiguateProfileUpdates) mask |= (uint)MembershipTransition.ProfileUpdate >> 4;
        if (!DoDisambiguate || !DisambiguateKicks) mask |= (uint)MembershipTransition.Kick >> 4;
        if (!DoDisambiguate || !DisambiguateUnbans) mask |= (uint)MembershipTransition.Unban >> 4;
        if (!DoDisambiguate || !DisambiguateInviteActions || !DisambiguateInviteAccepted) mask |= (uint)MembershipTransition.InviteAccepted >> 4;
        if (!DoDisambiguate || !DisambiguateInviteActions || !DisambiguateInviteRejected) mask |= (uint)MembershipTransition.InviteRejected >> 4;
        if (!DoDisambiguate || !DisambiguateInviteActions || !DisambiguateInviteRetracted) mask |= (uint)MembershipTransition.InviteRetracted >> 4;
        if (!DoDisambiguate || !DisambiguateKnockActions || !DisambiguateKnockAccepted) mask |= (uint)MembershipTransition.KnockAccepted >> 4;
        if (!DoDisambiguate || !DisambiguateKnockActions || !DisambiguateKnockRejected) mask |= (uint)MembershipTransition.KnockRejected >> 4;
        if (!DoDisambiguate || !DisambiguateKnockActions || !DisambiguateKnockRetracted) mask |= (uint)MembershipTransition.KnockRetracted >> 4;
        mask = (mask << 4) + 0b1111;
        // Console.WriteLine(mask.ToString("b24"));
        foreach (var entry in entries) {
            if (((uint)entry.State & 0b1111_1111_0000) == 0) {
                yield return entry;
                continue;
            }

            var newState = (MembershipTransition)((uint)entry.State & mask);
            // Console.WriteLine(((uint)newState).ToString("b32"));
            yield return newState == entry.State ? entry : entry with { State = newState };
        }
    }
}