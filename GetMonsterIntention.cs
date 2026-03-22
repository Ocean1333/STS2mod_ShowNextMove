using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using System.Reflection;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using System.ComponentModel.Design;

namespace IntentionStateMachine;

public static class GetMonsterIntention
{
    private static readonly FieldInfo? MachineField = typeof(MonsterModel).GetField("_moveStateMachine",
        BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly FieldInfo? currentStateField = typeof(MonsterMoveStateMachine).GetField("_currentState",
        BindingFlags.NonPublic | BindingFlags.Instance);
    public static string GetNextIntention(NCreature creature)
    {
        var monsterEntity = creature.Entity;
        var monster = creature.Entity?.Monster;
        if (monster == null) return null;
        // 获取状态机
        var machine = MachineField?.GetValue(monster) as MonsterMoveStateMachine;
        if (machine == null) return "Cannot get machine";

        var currentState = currentStateField?.GetValue(machine) as MonsterState;
        if (currentState == null) return "Cannot get current state";

        var Rng = creature.Entity.CombatState?.RunState.Rng.MonsterAi;
        if (Rng == null) return "Cannot get rng";
        var simulationRng = new Rng(Rng.Seed, Rng.Counter);

        var nextMoveState = GetNextMoveState(currentState, machine, monsterEntity, simulationRng);
        if (nextMoveState == null) return "No next move";

        var intents = nextMoveState.Intents;
        if (intents == null || intents.Count == 0) return "No intents";

        var targets = monsterEntity.CombatState?.Players.Select(p => p.Creature).ToList() ?? new List<Creature>();
        var owner = monsterEntity;

        var descriptions = new List<string>();
        foreach (var intent in intents)
        {
            var hoverTip = intent.GetHoverTip(targets, owner);
            descriptions.Add(hoverTip.Description);
        }
        return  string.Join("\n", descriptions);
        // string nextStateId = currentState.GetNextState(creature.Entity, simulationRng);
        // var machineField = monster.GetType().GetField("_moveStateMachine", BindingFlags.NonPublic | BindingFlags.Instance);

        // var nextmove = monster.NextMove;
        // if (nextmove?.Intents == null || nextmove.Intents.Count == 0) return "This monster has no intention";
        // IReadOnlyList<AbstractIntent> intents = nextmove.Intents;

    }
    private static MoveState GetNextMoveState(MonsterState state, MonsterMoveStateMachine machine, Creature owner, Rng rng, int depth = 0)
    {
        const int maxDepth = 20; // 防止无限循环
        if (depth > maxDepth) return null;
        // 调用当前状态的 GetNextState
        string nextStateId = state.GetNextState(owner, rng);
        if (string.IsNullOrEmpty(nextStateId)) return null;
        // 获取所有状态字典
        var states = machine.States;
        if (states == null || !states.TryGetValue(nextStateId, out var nextState))
            return null;
        // 如果是 MoveState 则返回，否则递归继续
        if (nextState is MoveState moveState)
            return moveState;
        else
            return GetNextMoveState(nextState, machine, owner, rng, depth + 1);
    }
    // private static string DescribeIntention(AbstractIntent intent)
    // {
    //     var type = intent.GetType();

    // }
}