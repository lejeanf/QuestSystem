using System;
using UnityEngine;

namespace jeanf.questsystem
{
    /// <summary>
    /// Tracks when <see cref="QuestManager"/> has finished loading addressable quest definitions.
    /// Consumers should await <see cref="WhenReadyAsync"/> before driving quest lifecycle (start/finish/init).
    /// </summary>
    public static class QuestCatalogue
    {
        public static bool IsReady { get; private set; }
        public static bool HasFailed { get; private set; }
        public static Exception LoadFailure { get; private set; }

        public static event Action Ready;
        public static event Action<Exception> Failed;

        public static async Awaitable WhenReadyAsync()
        {
            while (!IsReady && !HasFailed)
                await Awaitable.NextFrameAsync();

            if (HasFailed)
                throw new InvalidOperationException(
                    "Quest catalog failed to load addressable quests.",
                    LoadFailure);
        }

        internal static void BeginLoad()
        {
            IsReady = false;
            HasFailed = false;
            LoadFailure = null;
        }

        internal static void MarkReady()
        {
            if (IsReady)
                return;

            IsReady = true;
            Ready?.Invoke();
        }

        internal static void MarkFailed(Exception exception)
        {
            if (HasFailed)
                return;

            HasFailed = true;
            LoadFailure = exception;
            Failed?.Invoke(exception);
        }

        internal static void Reset()
        {
            IsReady = false;
            HasFailed = false;
            LoadFailure = null;
        }
    }
}
