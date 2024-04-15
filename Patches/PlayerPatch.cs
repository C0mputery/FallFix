using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace FallFix.Patches
{
    [HarmonyPatch(typeof(Player))]
    public static class PlayerPatch
    {
        internal static void FixFalling(Player player)
        {
            FallFix.Logger.LogInfo("Fixing falling");
            player.refs.ragdoll.rigList.ForEach(rig => rig.velocity = Vector3.zero);
            player.data.sinceGrounded = 0f;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(Player.Update))]
        static IEnumerable<CodeInstruction> ConnectionStartPatch(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .SearchForward(i => i.opcode == OpCodes.Call && i.OperandIs(AccessTools.Method(typeof(Player), nameof(Player.MoveAllRigsInDirection))))
                .Advance(-1)
                .ThrowIfInvalid("ConnectionStartPatch did not work!")
                .Insert([
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlayerPatch), nameof(FixFalling)))
                    ] )
                .InstructionEnumeration();
        }
    }
}
