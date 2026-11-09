using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModularFI
{
    /// <summary>
    /// The registration slots behind the Register*Override methods.
    /// An override slot holds one delegate: the first registration wins, a later one from
    /// somewhere else is refused and logged with both owners, and registering the same
    /// delegate again (an addon that runs on every Space Centre visit) is accepted silently.
    /// A Pre/Post list holds any number of delegates, each one once.
    /// </summary>
    internal static class HookRegistry
    {
        internal static bool Register<T>(ref T slot, T dlg, string hook) where T : class
        {
            Delegate d = dlg as Delegate;
            if (d == null)
            {
                print(hook + ": a null override was ignored");
                return false;
            }

            if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
            {
                print(hook + " override registered from the " + HighLogic.LoadedScene + " scene by " + Owner(d)
                      + ". The Space Centre is the usual place, so that every vessel of a game uses the same hooks.");
            }

            Delegate current = slot as Delegate;
            if (current == null)
            {
                slot = dlg;
                print(hook + " is overridden by " + Owner(d));
                return true;
            }

            if (current.Equals(d))
            {
                return true;
            }

            print(hook + " is already overridden by " + Owner(current) + ". The override from " + Owner(d) + " is ignored.");
            return false;
        }

        internal static bool Unregister<T>(ref T slot, T dlg, string hook) where T : class
        {
            Delegate current = slot as Delegate;
            Delegate d = dlg as Delegate;
            if (current == null || d == null || !current.Equals(d))
            {
                return false;
            }

            slot = null;
            print(hook + " override from " + Owner(d) + " removed. The stock code runs again.");
            return true;
        }

        /// <summary>
        /// Adds a handler to a Pre/Post list unless the same handler is already in it.
        /// </summary>
        internal static void Add<T>(ref T list, T dlg) where T : class
        {
            Delegate d = dlg as Delegate;
            if (d == null)
            {
                return;
            }

            Delegate current = list as Delegate;
            if (current != null)
            {
                Delegate[] handlers = current.GetInvocationList();
                for (int i = 0; i < handlers.Length; i++)
                {
                    if (handlers[i].Equals(d))
                    {
                        return;
                    }
                }
            }

            list = Delegate.Combine(current, d) as T;
        }

        internal static void Remove<T>(ref T list, T dlg) where T : class
        {
            list = Delegate.Remove(list as Delegate, dlg as Delegate) as T;
        }

        private static readonly Dictionary<Delegate, int> handlerFailures = new Dictionary<Delegate, int>();

        /// <summary>
        /// Logs an exception thrown by a Pre/Post handler: in full the first time, then one
        /// line every thousand failures, so a handler that throws every step does not fill
        /// the log by itself.
        /// </summary>
        internal static void ReportHandlerException(string hook, Delegate handler, Exception e)
        {
            int failures;
            handlerFailures.TryGetValue(handler, out failures);
            failures++;
            handlerFailures[handler] = failures;

            if (failures == 1)
            {
                Debug.LogError("[ModularFlightIntegrator] " + hook + " handler " + Owner(handler) + " threw an exception. It is skipped this step; further exceptions from it are counted and logged every 1000 failures.\n" + e);
            }
            else if (failures % 1000 == 0)
            {
                Debug.LogError("[ModularFlightIntegrator] " + hook + " handler " + Owner(handler) + " has thrown " + failures + " times. Last: " + e.GetType().Name + ": " + e.Message);
            }
        }

        /// <summary>
        /// "Assembly.Type.Method" of a delegate, for the log.
        /// </summary>
        internal static string Owner(Delegate d)
        {
            if (d == null || d.Method == null)
            {
                return "an unknown delegate";
            }

            Type type = d.Method.DeclaringType;
            string typeName = type == null ? "?" : type.Assembly.GetName().Name + "." + type.Name;
            return typeName + "." + d.Method.Name;
        }

        private static void print(string msg)
        {
            MonoBehaviour.print("[ModularFlightIntegrator] " + msg);
        }
    }
}
